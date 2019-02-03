using CsvHelper.Configuration;
using System;

namespace moveDataTimeseries.fieldsDefinition
{


    /// <summary>
    /// données de paramètres de voies Aziumut
    /// </summary>
    public class Parametres : IAzValue
    {
        [Tag]
        public int voieid { get; set; }
        [Tag]
        public string pn { get; set; }
        [Field]
        public string value { get; set; }
       
        [Tag]
        public int td { get; set; }

        public DateTime? Time { get; set; }

        public long? tstamp
        {
            get
            {
                if (Time.HasValue)
                    return new DateTimeOffset(Time.Value).ToUnixTimeMilliseconds() * 1000000;
                return null;
            }
        }

        /// <summary>
        /// overriden to render in the influxDb line protocole
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"voieid={voieid},td={td},parm={pn} value={value} {tstamp}";
        }

        public sealed class ParametersMap : ClassMap<Parametres>
        {
            public ParametersMap()
            {
                Map(m => m.voieid).Index(0);
                Map(m => m.pn).Index(1);
                Map(m => m.value).Index(2);
                Map(m => m.td).Index(4);
                Map(m => m.Time).ConvertUsing(row =>
                {
                    if (string.IsNullOrEmpty(row.GetField(3)))
                        return null;
                    var dt=DateTimeOffset.Parse(row.GetField(3));
                    return dt.UtcDateTime;
                });
            }
        }
      
    }
}
