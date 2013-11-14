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

        public static IEntryModel DummyModel = new EntryModelBase(null) { Name="Dummy" };

        protected EntryModelBase(IProfile profile)
        {
            Profile = profile;
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

        public override string ToString()
        {
            return this.FullPath;
        }
        
        #endregion

        #region Data

        private string _name;
        private bool _isRenamable = false;        
        protected Lazy<IEntryModel> _parentFunc = new Lazy<IEntryModel>(() => null);

        #endregion

        #region Public Properties

        public IProfile Profile { get; protected set; }
        public bool IsDirectory { get; protected set; }
        public IEntryModel Parent { get { return _parentFunc.Value; } }
        public string Name { get { return _name; } set { string org = _name; _name = value; OnRenamed(org, _name); } }
        public string Label { get; protected set; }
        public bool IsRenamable { get { return _isRenamable; } set { _isRenamable = value; NotifyOfPropertyChange(() => IsRenamable); } }
        public string Description { get; protected set; }
        public string FullPath { get; protected set; }

        #endregion


        
    }
}
