using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestApp
{
    public class BasicParamInitalizers :  IViewModelInitializer<IExplorerViewModel>
    {
        private bool _expand;
        private bool _multiSelect;
        private bool _enableDrag;
        private bool _enableDrop;
        public BasicParamInitalizers(bool expand, bool multiSelect, bool enableDrag, bool enableDrop)
        {
            _expand = expand;
            _multiSelect = multiSelect;
            _enableDrag = enableDrag;
            _enableDrop = enableDrop;
        }

        public async Task InitalizeAsync(IExplorerViewModel viewModel)
        {
            viewModel.FileList.EnableDrag = _enableDrag;
            viewModel.FileList.EnableDrop = _enableDrop;
            viewModel.FileList.EnableMultiSelect = _multiSelect;
            if (_expand)
                viewModel.DirectoryTree.ExpandRootEntryModels();
        }
    }

    public class ColumnInitializers : IViewModelInitializer<IExplorerViewModel>
    {
        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            explorerModel.FileList.Columns.ColumnList = new ColumnInfo[] 
                {
                    ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", new ValueComparer<IEntryModel>(p => p.Label), 200),   
                    ColumnInfo.FromBindings("Description", "EntryModel.Description", "", new ValueComparer<IEntryModel>(p => p.Description), 200),
                    ColumnInfo.FromTemplate("FSI.Size", "GridSizeTemplate", "", 
                    new ValueComparer<IEntryModel>(p => 
                        (p is FileSystemInfoModel) ? (p as FileSystemInfoExModel).Size
                        : 0), 200),  
                    ColumnInfo.FromBindings("FSI.Attributes", "EntryModel.Attributes", "", 
                        new ValueComparer<IEntryModel>(p => 
                            (p is FileSystemInfoModel) ? (p as FileSystemInfoModel).Attributes
                            : System.IO.FileAttributes.Normal), 200)   
                };

            explorerModel.FileList.Columns.ColumnFilters = new ColumnFilter[]
                {
                    ColumnFilter.CreateNew("0 - 9", "EntryModel.Label", e => Regex.Match(e.Label, "^[0-9]").Success),
                    ColumnFilter.CreateNew("A - H", "EntryModel.Label", e => Regex.Match(e.Label, "^[A-Ha-h]").Success),
                    ColumnFilter.CreateNew("I - P", "EntryModel.Label", e => Regex.Match(e.Label, "^[I-Pi-i]").Success),
                    ColumnFilter.CreateNew("Q - Z", "EntryModel.Label", e => Regex.Match(e.Label, "^[Q-Zq-z]").Success),
                    ColumnFilter.CreateNew("The rest", "EntryModel.Label", e => Regex.Match(e.Label, "^[^A-Za-z0-9]").Success),
                    ColumnFilter.CreateNew("Directories", "EntryModel.Description", e => e.IsDirectory),
                    ColumnFilter.CreateNew("Files", "EntryModel.Description", e => !e.IsDirectory)
                };
        }
    }

    public class ScriptCommandsInitializers : IViewModelInitializer<IExplorerViewModel>
    {
        public static IScriptCommand TransferCommand { get; private set; }

        private IWindowManager _windowManager;
        private IEventAggregator _events;
        public ScriptCommandsInitializers(IWindowManager windowManager, IEventAggregator events)
        {
            _windowManager = windowManager;
            _events = events;
        }

        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            var initilizer = AppViewModel.getInitializer(_windowManager, _events, explorerModel.RootModels.ToArray(),
                new ColumnInitializers(),
                new ScriptCommandsInitializers(_windowManager, _events),
                new ToolbarCommandsInitializers(_windowManager));

            explorerModel.DirectoryTree.Commands.ScriptCommands.NewWindow =
                Explorer.NewWindow(initilizer, FileExplorer.ExtensionMethods.GetCurrentDirectoryFunc);

            explorerModel.FileList.Commands.ScriptCommands.NewWindow =
               Explorer.NewWindow(initilizer, FileExplorer.ExtensionMethods.GetFileListSelectionFunc);

            explorerModel.FileList.Commands.ScriptCommands.Open =
             FileList.IfSelection(evm => evm.Count() == 1,
                 FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                     FileList.OpenSelectedDirectory, //Selected directory                        
                     FileList.AssignSelectionToParameter(
                         new OpenWithScriptCommand(null))),  //Selected non-directory
                 ResultCommand.NoError //Selected more than one item, ignore.
                 );

            explorerModel.FileList.Commands.ScriptCommands.NewFolder =
                FileList.Do(flvm => ScriptCommands.CreatePath(
                        flvm.CurrentDirectory, "NewFolder", true, true,
                        m => FileList.Refresh(FileList.Select(fm => fm.Equals(m), ResultCommand.OK), true)));

            explorerModel.FileList.Commands.ScriptCommands.Delete =
                 FileList.IfSelection(evm => evm.Count() >= 1,
                    ScriptCommands.IfOkCancel(_windowManager, pd => "Delete",
                        pd => String.Format("Delete {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                        new ShowProgress(_windowManager,
                                    ScriptCommands.RunInSequence(
                                        FileList.AssignSelectionToParameter(
                                            DeleteFileBasedEntryCommand.FromParameter),
                                        new HideProgress())),
                        ResultCommand.NoError),
                    NullScriptCommand.Instance);

            explorerModel.FileList.Commands.ScriptCommands.Copy =
                 FileList.IfSelection(evm => evm.Count() >= 1,
                   ScriptCommands.IfOkCancel(_windowManager, pd => "Copy",
                        pd => String.Format("Copy {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                            ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Copy)),
                            ResultCommand.NoError),
                    NullScriptCommand.Instance);

            explorerModel.FileList.Commands.ScriptCommands.Cut =
                  FileList.IfSelection(evm => evm.Count() >= 1,
                   ScriptCommands.IfOkCancel(_windowManager, pd => "Cut",
                        pd => String.Format("Cut {0} items?", (pd["FileList"] as IFileListViewModel).Selection.SelectedItems.Count),
                            ScriptCommands.RunInSequence(FileList.AssignSelectionToParameter(ClipboardCommands.Cut)),
                            ResultCommand.NoError),
                    NullScriptCommand.Instance);

            explorerModel.DirectoryTree.Commands.ScriptCommands.Delete =
                       ScriptCommands.IfOkCancel(_windowManager, pd => "Delete",
                           pd => String.Format("Delete {0}?", ((pd["DirectoryTree"] as IDirectoryTreeViewModel).Selection.RootSelector.SelectedValue.Label)),
                                 new ShowProgress(_windowManager,
                                        ScriptCommands.RunInSequence(
                                            DirectoryTree.AssignSelectionToParameter(
                                                DeleteFileBasedEntryCommand.FromParameter),
                                            new HideProgress())),
                           ResultCommand.NoError);


            explorerModel.Commands.ScriptCommands.Transfer =
                TransferCommand =
                new TransferCommand((effect, source, destDir) =>
                    source.Profile is IDiskProfile ?
                        (IScriptCommand)new FileTransferScriptCommand(source, destDir, effect == DragDropEffects.Move)
                        : ResultCommand.Error(new NotSupportedException())
                    , _windowManager);
        }
    }

    public class ToolbarCommandsInitializers : IViewModelInitializer<IExplorerViewModel>
    {
        private IWindowManager _windowManager;

        public ToolbarCommandsInitializers(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            explorerModel.FileList.Commands.ToolbarCommands.ExtraCommandProviders = new[] { 
                new StaticCommandProvider(
                    new CommandModel(ExplorerCommands.NewWindow) { IsVisibleOnToolbar = false }
                    ),
                new FileBasedCommandProvider(), //Open, Cut, Copy, Paste etc
                new StaticCommandProvider(
                    
                    new SeparatorCommandModel(),
                    new SelectGroupCommand( explorerModel.FileList),    
                    new ViewModeCommand( explorerModel.FileList),
                    new GoogleExportCommandModel(() => explorerModel.RootModels)
                    { IsVisibleOnToolbar = false, WindowManager = _windowManager },
                    
                    new SeparatorCommandModel(),
                    new CommandModel(ExplorerCommands.NewFolder) { IsVisibleOnMenu = false, Symbol = Convert.ToChar(0xE188) },
                    new DirectoryCommandModel(new CommandModel(ExplorerCommands.NewFolder) { Header = Strings.strFolder })
                        { IsVisibleOnToolbar = false, Header = Strings.strNew, IsEnabled = true}
                    )
            };

            explorerModel.DirectoryTree.Commands.ToolbarCommands.ExtraCommandProviders = new[] { 
                new StaticCommandProvider(
                    new CommandModel(ExplorerCommands.NewWindow) { IsVisibleOnToolbar = false },
                    new CommandModel(ExplorerCommands.Refresh) { IsVisibleOnToolbar = false },
                    new CommandModel(ApplicationCommands.Delete)  { IsVisibleOnToolbar = false },
                    new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnToolbar = false }                    
                    )
              };
        }
    }
}


