using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Cinch;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.Models;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    public class CommandViewModel : PropertyChangedBase, ICommandViewModel
    {
        #region Constructor

        public CommandViewModel(ICommandModel commandModel)
        {
            CommandModel = commandModel;

            if (CommandModel != null)
                Command = new SimpleCommand()
                {
                    CanExecuteDelegate = p => CommandModel.Command.CanExecute(
                        ParameterDic.FromParameterPair(new ParameterPair("Parameter", p))),
                    ExecuteDelegate = p =>
                        new ScriptRunner().Run(CommandModel.Command,
                                ParameterDic.FromParameterPair(new ParameterPair("Parameter", p)))

                };

            if (CommandModel is IDirectoryCommandModel)
            {
                foreach (var c in (CommandModel as IDirectoryCommandModel).SubCommands)
                {
                    SubCommands.Add(new CommandViewModel(c));
                }
            }
        }

        #endregion

        #region Methods

        //public async 

        private ToolbarItemType getCommandType()
        {
            return UserControls.ToolbarItemType.Button;
        }


        public int CompareTo(ICommandViewModel other)
        {
            if (other != null)
                return this.CommandModel.CompareTo(other.CommandModel);
            return 0;
        }

        public int CompareTo(object obj)
        {
            if (obj is ICommandViewModel)
                return CompareTo(obj as ICommandViewModel);
            return 0;
        }


        #endregion

        #region Data

        private ICommandModel _commandModel;
        private ICommand _command;
        private ObservableCollection<ICommandViewModel> _subCommands = new ObservableCollection<ICommandViewModel>();

        #endregion

        #region Public Properties

        public ICommandModel CommandModel { get { return _commandModel; } set { _commandModel = value; NotifyOfPropertyChange(() => CommandModel); } }

        public ICommand Command { get { return _command; } set { _command = value; NotifyOfPropertyChange(() => Command); } }

        public ToolbarItemType CommandType { get { return getCommandType(); } }

        public ObservableCollection<ICommandViewModel> SubCommands { get { return _subCommands; } }

        #endregion


        public object CommandParameter
        {
            get { throw new NotImplementedException(); }
        }

    }
}
