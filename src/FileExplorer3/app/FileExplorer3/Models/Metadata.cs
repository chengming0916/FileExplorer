using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    public class Metadata : PropertyChangedBase, IMetadata
    {
        #region Cosntructor

        public Metadata(DisplayType displayType, string header, object content, bool isEditable = false)
        {
            DisplayType = displayType;
            _header = header;
            _content = content;
            _isEditable = isEditable;

            IsVisibleInStatusbar = true;
            IsVisibleInSidebar = true;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        object _content;
        string _header;
        Func<object, bool> _valueChanged;
        private bool _isEditable;

        #endregion

        #region Public Properties

        public bool IsHeader { get; set; }
        public bool IsEditable { get { return _isEditable; } set { _isEditable = value; NotifyOfPropertyChange(() => IsEditable); } }
        public DisplayType DisplayType { get; set; }
        public bool IsVisibleInStatusbar { get; set; }
        public bool IsVisibleInSidebar { get; set; }
        public string HeaderText { get { return _header; } set { _header = value; NotifyOfPropertyChange(() => HeaderText); } }
        public object Content
        {
            get { return _content; }
            set
            {
                if (_valueChanged(value))
                    _content = value;
                NotifyOfPropertyChange(() => Content);
                
                
            }
        }

        #endregion


       
    }
}
