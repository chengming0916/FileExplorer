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

namespace FileExplorer.Models
{
   
    public interface IProfile
    {
        #region Cosntructor
        
        #endregion

        #region Methods

        ISuggestSource GetSuggestSource();

        IComparer<IEntryModel> GetComparer(ColumnInfo column);

        /// <summary>
        /// Return the entry that represent the path, or null if not exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<IEntryModel> ParseAsync(string path);

        Task<IEnumerable<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null);


        #region Drag and Drop

        IDataObject GetDataObject(IEnumerable<IEntryModel> entries);

        DragDropEffects QueryDrag(IEnumerable<IEntryModel> entries);

        void OnDragCompleted(IEnumerable<IEntryModel> entries, IDataObject da, DragDropEffects effect);

        IEnumerable<IEntryModel> GetEntryModels(IDataObject dataObject);

        QueryDropResult QueryDrop(IEnumerable<IEntryModel> entries, IEntryModel dest, DragDropEffects allowedEffects);        
        DragDropEffects OnDropCompleted(IEnumerable<IEntryModel> entries, IDataObject da, IEntryModel dest, DragDropEffects allowedEffects);

        #endregion

        //Task<ImageSource> GetIconAsync(IEntryModel entry, int size);

        /// <summary>
        /// Return the sequence of icon is extracted and returned, EntryViewModel will run each extractor 
        /// and set Icon to it's GetIconForModel() result.
        /// Default is GetDefaultIcon.Instance then GetFromProfile.Instance.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry);

        string RootDisplayName { get; }

        /// <summary>
        /// Transfer the source entries to dest directory.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        Task<IEnumerable<IEntryModel>> TransferAsync(TransferMode mode, IEntryModel[] source, IEntryModel dest);



        /// <summary>
        /// Rename an entry.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        Task<bool> Rename(IEntryModel source, string newName);
        //IResult<ImageSource> GetIcon();
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties

        IEntryHierarchyComparer HierarchyComparer { get; }
        IMetadataProvider MetadataProvider { get; }
        
        #endregion
    }
}
