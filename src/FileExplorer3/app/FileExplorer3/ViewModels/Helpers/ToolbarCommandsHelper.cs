using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels.Helpers
{
    /// <summary>
    /// Populate Commands (CommandViewModel, e.g. ToggleView), and notify the commands when AppliedModels changes.
    /// For use in toolbar and context menu.
    /// </summary>
    public interface IToolbarCommandsHelper : IExportCommandBindings
    {
        IProfile[] RootProfiles { set; }

        IEntryModel[] AppliedModels { get; }
        IEntriesHelper<ICommandViewModel> CommandModels { get; }

        ICommandProvider[] ExtraCommandProviders { get; set; }
    }

    public class ToolbarCommandsHelper : NotifyPropertyChanged, IToolbarCommandsHelper,
        IHandle<SelectionChangedEvent>, IHandle<DirectoryChangedEvent>
    {
        #region Constructor

        protected ToolbarCommandsHelper(IProfile[] rootProfiles)
        {
            //This doesn't work.
            CommandModels = new EntriesHelper<ICommandViewModel>(loadCommandsTask);
        }

        public ToolbarCommandsHelper(
            IEventAggregator events,
            Func<DirectoryChangedEvent, IEntryModel[]> whenDirectoryChanged = null,
            Func<SelectionChangedEvent, IEntryModel[]> whenSelectionChanged = null,
            IProfile[] rootProfiles = null, params ICommandProvider[] extraCommandProviders)
        {
            _extraCommandProviders = extraCommandProviders;

            _whenDirectoryChanged = whenDirectoryChanged;
            _whenSelectionChanged = whenSelectionChanged;

            if (events != null)
                if (_whenDirectoryChanged != null || _whenSelectionChanged != null)
                    events.Subscribe(this);

            _rootProfiles = rootProfiles;
            CommandModels = new EntriesHelper<ICommandViewModel>(loadCommandsTask);
        }

        #endregion

        #region Methods

        protected override void NotifyOfPropertyChanged(string propertyName)
        {
            base.NotifyOfPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "ExtraCommandProviders":
                case "AppliedModels":
                    if (propertyName == "ExtraCommandProviders")
                        CommandModels.IsLoaded = false; //Reset CommandModels
                    AsyncUtils.RunSync(() => CommandModels.LoadAsync(propertyName == "ExtraCommandProviders"));
                    if (AppliedModels != null)
                        foreach (var commandVM in CommandModels.AllNonBindable)
                            commandVM.CommandModel.NotifySelectionChanged(AppliedModels);
                    break;
            }
        }

        async Task<IEnumerable<ICommandViewModel>> loadCommandsTask(bool refresh)
        {
            List<ICommandModel> cmList = new List<ICommandModel>() { };

            foreach (var cp in _extraCommandProviders)
                cmList.AddRange(cp.GetCommandModels());

            if (_rootProfiles != null)
                foreach (var cp in _rootProfiles.SelectMany(p => p.CommandProviders))
                    cmList.AddRange(cp.GetCommandModels());

            return cmList.Select(cm => new CommandViewModel(cm)).ToArray();
        }

        public void Handle(SelectionChangedEvent message)
        {
            if (_whenSelectionChanged != null)
                AppliedModels = _whenSelectionChanged(message);
        }

        public void Handle(DirectoryChangedEvent message)
        {
            if (_whenDirectoryChanged != null)
                AppliedModels = _whenDirectoryChanged(message);
        }


        #endregion

        #region Data

        IEntryModel[] _appliedModels = null;
        IProfile[] _rootProfiles = null;
        private ICommandProvider[] _extraCommandProviders;
        protected List<IScriptCommandBinding> _exportedCommandBindings = new List<IScriptCommandBinding>(); //Remove
        private Func<DirectoryChangedEvent, IEntryModel[]> _whenDirectoryChanged;
        private Func<SelectionChangedEvent, IEntryModel[]> _whenSelectionChanged;

        #endregion

        #region Public Properties

        public IProfile[] RootProfiles { set { _rootProfiles = value; } }

        public ICommandProvider[] ExtraCommandProviders
        {
            get { return _extraCommandProviders; }
            set { _extraCommandProviders = value; NotifyOfPropertyChanged(() => ExtraCommandProviders); }
        }

        public IEntryModel[] AppliedModels
        {
            get { return _appliedModels; }
            set { _appliedModels = value; NotifyOfPropertyChanged(() => AppliedModels); }
        }
        public IEntriesHelper<ICommandViewModel> CommandModels { get; private set; }

        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings //Remove
        {
            get { return _exportedCommandBindings; }
        }

        #endregion









    }
}
