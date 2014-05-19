using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.Models;

namespace FileExplorer.WPF.Models
{
    /// <summary>
    /// Owned by IProfile, for drag drop handling.
    /// </summary>
    public interface IShellDragDropHandler : IDragDropHandler
    {
        Task<IDataObject> GetDataObject(IEnumerable<IEntryModel> entries);
        IEnumerable<IEntryModel> GetEntryModels(IDataObject dataObject);
    }
}
