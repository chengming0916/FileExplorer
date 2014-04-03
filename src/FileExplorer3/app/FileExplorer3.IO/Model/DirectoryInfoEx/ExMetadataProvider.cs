using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.Models
{
    public class FileSystemInfoExMetadataProvider : MetadataProviderBase
    {
        public override IEnumerable<IMetadata> GetMetadata(IEnumerable<IEntryModel> selectedModels,
            int modelCount, IEntryModel parentModel)
        {
            //Items.Add(MetadataViewModel.FromText("", String.Format("{0} items", flvm.Items.Count()), true));
            //if (SelectionCount > 0)
            //    Items.Add(MetadataViewModel.FromText("", String.Format("{0} items selected", SelectionCount), true));

            yield return new Metadata(DisplayType.Text, "", String.Format("{0} items", modelCount)) { IsHeader = true, IsVisibleInSidebar = false };

            if (selectedModels.Count() > 0)
            {
                yield return new Metadata(DisplayType.Text, "", String.Format("{0} items selected",
                    selectedModels.Count())) { IsHeader = true, IsVisibleInSidebar = false };

                long size = 0;
                foreach (var m in selectedModels)
                    if (m is FileSystemInfoExModel)
                        size += (m as FileSystemInfoExModel).Size;
                if (size > 0)
                    yield return new Metadata(DisplayType.Kb, "Size", size);

                yield return new Metadata(DisplayType.Number, "Number", 10000);
                yield return new Metadata(DisplayType.Percent, "Percent", 10);
                yield return new Metadata(DisplayType.Boolean, "Boolean", true, false);
            }


        }
    }
}
