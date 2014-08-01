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
    public static class IOInitializeHelpers
    {
        #region FileList_ColumnInfo_For_DiskBased_Items
        public static ColumnInfo[] FileList_ColumList_For_DiskBased_Items = new ColumnInfo[] 
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
        #endregion

        #region FileList_ColumnFilter_For_DiskBased_Items
        public static ColumnFilter[] FileList_ColumnFilter_For_DiskBased_Items = new ColumnFilter[]
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

        public static IScriptCommand FileList_Open_For_DiskBased_Items =
             UIScriptCommands.FileListAssignSelection("{Selection}",                        //Assign Selection
               ScriptCommands.IfArrayLength(ComparsionOperator.Equals, "{Selection}", 1,    //If Selection.Length = 1
                 ScriptCommands.AssignArrayItem("{Selection}", 0, "{FirstSelected}",        //FirstSelected = Selection[0]
                   ScriptCommands.IfPropertyIsTrue("{FirstSelected}", "IsDirectory",        //FirstSelected.IsDirectory?                   
                        UIScriptCommands.NotifyDirectoryChanged("{Selection}"),             //True -> Broadcast ChangeDirectory using {Events}
                        IOScriptCommands.DiskRun("{FirstSelected}")                         //False -> DiskRun
                        )
               )));

        public static IScriptCommand FileList_Delete_For_DiskBased_Items =
                UIScriptCommands.FileListAssignSelection("{Selection}",                     //Assign Selection
                ScriptCommands.AssignProperty("{Selection}", "Length", "{Selection-Length}",  //Assign Selection Length
                ScriptCommands.IfValue<int>(ComparsionOperator.GreaterThanOrEqual, "{Selection-Length}", 1, //If Selection Length >= 1
                  ScriptCommands.AssignArrayItem("{Selection}", 0, "{FirstSelected}",  //True, FirstSelected = Selection[0]
                  UIScriptCommands.MessageBoxYesNo("FileExplorer", "Delete {FirstSelected} and {Selection-Length} Item(s)?", //Yes, ShowMessageBox   
                  CoreScriptCommands.DiskDeleteMultiple("{Selection}", true))))));   //User clicked yes, Call Delete.
  
            //explorerModel.FileList.Commands.Commands.NewFolder =
            //     FileList.Do(flvm => WPFScriptCommands.CreatePath(
            //            flvm.CurrentDirectory, "NewFolder", true, true,
            //         //FileList.Do(flvm => CoreScriptCommands.DiskCreateFolder(
            //         //        flvm.CurrentDirectory, "NewFolder", "{DestinationFolder}", NameGenerationMode.Rename, 
            //            m => FileList.Refresh(FileList.Select(fm => fm.Equals(m), ResultCommand.OK), true)));

    }
}
