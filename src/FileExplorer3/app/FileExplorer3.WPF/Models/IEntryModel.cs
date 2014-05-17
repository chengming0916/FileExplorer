using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.WPF.Models
{
    public interface IEntryModel : INotifyPropertyChangedEx, IEquatable<IEntryModel>
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
