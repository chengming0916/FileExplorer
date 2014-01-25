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
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

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
                if (commandModel.RoutedCommand != null)
                    CommandBinding = ScriptCommandBinding.ForRoutedUICommand(commandModel.RoutedCommand);
                else
                    CommandBinding = ScriptCommandBinding.FromScriptCommand(
                        ApplicationCommands.NotACommand, commandModel, cm => cm.Command,
                        ParameterDicConverters.ConvertUIParameter, ScriptBindingScope.Local);

            CommandModel.PropertyChanged += (o, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case "IsChecked":
                        case "HeaderImage":
                        case "HeaderIcon":
                            RefreshIcon();
                            break;
                        case "SubCommands":
                            SubCommands.LoadAsync(true);
                            RefreshIcon();
                            break;
                    }
                };

            RefreshIcon();

            if (commandModel is IDirectoryCommandModel)
            {
                IDirectoryCommandModel directoryModel = CommandModel as IDirectoryCommandModel;
                SubCommands = new EntriesHelper<ICommandViewModel>(
                    (cts) => Task.Run<IEnumerable<ICommandViewModel>>(
                        () => directoryModel.SubCommands.Select(c => (ICommandViewModel)new CommandViewModel(c, this))));
                SubCommands.LoadAsync(false);
            }
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
        private IScriptCommandBinding _commandBinding;
        private bool _subCommandsLoaded = false;
        private EntriesHelper<ICommandViewModel> _subCommands;
        private ICommandViewModel _parentCommandViewModel;

        #endregion

        #region Public Properties

        public IEntriesHelper<ICommandViewModel> SubCommands { get; private set; }

        public ICommandModel CommandModel { get { return _commandModel; } set { _commandModel = value; NotifyOfPropertyChange(() => CommandModel); } }

        public IScriptCommandBinding CommandBinding { get { return _commandBinding; } set { _commandBinding = value; NotifyOfPropertyChange(() => CommandBinding); } }

        public ToolbarItemType CommandType { get { return getCommandType(); } }

        public Object Icon
        {
            get
            {
                return _icon is System.Windows.Controls.Image ?
                    new System.Windows.Controls.Image() { Source = (_icon as System.Windows.Controls.Image).Source } : null;
                ;

            }
            set { _icon = value; NotifyOfPropertyChange(() => Icon); }
        }

        public bool IsVisibleInContextMenu { get { return CommandType != ToolbarItemType.Separator; } }

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
