using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINRT
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.BaseControls;
using System.Windows;
using FileExplorer.ViewModels.Helpers;
using Cofe.Core.Script;

namespace FileExplorer.Models
{
   
    public interface IProfile
    {
        
        #region Methods

        

        IComparer<IEntryModel> GetComparer(ColumnInfo column);

        /// <summary>
        /// Return the entry that represent the path, or null if not exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IEntryModel> ParseAsync(string path);

        Task<IList<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null, bool refresh = false);

        /// <summary>
        /// Return the sequence of icon is extracted and returned, EntryViewModel will run each extractor 
        /// and set Icon to it's GetIconForModel() result.
        /// Default is GetDefaultIcon.Instance then GetFromProfile.Instance.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry);

        string RootDisplayName { get; }

        #endregion

        #region Data
        
        #endregion

        #region Public Properties

        IPathHelper Path { get; }

        IEntryHierarchyComparer HierarchyComparer { get; }
        IMetadataProvider MetadataProvider { get; }
        IEnumerable<ICommandProvider> CommandProviders { get; }
        ISuggestSource SuggestSource { get; }
        IDragDropHandler DragDrop { get; }
        //IDiskPathMapper PathMapper { get; }

        IEventAggregator Events { get; }
        
        #endregion
    }

    public interface IDiskProfile : IProfile
    {
        IDiskIOHelper DiskIO { get; }
    }
}
