using Caliburn.Micro;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models.Bookmark
{
    public class BookmarkProfile : NotifyPropertyChanged, IProfile
    {
        #region fields

        public static BookmarkProfile Instance = new BookmarkProfile();
        private string _rootLabel;

        #endregion

        #region constructors

        public BookmarkProfile(string rootLabel = "Bookmarks", IEventAggregator events = null)
        {
            Path = PathHelper.Web;
            _rootLabel = rootLabel;
            Events = events ?? new EventAggregator();
        }


        #endregion

        #region events

        #endregion

        #region properties


        public string[] PathPatterns
        {
            get { return new string[]{ _rootLabel + "." }; }
        }

        public string ProfileName
        {
            get { return "Bookmarks"; }
        }

        public byte[] ProfileIcon
        {
            get { return new byte[] {}; }
        }

        public IConverterProfile[] Converters
        {
            get { return new IConverterProfile[] {}; }
        }

        public string RootDisplayName
        {
            get { return _rootLabel; }
        }

        public IPathHelper Path
        {
            get;
            private set;
        }

        public IEntryHierarchyComparer HierarchyComparer
        {
            get;
            private set;
        }

        public IMetadataProvider MetadataProvider
        {
            get;
            private set;
        }

        public IEnumerable<ICommandProvider> CommandProviders
        {
            get;
            private set;
        }

        public IEventAggregator Events
        {
            get;
            private set;
        }

        public ISuggestSource SuggestSource
        {
            get { throw new NotImplementedException(); }
        }


        #endregion

        #region methods

        #endregion


      
        public IComparer<IEntryModel> GetComparer(Defines.ColumnInfo column)
        {
            throw new NotImplementedException();
        }

        public Task<IEntryModel> ParseAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IList<IEntryModel>> ListAsync(IEntryModel entry, System.Threading.CancellationToken ct, Func<IEntryModel, bool> filter = null, bool refresh = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IModelIconExtractor<IEntryModel>> GetIconExtractSequence(IEntryModel entry)
        {
            throw new NotImplementedException();
        }

        public bool MatchPathPattern(string path)
        {
            throw new NotImplementedException();
        }

    }
}
