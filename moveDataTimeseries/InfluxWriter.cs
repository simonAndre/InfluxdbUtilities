using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace moveDataTimeseries
{
    public class InfluxWriter
    {
        private ICsvDataLoading _csvdataloading;
        private bool _verbose;

        private ExportOptions Options { get; }

        public InfluxWriter(ICsvDataLoading csvdataloading)
        {
            _csvdataloading = csvdataloading;
            Options = _csvdataloading.Options as ExportOptions;
            _verbose = Options.Verbose;
        }



        private class InfluxPayload
        {
            public InfluxPayload(string filepath, int batchcount, bool verbose)
            {
                currentFileSize = 0;
                lines = 0;
                //filename = filepath.Replace(".csv", $"_out_{batchcount}.csv");
                //writer = new StreamWriter(File.Create(filename));
                //if (verbose)
                //    Console.WriteLine($">>>>>> create file {filename}");
            }
            public LineProtocolPayload payload { get; set; }
            public int lines { get; set; }
            public int currentFileSize { get; set; }
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
        public async Task<Tuple<int,int>> writeInfludb()
        {
            //get properties for fields and tags (tags=indexed fields in influxdb)
            var tags = _csvdataloading.Datatype.GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is TagAttribute));
            var fields = _csvdataloading.Datatype.GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is FieldAttribute));
            string measurementname = Options.tablename?? _csvdataloading.Datatype.Name;
            var client = new LineProtocolClient(new Uri(Options.serveruri), Options.database);

            ConcurrentQueue<LineProtocolPayload> payloadQueue = new ConcurrentQueue<LineProtocolPayload>();
            try
            {
                return await _csvdataloading.BatchRunAsync(
                    // Action per line
                    item =>
                    {
                        LineProtocolPayload payload;
                        if (!payloadQueue.TryPeek(out payload))
                        {
                            payload = new LineProtocolPayload();
                            payloadQueue.Enqueue(payload);
                            if (_verbose)
                                Console.WriteLine(">>>>>> new payload creation");
                        }
                        if (item.Time.HasValue)         //without time
                        {
                            var tagsvalue = new Dictionary<string, string>(tags.Select(t => new KeyValuePair<string, string>(t.Name, t.GetValue(item).ToString())));
                            var fieldsvalue = new Dictionary<string, object>(fields.Select(t => new KeyValuePair<string, object>(t.Name, t.GetValue(item))));
                            var influxline = new LineProtocolPoint(measurementname, fieldsvalue, tagsvalue, item.Time.Value);
                            if (_verbose)
                                Console.WriteLine($"add point to PL : {item}");
                            payload.Add(influxline);
                        }
                        return false;
                    },
                //actionBatchStart
                async batchcount =>
                {
                    LineProtocolPayload payload;
                    Console.WriteLine($">>>>>>>  sending batch of data to influxdb");
                    if (payloadQueue.TryDequeue(out payload))
                    {
                        var resas = await client.WriteAsync(payload);
                        if (!resas.Success)
                            Console.Error.WriteLine(resas.ErrorMessage);
                    }
                }, Options.startline, Options.endline, Options.batchsize);
            }
            finally
            {
            }
        }

      
    }
}