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
    public class BasicParamInitalizers : IViewModelInitializer<IExplorerViewModel>
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
            viewModel.DirectoryTree.EnableDrag = _enableDrag;
            viewModel.DirectoryTree.EnableDrop = _enableDrop;
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
                    ColumnInfo.FromBindings("Type", "EntryModel.Description", "", new ValueComparer<IEntryModel>(p => p.Description), 200),
                    
                    ColumnInfo.FromBindings("Time", "EntryModel.LastUpdateTimeUtc", "", 
                        new ValueComparer<IEntryModel>(p => 
                            (p is DiskEntryModelBase) ? (p as DiskEntryModelBase).LastUpdateTimeUtc
                            : DateTime.MinValue), 200), 
        
                    ColumnInfo.FromTemplate("Size", "GridSizeTemplate", "", 
                    new ValueComparer<IEntryModel>(p => 
                        (p is DiskEntryModelBase) ? (p as DiskEntryModelBase).Size
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
                    ColumnFilter.CreateNew("Today", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year == dt.Year && e.LastUpdateTimeUtc.Month == dt.Month && e.LastUpdateTimeUtc.Day == dt.Day;
                        }),
                    ColumnFilter.CreateNew("Earlier this month", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year == dt.Year && e.LastUpdateTimeUtc.Month == dt.Month;
                        }),
                     ColumnFilter.CreateNew("Earlier this year", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year == dt.Year;
                        }), 
                    ColumnFilter.CreateNew("A long time ago", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year != dt.Year;
                        }),    
                    ColumnFilter.CreateNew("Directories", "EntryModel.Description", e => e.IsDirectory),
                    ColumnFilter.CreateNew("Files", "EntryModel.Description", e => !e.IsDirectory)
                };
        }
    }

    public class MdiWindowInitializers : IViewModelInitializer<IExplorerViewModel>
    {

        private IExplorerInitializer _initializer;
        private WPF.MDI.MdiContainer _container;
        public MdiWindowInitializers(IExplorerInitializer initializer, WPF.MDI.MdiContainer container)
        {
            _container = container;
            _initializer = initializer;
        }

        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            explorerModel.DirectoryTree.Commands.ScriptCommands.NewWindow =
              new MdiWindow.OpenInNewWindowCommand(_container, _initializer, FileExplorer.ExtensionMethods.GetCurrentDirectoryFunc);

            explorerModel.FileList.Commands.ScriptCommands.NewWindow =
                new MdiWindow.OpenInNewWindowCommand(_container, _initializer, FileExplorer.ExtensionMethods.GetFileListSelectionFunc);
        }
    }

    public class ScriptCommandsInitializers : IViewModelInitializer<IExplorerViewModel>
    {
        public static IScriptCommand TransferCommand { get; private set; }

        private IWindowManager _windowManager;
        private IEventAggregator _events;
        private IProfile[] _profiles;
        public ScriptCommandsInitializers(IWindowManager windowManager, IEventAggregator events, params IProfile[] profiles)
        {
            _windowManager = windowManager;
            _events = events;
            _profiles = profiles;
        }

        public async Task InitalizeAsync(IExplorerViewModel explorerModel)
        {
            var initilizer = AppViewModel.getInitializer(_windowManager, _events, explorerModel.RootModels.ToArray(),
                new ColumnInitializers(),
                new ScriptCommandsInitializers(_windowManager, _events),
                new ToolbarCommandsInitializers(_windowManager));


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
                        ScriptCommands.ShowProgress(_windowManager, "Delete",
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
                                ScriptCommands.ShowProgress(_windowManager, "Delete",
                                        ScriptCommands.RunInSequence(
                                            DirectoryTree.AssignSelectionToParameter(
                                                DeleteFileBasedEntryCommand.FromParameter),
                                            new HideProgress())),
                           ResultCommand.NoError);

            if (_profiles.Length > 0)
                explorerModel.DirectoryTree.Commands.ScriptCommands.Map =
                    Explorer.PickDirectory(initilizer, _profiles,
                    dir => Explorer.BroadcastRootChanged(RootChangedEvent.Created(dir)), ResultCommand.NoError);

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
               
                new FileBasedCommandProvider(), //Open, Cut, Copy, Paste etc
                new StaticCommandProvider(
                     //new CommandModel(ExplorerCommands.CloseTab) { IsEnabled = true, Header="CloseTab", IsVisibleOnToolbar = true },
                    new SeparatorCommandModel(),
                    new SelectGroupCommand( explorerModel.FileList),    
                    new ViewModeCommand( explorerModel.FileList),
                    new GoogleExportCommandModel(() => explorerModel.RootModels)
                    { IsVisibleOnMenu = true, WindowManager = _windowManager },
                    
                    new SeparatorCommandModel(),
                    new CommandModel(ExplorerCommands.NewFolder) { IsVisibleOnToolbar = true, Symbol = Convert.ToChar(0xE188) },
                    new DirectoryCommandModel(new CommandModel(ExplorerCommands.NewFolder) { Header = Strings.strFolder })
                        { IsVisibleOnMenu = true, Header = Strings.strNew, IsEnabled = true},
                    new ToggleVisibilityCommand(explorerModel.FileList.Sidebar, ExplorerCommands.TogglePreviewer)
                    //new CommandModel(ExplorerCommands.TogglePreviewer) { IsVisibleOnMenu = false, Header = "", IsHeaderAlignRight = true, Symbol = Convert.ToChar(0xE239) }
                    )
            };

            explorerModel.DirectoryTree.Commands.ToolbarCommands.ExtraCommandProviders = new[] { 
                new StaticCommandProvider(
                    new CommandModel(ExplorerCommands.NewWindow) { IsVisibleOnMenu = true },
                    new CommandModel(ExplorerCommands.OpenTab) { IsVisibleOnMenu = true },
                     //new CommandModel(ApplicationCommands.New) { IsVisibleOnMenu = true },
                    new CommandModel(ExplorerCommands.Refresh) { IsVisibleOnMenu = true },
                    new CommandModel(ApplicationCommands.Delete) { IsVisibleOnMenu = true },
                    new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnMenu = true },
              
                    new CommandModel(ExplorerCommands.Map)  { 
                        Symbol = Convert.ToChar(0xE17B), 
                        IsEnabled = true,
                        IsHeaderVisible = false, IsVisibleOnToolbar = true
                    },
                    new CommandModel(ExplorerCommands.Unmap)  { 
                        Symbol = Convert.ToChar(0xE17A),
                        IsVisibleOnMenu = true,
                        IsVisibleOnToolbar = true
                    }
                    )
              };
        }
    }
}


