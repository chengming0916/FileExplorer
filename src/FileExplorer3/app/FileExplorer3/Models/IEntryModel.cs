using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FileExplorer.Models
{
    public interface IEntryModel : INotifyPropertyChanged, IEquatable<IEntryModel>
    {
        IProfile Profile { get; }

        bool IsDirectory { get;  }
        IEntryModel Parent { get; }
        string Label { get; }
        string Name { get; }
        string Description { get; }
        string FullPath { get; }
        bool IsRenamable { get; }

        DateTime CreationTimeUtc { get; }
        DateTime LastUpdateTimeUtc { get; }
    }
}
