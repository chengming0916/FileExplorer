using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.ViewModels.Actions
{
    public class DoSomething : IResult
    {
        #region Cosntructor

        public DoSomething(System.Action action)
        {
            _action = action;
        }

        #endregion

        #region Methods

        public event EventHandler<ResultCompletionEventArgs> Completed;

        public void Execute(ActionExecutionContext context)
        {
            try
            {
                _action();
                Completed(this, new ResultCompletionEventArgs());
            }
            catch (Exception ex)
            {
                Completed(this, new ResultCompletionEventArgs() { Error = ex });
            }
        }

        #endregion

        #region Data

        private System.Action _action;

        #endregion

        #region Public Properties

        #endregion
    }
}
