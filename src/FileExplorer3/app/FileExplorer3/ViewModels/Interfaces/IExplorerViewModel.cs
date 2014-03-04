﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;
using Caliburn.Micro;

namespace FileExplorer.ViewModels
{
    public interface IExplorerViewModel  : ISupportCommandManager
    {
        IEntryModel[] RootModels { get; set; }

        IDirectoryTreeViewModel DirectoryTree { get; }
        IFileListViewModel FileList { get; }
        IStatusbarViewModel Statusbar { get; }

        Task GoAsync(string gotoPath);
        Task GoAsync(IEntryModel entryModel);
    }
}
