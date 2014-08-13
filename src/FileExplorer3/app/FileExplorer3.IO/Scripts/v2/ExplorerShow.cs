using FileExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class IOScriptCommands
    {
        public static IScriptCommand ExplorerShow(IProfile[] profiles, IEntryModel[] rootDirectories,
            string explorerVariable = "{Explorer}", IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Assign(new Dictionary<string, object>()
                {
                    {"{Profiles}", profiles },
                    {"{RootDirectories}", rootDirectories },
                    {"{OnModelCreated}", IOInitializeHelpers.Explorer_Initialize_Default }, 
                    {"{OnViewAttached}", UIScriptCommands.ExplorerGotoStartupPathOrFirstRoot() }

                }, false,
                  UIScriptCommands.ExplorerShow("{OnModelCreated}", "{OnViewAttached}", "{WindowManager}",
                        "{Events}", explorerVariable, nextCommand));
        }

        public static IScriptCommand FileOpen(IProfile[] profiles, IEntryModel[] rootDirectories,
           bool enableMultiSelect = true, string filterString = "All files (*.*)|*.*",
           string selectionPathsVariable = "{Selection}",
           IScriptCommand nextCommand = null, IScriptCommand cancelCommand = null)
        {
            return ScriptCommands.Assign(new Dictionary<string, object>()
                {
                    {"{Profiles}", profiles },
                    {"{RootDirectories}", rootDirectories },
                    {"{EnableDrag}", false },
                    {"{EnableDrop}", false },
                    {"{FilterString}", filterString },
                    {"{EnableMultiSelect}", enableMultiSelect },
                    { "{FileListNewWindowCommand}", NullScriptCommand.Instance },
                    {"{OnModelCreated}", IOInitializeHelpers.Explorer_Initialize_Default }, 
                    {"{OnViewAttached}", UIScriptCommands.ExplorerGotoStartupPathOrFirstRoot() }
                }, false,
                  UIScriptCommands.FilePick(ExplorerMode.FileOpen, "{OnModelCreated}", "{OnViewAttached}", "{WindowManager}",
                        "{Events}", selectionPathsVariable, nextCommand, cancelCommand));
        }

        public static IScriptCommand FileSave(IProfile[] profiles, IEntryModel[] rootDirectories,
            string filterString = "All files (*.*)|*.*",
            string selectionPathsVariable = "{Selection}",
            IScriptCommand nextCommand = null, IScriptCommand cancelCommand = null)
        {
            return ScriptCommands.Assign(new Dictionary<string, object>()
                {
                    {"{Profiles}", profiles },
                    {"{RootDirectories}", rootDirectories },
                    {"{EnableDrag}", false },
                    {"{EnableDrop}", false },
                    {"{FilterString}", filterString },
                    {"{EnableMultiSelect}", false },
                    {"{FileListNewWindowCommand}", NullScriptCommand.Instance },
                    {"{OnModelCreated}", IOInitializeHelpers.Explorer_Initialize_Default }, 
                    {"{OnViewAttached}", UIScriptCommands.ExplorerGotoStartupPathOrFirstRoot() }
                }, false,
                  UIScriptCommands.FilePick(ExplorerMode.FileSave, "{OnModelCreated}", "{OnViewAttached}", "{WindowManager}",
                        "{Events}", selectionPathsVariable, nextCommand, cancelCommand));
        }

    }
}
