using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public interface IEntryViewModel : INotifyPropertyChangedEx
    {
        IProfile Profile { get; }
        IEntryModel EntryModel { get; }

        bool IsEditable { get; set; }
        bool IsEditing { get; set; }
        bool IsSelected { get; set; }
    }
}
