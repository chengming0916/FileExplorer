using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;
using FileExplorer.BaseControls;
using System.Windows.Input;
using FileExplorer.Defines;
using vm = FileExplorer.ViewModels;

namespace FileExplorer.ViewModels
{
    public class FileListCommandManager : CommandManagerBase, IExportCommandBindings
    {
        #region Constructor

        public FileListCommandManager(IFileListViewModel flvm, IEventAggregator events,
            params IExportCommandBindings[] additionalBindingExportSource)
        {
            _flvm = flvm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("FileList", flvm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();
            ScriptCommands.Open = FileList.IfSelection(evm => evm.Count() == 1,
                   FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                       FileList.OpenSelectedDirectory,  //Selected directory
                       ResultCommand.NoError),   //Selected non-directory
                   ResultCommand.NoError //Selected more than one item.                   
                   );

            ScriptCommands.Delete = NullScriptCommand.Instance;

            ScriptCommands.NewFolder = NullScriptCommand.Instance;

            ScriptCommands.Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                pd.AsVMParameterDic().FileList.ProcessedEntries.EntriesHelper.LoadAsync(true);
                return ResultCommand.OK;
            });

            ScriptCommands.ToggleRename = FileList.IfSelection(evm => evm.Count() == 1 && evm[0].IsRenamable,
                FileList.ToggleRename, NullScriptCommand.Instance);

            ScriptCommands.Copy =
                 FileList.IfSelection(evm => evm.Count() >= 1,
                    vm.ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Copy)),
                    NullScriptCommand.Instance);

            ScriptCommands.Cut =
                 FileList.IfSelection(evm => evm.Count() >= 1,
                    vm.ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Cut)),
                    NullScriptCommand.Instance);

            ScriptCommands.Paste = vm.ScriptCommands.RunInSequence(
                FileList.AssignCurrentDirectoryToDestination(
                    FileList.AssignSelectionToParameter(ClipboardCommands.Paste(ExtensionMethods.GetFileListCurrentDirectoryFunc,
                    (dragDropEffects, src, dest) => new SimpleScriptCommand("Paste", (pm) =>
                        {
                            dest.Profile.DragDrop.OnDropCompleted(src.ToList(), null, dest, dragDropEffects);
                            return ResultCommand.NoError;
                        })))
                    )
            );

            ScriptCommands.NewWindow = NullScriptCommand.Instance;

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.ScriptCommands.Open, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewFolder, this, (ch) => ch.ScriptCommands.NewFolder, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Refresh, this, (ch) => ch.ScriptCommands.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.ScriptCommands.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.ScriptCommands.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Cut, this, (ch) => ch.ScriptCommands.Cut, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Copy, this, (ch) => ch.ScriptCommands.Copy, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Paste, this, (ch) => ch.ScriptCommands.Paste, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewWindow, this, (ch) => ch.ScriptCommands.NewWindow, ParameterDicConverter, ScriptBindingScope.Local),
                new ScriptCommandBinding(ExplorerCommands.ToggleCheckBox, p => true, p => ToggleCheckBox(), ParameterDicConverter, ScriptBindingScope.Explorer),
                new ScriptCommandBinding(ExplorerCommands.ToggleViewMode, p => true, p => ToggleViewMode(), ParameterDicConverter, ScriptBindingScope.Explorer)
                ));

            _exportBindingSource = exportBindingSource.ToArray();

            IEntryModel _currentDirectoryModel = null;
            ToolbarCommands = new ToolbarCommandsHelper(events,
                message => { _currentDirectoryModel = message.NewModel; return new IEntryModel[] { _currentDirectoryModel }; },
                message => message.SelectedModels.Count() == 0 ? new IEntryModel[] { _currentDirectoryModel } : message.SelectedModels.ToArray())
                {
                    ExtraCommandProviders = new[] { 
                        new StaticCommandProvider(new SelectGroupCommand(flvm), new ViewModeCommand(flvm), new SeparatorCommandModel()) 
                    }
                };
        }

        #endregion

        #region Methods

        public void ToggleViewMode()
        {
            var viewModeWoSeparator = ViewModeCommand.ViewModes.Where(vm => vm.IndexOf(",-1") == -1).ToArray();

            int curIdx = ViewModeCommand.findViewMode(viewModeWoSeparator, _flvm.ItemSize);
            int nextIdx = curIdx + 1;
            if (nextIdx >= viewModeWoSeparator.Count()) nextIdx = 0;

            string viewMode; int step; int itemHeight;
            ViewModeCommand.parseViewMode(viewModeWoSeparator[nextIdx], out viewMode, out step, out itemHeight);
            ViewModeCommand vmc = this.ToolbarCommands.CommandModels.AllNonBindable.First(c => c.CommandModel is ViewModeCommand)
                .CommandModel as ViewModeCommand;
            vmc.SliderValue = step;
        }

        public void ToggleCheckBox()
        {
            _flvm.IsCheckBoxVisible = !_flvm.IsCheckBoxVisible;
        }

        #endregion

        #region Data

        private IFileListViewModel _flvm;

        #endregion

        #region Public Properties


        #endregion




    }
}
