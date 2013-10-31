using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.Models
{
    public class FileSystemInfoMetadataProvider : IMetadataProvider
    {
        public IEnumerable<IMetadata> GetMetadata(IEnumerable<IEntryModel> models)
        {
            long size = 0;
            foreach (var m in models)
                if (m is FileSystemInfoModel)
                size += (m as FileSystemInfoModel).Size;
            if (size > 0)
                yield return new Metadata(DisplayType.Text, "Size", size);
        }
    }
}
