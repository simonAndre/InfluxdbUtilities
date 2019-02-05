using CommandLine;

namespace moveDataTimeseries
{
    public class Optionsbase
    {
        [Option('v', "verbose", Default = 1, Required = false, HelpText = "Set output to verbose messages. Verbosity : 0=mute, 1=lowlevel, 2=verbose")]
        public Verbosity Verbose { get; set; }

    }
    public class DataLoadOptions: Optionsbase
    {
    
        [Option('b', "batchsize", Required = false, Default = 500, HelpText = "size of the batch to group the data to send to the database in points (influxdb lines).")]
        public int batchsize { get; set; }

        [Option('s', "start line", Required = false, Default = 0, HelpText = "line to start the conversion/extraction")]
        public int startline { get; set; }

        [Option('e', "end line", Required = false, Default = -1, HelpText = "line to end the conversion/extraction (-1 means : don't stop before the end of the file)")]
        public int endline { get; set; }

        [Option('t', "data type", Required = true, Default = "", HelpText = "type of data to read, to get a list of handled types, use the typelist command")]
        public string datatype { get; set; }

        [Option('f', Required = true, HelpText = "csv file path")]
        public string filepath { get; set; }

        [Option("tablename", Required = false, HelpText = "optional : to override the table name to import the data to (by default, infered from the datatype name)")]
        public string tablename { get; set; }

        [Option("force", Required = false,Default =true, HelpText = "true to continue on error, false to stop at the first encounterd error")]
        public bool force{ get; set; }

    }

    [Verb("export", HelpText = "Export data to influxDb database directly")]
    public class ExportOptions : DataLoadOptions
    {
        [Option("serveruri", Required = false, Default = "http://localhost:8086", HelpText = "influxdb server URI")]
        public string serveruri { get; set; }
        [Option("db", Required = false, Default = "Export", HelpText = "influxdb database name")]
        public string database { get; set; }
    }

    [Verb("convert", HelpText = "convert data to a file in line protocole ready to be imported to an influxDb database via the -import option of the influx command")]
    public class ConvertOptions : DataLoadOptions
    {
        [Option("fsmax", Required = false, Default = 20, HelpText = "in Mb : max size of the out files (if the input file oversize this limit, many output iles will be generated)")]
        public int filesizemax { get; set; }
        
    }

    [Verb("explore", HelpText = "To check the input data or just write out some lines in the influxdb line protocole")]
    public class ExploreOptions : DataLoadOptions
    {

    }

    [Verb("typelist", HelpText = "write out the list of managed types of data")]
    public class TypeListOptions : Optionsbase    
    {
    }

}
