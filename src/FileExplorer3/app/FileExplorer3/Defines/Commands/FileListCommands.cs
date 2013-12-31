using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.Defines.Commands
{
    public static class FileListCommands
    {
        public static string strToggleCheckBox = "Checkboxes";
        public static string strToggleViewMode = "ViewMode";
        public static RoutedUICommand ToggleCheckBox = new RoutedUICommand(strToggleCheckBox, "ToggleCheckBox", typeof(FileListCommands));

        public static RoutedUICommand ToggleViewMode = new RoutedUICommand(strToggleViewMode, "ToggleViewMode", typeof(FileListCommands));
    }
}
