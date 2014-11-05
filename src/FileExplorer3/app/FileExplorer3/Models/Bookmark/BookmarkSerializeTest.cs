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
        public static void Test()
        {
            BookmarkModel root = new BookmarkModel(BookmarkModel.BookmarkEntryType.Root, "");
            BookmarkModel sub = new BookmarkModel(BookmarkModel.BookmarkEntryType.Directory, "/Sub");
            root.SubModels.Add(sub);
            sub.SubModels.Add(new BookmarkModel(BookmarkModel.BookmarkEntryType.Link, "/Sub/Link"));

            XmlSerializer serializer = new XmlSerializer(typeof(BookmarkModel));
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, root);

            ms.Seek(0, SeekOrigin.Begin);
            BookmarkModel root1 = serializer.Deserialize(ms) as BookmarkModel;
            Debug.WriteLine(root1);
        }

    }
}
