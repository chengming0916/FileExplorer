using FileExplorer.Defines;
using FileExplorer.WPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.WPF.ViewModels
{
    public class ToggleVisibilityCommand : CommandModel
    {
        public ToggleVisibilityCommand(IToggleableVisibility vm, RoutedUICommand routedCommand)
            : base(routedCommand)
        {
            Symbol = Convert.ToChar(0xE239);
            IsVisibleOnToolbar = true;
            Header = "";
            IsHeaderAlignRight = true;

            vm.PropertyChanged += (PropertyChangedEventHandler)((o,e) => 
            {
              switch (e.PropertyName)
              {
                  case "IsVisible" :
                      Symbol = vm.IsVisible ? Convert.ToChar(0xE23a) : Convert.ToChar(0xE239);
                      break;
              }
            });
        }

        
    }
}
