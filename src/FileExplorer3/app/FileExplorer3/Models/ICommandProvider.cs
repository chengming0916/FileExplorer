using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    /// <summary>
    /// Provide CommandModels given entries.
    /// </summary>
    public interface ICommandProvider
    {
        Task<IEnumerable<ICommandModel>> GetCommandsAsync(params IEntryModel[] appliedModels);
    }
}
