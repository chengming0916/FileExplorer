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
            new CommandModel(ApplicationCommands.Open) { IsVisibleOnToolbar = false },
            new SeparatorCommandModel(),
           
            new CommandModel(ApplicationCommands.Cut) { IsVisibleOnToolbar = false },
            new CommandModel(ApplicationCommands.Copy) { IsVisibleOnToolbar = false },
            new CommandModel(ApplicationCommands.Paste) { IsVisibleOnToolbar = false },

            new SeparatorCommandModel(),
            new CommandModel(ApplicationCommands.Delete)  { IsVisibleOnToolbar = false },
            new CommandModel(ExplorerCommands.Rename)  { IsVisibleOnToolbar = false }
            
            
            )
        {

        }
    }
}
