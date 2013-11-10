using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
    /// <summary>
    /// Load a directory content to context["EntryList"], with format IEnumerable&lt;IEntryModel&gt;
    /// </summary>
    public class EntryListHelper : IEntryListHelper
    {
        #region Methods


        public async Task<IList<IEntryModel>> ListAsync(IEntryViewModel parentModel, Func<IEntryModel, bool> filter = null)
        {
            var result = await parentModel.EntryModel.Profile.ListAsync(parentModel.EntryModel, filter);
            var entryModels = from m in result
                              where filter(m)
                              select (IEntryModel)m;
            return entryModels.ToList();
        }

        public void AppendEntryList(IFileListViewModel targetModel,
            IEntryViewModel parentModel, IList<IEntryModel> entryModels)
        {
            foreach (var em in entryModels)
            {
                var evm = EntryViewModel.FromEntryModel(em);
                targetModel.Items.Add(evm);
            }
        }

        public void ReplaceEntryList(IFileListViewModel targetModel,
            IEntryViewModel parentModel, IList<IEntryModel> entryModels)
        {
            targetModel.Items.Clear();
            AppendEntryList(targetModel, parentModel, entryModels);
        }


        #endregion

        #region Data


        #endregion

        #region Public Properties

        #endregion

    }
}
