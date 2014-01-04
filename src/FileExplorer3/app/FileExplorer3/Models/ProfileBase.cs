﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.Models
{
    public abstract class ProfileBase : IProfile 
    {

        #region Constructor

        public ProfileBase()
        {
            RootDisplayName = "Root";

            SuggestSource = new ProfileSuggestionSource(this);
            
            HierarchyComparer = PathComparer.Default;
            MetadataProvider = new NullMetadataProvider();
            CommandProviders = new List<ICommandProvider>();

            DragDrop = new NullDragDropHandler();
        }
        
        #endregion

        #region Methods

        public virtual IComparer<IEntryModel> GetComparer(ColumnInfo column)
        {            
            return column.Comparer;
        }

        public virtual IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry)
        {
            yield return GetDefaultIcon.Instance;
        }

         public abstract Task<IEntryModel> ParseAsync(string path);
         public abstract Task<IEnumerable<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null);
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties

        public ISuggestSource SuggestSource { get; protected set; }
        public IDragDropHandler DragDrop { get; protected set; }
        public string RootDisplayName { get; protected set; }
        public IEntryHierarchyComparer HierarchyComparer { get; protected set; }
        public IMetadataProvider MetadataProvider { get; protected set; }        
        public IEnumerable<ICommandProvider> CommandProviders { get; protected set; }
        
        #endregion

    }
}
