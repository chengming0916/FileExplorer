using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public static class TaskUtils
    {
        /// <summary>
        /// Make sure continueTask runs even if prevTask is completed / faulted.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prevTask"></param>
        /// <param name="continueTask"></param>
        public static  void ContinueWithCheck<T>(this Task<T> prevTask, Action<Task<T>> continueTask)
        {
            if (prevTask.Status == TaskStatus.Running)
                prevTask.ContinueWith(continueTask);
            else continueTask(prevTask);
        }

    }
}
