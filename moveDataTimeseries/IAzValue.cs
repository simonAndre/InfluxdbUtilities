using CsvHelper.Configuration;
using System;

namespace moveDataTimeseries
{
    public interface IAzValue
    {
        DateTime? Time { get; }
   
    }
}
