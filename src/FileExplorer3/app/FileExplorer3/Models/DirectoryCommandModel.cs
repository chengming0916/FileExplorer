using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofe.Core.Script;

namespace FileExplorer.Models
{
    public class DirectoryCommandModel : CommandModel, IDirectoryCommandModel
    {        
        #region Constructor

        public DirectoryCommandModel(IScriptCommand command, Func<IEnumerable<ICommandModel>> getCommandFunc)
             : base(command)
        {
            _getCommandFunc = getCommandFunc;
        }

        public DirectoryCommandModel(IScriptCommand command, params ICommandModel[] commandModels)
            : this(command, () => commandModels)
        {            
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private Func<IEnumerable<ICommandModel>> _getCommandFunc;

        #endregion

        #region Public Properties

        public IEnumerable<ICommandModel> SubCommands
        {
            get { return _getCommandFunc(); }
        }

        #endregion
    }
}
