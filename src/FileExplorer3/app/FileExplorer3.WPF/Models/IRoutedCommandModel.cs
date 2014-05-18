using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace FileExplorer.WPF.Models
{
    public interface IRoutedCommandModel : ICommandModel
    {
        RoutedUICommand RoutedCommand { get; }

        /// <summary>
        /// Bitmap Icon.
        /// </summary>
        Bitmap HeaderIcon { get; }

        Func<ICommandModel, ImageSource> HeaderImageFunc { get; }
    }
}
