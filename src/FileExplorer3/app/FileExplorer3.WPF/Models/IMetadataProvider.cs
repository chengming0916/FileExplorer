using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.Models
{
    public interface IMetadataProvider
    {
       
        Task<IEnumerable<IMetadata>> GetMetadataAsync
           (IEnumerable<IEntryModel> selectedModels,
           int modelCount, IEntryModel parentModel);
    }
}
