using FileExplorer.Defines;
using FileExplorer.IO;
using FileExplorer.Models;
using FileExplorer.WPF;
using FileExplorer.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static class UIInitializeHelpers
    {



        #region FileList_ColumnInfo_For_DiskBased_Items

        public static ColumnInfo[] FileList_ColumList = new ColumnInfo[] 
                {
                    ColumnInfo.FromTemplate("Name", "GridLabelTemplate", "EntryModel.Label", new ValueComparer<IEntryModel>(p => p.Label), 200),   
                    ColumnInfo.FromBindings("Type", "EntryModel.Description", "", new ValueComparer<IEntryModel>(p => p.Description), 200),                                       
                               ColumnInfo.FromBindings("Time", "EntryModel.LastUpdateTimeUtc", "", 
                        new ValueComparer<IEntryModel>(p => p.LastUpdateTimeUtc), 200), 
                };
        #endregion

        #region FileList_ColumnFilter_For_DiskBased_Items
        public static ColumnFilter[] FileList_ColumnFilter = new ColumnFilter[]
                {
                    ColumnFilter.CreateNew<IEntryModel>("0 - 9", "EntryModel.Label", e => Regex.Match(e.Label, "^[0-9]").Success),
                    ColumnFilter.CreateNew<IEntryModel>("A - H", "EntryModel.Label", e => Regex.Match(e.Label, "^[A-Ha-h]").Success),
                    ColumnFilter.CreateNew<IEntryModel>("I - P", "EntryModel.Label", e => Regex.Match(e.Label, "^[I-Pi-i]").Success),
                    ColumnFilter.CreateNew<IEntryModel>("Q - Z", "EntryModel.Label", e => Regex.Match(e.Label, "^[Q-Zq-z]").Success),
                    ColumnFilter.CreateNew<IEntryModel>("The rest", "EntryModel.Label", e => Regex.Match(e.Label, "^[^A-Za-z0-9]").Success),
                    ColumnFilter.CreateNew<IEntryModel>("Today", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year == dt.Year && 
                                e.LastUpdateTimeUtc.Month == dt.Month && 
                                e.LastUpdateTimeUtc.Day == dt.Day;
                        }),
                    ColumnFilter.CreateNew<IEntryModel>("Earlier this month", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year == dt.Year && e.LastUpdateTimeUtc.Month == dt.Month;
                        }),
                     ColumnFilter.CreateNew<IEntryModel>("Earlier this year", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year == dt.Year;
                        }), 
                    ColumnFilter.CreateNew<IEntryModel>("A long time ago", "EntryModel.LastUpdateTimeUtc", e => 
                        {
                            DateTime dt = DateTime.UtcNow;
                            return e.LastUpdateTimeUtc.Year != dt.Year;
                        }),    
                    ColumnFilter.CreateNew<IEntryModel>("Directories", "EntryModel.Description", e => e.IsDirectory),
                    ColumnFilter.CreateNew<IEntryModel>("Files", "EntryModel.Description", e => !e.IsDirectory)
                };
        #endregion

        public static IScriptCommand FileList_Open =
             UIScriptCommands.FileListAssignSelection("{Selection}",                        //Assign Selection
               ScriptCommands.IfArrayLength(ComparsionOperator.Equals, "{Selection}", 1,    //If Selection.Length = 1
                 ScriptCommands.AssignArrayItem("{Selection}", 0, "{FirstSelected}",        //FirstSelected = Selection[0]
                   ScriptCommands.IfAssigned("{FirstSelected[0].LinkPath}", 
                     CoreScriptCommands.ParsePath("{Profiles}", "{FirstSelected.LinkPath}", "{Link-Entry}",
                       ScriptCommands.IfAssigned("{Link-Entry}", 
                         ScriptCommands.IfTrue("{Link-Entry.IsDirectory}", 
                            UIScriptCommands.ExplorerGoTo("{Explorer}", "{Link-Entry}"),
                            ResultCommand.NoError))), //Non-Directory, do nothing.
                     ScriptCommands.IfPropertyIsTrue("{FirstSelected}", "IsDirectory",        //FirstSelected.IsDirectory?                   
                        UIScriptCommands.NotifyDirectoryChanged("{Selection}"),             //True -> Broadcast ChangeDirectory using {GlobalEvents}
                        ResultCommand.NoError                         //False -> Do nothing
                        )
               ))));




        public static IScriptCommand FileList_Delete = ResultCommand.NoError; //Not implemented
                //UIScriptCommands.FileListAssignSelection("{Selection}",                     //Assign Selection
                //ScriptCommands.AssignProperty("{Selection}", "Length", "{Selection-Length}",  //Assign Selection Length
                //ScriptCommands.IfValue<int>(ComparsionOperator.GreaterThanOrEqual, "{Selection-Length}", 1, //If Selection Length >= 1
                //  ScriptCommands.AssignArrayItem("{Selection}", 0, "{FirstSelected}",  //True, FirstSelected = Selection[0]
                //  UIScriptCommands.MessageBoxYesNo("FileExplorer", "Delete {FirstSelected} and {Selection-Length} Item(s)?", //Yes, ShowMessageBox   
                //  CoreScriptCommands.DiskDeleteMultiple("{Selection}", true))))));   //User clicked yes, Call Delete.

        public static IScriptCommand FileList_NewFolder = ResultCommand.NoError; //Not implemented
            //UIScriptCommands.ExplorerAssignCurrentDirectory("{FileList}", "{CurrentDirectory}",
            //    CoreScriptCommands.DiskCreateFolder("{CurrentDirectory.Profile}", "{CurrentDirectory.FullPath}\\NewFolder",
            //        "{NewFolder}", NameGenerationMode.Rename,
            //        UIScriptCommands.FileListRefreshThenSelect("{FileList}", "{NewFolder}", true, ResultCommand.OK)));

        public static IScriptCommand FileList_Selection_Is_One_Folder =
          UIScriptCommands.FileListAssignSelection("{Selection}",                     //Assign Selection
           ScriptCommands.IfArrayLength(ComparsionOperator.Equals, "{Selection}", 1,
             ScriptCommands.IfTrue("{Selection[0].IsDirectory}", ResultCommand.OK)));

        #region FileList_NewWindow, NewTabbedWindow, OpenTab
        public static IScriptCommand FileList_NewWindow =
         ScriptCommands.AssignCanExecuteCondition(FileList_Selection_Is_One_Folder,
          UIScriptCommands.ExplorerGetParameter("{Explorer}", ExplorerParameterType.RootModels, "{RootDirectories}",
           UIScriptCommands.ExplorerNewWindow("{OnModelCreated}", "{OnViewAttached}",
               "{WindowManager}", "{GlobalEvents}", "{Explorer}",
                   UIScriptCommands.ExplorerGoTo("{Explorer}", "{Selection[0]}"))));

        public static IScriptCommand FileList_NewTabbedWindow =
             ScriptCommands.AssignCanExecuteCondition(FileList_Selection_Is_One_Folder,
                   UIScriptCommands.ExplorerGetParameter("{Explorer}", ExplorerParameterType.RootModels, "{RootDirectories}",
                    UIScriptCommands.ExplorerNewTabWindow("{OnModelCreated}", "{OnViewAttached}", "{OnTabExplorerCreated}", "{OnTabExplorerAttached}",
                        "{WindowManager}", "{GlobalEvents}", "{TabbedExplorer}",
                        UIScriptCommands.TabExplorerNewTab("{TabbedExplorer}", "{Selection[0]}", "{Explorer}", null))));


        public static IScriptCommand FileList_OpenTab =
            ScriptCommands.AssignCanExecuteCondition(FileList_Selection_Is_One_Folder,
                   UIScriptCommands.ExplorerGetParameter("{Explorer}", ExplorerParameterType.RootModels, "{RootDirectories}",
                   UIScriptCommands.TabExplorerNewTab("{TabbedExplorer}", "{Selection[0]}", "{Explorer}", null)));
        #endregion

        #region DirectoryTree_NewWindow, NewTabbedWindow, OpenTab
        public static IScriptCommand DirectoryTree_NewWindow =
         UIScriptCommands.ExplorerAssignCurrentDirectory("{CurrentDirectory}", 
          UIScriptCommands.ExplorerGetParameter("{Explorer}", ExplorerParameterType.RootModels, "{RootDirectories}",
           UIScriptCommands.ExplorerNewWindow("{OnModelCreated}", "{OnViewAttached}",
               "{WindowManager}", "{GlobalEvents}", "{Explorer}",
                   UIScriptCommands.ExplorerGoTo("{Explorer}", "{CurrentDirectory}"))));

        public static IScriptCommand DirectoryTree_NewTabbedWindow =
             UIScriptCommands.ExplorerAssignCurrentDirectory("{CurrentDirectory}", 
                   UIScriptCommands.ExplorerGetParameter("{Explorer}", ExplorerParameterType.RootModels, "{RootDirectories}",
                    UIScriptCommands.ExplorerNewTabWindow("{OnModelCreated}", "{OnViewAttached}", "{OnTabExplorerCreated}", "{OnTabExplorerAttached}",
                        "{WindowManager}", "{GlobalEvents}", "{TabbedExplorer}",
                        UIScriptCommands.TabExplorerNewTab("{TabbedExplorer}", "{CurrentDirectory}", "{Explorer}", null))));


        public static IScriptCommand DirectoryTree_OpenTab =
            UIScriptCommands.ExplorerAssignCurrentDirectory("{CurrentDirectory}", 
                   UIScriptCommands.ExplorerGetParameter("{Explorer}", ExplorerParameterType.RootModels, "{RootDirectories}",
                   UIScriptCommands.TabExplorerNewTab("{TabbedExplorer}", "{CurrentDirectory}", "{Explorer}", null)));
        #endregion

        #region Map/Unmap
        public static IScriptCommand DirectoryTree_Map_From_Profiles =
            UIScriptCommands.ProfilePicker("{Profiles}", "{Profile}", "{WindowManager}",
                   CoreScriptCommands.ParsePath("{Profile}", "", "{RootDirectories}",
                   ScriptCommands.AssignProperty("{RootDirectories}", "FullPath", "{StartupPath}",
                    UIScriptCommands.ExplorerPick(ExplorerMode.DirectoryOpen, "{OnModelCreated}", "{OnViewAttached}",
                     "{WindowManager}", null, "{Selection}", "{SelectionPath}",
                      UIScriptCommands.NotifyRootCreated("{Selection}",
                        UIScriptCommands.ExplorerGoTo("{Explorer}", "{Selection}"))))));

        //To-Do: Update to new ScriptCommand.
        public static IScriptCommand DirectoryTree_Unmap =
            Explorer.DoSelection(ems =>
                Script.ScriptCommands.If(pd => (ems.First() as FileExplorer.WPF.ViewModels.IDirectoryNodeViewModel).Selection.IsFirstLevelSelector(),
                        Script.WPFScriptCommands.IfOkCancel(new Caliburn.Micro.WindowManager(), pd => "Unmap",
                            pd => String.Format("Unmap {0}?", ems.First().EntryModel.Label),
                            Explorer.BroadcastRootChanged(FileExplorer.WPF.Defines.RootChangedEvent.Deleted(ems.Select(em => em.EntryModel).ToArray())),
                            ResultCommand.OK),
                        Script.ScriptCommands.NoCommand), Script.ScriptCommands.NoCommand);

        #endregion
        
        /// <summary>
        /// <para>Call ExplorerDefault(), ExplorerDefaultToolbarCommands() 
        /// and assign the followings parameter to CommandManager: (for use when OpenInNewWindow)
        /// OnViewAttached, OnModelCreated, EnableDrag/Drop/MultiSelect.</para>
        /// 
        /// <para>ExplorerDefault() allow you to change these parameters (in additional to above):
        /// RootDirectories, ColumnList, ColumnFilters, FilterString, ViewMode, ItemSize, ShowToolbar/Sidebar/GridHeader.</para>
        ///
        /// <para>ExplorerDefaultToolbarCommands() assign the CommandModels in Toolbar and ContextMenu, they are not
        /// Customizable currently.  You can only disable the command.</para>
        ///
        /// </summary>
        public static IScriptCommand Explorer_Initialize_Default =
            ScriptCommands.RunSequence(null,
                   UIScriptCommands.ExplorerDefault(),
                   UIScriptCommands.ExplorerDefaultToolbarCommands(),
                   UIScriptCommands.ExplorerAssignScriptParameters("{Explorer}",
                        "{RootDirectories},{Profiles},{GlobalEvents},{OnViewAttached},{OnModelCreated},{EnableDrag},{EnableDrop},{EnableMultiSelect},{EnableTabsWhenOneTab}")
                   );

        public static IScriptCommand TabbedExplorer_Initialize_Default =
          ScriptCommands.Assign("{FileListNewWindowCommand}", UIInitializeHelpers.FileList_NewTabbedWindow, false,
          ScriptCommands.Assign("{FileListOpenTabCommand}", UIInitializeHelpers.FileList_OpenTab, false,
          ScriptCommands.Assign("{DirectoryTreeNewWindowCommand}", UIInitializeHelpers.DirectoryTree_NewWindow, false,
          ScriptCommands.Assign("{DirectoryTreeOpenTabCommand}", UIInitializeHelpers.DirectoryTree_OpenTab, false,
          Explorer_Initialize_Default))));

    }
}
