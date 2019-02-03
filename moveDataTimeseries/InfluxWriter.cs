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
    public class InfluxWriter<T> where T : IAzValue
    {
        private ICsvDataLoading<T> _csvdataloading;
        private bool _verbose;
        public InfluxWriter(ICsvDataLoading<T> csvdataloading, bool verbose = false)
        {
            _csvdataloading = csvdataloading;
            _verbose = verbose;
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
        public async Task<Tuple<int,int>> writeInfludb(string database,string tablename, int start = 0, int end = -1, string uri = "http://localhost:8086", int batchsize = 100)
        {
            //get properties for fields and tags (tags=indexed fields in influxdb)
            var tags = typeof(T).GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is TagAttribute));
            var fields = typeof(T).GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is FieldAttribute));
            string measurementname = tablename??typeof(T).Name;
            var client = new LineProtocolClient(new Uri(uri), database);

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
                }, start, end, batchsize);
            }
            finally
            {
            }
        }

        [Obsolete]
        public async void writeInfludb_(IEnumerable<T> dataset, string database, string uri = "http://localhost:8086")
        {
            var tags = typeof(T).GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is TagAttribute));
            var fields = typeof(T).GetProperties().Where(p => p.GetCustomAttributes().Any(a => a is FieldAttribute));
            string measurementname = typeof(T).Name;
            var client = new LineProtocolClient(new Uri(uri), database);
            LineProtocolPayload payload;

            payload = new LineProtocolPayload();
            foreach (IAzValue item in dataset)
            {
                if (item.Time.HasValue)
                {
                    var tagsvalue = new Dictionary<string, string>(tags.Select(t => new KeyValuePair<string, string>(t.Name, t.GetValue(item).ToString())));
                    var fieldsvalue = new Dictionary<string, object>(fields.Select(t => new KeyValuePair<string, object>(t.Name, t.GetValue(item))));
                    var influxline = new LineProtocolPoint(measurementname, fieldsvalue, tagsvalue, item.Time.Value);
                    payload.Add(influxline);
                }
                //if(i++%10000==0)
                //    Flush(i,client,ref payload);
            }
            //Flush(i, client,ref payload);
            var res = await client.WriteAsync(payload);
            if (!res.Success)
                Console.Error.WriteLine(res.ErrorMessage);
        }

        private void Flush(int i, LineProtocolClient client, ref LineProtocolPayload payload)
        {
            if (_verbose)
                Console.WriteLine($"lines to {i} flushed");
            client.WriteAsync(payload).ContinueWith(t =>
            {
                if (!t.Result.Success)
                    Console.Error.WriteLine(t.Result.ErrorMessage);
            }).Wait();
            payload = new LineProtocolPayload();
        }
    }
}