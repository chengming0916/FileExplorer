﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class IOScriptCommands
    {
        public static IScriptCommand ExplorerDefault(string explorerVariable = "{Explorer}", 
            string fileListVariable = "{FileList}", string directoryTreeVariable = "{DirectoryTree}", 
            string breadcrumbVariable = "{Breadcrumb}", IScriptCommand nextCommand = null)
        {
            return new ExplorerDefault()
            {
                ExplorerKey = explorerVariable,
                FileListKey = fileListVariable,
                DirectoryTreeKey = directoryTreeVariable,
                BreadcrumbKey = breadcrumbVariable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand ExplorerDefault(IScriptCommand nextCommand = null)
        {
            return ExplorerDefault("{Explorer}", "{FileList}", "{DirectoryTree}", "{Breadcrumb}", nextCommand);
        }
    }

    /// <summary>
    /// Set default ScriptCommand and parameter for DiskBased use.
    /// </summary>
    public class ExplorerDefault : ScriptCommandBase
    {
        /// <summary>
        /// Point to Explorer (IExplorerViewModel).
        /// </summary>
        public string ExplorerKey { get; set; }

        /// <summary>
        /// Point to FileList (IFileListViewModel).
        /// </summary>
        public string FileListKey { get; set; }

        /// <summary>
        /// Point to DirectoryTree (IDirectoryTreeViewModel).
        /// </summary>
        public string DirectoryTreeKey { get; set; }

        /// <summary>
        /// Point to Breadcrumb (IBreadcrumbViewModel).
        /// </summary>
        public string BreadcrumbKey { get; set; }

        public ExplorerDefault()
            : base("ExplorerDefault")
        {
            ExplorerKey = "{Explorer}";
            FileListKey = "{FileList}";
            DirectoryTreeKey = "{DirectoryTree}";
            BreadcrumbKey = "{Breadcrumb}";
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            

            return ScriptCommands.Assign(new Dictionary<string, object>()
                {
                    { "{ColumnList}", IOInitializeHelpers.FileList_ColumList_For_DiskBased_Items }, 
                    { "{ColumnFilters}", IOInitializeHelpers.FileList_ColumnFilter_For_DiskBased_Items }, 
                    { "{FileListOpenCommand}", IOInitializeHelpers.FileList_Open_For_DiskBased_Items }, 
                    { "{FileListDeleteCommand}", IOInitializeHelpers.FileList_Delete_For_DiskBased_Items }, 
                    { "{FileListNewFolderCommand}", IOInitializeHelpers.FileList_NewFolder_ForDiskBased_Items }, 
                    { "{FileListCutCommand}", IOInitializeHelpers.FileList_Cut_For_DiskBased_Items },
                    { "{FileListCopyCommand}", IOInitializeHelpers.FileList_Copy_For_DiskBased_Items },
                    { "{FileListPasteCommand}", IOInitializeHelpers.FileList_Paste_For_DiskBased_Items },                    
                }, true,
                ScriptCommands.RunCommandsInSequence(NextCommand,
                        UIScriptCommands.ExplorerAssignScriptParameters(ExplorerKey, "{Profiles}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerKey, ExplorerParameterType.RootModels, "{RootDirectories}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerKey, ExplorerParameterType.EnableDrag, "{EnableDrag}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerKey, ExplorerParameterType.EnableDrop, "{EnableDrop}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerKey, ExplorerParameterType.EnableMultiSelect, "{EnableMultiSelect}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerKey, ExplorerParameterType.ColumnList, "{ColumnList}"),
                        UIScriptCommands.ExplorerSetParameters(ExplorerKey, ExplorerParameterType.ColumnFilters, "{ColumnFilters}"),
                        UIScriptCommands.SetScriptCommand(FileListKey, "Open", "{FileListOpenCommand}"),
                        UIScriptCommands.SetScriptCommand(FileListKey, "Delete", "{FileListDeleteCommand}"),
                        UIScriptCommands.SetScriptCommand(FileListKey, "NewFolder", "{FileListNewFolderCommand}"),
                        UIScriptCommands.SetScriptCommand(FileListKey, "Cut", "{FileListCutCommand}"),
                        UIScriptCommands.SetScriptCommand(FileListKey, "Copy", "{FileListCopyCommand}"),
                        UIScriptCommands.SetScriptCommand(FileListKey, "Paste", "{FileListPasteCommand}")
                       )                
                );
        }
    }
}