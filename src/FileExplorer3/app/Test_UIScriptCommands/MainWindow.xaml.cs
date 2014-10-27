using Caliburn.Micro;
using FileExplorer;
using FileExplorer.Models;
using FileExplorer.Script;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test_UIScriptCommands
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            IEventAggregator _events = new EventAggregator();
            IWindowManager _windowManager = new AppWindowManager();
            IProfile _exProfile = new FileSystemInfoExProfile(_events, _windowManager);
            IProfile[] _profiles = new IProfile[] { _exProfile };
            IEntryModel[] _rootDirs = new IEntryModel[] { AsyncUtils.RunSync(() => _exProfile.ParseAsync("")) };

            explorer.WindowManager = _windowManager;
            explorer.ViewModel.Initializer =
                new ScriptCommandInitializer()
                {
                    OnModelCreated = ScriptCommands.Run("{OnModelCreated}"),
                    OnViewAttached = ScriptCommands.Run("{OnViewAttached}"),
                    RootModels = _rootDirs,
                    WindowManager = _windowManager,
                    StartupParameters = new ParameterDic()
                    {
                         { "Profiles", _profiles },
                         { "RootDirectories", _rootDirs },	 
                         { "GlobalEvents", _events },
                         { "WindowManager", _windowManager },
                         { "StartupPath", "" },                         
                         { "ViewMode", "List" }, 
                         { "ItemSize", 16 },
                         { "EnableDrag", true }, 
                         { "EnableDrop", true }, 
                         { "FileListNewWindowCommand", NullScriptCommand.Instance }, //Disable NewWindow Command.
                         { "EnableMultiSelect", true},
                         { "ShowToolbar", true }, 
                         { "ShowGridHeader", true }, 
                         { "OnModelCreated", IOInitializeHelpers.Explorer_Initialize_Default }, 
                         { "OnViewAttached", UIScriptCommands.ExplorerGotoStartupPathOrFirstRoot() }
                    }
                };
            
        }

        private void execute_Click(object sender, RoutedEventArgs e)
        {
            IScriptCommand cmd = null;
            ParameterDic pd1 = new ParameterDic();  
            var selectedItem = cbCommand.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
                switch (selectedItem.Name)
            {
                case "goto":
                   cmd =
                        CoreScriptCommands.ParsePath("{Profiles}", tbDirectory.Text, "{Directory}",
                            UIScriptCommands.ExplorerGoTo("{Directory}"));
                    break;
                case "expand":
                    cmd = 
                       CoreScriptCommands.ParsePath("{Profiles}", tbDirectory.Text, "{Directory}",
                           UIScriptCommands.DirectoryTreeToggleNode("{DirectoryTree}", "{Directory}", DirectoryTreeToggleMode.Expand));
                    break;

                case "collapse":
                    cmd = 
                    CoreScriptCommands.ParsePath("{Profiles}", tbDirectory.Text, "{Directory}",
                           UIScriptCommands.DirectoryTreeToggleNode("{DirectoryTree}", "{Directory}", DirectoryTreeToggleMode.Collapse));
                    break;
                case "assignCurrent":

                    cmd = UIScriptCommands.ExplorerAssignCurrentDirectory("{CurrentDirectory}",                        
                        ScriptCommands.SetProperty("{tbDirectory}", (TextBlock tb) => tb.Text, "{CurrentDirectory.FullPath}"));
                    pd1.SetValue("{tbDirectory}", tbDirectory);                                   
                    break;
                case "assignScriptParam":
                    explorer.ViewModel.Commands.Execute(
                        ScriptCommands.Assign("{LastEdit}", DateTime.Now, false,
                          ScriptCommands.Assign("{Today}", DateTime.Now.DayOfWeek, false,
                          UIScriptCommands.ExplorerAssignScriptParameters("{Explorer}", "{LastEdit},{Today}"))));
                    cmd = UIScriptCommands.MessageBoxOK("ExplorerAssignScriptParameters", "LastEdit = {LastEdit}, Today = {Today}");
                    break;
                    case "setParameters":
                        cmd =  //Warning, Parameter.Width/Height is binded in AppWindowManager.                            
                            UIScriptCommands.ExplorerGetParameter(ExplorerParameterType.ExplorerWidth, "{Width}", 
                            UIScriptCommands.ExplorerGetParameter(ExplorerParameterType.ExplorerHeight, "{Height}", 
                            ScriptCommands.AddValue("{Width}", -150, "{Width}",
                            ScriptCommands.AddValue("{Height}", -150, "{Height}",
                                UIScriptCommands.ExplorerNewWindow("{OnModelCreated}", "{OnViewAttached}", "{WindowManager}", "{Events}", 
                                    "{TestExplorer}", 
                                    UIScriptCommands.ExplorerSetParameter("{TestExplorer}", ExplorerParameterType.ExplorerWidth, "{Width}", 
                                    UIScriptCommands.ExplorerSetParameter("{TestExplorer}", ExplorerParameterType.ExplorerHeight, "{Height}")))))));
                              
                            ////UIScriptCommands.ExplorerGetParameter(ExplorerParameterType.ColumnFilters, "{Width}", 
                            ////UIScriptCommands.ExplorerGetParameter(ExplorerParameterType.ExplorerHeight, "{Height}", 
                            ////ScriptCommands.AddValue("{Width}", 50, "{Width}",
                            ////ScriptCommands.AddValue("{Height}", 50, "{Height}",
                            //ScriptCommands.Assign("{ColumnFilters}", IOInitializeHelpers.FileList_ColumList_For_DiskBased_Items, false, 
                            //ScriptCommands.Assign("{ColumnList}", IOInitializeHelpers.FileList_ColumList_For_DiskBased_Items, false, 
                            //UIScriptCommands.ExplorerSetParameter(ExplorerParameterType.ColumnFilters, "{ColumnFilters}", 
                            //UIScriptCommands.ExplorerSetParameter(ExplorerParameterType.ColumnList, "{ColumnList}"))));
                        break;
            }

            if (cmd != null)
                 explorer.ViewModel.Commands.ExecuteAsync(cmd, pd1);
        }
    }
}
