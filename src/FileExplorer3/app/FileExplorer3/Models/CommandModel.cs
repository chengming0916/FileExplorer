using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core.Script;

namespace FileExplorer.Models
{
    public class CommandModel : PropertyChangedBase, ICommandModel
    {

        #region Constructor

        public CommandModel(IScriptCommand command, object parameter = null)
        {
            _command = command;
            _parameter = parameter;
        }

        #endregion

        #region Methods
       
        public int CompareTo(object obj)
        {
            if (obj is ICommandModel)
                return CompareTo(obj as ICommandModel);
            return -1;
        }

        public int CompareTo(ICommandModel other)
        {
            if (other != null)
                return -this.Priority.CompareTo(other.Priority);
            return -1;
        }
        #endregion

        #region Data
        private string _commandType;
        private IScriptCommand _command;

        private char? _symbol;
        private System.Drawing.Bitmap _headerIcon;
        private string _toolTip;
        
        private string _header;
        private int _priority;
        private bool _isChecked, _isEnabled;
        private object _parameter;        

        #endregion

        #region Public Properties
        public string CommandType { get { return _commandType; } set { _commandType = value; NotifyOfPropertyChange(() => CommandType); } }
        public IScriptCommand Command { get { return _command; } set { _command = value; NotifyOfPropertyChange(() => Command); } } 

        public char? Symbol { get { return _symbol; } set { _symbol = value; NotifyOfPropertyChange(() => Symbol); } }
        public System.Drawing.Bitmap HeaderIcon { get { return _headerIcon; } set { _headerIcon = value; NotifyOfPropertyChange(() => HeaderIcon); } }
        public string ToolTip { get { return _toolTip; } set { _toolTip = value; NotifyOfPropertyChange(() => ToolTip); } }
        public string Header { get { return _header; } set { _header = value; NotifyOfPropertyChange(() => Header); } }
         public bool IsChecked { get { return _isChecked; } set { _isChecked = value; NotifyOfPropertyChange(() => IsChecked); } }
         public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; NotifyOfPropertyChange(() => IsEnabled); } }

        public int Priority { get { return _priority; } set { _priority = value; NotifyOfPropertyChange(() => Priority); } }

        #endregion

    }
}
