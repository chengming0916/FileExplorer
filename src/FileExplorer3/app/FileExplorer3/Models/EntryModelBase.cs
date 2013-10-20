using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.Models
{
    public abstract class EntryModelBase : PropertyChangedBase, IEntryModel
    {
        #region Cosntructor

        protected EntryModelBase()
        {            
        }


        
        #endregion

        #region Methods

        public bool Equals(IEntryModel other)
        {
            return FullPath.Equals(other.FullPath);
        }
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties
        
        public bool IsDirectory { get; protected set; }
        public IEntryModel Parent { get; protected set; }
        public string Label { get; protected set; }
        public string Description { get; protected set; }
        public string FullPath { get; protected set; }

        #endregion


        
    }
}
