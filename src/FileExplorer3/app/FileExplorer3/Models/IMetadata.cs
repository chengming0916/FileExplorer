using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    public interface IMetadata : INotifyPropertyChanged
    {
        DisplayType DisplayType { get; }
        string Header { get; }
        object Content { get; }
    }
}
