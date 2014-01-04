using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.ViewModels;

namespace FileExplorer.Models
{
    public class OpenSelectedFileEx : ScriptCommandBase
    {
        public OpenSelectedFileEx()
            : base("OpenSelectedFileEx")
        {

        }

        public override bool CanExecute(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList"))
                return false;

            var selectedItems = (pm["FileList"] as IFileListViewModel).Selection.SelectedItems;
            return selectedItems.Count == 1 && !selectedItems[0].EntryModel.IsDirectory
                && selectedItems[0].EntryModel is FileSystemInfoExModel;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList") || !(pm["FileList"] is IFileListViewModel))
                return ResultCommand.Error(new KeyNotFoundException("FileList"));

            IFileListViewModel flvm = pm["FileList"] as IFileListViewModel;
            
            var selectedFile = flvm.Selection.SelectedItems[0].EntryModel;
            Process.Start((selectedFile as FileSystemInfoExModel).FullPath);

            return ResultCommand.OK;
        }
    }
}
