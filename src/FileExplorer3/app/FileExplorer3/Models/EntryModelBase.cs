using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;

namespace FileExplorer.Models
{
    public class EntryModelBase : PropertyChangedBase, IEntryModel
    {
        #region Cosntructor

        public static IEntryModel DummyModel = new EntryModelBase(null) { Name="Dummy" };

        protected EntryModelBase(IProfile profile)
        {
            Profile = profile;
            _isRenamable = false;
            _parentFunc = () => AsyncUtils.RunSync(() => Profile.ParseAsync(Profile.Path.GetDirectoryName(FullPath)));
            CreationTimeUtc = DateTime.MinValue;
            LastUpdateTimeUtc = DateTime.MinValue;
        }


        
        #endregion

        #region Methods

        public bool Equals(IEntryModel other)
        {
            if (other == null)
                return false;
            return FullPath.Equals(other.FullPath);
        }

        protected virtual void OnRenamed(string orgName, string newName)
        {

        }

        public override string ToString()
        {
            return this.FullPath;
        }
        
        #endregion

        #region Data

        protected string _name;
        protected bool _isRenamable = false;
        private IEntryModel _parent = null;
        protected Func<IEntryModel> _parentFunc;

        #endregion

        #region Public Properties

        public IProfile Profile { get; protected set; }
        public bool IsDirectory { get; protected set; }
        public IEntryModel Parent { get { return _parent != null ? _parent  : _parent = _parentFunc(); } }
        public string Name { get { return _name; } set { string org = _name; _name = value;
            if (org != null && !org.Equals(Name)) OnRenamed(org, _name); } }
        public string Label { get; protected set; }
        public bool IsRenamable { get { return _isRenamable; } set { _isRenamable = value; NotifyOfPropertyChange(() => IsRenamable); } }
        public string Description { get; protected set; }
        public string FullPath { get; protected set; }

        public DateTime CreationTimeUtc { get; protected set; }
        public DateTime LastUpdateTimeUtc { get; protected set; }

        #endregion


        
    }

    public class DiskEntryModelBase : EntryModelBase
    {
        public DiskEntryModelBase(IProfile profile)
            : base(profile)
        {
            DiskProfile = profile as IDiskProfile;
            if (DiskProfile == null)
                throw new ArgumentException();
        }

        protected override void OnRenamed(string orgName, string newName)
        {            
            //string dirName = this.Profile.Path.GetDirectoryName(this.FullPath);
            new ScriptRunner().RunAsync(new ParameterDic(), this.Rename(newName));            
            //Then DiskIO raise NotifyChange, and refresh the ExplorerViewModel.
        }

        public IDiskProfile DiskProfile { get; private set; }
        public long Size { get; protected set; }

    }
}
