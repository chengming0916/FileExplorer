using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
    public interface IEntryListHelper
    {
        Task<IList<IEntryModel>> ListAsync(IEntryViewModel parentModel, Func<IEntryModel, bool> filter = null);

        void AppendEntryList(IFileListViewModel targetModel, IEntryViewModel parentModel, IList<IEntryModel> entryModels);
        void ReplaceEntryList(IFileListViewModel targetModel, IEntryViewModel parentModel, IList<IEntryModel> entryModels);
    }
}
