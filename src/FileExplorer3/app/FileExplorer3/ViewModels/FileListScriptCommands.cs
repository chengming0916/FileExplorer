using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels
{
    public class IfFileList : IfScriptCommand
    {
        public IfFileList(Func<IFileListViewModel, bool> condition, IScriptCommand ifTrueCommand,
            IScriptCommand otherwiseCommand)
            : base(pd =>
                {
                    if (!pd.ContainsKey("FileList"))
                        return false;
                    IFileListViewModel flvm = pd["FileList"] as IFileListViewModel;
                    return condition(flvm);
                }, ifTrueCommand, otherwiseCommand)
        {

        }

    }

    public class IfFileListSelection : IfFileList
    {
        public IfFileListSelection(Func<IList<IEntryViewModel>, bool> condition, IScriptCommand ifTrueCommand,
            IScriptCommand otherwiseCommand)
            : base(flvm => condition(flvm.Selection.SelectedItems), ifTrueCommand, otherwiseCommand)
        {

        }
    }

    

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
            IEventAggregator events = pm["Events"] as IEventAggregator;

            var newDirectory = flvm.Selection.SelectedItems[0].EntryModel;
            events.Publish(new DirectoryChangedEvent(flvm,
                   newDirectory, flvm.CurrentDirectory));            

            return ResultCommand.OK;
        }
    }
}
