using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels.Helpers
{
    /// <summary>
    /// Helper view model class that provide support of loading sub-entries.
    /// </summary>
    /// <typeparam name="VM"></typeparam>
    public interface ISubEntriesHelper<VM> : INotifyPropertyChanged
    {
        /// <summary>
        /// Call to load sub-entries.
        /// </summary>
        /// <param name="force">Load sub-entries even if it's already loaded.</param>
        /// <returns></returns>
        Task<IEnumerable<VM>> LoadAsync(bool force = false);

        /// <summary>
        /// Used to preload sub-entries.
        /// </summary>
        /// <param name="viewModels"></param>
        void SetEntries(params VM[] viewModels);

        /// <summary>
        /// Load when expand the first time.
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Whether subentries loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// A list of sub-entries, after loaded.
        /// </summary>
        ObservableCollection<VM> All { get; }
    }
}
