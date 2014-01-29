using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.Defines
{
    public static class FileListCommands
    {
        public static RoutedUICommand UpOneLevel = new RoutedUICommand(Strings.strInvertSelect, "InvertSelect", typeof(FileListCommands));
        public static RoutedUICommand InvertSelect = new RoutedUICommand(Strings.strInvertSelect, "InvertSelect", typeof(FileListCommands));
        public static RoutedUICommand UnselectAll = new RoutedUICommand(Strings.strUnselectAll, "UnselectAll", typeof(FileListCommands));
        public static RoutedUICommand ToggleCheckBox = new RoutedUICommand(Strings.strToggleCheckBox, "ToggleCheckBox", typeof(FileListCommands));
        public static RoutedUICommand ToggleViewMode = new RoutedUICommand(Strings.strToggleViewMode, "ToggleViewMode", typeof(FileListCommands));

        public static RoutedUICommand Refresh = new RoutedUICommand(Strings.strRefresh, "Refresh", typeof(FileListCommands),
            new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.R, ModifierKeys.Control) }));

        public static RoutedUICommand Rename = new RoutedUICommand(Strings.strRename, "Rename", typeof(FileListCommands),
            new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F2) }));
    }
}
