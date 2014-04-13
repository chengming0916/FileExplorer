using FileExplorer.Defines;
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
        public FileBasedCommandProvider()
            : base(
            new CommandModel(ApplicationCommands.Open) { IsVisibleOnMenu = true },
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
