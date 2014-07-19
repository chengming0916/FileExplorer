using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.WPF.BaseControls;
using System.Windows.Input;
using FileExplorer.Defines;
using FileExplorer.WPF.Defines;
using FileExplorer.Models;

namespace FileExplorer.WPF.ViewModels
{
    public class FileListCommandManager : CommandManagerBase, IExportCommandBindings
    {
        #region Constructor        

        public FileListCommandManager(IFileListViewModel flvm, IWindowManager windowManager, IEventAggregator events,
            params IExportCommandBindings[] additionalBindingExportSource)
        {
            _flvm = flvm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("FileList", flvm),
                 new Tuple<string, object>("Events", events));

           

            #region Set ScriptCommands

            Commands = new DynamicDictionary<IScriptCommand>();
            Commands.Open = FileList.IfSelection(evm => evm.Count() == 1,
                   FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                       FileList.OpenSelectedDirectory,  //Selected directory
                       ResultCommand.NoError),   //Selected non-directory
                   ResultCommand.NoError //Selected more than one item.                   
                   );



            Commands.Delete = FileList.IfSelection(evm => evm.Count() >= 1,
                    WPFScriptCommands.IfOkCancel(windowManager, pd => "Delete",
                        pd => String.Format("Delete {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                        WPFScriptCommands.ShowProgress(windowManager, "Delete",
                            ScriptCommands.RunCommands(RunCommands.RunMode.Queue, new HideProgress(),
                               FileList.AssignSelectionToParameter(
                                 CoreScriptCommands.DiskDeleteMultiple("Parameter"))), true), 
                     ResultCommand.NoError),
                     NullScriptCommand.Instance);

            Commands.NewFolder = NullScriptCommand.Instance;

            Commands.Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                pd.AsVMParameterDic().FileList.ProcessedEntries.EntriesHelper.LoadAsync(UpdateMode.Update, true);
                return ResultCommand.OK;
            });

            Commands.ToggleRename = FileList.IfSelection(evm => evm.Count() == 1 && evm[0].IsRenamable,
                FileList.ToggleRename, NullScriptCommand.Instance);

            Commands.Copy =
                 FileList.IfSelection(evm => evm.Count() >= 1,
                    FileExplorer.Script.ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Copy)),
                    NullScriptCommand.Instance);

            Commands.Cut =
                 FileList.IfSelection(evm => evm.Count() >= 1,
                    FileExplorer.Script.ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Cut)),
                    NullScriptCommand.Instance);

            Commands.Paste = FileExplorer.Script.ScriptCommands.RunInSequence(
                FileList.AssignCurrentDirectoryToDestination(
                    FileList.AssignSelectionToParameter(ClipboardCommands.Paste(
                    FileExplorer.Script.WPFExtensionMethods.GetFileListCurrentDirectoryFunc,
                    (dragDropEffects, src, dest) => new SimpleScriptCommand("Paste", (pm) =>
                        {
                            dest.Profile.DragDrop().OnDropCompleted(src.ToList(), null, dest, dragDropEffects);
                            return ResultCommand.NoError;
                        })))
                    )
            );

            Commands.OpenTab = NullScriptCommand.Instance;
            Commands.NewWindow = NullScriptCommand.Instance;

            Commands.ZoomIn = FileList.Zoom(ZoomMode.ZoomIn);
            Commands.ZoomOut = FileList.Zoom(ZoomMode.ZoomOut);

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.Commands.Open, ParameterDicConverter, ScriptBindingScope.Local),
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewFolder, this, (ch) => ch.Commands.NewFolder, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Refresh, this, (ch) => ch.Commands.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.Commands.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.Commands.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Cut, this, (ch) => ch.Commands.Cut, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Copy, this, (ch) => ch.Commands.Copy, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Paste, this, (ch) => ch.Commands.Paste, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.NewWindow, this, (ch) => ch.Commands.NewWindow, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.OpenTab, this, (ch) => ch.Commands.OpenTab, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.IncreaseZoom, this, (ch) => ch.Commands.ZoomIn, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.DecreaseZoom, this, (ch) => ch.Commands.ZoomOut, ParameterDicConverter, ScriptBindingScope.Local),
                new ScriptCommandBinding(ExplorerCommands.ToggleCheckBox, p => true, p => ToggleCheckBox(), ParameterDicConverter, ScriptBindingScope.Explorer),
                new ScriptCommandBinding(ExplorerCommands.ToggleViewMode, p => true, p => ToggleViewMode(), ParameterDicConverter, ScriptBindingScope.Explorer)
                ));

            _exportBindingSource = exportBindingSource.ToArray();

            IEntryModel _currentDirectoryModel = null;
            ToolbarCommands = new ToolbarCommandsHelper(events, ParameterDicConverter,
                message => { _currentDirectoryModel = message.NewModel; return new IEntryModel[] { _currentDirectoryModel }; },
                message => message.SelectedModels.Count() == 0 && _currentDirectoryModel != null ? new IEntryModel[] { _currentDirectoryModel } : message.SelectedModels.ToArray())
                {
                    ExtraCommandProviders = new[] { 
                        new FileBasedCommandProvider(), //Open, Cut, Copy, Paste etc    
                        new StaticCommandProvider(new SelectGroupCommand(flvm), 
                            new ViewModeCommand(flvm),                             
                            new SeparatorCommandModel(),
                            new CommandModel(ExplorerCommands.NewFolder) { IsVisibleOnToolbar = true, 
                                HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE188)                        
                            },
                            new DirectoryCommandModel(
                                new CommandModel(ExplorerCommands.NewFolder) { Header = Strings.strFolder, IsVisibleOnMenu = true }) 
                                { IsVisibleOnMenu = true, Header = Strings.strNew, IsEnabled = true},
                            new ToggleVisibilityCommand(flvm.Sidebar, ExplorerCommands.TogglePreviewer)  
                            ) 
                    }
                };
        }

        #endregion

        #region Methods        

        public void ToggleViewMode()
        {            
            var viewModeWoSeparator = ViewModeCommand.ViewModes.Where(vm => vm.IndexOf(",-1") == -1).ToArray();

            int curIdx = ViewModeCommand.findViewMode(viewModeWoSeparator, _flvm.Parameters.ItemSize);
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
