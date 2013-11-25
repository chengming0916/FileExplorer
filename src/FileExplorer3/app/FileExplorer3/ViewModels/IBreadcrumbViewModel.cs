using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public interface IBreadcrumbViewModel : ISupportSelectionHelper<IBreadcrumbItemViewModel, IEntryModel>
    {
        ISubEntriesHelper<IBreadcrumbItemViewModel> Entries { get; set; }
    }
}
