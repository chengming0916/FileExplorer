using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Actions
{
    public class BroadcastChild : IResult
    { 
        #region Cosntructor

        public BroadcastChild(IEntryModel model, Action<IDirectoryNodeViewModel> action)
        {

        }
        
        #endregion

        #region Methods

        public void Execute(ActionExecutionContext context)
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Data

        IEntryModel _model;
        
        #endregion

        #region Public Properties

        public event EventHandler<ResultCompletionEventArgs> Completed;
        
        #endregion
       

        
    }
}
