using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileExplorer.Models
{
    public interface IEntryModelIconExtractor
    {
        Task<ImageSource> GetIconForModelAsync(IEntryModel model, CancellationToken ct);
    }

}
