using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    /// <summary>
    /// Profile that reside in another profile to convert entry model when listing to a different entry model.    
    /// </summary>
    public interface IConverterProfile : IProfile
    {
        IEntryModel Convert(IEntryModel entryModel);
    }
  

}
