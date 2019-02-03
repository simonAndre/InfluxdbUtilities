using CsvHelper.Configuration;
using moveDataTimeseries.fieldsDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace moveDataTimeseries
{
    public class FieldConfigurations
    {
        public static Configuration GetConfiguration(string datatypeName)
        {
            var conf = new CsvHelper.Configuration.Configuration()
            {
                Delimiter = "\t",
                HasHeaderRecord = false
            };
            switch (datatypeName)
            {
                case "Parametres":
                    conf.RegisterClassMap<Parametres.ParametersMap>();
                    return conf;
                case "Data":
                    conf.RegisterClassMap<Data.DataMap>();
                    return conf;
                default:
                    throw new Exception($"bad type name : the parsing of the type {datatypeName} is not implemented");
            }
        }

        private static List<string> _typeList;
        public static List<string> GetTypeList()
        {
            if (_typeList == null)
            {
                var asm = typeof(Parametres).Assembly;
                var tl= asm.GetTypes().Where(p =>
                      p.Namespace == "moveDataTimeseries.fieldsDefinition"
                      && !p.IsSealed
                );
                _typeList=tl.Select(t => t.Name).ToList();
            }
            return _typeList;
        }
    }
}
