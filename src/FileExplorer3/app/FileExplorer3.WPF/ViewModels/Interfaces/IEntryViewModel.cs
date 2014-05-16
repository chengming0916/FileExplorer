using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
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
