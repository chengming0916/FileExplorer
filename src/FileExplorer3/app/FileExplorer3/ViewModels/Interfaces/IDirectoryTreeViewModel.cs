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
        IEntryModel[] RootModels { set; }
        IProfile[] Profiles { set; }

        bool EnableDrag { get; set; }
        bool EnableDrop { get; set; }

        Task SelectAsync(IEntryModel value);
        void ExpandRootEntryModels();
    }

    public interface IDirectoryNodeViewModel : IEntryViewModel, ISupportTreeSelector<IDirectoryNodeViewModel, IEntryModel>, IDraggable
    {
        bool ShowCaption { get; set; }        
    }

}
