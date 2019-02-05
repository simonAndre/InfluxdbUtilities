using CsvHelper.Configuration;
using System;
using System.Globalization;

namespace moveDataTimeseries.fieldsDefinition
{

    /// <summary>
    /// données de data brutes Aziumut
    /// </summary>
    public class Data : IAzValue
    {
         [Tag]
        public int AffaireID { get; set; }
        [Tag]
        public int voieid { get; set; }
        [Tag]
        public int TypeVoieID { get; set; }
        [Field]
        public int ntsid { get; set; }
        [Tag]
        public int TypeCanalID { get; set; }
        [Field]
        public double? value { get; set; }
       
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
            return $"aff={AffaireID},vid={voieid},tv={TypeVoieID},nid={ntsid} value={value},tcid={TypeCanalID} {tstamp}";
        }

        //AffaireID,voieid,TypeVoieID,ntsid,date,TypeCanalID,ConvertedValue
        public sealed class DataMap : ClassMap<Data>
        {
            public DataMap()
            {
                Map(m => m.AffaireID).Index(0);
                Map(m => m.voieid).Index(1);
                Map(m => m.TypeVoieID).Index(2);
                Map(m => m.ntsid).Index(3).Optional().Default(0);
                Map(m => m.TypeCanalID).Index(5);
                Map(m => m.value).Index(6).ConvertUsing(row => {
                    double d;
                //double.Parse(row.GetField(6).Replace('.', ','))
                if (double.TryParse(row.GetField(6), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture("en-US"), out d))
                        return d;
                    throw new Exception($"bad value : {row.GetField(6)}");
                });
                Map(m => m.Time).ConvertUsing(row =>
                {
                    if (string.IsNullOrEmpty(row.GetField(4)))
                        return null;
                    var dt=DateTimeOffset.Parse(row.GetField(4));
                    return dt.UtcDateTime;
                });
            }
        }
      
    }
}
