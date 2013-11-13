using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public interface IEntryViewModel : INotifyPropertyChangedEx
    {
        IEntryModel EntryModel { get; }

        bool IsEditable { get; set; }
        bool IsEditing { get; set; }
        bool IsSelected { get; set; }

        Lazy<ImageSource> Icon { get; }

        IEntryViewModel Clone();
    }
}
