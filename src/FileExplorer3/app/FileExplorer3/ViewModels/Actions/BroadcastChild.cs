using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.ViewModels.Actions
{
    public class BroadcastChild : IResult
    { 
        #region Cosntructor
        
        #endregion

        #region Methods
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties
        
        #endregion
        public event EventHandler<ResultCompletionEventArgs> Completed;

        public void Execute(ActionExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
