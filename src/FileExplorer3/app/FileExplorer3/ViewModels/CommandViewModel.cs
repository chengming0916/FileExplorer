using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Cinch;
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.Models;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    public class CommandViewModel : PropertyChangedBase, ICommandViewModel
    {
        #region Constructor

        public CommandViewModel(ICommandModel commandModel, ICommandViewModel parentCommandViewModel = null)
        {
            CommandModel = commandModel;
            _parentCommandViewModel = parentCommandViewModel;

            if (CommandModel != null)
                Command = new SimpleCommand()
                {
                    CanExecuteDelegate = p => CommandModel.Command == null || CommandModel.Command.CanExecute(
                        ParameterDic.FromParameterPair(new ParameterPair("Parameter", p))),
                    ExecuteDelegate = p => 
                    {
                        if (CommandModel.Command != null)
                            new ScriptRunner().Run(CommandModel.Command,
                                    ParameterDic.FromParameterPair(new ParameterPair("Parameter", p)));
                    }

                };

            if (CommandModel is IDirectoryCommandModel)
            {
                foreach (var c in (CommandModel as IDirectoryCommandModel).SubCommands)
                {
                    SubCommands.Add(new CommandViewModel(c, this));
                }
            }

            CommandModel.PropertyChanged += (o, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case "IsChecked": 
                        case "HeaderImage" : 
                        case "HeaderIcon" :
                            RefreshIcon();
                            break;
                    }
                };

            RefreshIcon();
        }

        #endregion

        #region Methods

        public void RefreshIcon()
        {
            if (CommandModel.IsChecked)
                Icon = null;            
            if (CommandModel.HeaderImageFunc != null)
                Icon = new System.Windows.Controls.Image() { Source = CommandModel.HeaderImageFunc(CommandModel) };
            if (CommandModel.HeaderIcon != null)
                Icon = new System.Windows.Controls.Image() { Source = BitmapSourceUtils.CreateBitmapSourceFromBitmap(CommandModel.HeaderIcon) };
        }

        private ToolbarItemType getCommandType()
        {
            if (CommandModel is ISeparatorModel)
                return ToolbarItemType.Separator;

            if (CommandModel is ISelectorCommandModel)
                return (CommandModel as ISelectorCommandModel).IsComboBox ? ToolbarItemType.Combo : ToolbarItemType.Check;

            if (_parentCommandViewModel != null &&
                (_parentCommandViewModel.CommandType == ToolbarItemType.Combo || _parentCommandViewModel.CommandType == ToolbarItemType.Check)
                )
                return (CommandModel as ISelectorCommandModel).IsComboBox ? ToolbarItemType.Combo : ToolbarItemType.Check;

            if (CommandModel is ISliderCommandModel)
                return ToolbarItemType.MenuButton;

            if (CommandModel is IDirectoryCommandModel)
                return ToolbarItemType.MenuButton;

            return ToolbarItemType.Button;
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

        private object _icon = null;
        private ICommandModel _commandModel;
        private ICommand _command;
        private ObservableCollection<ICommandViewModel> _subCommands = new ObservableCollection<ICommandViewModel>();
        private ICommandViewModel _parentCommandViewModel;

        #endregion

        #region Public Properties

        public ICommandModel CommandModel { get { return _commandModel; } set { _commandModel = value; NotifyOfPropertyChange(() => CommandModel); } }        

        public ICommand Command { get { return _command; } set { _command = value; NotifyOfPropertyChange(() => Command); } }

        public ToolbarItemType CommandType { get { return getCommandType(); } }

        public Object Icon { get { return _icon; } set { _icon = value; NotifyOfPropertyChange(() => Icon); } }

        public ObservableCollection<ICommandViewModel> SubCommands { get { return _subCommands; } }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return CommandModel is ISliderStepCommandModel ? (CommandModel as ISliderStepCommandModel).VerticalAlignment :
                    VerticalAlignment.Center;
            }
        }

        public bool IsSliderEnabled { get { return _commandModel is ISliderCommandModel; } }
        public bool IsSliderStep
        {
            get
            {
                return _parentCommandViewModel != null &&
                    _parentCommandViewModel.CommandModel is ISliderCommandModel &&
                    CommandType != ToolbarItemType.Separator;
            }
        }

        #endregion


        public object CommandParameter
        {
            get { throw new NotImplementedException(); }
        }

    }
}
