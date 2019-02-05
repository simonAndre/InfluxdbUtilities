using CsvHelper;
using moveDataTimeseries.fieldsDefinition;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moveDataTimeseries
{

    public class CsvDataLoading : ICsvDataLoading
    {
        private string _filepath;
        private string _measurementname;
        private Verbosity _verbose;

        public CsvDataLoading(DataLoadOptions options)
        {
            _filepath = options.filepath;
            _measurementname = (options.tablename ?? options.datatype.ToLower()).ToLower();
            _verbose = options.Verbose;
            Datatype = this.GetType().Assembly.GetType("moveDataTimeseries.fieldsDefinition." + options.datatype, true, true);
            Options = options;

            if (_verbose > 0)
            {
                Console.WriteLine($"import data file path : {_filepath}");
                Console.WriteLine($"data type : {Datatype.Name}");
                Console.WriteLine($"start from line : {options.startline}");
                Console.WriteLine($"up to line : {options.endline}");
                Console.WriteLine($"batches max size  : {options.batchsize}");
                Console.WriteLine($"measurementname to import to : {_measurementname}");
                if (options.Verbose == Verbosity.verbose)
                    Console.WriteLine($"Verbose output enabled.");
                Console.WriteLine("");
            }
        }

        public Type Datatype { get; }
        public DataLoadOptions Options { get; }

        private class Influxfile
        {
            public Influxfile(string filepath, int batchcount, Verbosity verbose)
            {
                currentFileSize = 0;
                lines = 0;
                filename = filepath.Replace(".csv", $"_out_{batchcount}.csv");
                writer = new StreamWriter(File.Create(filename));
                if (verbose > Verbosity.mute)
                    Console.WriteLine($">>>>>> create file {filename}");
            }
            public StreamWriter writer { get; set; }
            public int lines { get; set; }
            public int currentFileSize { get; set; }
            public string filename { get; set; }
        }

        /// <summary>
        /// convert the data to influxdb import format files of filesize> max size
        /// nota : in the class <typeparamref name="T"/>, the method ToString must be overridden to render a line in the influxDb line protocole
        /// </summary>
        /// <param name="filesizemax"></param>
        /// <returns>nb of files created</returns>
        public async Task<Tuple<int, int>> ConvertInfluxAsync(int filesizemax = 23068672)
        {
            //ConcurrentQueue<StreamWriter> writerQueue = new ConcurrentQueue<StreamWriter>();
            ConcurrentQueue<Influxfile> writerQueue = new ConcurrentQueue<Influxfile>();

            return await BatchRunAsync(
                item =>
                {
                    Influxfile iflxfile;
                    if (!writerQueue.TryPeek(out iflxfile))
                        throw new Exception("Algo exception : unable to retreive Stream writer");
                    string influxline = _measurementname + ',' + item.ToString();
                    iflxfile.writer.WriteLine(influxline);
                    iflxfile.lines++;
                    iflxfile.currentFileSize += influxline.Length;
                    if (_verbose == Verbosity.verbose)
                        Console.WriteLine($"add point {iflxfile.lines} to file :   >{item}");
                    return (iflxfile.currentFileSize >= filesizemax);
                },
                //actionBatchStart
                batchcount =>
                    {
                        writerQueue.Enqueue(new Influxfile(_filepath, batchcount, _verbose));
                    },
                //actionBatchEnd
                async batchcount =>
                    {
                        Influxfile iflxfile;
                        if (writerQueue.TryDequeue(out iflxfile))
                        {
                            if (_verbose > Verbosity.mute)
                                Console.WriteLine($">>>>>> Flush {iflxfile.lines} lines to export file {iflxfile.filename}");
                            await iflxfile.writer.FlushAsync();
                            iflxfile.writer.Close();
                        }
                    },
                Options.startline, Options.endline, Options.batchsize);
        }



        public async Task<Tuple<int, int>> Explore()
        {
            return await BatchRunAsync(line =>
                    {
                        if (Options.Verbose > Verbosity.mute)
                            Console.WriteLine(_measurementname + ',' + line);
                        return false;
                    }, start: Options.startline, end: Options.endline
                );
        }

        /// <summary>
        /// run a action on the csv file data per batch of <batchsie></batchsie> max size.
        /// </summary>
        /// <param name="actionPerLine">action to perform on a line of data : param=item read, result : true= need flush</param>
        /// <param name="actionBatchStart">action to perform at the begining of the batch. param1=batch nb</param>
        /// <param name="actionBatchEnd">action to perform at the end of the batch. param1=batch nb</param>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <param name="batchsize">size of the batch in points (influxdb lines)</param>
        /// <returns>nb of lines | nb of batches</returns>
        public async Task<Tuple<int, int>> BatchRunAsync(Func<IAzValue, bool> actionPerLine, Action<int> actionBatchStart = null, Func<int, Task> actionBatchEnd = null, int start = 0, int end = -1, int batchsize = 100)
        {
            int size = 0, batchcount = 0, nb = 0;
            actionBatchStart?.Invoke(0);
            foreach (IAzValue item in ReadData(start, end))
            {
                size++;
                nb++;
                if (actionPerLine(item) || size >= batchsize)
                {
                    size = 0;
                    if (actionBatchEnd != null && batchcount++ > 0)
                        await actionBatchEnd(batchcount);
                    actionBatchStart?.Invoke(batchcount);
                }
            }
            if (actionBatchEnd != null)
                await actionBatchEnd(batchcount);
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
            {
                using (var csv = new CsvReader(reader, FieldConfigurations.GetConfiguration(Datatype.Name)))
                {
                    var records = csv.GetRecords(Datatype);
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
}
