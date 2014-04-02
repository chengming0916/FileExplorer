using Cofe.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public abstract class MetadataProviderBase : IMetadataProvider
    {

        public virtual IEnumerable<IMetadata> GetMetadata(IEnumerable<IEntryModel> selectedModels, int modelCount,
            IEntryModel parentModel)
        {
            return AsyncUtils.RunSync(() => GetMetadataAsync(selectedModels, modelCount, parentModel));
        }


        public async virtual Task<IEnumerable<IMetadata>> GetMetadataAsync(IEnumerable<IEntryModel> selectedModels, int modelCount, IEntryModel parentModel)
        {
            return GetMetadata(selectedModels, modelCount, parentModel);
        }
    }


    public class NullMetadataProvider : MetadataProviderBase
    {

        public override IEnumerable<IMetadata> GetMetadata(IEnumerable<IEntryModel> selectedModels, int modelCount, 
            IEntryModel parentModel)
        {
            yield break;
        }
    }
}
