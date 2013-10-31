using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class NullMetadataProvider : IMetadataProvider
    {
        public IEnumerable<IMetadata> GetMetadata(IEnumerable<IEntryModel> models)
        {
            yield break;
        }
    }
}
