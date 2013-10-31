using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface IMetadataProvider
    {
        IEnumerable<IMetadata> GetMetadata(IEnumerable<IEntryModel> models);
    }
}
