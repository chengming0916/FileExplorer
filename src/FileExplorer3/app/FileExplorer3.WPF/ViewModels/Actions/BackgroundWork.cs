﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer3.ViewModels.Actions
{
    public class BackgroundWork : IResult
    {
        private readonly Action _work;
        private readonly Action _onSuccess;
        private readonly Action<Exception> _onFail;

        public BackgroundWork(Action work, Action onSuccess, Action<Exception> onFail)
        {
            _work = work;
            _onSuccess = onSuccess;
            _onFail = onFail;
        }

        #region Implementation of IResult

        public void Execute(ActionExecutionContext context)
        {
            Exception error = null;
            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                try
                {
                    _work();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                if (error == null && _onSuccess != null)
                    _onSuccess.OnUIThread();

                if (error != null && _onFail != null)
                {
                    Caliburn.Micro.Execute.OnUIThread(() => _onFail(error));
                }

                Completed(this, new ResultCompletionEventArgs { Error = error });
            };
            worker.RunWorkerAsync();
        }

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        #endregion
    }
}
