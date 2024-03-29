﻿using Caliburn.Micro;
using FileExplorer.Models;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.Script
{
    public static partial class UIScriptCommands
    {
        public static IScriptCommand ExplorerDefaultToolbarCommands(string explorerVariable,
            string windowManagerVariable, IScriptCommand nextCommand = null)
        {
            return new ExplorerDefaultToolbarCommands()
            {
                ExplorerKey = explorerVariable,
                WindowManagerKey = windowManagerVariable,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand ExplorerDefaultToolbarCommands(IScriptCommand nextCommand = null)
        {
            return ExplorerDefaultToolbarCommands("{Explorer}", "{WindowManager}", nextCommand);
        }
    }

    /// <summary>
    /// Set default ScriptCommand and parameter for DiskBased use.
    /// </summary>
    public class ExplorerDefaultToolbarCommands : ScriptCommandBase
    {
        /// <summary>
        /// Point to Explorer (IExplorerViewModel).
        /// </summary>
        public string ExplorerKey { get; set; }

        public string WindowManagerKey { get; set; }

        public ExplorerDefaultToolbarCommands()
            : base("ExplorerDefault")
        {
            ExplorerKey = "{Explorer}";
            WindowManagerKey = "{WindowManager}";
            ContinueOnCaptureContext = true;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            IWindowManager windowManager = pm.GetValue<IWindowManager>(WindowManagerKey) ?? new WindowManager();
            return UIScriptCommands.ExplorerDo(ExplorerKey, explorerModel =>
                {
                    #region FileList
                    explorerModel.FileList.Commands.ToolbarCommands.ExtraCommandProviders = new[] {                               
                                
                new StaticCommandProvider(                    
                    new SeparatorCommandModel(),
                    new SelectGroupCommand( explorerModel.FileList),    
                    new ViewModeCommand( explorerModel.FileList),                    
                    new SeparatorCommandModel(),
                    new CommandModel(ExplorerCommands.NewFolder) { IsVisibleOnToolbar = true, 
                        HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE188)
                        //Symbol = Convert.ToChar(0xE188) 
                    },
                    new DirectoryCommandModel(
                        new CommandModel(ExplorerCommands.NewFolder) { Header = Strings.strFolder, IsVisibleOnMenu = true }
                        )
                        { IsVisibleOnMenu = true, Header = Strings.strNew, IsEnabled = true},
                    new ToggleVisibilityCommand(explorerModel.FileList.Sidebar, ExplorerCommands.TogglePreviewer)                    
                    //new CommandModel(ExplorerCommands.TogglePreviewer) { IsVisibleOnMenu = false, Header = "", IsHeaderAlignRight = true, Symbol = Convert.ToChar(0xE239) }
                    )};

                    #endregion
                    #region DirectoryTree
                    explorerModel.DirectoryTree.Commands.ToolbarCommands.ExtraCommandProviders = new[] { 
                new StaticCommandProvider(
                     new DirectoryCommandModel(
                    new CommandModel(ExplorerCommands.NewWindow) { IsVisibleOnMenu = true},
                    new CommandModel(ExplorerCommands.OpenTab) { IsVisibleOnMenu = true  })
                   { Header = "Open", IsVisibleOnMenu = true, IsEnabled = true },
                     //new CommandModel(ApplicationCommands.New) { IsVisibleOnMenu = true },
                    new CommandModel(ExplorerCommands.Refresh) { IsVisibleOnMenu = true },
                    new CommandModel(ApplicationCommands.Delete) { IsVisibleOnMenu = true },
                    new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnMenu = true },
              
                    new CommandModel(ExplorerCommands.Map)  { 
                        //Symbol = Convert.ToChar(0xE17B), 
                        HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE17B),
                        IsEnabled = true,
                        IsHeaderVisible = false, IsVisibleOnToolbar = true
                    },
                    new CommandModel(ExplorerCommands.Unmap)  { 
                        HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE17A),
                        IsVisibleOnMenu = true,
                        IsVisibleOnToolbar = true
                    }
                    )
                };
                    #endregion
                }
                , NextCommand);
        }
    }
}
