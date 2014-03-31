using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public interface IToggleableVisibility : INotifyPropertyChanged
    {
        bool IsVisible { get; set; }
    }

    public interface IPreviewerViewModel :  
        IToggleableVisibility, ISupportCommandManager
    {
       
    }
}
