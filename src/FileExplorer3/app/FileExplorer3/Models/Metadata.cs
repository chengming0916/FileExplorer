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

        public Metadata(DisplayType displayType, string header, object content)
        {
            DisplayType = displayType;
            Header = header;
            Content = content;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        object _content;
        string _header;

        #endregion

        #region Public Properties

        public DisplayType DisplayType { get; set; }
        public string Header { get { return _header; } set { _header = value; NotifyOfPropertyChange(() => Header); } }
        public object Content { get { return _content; } set { _content = value; NotifyOfPropertyChange(() => Content); } }

        #endregion
    }
}
