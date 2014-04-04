using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class BasicMetadataProvider : MetadataProviderBase
    {

        public override async Task<IEnumerable<IMetadata>> GetMetadataAsync(IEnumerable<IEntryModel> selectedModels, int modelCount, IEntryModel parentModel)
        {
            List<IMetadata> retList = new List<IMetadata>();

            foreach (var m in await base.GetMetadataAsync(selectedModels, modelCount, parentModel))
                retList.Add(m);

            retList.Add(new Metadata(DisplayType.Text, MetadataStrings.strCategoryInfo,
                "", String.Format(MetadataStrings.strTotalItems, modelCount)) { IsHeader = true, IsVisibleInSidebar = false });

            if (selectedModels.Count() > 0)
            {
                retList.Add(new Metadata(DisplayType.Text, MetadataStrings.strCategoryInfo,
                    "", String.Format(MetadataStrings.strSelectedItems,
                    selectedModels.Count())) { IsHeader = true, IsVisibleInSidebar = false });
            }

            return retList;
        }
    }
}
