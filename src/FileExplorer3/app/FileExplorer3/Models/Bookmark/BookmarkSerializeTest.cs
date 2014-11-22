using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileExplorer.Models.Bookmark
{
    public static class BookmarkSerializeTest
    {
        public static BookmarkModel CreateTestData(BookmarkProfile profile, string rootLabel = "Bookmarks")
        {
            BookmarkModel root = new BookmarkModel(profile, BookmarkModel.BookmarkEntryType.Root, rootLabel);
            BookmarkModel sub = new BookmarkModel(profile, BookmarkModel.BookmarkEntryType.Directory, rootLabel + "/Sub");
            root.SubModels.Add(sub);
            sub.SubModels.Add(new BookmarkModel(profile, BookmarkModel.BookmarkEntryType.Link, rootLabel + "/Sub/Link"));
            return root;
        }

        public static void Test()
        {
            BookmarkProfile profile = new BookmarkProfile("Bmarks");

            BookmarkModel root = CreateTestData(profile);

            XmlSerializer serializer = new XmlSerializer(typeof(BookmarkModel));
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, root);

            ms.Seek(0, SeekOrigin.Begin);
            BookmarkModel root1 = serializer.Deserialize(ms) as BookmarkModel;
            Debug.WriteLine(root1);
        }

    }
}
