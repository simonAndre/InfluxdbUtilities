﻿using CommandLine;

namespace moveDataTimeseries
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Option('b', "batchsize", Required = false, Default = 500, HelpText = "size of the batch to group the data to send to the database in points (influxdb lines).")]
        public int batchsize { get; set; }
        [Option('s', "start line", Required = false, Default = 0, HelpText = "line to start the conversion/extraction")]
        public int startline { get; set; }
        [Option('e', "end line", Required = false, Default = -1, HelpText = "line to end the conversion/extraction (-1 means : don't stop before the end of the file)")]
        public int endline { get; set; }

        [Option("tablename", Required = false, HelpText = "optional : to override the table name to import the data to (by default, infered from the file name)")]
        public string tablename { get; set; }


        [Option('f', Required = true, HelpText = "csv file path")]
        public string filepath { get; set; }

    }

    [Verb("export", HelpText = "Export data to influxDb database directly")]
    public class ExportOptions : Options
    {
        [Option("serveruri", Required = false, Default = "http://localhost:8086", HelpText = "influxdb server URI")]
        public string serveruri { get; set; }
        [Option("db", Required = false, Default = "Export", HelpText = "influxdb database name")]
        public string database { get; set; }
    }
    [Verb("convert", HelpText = "convert data to a file in line protocole ready to be imported to an influxDb database via the -import option of the influx command")]
    public class ConvertOptions : Options
    {
        [Option("fsmax", Required = false, Default = 20, HelpText = "in Mb : max size of the out files (if the input file oversize this limit, many output iles will be generated)")]
        public int filesizemax { get; set; }
        
    }
    [Verb("explore", HelpText = "just write out some lines in the influxdb line protocole")]
    public class ExploreOptions : Options
    {
    }

}