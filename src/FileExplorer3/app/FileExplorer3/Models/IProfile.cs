using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINRT
using Windows.UI.Xaml.Media;
#else
//using System.Windows.Media;
#endif
//using Caliburn.Micro;
//using FileExplorer.WPF.BaseControls;
using System.Windows;
//using FileExplorer.WPF.ViewModels.Helpers;
using Cofe.Core.Script;
using System.Threading;
using System.ComponentModel;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
   
    public interface IProfile : INotifyPropertyChanged
    {
        
        #region Methods

        

        IComparer<IEntryModel> GetComparer(ColumnInfo column);

        /// <summary>
        /// Return the entry that represent the path, or null if not exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IEntryModel> ParseAsync(string path);

        Task<IList<IEntryModel>> ListAsync(IEntryModel entry, CancellationToken ct , Func<IEntryModel, bool> filter = null, bool refresh = false);

        /// <summary>
        /// Return the sequence of icon is extracted and returned, EntryViewModel will run each extractor 
        /// and set Icon to it's GetIconForModel() result.
        /// Default is GetDefaultIcon.Instance then GetFromProfile.Instance.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry);


        #endregion

        #region Data
        
        #endregion

        #region Public Properties
        string ProfileName { get; }
        byte[] ProfileIcon { get; }

        string RootDisplayName { get; }
        IPathHelper Path { get; }

        IEntryHierarchyComparer HierarchyComparer { get; }
        IMetadataProvider MetadataProvider { get; }
        IEnumerable<ICommandProvider> CommandProviders { get; }
        IDragDropHandler DragDrop { get; }
        
        //IDiskPathMapper PathMapper { get; }
        ISuggestSource SuggestSource { get; }
        
        #endregion
    }

    
}
