using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace moveDataTimeseries
{
    public class InfluxWriter
    {
        private ICsvDataLoading _csvdataloading;
        private Verbosity _verbose;

        private ExportOptions Options { get; }

        public InfluxWriter(ICsvDataLoading csvdataloading)
        {
            _csvdataloading = csvdataloading;
            Options = _csvdataloading.Options as ExportOptions;
            _verbose = Options.Verbose;
        }



        private class InfluxPayload
        {
            public InfluxPayload(int batchcount, Verbosity verbose)
            {
                lines = 0;
                batchnumber = batchcount;
                payload = new LineProtocolPayload();
                if (verbose > Verbosity.lowlevel)
                    Console.WriteLine(">>>>>> new payload creation");
            }
            public LineProtocolPayload payload { get; set; }
            public int lines { get; set; }
            public int batchnumber { get; set; }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="uri"></param>
        /// <param name="batchsize">size of the batches in ponts</param>
        /// <returns>nb of batchs done</returns>
        public async Task<Tuple<int, int>> writeInfludb()
        {
            //get properties for fields and tags (tags=indexed fields in influxdb)
            var tags = _csvdataloading.Datatype.GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is TagAttribute));
            var fields = _csvdataloading.Datatype.GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is FieldAttribute));
            string measurementname = (Options.tablename ?? _csvdataloading.Datatype.Name).ToLower();
            var client = new LineProtocolClient(new Uri(Options.serveruri), Options.database);
            ConcurrentQueue<InfluxPayload> payloadQueue = new ConcurrentQueue<InfluxPayload>();
            return await _csvdataloading.BatchRunAsync(
                // Action per line
                item =>
                {
                    InfluxPayload bunch;
                    if (!payloadQueue.TryPeek(out bunch))
                        throw new Exception("Algo exception : unable to retreive payload bunch");

                    if (item.Time.HasValue)         //without time, no time-serie !
                        {
                        var tagsvalue = new Dictionary<string, string>(tags.Select(t => new KeyValuePair<string, string>(t.Name.ToLower(), t.GetValue(item).ToString())));
                        var fieldsvalue = new Dictionary<string, object>(fields.Select(t => new KeyValuePair<string, object>(t.Name.ToLower(), t.GetValue(item))));
                        var influxline = new LineProtocolPoint(measurementname, fieldsvalue, tagsvalue, item.Time.Value);
                        bunch.payload.Add(influxline);
                        bunch.lines++;
                        if (_verbose == Verbosity.verbose)
                            Console.WriteLine($"add point {bunch.lines} to PL #{bunch.batchnumber} :   >{item}");
                    }
                    return false;
                },
            //actionBatchStart
            async batchcount =>
            {
                payloadQueue.Enqueue(new InfluxPayload(batchcount, Options.Verbose));
            },
            actionBatchEnd:async (batchcount,lines,totallines) =>
            {
                InfluxPayload bunch;
                if (payloadQueue.TryDequeue(out bunch))
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    if (Options.Verbose > Verbosity.mute)
                        Console.WriteLine($">>> start to send batch of data #{bunch.batchnumber} ({bunch.lines} lines) to influxdb. total line already sent : {totallines}");
                    var resas = await client.WriteAsync(bunch.payload);
                    if (!resas.Success)
                        Console.Error.WriteLine($">>>!!!!>>> Error sending to influx the batch #{bunch.batchnumber} :  {resas.ErrorMessage}");
                    Console.WriteLine($">>> batch #{bunch.batchnumber} ({bunch.lines} lines) sent to the server in {sw.ElapsedMilliseconds}ms : OK");
                    bunch.payload = null;
                    bunch = null;
                }
            },start: Options.startline,end: Options.endline,batchsize: Options.batchsize);
        }
    }
}