using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.ViewModels.Actions
{
    public class ToggleRename : IResult
    { 
        #region Cosntructor

        public ToggleRename(IFileListViewModel model, bool? value = null)
        {
            _model = model;
            _value = value;
        }
        
        #endregion

        #region Methods

        public event EventHandler<ResultCompletionEventArgs> Completed;

        public void Execute(ActionExecutionContext context)
        {
            var first = _model.Selection.SelectedItems.FirstOrDefault();
            if (first != null && first.IsEditable)
                if (_value.HasValue)
                    first.IsEditing = _value.Value;
                else first.IsEditing = !first.IsEditing;
            Completed(this, new ResultCompletionEventArgs());
        }

        
        #endregion

        #region Data

        IFileListViewModel _model;
        bool? _value;
        
        #endregion

        #region Public Properties
        
        #endregion
    }
}
