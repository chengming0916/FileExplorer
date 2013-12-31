using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Cofe.Core.Script;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.Models
{
    /// <summary>
    /// Model for a Command, which is shown as ToolbarExItem in FileExplorer.
    /// </summary>
    public interface ICommandModel : IComparable<ICommandModel>, IComparable, INotifyPropertyChanged
    {
        string CommandType { get; }

        RoutedUICommand RoutedCommand { get; set; }
        IScriptCommand Command { get; set; }

        /// <summary>
        /// Based on Segoe UI Symbol Icons.
        /// See - http://www.adamdawes.com/windows8/win8_segoeuisymbol.html
        /// </summary>
        char? Symbol { get; }

        /// <summary>
        /// Bitmap Icon.
        /// </summary>
        Bitmap HeaderIcon { get; }

        Func<ICommandModel, System.Windows.Media.ImageSource> HeaderImageFunc { get; }

        string ToolTip { get; }

        string Header { get; }

        /// <summary>
        /// Used by IComparable, Lowert priority show up earlier.
        /// </summary>
        int Priority { get; }

        bool IsChecked { get; }

        bool IsEnabled { get; }

        void NotifySelectionChanged(IEntryModel[] appliedModels);
    }

    public interface ISeparatorModel : ICommandModel
    {        
    }

    public interface IDirectoryCommandModel : ICommandModel
    {
        List<ICommandModel> SubCommands { get; }
    }

    public interface ISelectorCommandModel : IDirectoryCommandModel
    {
        bool IsComboBox { get; }
    }

    public interface ISliderCommandModel : IDirectoryCommandModel
    {
        int SliderMaximum { get; }
        int SliderMinimum { get; }
        int SliderValue { get; set; }
    }

    public interface ISliderStepCommandModel : ICommandModel
    {
        int SliderStep { get; }
        double? ItemHeight { get; }
        VerticalAlignment VerticalAlignment { get; }        
    }
    
}
