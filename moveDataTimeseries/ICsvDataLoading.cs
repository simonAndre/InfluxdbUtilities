using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace moveDataTimeseries
{
    public interface ICsvDataLoading
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <returns></returns>
        IEnumerable<IAzValue> ReadData(int start = 0, int end = -1);

        /// <summary>
        /// run a action per batch of <batchsie></batchsie> max size. Async form
        /// </summary>
        /// <param name="actionPerLine">action to perform on a line of data : param=item read, result : true= need flush</param>
        /// <param name="actionBatchStart">action to perform at the begining of the batch. param1=batch nb</param>
        /// <param name="actionBatchEnd">action to perform at the end of the batch. param1=batch nb</param>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <param name="batchsize">size of the batch in bytes</param>
        /// <returns></returns>
        Task<Tuple<int, int>> BatchRunAsync(Func<IAzValue, bool> actionPerLine, Action<int> actionBatchStart=null, Func<int, Task> actionBatchEnd=null, int start = 0, int end = -1, int batchsize = 100);

        /// <summary>
        /// Type of managed data 
        /// </summary>
        Type Datatype { get; }
        DataLoadOptions Options { get; }
    }
}