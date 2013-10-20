using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public interface IEntryListViewModel
    {        
        IObservableCollection<IEntryViewModel> Items { get; }

    }
}
