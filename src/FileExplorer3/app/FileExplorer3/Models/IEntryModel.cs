using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.Models
{
    public interface IEntryModel : INotifyPropertyChangedEx, IEquatable<IEntryModel>
    {
        bool IsDirectory { get;  }
        IEntryModel Parent { get; }
        string Label { get; }        
        string FullPath { get; }
    }
}
