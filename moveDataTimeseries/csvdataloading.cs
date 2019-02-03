using CsvHelper;
using moveDataTimeseries.fieldsDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moveDataTimeseries
{


    public class CsvDataLoading<IAzValue> : ICsvDataLoading<IAzValue>
    {
        private string _filepath;
        private string _measurementname;
        private bool _verbose;
        private string _datatype;
   
        public CsvDataLoading(string csvfilepath,string datatype, string measurementname, bool verbose = false)
        {
            _filepath = csvfilepath;
            _measurementname = measurementname ?? Path.GetFileName(_filepath).Replace(".csv", "");
            _verbose = verbose;
            _datatype = datatype;
        }



        /// <summary>
        /// convert the data to influxdb import format files of filesize> max size
        /// nota : in the class <typeparamref name="T"/>, the method ToString must be overridden to render a line in the influxDb line protocole
        /// </summary>
        /// <param name="filesizemax"></param>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <returns>nb of files created</returns>
        public int ConvertInflux(int filesizemax = 23068672, int start = 0, int end = -1)
        {
            StreamWriter Writer = null;
            try
            {
                return BatchRun(item => Writer.WriteLine(_measurementname + ',' + item.ToString()), (batchcount) =>
                    {
                        if (Writer != null)
                        {
                            Writer.Flush();
                            Writer.Close();
                        }
                        if (_verbose)
                            Console.WriteLine(">>>>>> Flush points to export file");
                        Writer = new StreamWriter(File.Create(_filepath.Replace(".csv", $"_out_{batchcount}.csv")));
                    }, start, end, filesizemax);
            }
            finally
            {
                Writer.Flush();
                Writer.Close();
            }
        }
        public async Task<Tuple<int,int>> ConvertInfluxAsync(int filesizemax = 23068672, int start = 0, int end = -1)
        {
            StreamWriter Writer = null;
            try
            {
                return await BatchRunAsync(
                    item =>
                    {
                        Writer.WriteLine(_measurementname + ',' + item.ToString());
                    },
                   async batchcount =>
            {
                if (Writer != null)
                {
                    await Writer.FlushAsync();
                    Writer.Close();
                }
                if (_verbose)
                    Console.WriteLine(">>>>>> Flush points to export file");
                Writer = new StreamWriter(File.Create(_filepath.Replace(".csv", $"_out_{batchcount}.csv")));
            },
                    start, end, filesizemax);
            }
            finally
            {
                await Writer.FlushAsync();
                Writer.Close();
            }
        }

        /// <summary>
        /// run a action on the csv file data per batch of <batchsie></batchsie> max size.
        /// </summary>
        /// <param name="actionPerLine">action to perform on a line of data</param>
        /// <param name="actionBatchStart">action to perform at the begining of the batch. param1=batch nb</param>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <param name="batchsize">size of the batch in bytes</param>
        /// <returns>nb of batches</returns>
        public int BatchRun(Action<IAzValue> actionPerLine, Action<int> actionBatchStart, int start = 0, int end = -1, int batchsize = 23068672)
        {
            int size = 0, batchcount = 0;
            actionBatchStart(0);
            foreach (IAzValue item in ReadData(start, end))
            {
                string s = item.ToString();
                size += s.Length;
                actionPerLine(item);
                if (size >= batchsize)
                {
                    size = 0;
                    batchcount++;
                    actionBatchStart(batchcount);
                }
            }
            return batchcount;
        }

        /// <summary>
        /// run a action on the csv file data per batch of <batchsie></batchsie> max size.
        /// </summary>
        /// <param name="actionPerLine">action to perform on a line of data</param>
        /// <param name="actionBatchStart">action to perform at the begining of the batch. param1=batch nb</param>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <param name="batchsize">size of the batch in points (influxdb lines)</param>
        /// <returns>nb of lines | nb of batches</returns>
        public async Task<Tuple<int, int>> BatchRunAsync(Action<IAzValue> actionPerLine, Func<int, Task> actionBatchStart, int start = 0, int end = -1, int batchsize = 100)
        {
            int size = 0, batchcount = 0, nb=0;
            await actionBatchStart(0);
            foreach (IAzValue item in ReadData(start, end))
            {
                size++;
                nb++;
                actionPerLine(item);
                if (size >= batchsize)
                {
                    size = 0;
                    batchcount++;
                    await actionBatchStart(batchcount);
                }
            }
            return new Tuple<int, int>(nb, batchcount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <returns></returns>
        public IEnumerable<IAzValue> ReadData(int start = 0, int end = -1)
        {
            if (end > -1 && end < start)
                throw new Exception("end must be greater than start");
            using (var reader = new StreamReader(_filepath, Encoding.UTF8))
            using (var csv = new CsvReader(reader, FieldConfigurations.GetConfiguration(_datatype)))
            {
                var records = csv.GetRecords(this.GetType().Assembly.GetType("moveDataTimeseries.fieldsDefinition."+_datatype));
                int i = start;

                foreach (IAzValue item in records.Skip(start))
                {
                    if (end > -1 && (i++ > end))
                        break;
                    yield return item;
                }
            }
        }

      
    }
}
