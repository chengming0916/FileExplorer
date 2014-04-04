using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.Models
{
    public class FileSystemInfoMetadataProvider : MetadataProviderBase
    {
        public FileSystemInfoMetadataProvider()
            : base(new BasicMetadataProvider())
        { }

        public override async Task<IEnumerable<IMetadata>> GetMetadataAsync(IEnumerable<IEntryModel> selectedModels,
            int modelCount, IEntryModel parentModel)
        {
            List<IMetadata> retList = new List<IMetadata>();

            foreach (var m in await base.GetMetadataAsync(selectedModels, modelCount, parentModel))
                retList.Add(m);

            if (selectedModels.Count() > 0)
            {
                long size = 0;
                foreach (var m in selectedModels)
                    if (m is FileSystemInfoModel)
                        size += (m as FileSystemInfoModel).Size;
                if (size > 0)
                    retList.Add(new Metadata(DisplayType.Kb, MetadataStrings.strCategoryInfo,
                        "Size", size));

                retList.Add(new Metadata(DisplayType.Number, MetadataStrings.strCategoryTest, "Number", 10000));
                retList.Add(new Metadata(DisplayType.Percent, MetadataStrings.strCategoryTest, "Percent", 10));
                retList.Add(new Metadata(DisplayType.Boolean, MetadataStrings.strCategoryTest, "Boolean", true, false));
            }

            return retList;

        }

    }
}
