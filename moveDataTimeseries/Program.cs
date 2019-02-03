using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace moveDataTimeseries
{

   

    class Program
    {
        /// <summary>
        /// usage : https://github.com/commandlineparser/commandline
        /// 
        /// </summary>


        public static void Main(string[] args)
        {
            Options options;
            var res = Parser.Default.ParseArguments<ExportOptions, ConvertOptions, ExploreOptions>(args)
              .WithParsed<ExportOptions>(o =>
              {
                  options = o;
                  Console.WriteLine($"database {o.serveruri}");
                  if (o.Verbose)
                      Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                  Export(o);
              })
              .WithParsed<ConvertOptions>(o =>
              {
                  options = o;
                  if (o.Verbose)
                      Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                  Convert(o);
              })
              .WithParsed<ExploreOptions>(o =>
              {
                  options = o;
                  if (o.Verbose)
                      Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                  Explore(o);
              });
            Console.WriteLine();
            Console.WriteLine("end of the job.");
            Console.WriteLine("Please, hit a key...");
            Console.ReadKey();

        }

        public static void Export(ExportOptions o)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var csv = new CsvDataLoading<Parametres>(o.filepath,o.tablename, o.Verbose);
            (new InfluxWriter<Parametres>(csv).writeInfludb(o.database,o.tablename, o.startline, o.endline, batchsize: o.batchsize,uri:o.serveruri))
                .ContinueWith(t => Console.WriteLine($"export done : {t.Result.Item1} points exported in {t.Result.Item2+1} batches. Duration : {sw.ElapsedMilliseconds}ms")).Wait();
        }


        public static void Explore(ExploreOptions o)
        {
            var csv = new CsvDataLoading<Parametres>(o.filepath, o.tablename, o.Verbose);
            Console.WriteLine($"read lines {o.startline} to {o.endline}");
            string filename=Path.GetFileName( o.filepath);
            string measurementname = o.tablename ?? filename.Replace(".csv", "");
            foreach (var item in csv.ReadData(o.startline, o.endline))
            {
                Console.WriteLine(measurementname +',' + item);
            }
        }

        public static void Convert(ConvertOptions o)
        {
            var csv = new CsvDataLoading<Parametres>(o.filepath, o.tablename,o.Verbose);
            int filesizemax = 1048576 * o.filesizemax; //filesize max in Mb
            //(csv.ConvertInfluxAsync(filesizemax)).Wait();
            csv.ConvertInflux(filesizemax);
        }

    }
}
