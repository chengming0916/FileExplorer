using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.WPF.Models
{
    public interface IEntryModelIconExtractor
    {
        Task<byte[]> GetIconBytesForModelAsync(IEntryModel model, CancellationToken ct);
    }

}
