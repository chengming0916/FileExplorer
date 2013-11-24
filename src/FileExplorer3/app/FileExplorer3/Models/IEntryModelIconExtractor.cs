using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileExplorer.Models
{
    public interface IEntryModelIconExtractor
    {
        Task<ImageSource> GetIconForModel(IEntryModel model);
    }

}
