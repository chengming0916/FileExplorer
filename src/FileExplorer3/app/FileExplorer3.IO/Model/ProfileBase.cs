﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.ViewModels.Helpers;
using FileExplorer.WPF.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;
using System.Text.RegularExpressions;

namespace FileExplorer.WPF.Models
{
    public abstract class ProfileBase : NotifyPropertyChanged, IProfile
    {

        #region Constructor

        public ProfileBase(IEventAggregator events)
        {
            ProfileName = "Unspecified";
            ProfileIcon = null;
            RootDisplayName = "Root";
            
            Path = PathHelper.Disk;
            SuggestSource = new ProfileSuggestionSource(this);

            HierarchyComparer = PathComparer.LocalDefault;
            MetadataProvider = new BasicMetadataProvider();
            CommandProviders = new List<ICommandProvider>();
            //PathMapper = NullDiskPatheMapper.Instance;

            DragDrop = new NullDragDropHandler();
            Events = events ?? new EventAggregator();
            Converters = new IConverterProfile[] { };

        }

        #endregion

        #region Methods

        public virtual IComparer<IEntryModel> GetComparer(ColumnInfo column)
        {
            return column.Comparer;
        }

        public virtual IEnumerable<IModelIconExtractor<IEntryModel>> GetIconExtractSequence(IEntryModel entry)
        {
            yield return GetDefaultIcon.Instance;
        }

        //public virtual IModelIconExtractor<IEntryModel> GetThumbnailExtractor(IEntryModel entry)
        //{
        //    return null;
        //}

        public abstract Task<IEntryModel> ParseAsync(string path);
        public abstract Task<IList<IEntryModel>> ListAsync(IEntryModel entry, CancellationToken ct, Func<IEntryModel, bool> filter = null, bool refresh = false);

        public bool MatchPathPattern(string path)
        {
            foreach (var pattern in _pathPatterns)
                if (new Regex(pattern, RegexOptions.IgnoreCase).Match(path).Success)
                    return true;
            return false;
        }

        #endregion

        #region Data

        string[] _pathPatterns = new string[] { };

        #endregion

        #region Public Properties
        public string[] PathPatterns
        {
            get { return _pathPatterns; }
            protected set
            {
                _pathPatterns = value;
            }
        }
        
        public string ProfileName { get; protected set; }
        public byte[] ProfileIcon { get; protected set; }
        public IPathHelper Path { get; protected set; }
        public ISuggestSource SuggestSource { get; protected set; }
        public IDragDropHandler DragDrop { get; protected set; }
        public string RootDisplayName { get; protected set; }
        public IEntryHierarchyComparer HierarchyComparer { get; protected set; }
        public IMetadataProvider MetadataProvider { get; protected set; }
        public IEnumerable<ICommandProvider> CommandProviders { get; protected set; }
        //public IDiskPathMapper PathMapper { get; protected set; }
        public IEventAggregator Events { get; protected set; }
        public IConverterProfile[] Converters { get; protected set; }
        
        #endregion

    }

}