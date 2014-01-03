using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core;
using Cofe.Core.Script;

namespace FileExplorer.ViewModels
{
    public class OpenSelectedDirectory : ScriptCommandBase
    {
        public OpenSelectedDirectory()
            : base("OpenSelectedDirectory")
        {
            
        }

        public override bool CanExecute(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList"))
                return false;

            var selectedItems = (pm["FileList"] as IFileListViewModel).Selection.SelectedItems;
            return selectedItems.Count == 1 && selectedItems[0].EntryModel.IsDirectory;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            if (!pm.ContainsKey("FileList") || !(pm["FileList"] is IFileListViewModel))
                return ResultCommand.Error(new KeyNotFoundException("FileList"));

            IFileListViewModel flvm = pm["FileList"] as IFileListViewModel;

            flvm.SignalChangeDirectory(flvm.Selection.SelectedItems[0].EntryModel);

            return ResultCommand.OK;
        }
    }
}
