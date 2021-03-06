﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace moveDataTimeseries
{



    class Program
    {
        /// <summary>
        /// usage of commandlineParser : https://github.com/commandlineparser/commandline
        /// 
        /// </summary>


        public static void Main(string[] args)
        {
            try
            {
                var res = Parser.Default.ParseArguments<ExportOptions, ConvertOptions, ExploreOptions, TypeListOptions>(args)
                  .WithParsed<ExportOptions>(o =>
                  {
                      Console.WriteLine($"Export data to influxDb Database.");
                      if (o.Verbose > Verbosity.mute)
                      {
                          Console.WriteLine($"export to server {o.serveruri}  database : {o.database}");
                      }

                      Stopwatch sw = Stopwatch.StartNew();
                      var csv = new CsvDataLoading(o);
                      (new InfluxWriter(csv).writeInfludb())
                          .ContinueWith(t => Console.WriteLine($"export done : {t.Result.Item1} points exported in {t.Result.Item2 + 1} batches. Duration : {sw.ElapsedMilliseconds}ms")).Wait();
                  })
                  .WithParsed<ConvertOptions>(o =>
                  {
                      Stopwatch sw = Stopwatch.StartNew();
                      var csv = new CsvDataLoading(o);
                      int filesizemax = 1048576 * o.filesizemax; //filesize max in Mb
                      csv.ConvertInfluxAsync(filesizemax).ContinueWith(t =>
                          Console.WriteLine($"Conversion done : {t.Result.Item1} points exported in {t.Result.Item2 + 1} files. Duration : {sw.ElapsedMilliseconds}ms"))
                          .Wait();
                  })
                  .WithParsed<ExploreOptions>(o =>
                  {
                      var csv = new CsvDataLoading(o);
                      csv.Explore().ContinueWith(t =>
                      {
                          Console.WriteLine($"Exploration done for {t.Result.Item1} points");
                      });
                  })
                  .WithParsed<TypeListOptions>(o =>
                  {
                      Console.WriteLine("Managed types :");
                      foreach (var datatype in FieldConfigurations.GetTypeList())
                      {
                          Console.WriteLine(datatype);
                      }
                      Console.WriteLine();
                  });
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fatal Error : {e.Message}");
                Console.WriteLine($"Stack trace: {e.StackTrace}");
            }
            Console.WriteLine();
            Console.WriteLine("end of the job.");
            // Console.WriteLine("Please, hit a key...");
            // Console.ReadKey();

        }


    }
}
