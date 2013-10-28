using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.Models
{
    public class EntryModelBase : PropertyChangedBase, IEntryModel
    {
        #region Cosntructor

        public static IEntryModel DummyModel = new EntryModelBase() { Name="Dummy" };

        protected EntryModelBase()
        {            
            IsRenamable = false;
        }


        
        #endregion

        #region Methods

        public bool Equals(IEntryModel other)
        {
            return FullPath.Equals(other.FullPath);
        }

        protected void OnRenamed(string orgName, string newName)
        {

        }
        
        #endregion

        #region Data

        private string _name;

        #endregion

        #region Public Properties
        
        public bool IsDirectory { get; protected set; }
        public IEntryModel Parent { get; protected set; }
        public string Name { get { return _name; } set { string org = _name; _name = value; OnRenamed(org, _name); } }
        public string Label { get; protected set; }        
        public bool IsRenamable { get; protected set; }
        public string Description { get; protected set; }
        public string FullPath { get; protected set; }

        #endregion


        
    }
}
