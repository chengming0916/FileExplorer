﻿using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.Models
{
    public class FileBasedCommandProvider : StaticCommandProvider
    {
        public FileBasedCommandProvider(ICommandManager cm)
            : base(
            new DirectoryCommandModel(
                //new CommandModel(cm.CommandDictionary.Open) { Header= Strings.strOpen, IsVisibleOnMenu = true },
                new CommandModel(ApplicationCommands.Open) { IsVisibleOnMenu = true },
                new CommandModel(ExplorerCommands.NewWindow) { IsVisibleOnMenu = true },
                new CommandModel(ExplorerCommands.OpenTab) { IsVisibleOnMenu = true })
                { Header = "Open", IsVisibleOnMenu = true, IsEnabled = true },
             
            new SeparatorCommandModel(),
           
            new CommandModel(ApplicationCommands.Cut) { IsVisibleOnMenu = true },
            new CommandModel(ApplicationCommands.Copy) { IsVisibleOnMenu = true },
            new CommandModel(ApplicationCommands.Paste) { IsVisibleOnMenu = true },

            new SeparatorCommandModel(),
            new CommandModel(ApplicationCommands.Delete)  { IsVisibleOnMenu = true },
            new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnMenu = true }
            
            
            )
        {

        }
    }
}
