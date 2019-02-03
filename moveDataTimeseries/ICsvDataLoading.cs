using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace moveDataTimeseries
{
    public interface ICsvDataLoading<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <returns></returns>
        IEnumerable<T> ReadData(int start = 0, int end = -1);
        /// <summary>
        /// run a action per batch of <batchsie></batchsie> max size.
        /// </summary>
        /// <param name="actionPerLine">action to perform on a line of data</param>
        /// <param name="actionBatchStart">action to perform at the begining of the batch. param1=batch nb</param>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <param name="batchsize">size of the batch in bytes</param>
        /// <returns>nb of batches</returns>
        int BatchRun(Action<IAzValue> actionPerLine, Action<int> actionBatchStart, int start = 0, int end = -1, int batchsize = 23068672);

        /// <summary>
        /// run a action per batch of <batchsie></batchsie> max size. Async form
        /// </summary>
        /// <param name="actionPerLine">action to perform on a line of data</param>
        /// <param name="actionBatchStart">action to perform at the begining of the batch. param1=batch nb</param>
        /// <param name="start">line to start</param>
        /// <param name="end">line to stop</param>
        /// <param name="batchsize">size of the batch in bytes</param>
        /// <returns></returns>
        Task<Tuple<int, int>> BatchRunAsync(Action<IAzValue> actionPerLine, Func<int, Task> actionBatchStart, int start = 0, int end = -1, int batchsize = 23068672);
    }
}