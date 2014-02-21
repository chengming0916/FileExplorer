using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace FileExplorer.Defines
{
    public static class ExplorerCommands
    {
        public static RoutedUICommand UpOneLevel = new RoutedUICommand(Strings.strInvertSelect, "InvertSelect", typeof(ExplorerCommands));
        public static RoutedUICommand InvertSelect = new RoutedUICommand(Strings.strInvertSelect, "InvertSelect", typeof(ExplorerCommands));
        public static RoutedUICommand UnselectAll = new RoutedUICommand(Strings.strUnselectAll, "UnselectAll", typeof(ExplorerCommands));
        public static RoutedUICommand ToggleCheckBox = new RoutedUICommand(Strings.strToggleCheckBox, "ToggleCheckBox", typeof(ExplorerCommands));
        public static RoutedUICommand ToggleViewMode = new RoutedUICommand(Strings.strToggleViewMode, "ToggleViewMode", typeof(ExplorerCommands));

        public static RoutedUICommand NewFolder = new RoutedUICommand(Strings.strNewFolder, "NewFolder", typeof(ExplorerCommands));


        public static RoutedUICommand Refresh = new RoutedUICommand(Strings.strRefresh, "Refresh", typeof(ExplorerCommands),
            new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.R, ModifierKeys.Control) }));

        public static RoutedUICommand Rename = new RoutedUICommand(Strings.strRename, "Rename", typeof(ExplorerCommands),
            new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F2) }));
    }
}
