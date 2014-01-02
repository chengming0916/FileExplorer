using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ICommandsHelper : ICommandContainer
    {
        IEntryModel[] AppliedModels { get; }
        EntriesHelper<ICommandViewModel> Commands { get; }        
    }

    public class CommandsHelper : NotifyPropertyChanged, ICommandsHelper
    {
        #region Constructor

        public CommandsHelper(IProfile[] rootProfiles, params ICommandModel[] extraCommands)
        {
            _extraCommands = extraCommands;
            _rootProfiles = rootProfiles;
            Commands = new EntriesHelper<ICommandViewModel>(loadCommandsTask);                        
        }

        #endregion

        #region Methods

        protected override void NotifyOfPropertyChanged(string propertyName)
        {
            base.NotifyOfPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "AppliedModels":
                    AsyncUtils.RunSync(() => Commands.LoadAsync(false));
                    foreach (var commandVM in Commands.AllNonBindable)
                        commandVM.CommandModel.NotifySelectionChanged(AppliedModels);
                    break;
            }
        }
        
        async Task<IEnumerable<ICommandViewModel>> loadCommandsTask()
        {
            List<ICommandModel> cmList = new List<ICommandModel>(_extraCommands) { };

            foreach (var cp in _rootProfiles.SelectMany(p => p.CommandProviders))
                cmList.AddRange(cp.CommandModels);

            return cmList.Select(cm => new CommandViewModel(cm)).ToArray();
        }


        #endregion

        #region Data

        IEntryModel[] _appliedModels = null;        
        IProfile[] _rootProfiles = null;
        private ICommandModel[] _extraCommands;
        protected List<ICommand> _exportedCommands = new List<ICommand>();

        #endregion

        #region Public Properties

        public IEntryModel[] AppliedModels { get { return _appliedModels; } 
            set { _appliedModels = value; NotifyOfPropertyChanged(() => AppliedModels);  } }
        public EntriesHelper<ICommandViewModel> Commands { get; private set; }

        public IEnumerable<ICommand> ExportedCommands
        {
            get { return _exportedCommands; }
        }

        #endregion






       
    }
}
