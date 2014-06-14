using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Script;

namespace FileExplorer.Models
{
    /// <summary>
    /// Provide CommandModels given entries.
    /// </summary>
    public interface ICommandProvider
    {
        List<ICommandModel> GetCommandModels();
    }

    /// <summary>
    /// Provide a fixed number of commands.
    /// </summary>
    public class StaticCommandProvider : ICommandProvider
    {
        private ICommandModel[] _commands;
        public StaticCommandProvider(params ICommandModel[] commands)
        {
            _commands = commands;
        }

        public List<ICommandModel> GetCommandModels()
        {
            return _commands.ToList();
        }
    }
}
