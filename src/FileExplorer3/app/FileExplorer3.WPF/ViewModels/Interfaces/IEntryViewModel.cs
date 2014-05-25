using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.Models;

namespace FileExplorer.WPF.ViewModels
{
    public interface IEntryViewModel : INotifyPropertyChangedEx, IDraggable, ISelectable
    {
        IEntryModel EntryModel { get; }

        bool IsRenamable { get; set; }
        bool IsRenaming { get; set; }        

        ImageSource Icon { get; }

        IEntryViewModel Clone();
    }
}
