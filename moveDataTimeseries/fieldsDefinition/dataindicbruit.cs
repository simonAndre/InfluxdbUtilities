using CsvHelper.Configuration;
using System;
using System.Globalization;

namespace moveDataTimeseries.fieldsDefinition
{

    /// <summary>
    /// données indicateurs Aziumut
    /// </summary>
    public class Dataindicbruit : IAzValue
    {
         [Tag]
        public int AffaireID { get; set; }

          [Tag]
        public int emplacementid { get; set; }

        [Tag]
        public int typeindicateurid { get; set; }

        [Field]
        public int fichemetierid { get; set; }

        [Field]
        public string code { get; set; }

        [Field]
        public int val { get; set; }
       
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
            return $"aff={AffaireID},empid={emplacementid},tindicid={typeindicateurid} fmid={fichemetierid},code={code},value={val} {tstamp}";
        }

        //AffaireID,voieid,TypeVoieID,ntsid,date,TypeCanalID,ConvertedValue
        public sealed class DataindicbruitMap : ClassMap<Dataindicbruit>
        {
            //fm.AffaireID, date,value,code,FicheMetierid,EmplacementID,typeindicateurid
            public DataindicbruitMap()
            {
                Map(m => m.AffaireID).Index(0);
                Map(m => m.emplacementid).Index(5);
                Map(m => m.fichemetierid).Index(4);
                Map(m => m.code).Index(3).Optional().Default(null);
                Map(m => m.typeindicateurid).Index(6);
                Map(m => m.val).Index(2);
                Map(m => m.Time).ConvertUsing(row =>
                {
                    if (string.IsNullOrEmpty(row.GetField(1)))
                        return null;
                    var dt=DateTimeOffset.Parse(row.GetField(1));
                    return dt.UtcDateTime;
                });
            }
        }
      
    }
}
