using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models.Bookmark
{
    public class BookmarkProfile : ProfileBase
    {
        #region fields

        public static BookmarkProfile Instance = new BookmarkProfile();
        private string _rootLabel;
        private BookmarkModel _rootModel;

        #endregion

        #region constructors

        public BookmarkProfile(string rootLabel = "Bookmarks", IEventAggregator events = null)
            : base(events)
        {
            ProfileName = "Bookmarks";
            HierarchyComparer = new PathHierarchyComparer<BookmarkModel>(StringComparison.CurrentCultureIgnoreCase);
            MetadataProvider = new BasicMetadataProvider();
            Path = PathHelper.Web;
            _rootLabel = rootLabel;
            _rootModel = BookmarkSerializeTest.CreateTestData(this, rootLabel);
            //new BookmarkModel(BookmarkModel.BookmarkEntryType.Root, rootLabel);
            PathPatterns = new string[] { _rootLabel + "." };
            DragDrop = new BookmarkDragDropHandler();
        }


        #endregion

        #region events

        #endregion

        #region properties

        public BookmarkModel RootModel { get { return _rootModel; } }

        #endregion

        #region methods

        private BookmarkModel lookup(BookmarkModel lookupEntryModel, string[] pathSplits, int idx)
        {
            if (lookupEntryModel == null)
                return null;

            if (idx >= pathSplits.Length)
                return lookupEntryModel;

            return
                lookup(lookupEntryModel.SubModels.FirstOrDefault(sub => sub.Name.Equals(pathSplits[idx])),
                pathSplits, idx + 1);
        }

        public override async Task<IEntryModel> ParseAsync(string path)
        {
            if (path.Equals(_rootLabel))
                return _rootModel;

            if (path.StartsWith(_rootLabel, StringComparison.CurrentCultureIgnoreCase))
                return lookup(_rootModel, path.Split(new char[] { Path.Separator }, StringSplitOptions.RemoveEmptyEntries), 1);

            return null;
        }

        public override async Task<IList<IEntryModel>> ListAsync(IEntryModel entry, CancellationToken ct, Func<IEntryModel, bool> filter = null, bool refresh = false)
        {
            BookmarkModel bm = entry as BookmarkModel;
            filter = filter ?? (em => true);
            if (bm != null)
                return bm.SubModels.Where(sub => filter(sub)).Cast<IEntryModel>().ToList();

            throw new NotImplementedException();
        }

        internal void RaiseEntryChanged(EntryChangedEvent evnt)
        {
            raiseEntryChanged(evnt);
        }

        #endregion


    }
}
