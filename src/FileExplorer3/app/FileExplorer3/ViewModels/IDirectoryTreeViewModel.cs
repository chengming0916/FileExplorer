using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface IDirectoryTreeViewModel : ISupportTreeSelector<IDirectoryNodeViewModel, IEntryModel>
    {
        Task SelectAsync(IEntryModel value);
    }

    public interface IDirectoryNodeViewModel : ISupportTreeSelector<IDirectoryNodeViewModel, IEntryModel>
    {
        bool ShowCaption { get; set; }
    }

}
