using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.UserControls;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class BreadcrumbViewModel : ViewAware, IBreadcrumbViewModel,
        IHandle<DirectoryChangedEvent>
    {
        #region Constructor

        public BreadcrumbViewModel(IEventAggregator events, 
            IEntryModel[] rootModels)
        {
            _profiles = rootModels.Select(rm => rm.Profile).Distinct();
            _events = events;

            if (events != null)
                events.Subscribe(this);

            Entries = new EntriesHelper<IBreadcrumbItemViewModel>();
            var selection = new TreeRootSelector<IBreadcrumbItemViewModel, IEntryModel>(Entries,
                _profiles.First().HierarchyComparer.CompareHierarchy);
            selection.SelectionChanged += (o, e) =>
                {
                    BroadcastDirectoryChanged(EntryViewModel.FromEntryModel(selection.SelectedValue));
                };
            Selection = selection;

            Entries.SetEntries(rootModels
                .Select(r => new BreadcrumbItemViewModel(events, this, r, null)).ToArray());


            SuggestSources = _profiles.Select(p => p.GetSuggestSource());

         
        }

        #endregion

        #region Methods

        protected void BroadcastDirectoryChanged(IEntryViewModel viewModel)
        {
            _events.Publish(new SelectionChangedEvent(this, new IEntryViewModel[] { viewModel }));
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);

            _sbox = (view as UserControl).FindName("sbox") as SuggestBoxBase;
            _switch = (view as UserControl).FindName("switch") as FileExplorer.BaseControls.Switch;


            _switch.AddValueChanged(FileExplorer.BaseControls.Switch.IsSwitchOnProperty, (o, e) =>
                {
                    if (!_switch.IsSwitchOn)
                    {
                        _sbox.Dispatcher.BeginInvoke(new System.Action(() =>
                            {
                                Keyboard.Focus(_sbox);
                                _sbox.Focus();
                                _sbox.SelectAll();
                            }), System.Windows.Threading.DispatcherPriority.Background);

                    }
                });

            if (Entries.AllNonBindable.Count() > 0)
                SelectAsync(Entries.AllNonBindable.First().Selection.Value);
        }


        public async Task SelectAsync(IEntryModel value)
        {
            _sbox.SetValue(SuggestBoxBase.TextProperty, value.FullPath);

            await Selection.LookupAsync(value,
                RecrusiveSearch<IBreadcrumbItemViewModel, IEntryModel>.LoadSubentriesIfNotLoaded,
                SetSelected<IBreadcrumbItemViewModel, IEntryModel>.WhenSelected,
                SetChildSelected<IBreadcrumbItemViewModel, IEntryModel>.ToSelectedChild);
        }

        void OnSuggestPathChanged()
        {
            if (!ShowBreadcrumb)
            {
                Task.Run(async () =>
                    {
                        foreach (var p in _profiles)
                        {
                            var found = await p.ParseAsync(SuggestedPath);
                            if (found != null)
                            {
                                _sbox.Dispatcher.BeginInvoke(new System.Action(() => { SelectAsync(found); }));
                                ShowBreadcrumb = true;
                                BroadcastDirectoryChanged(EntryViewModel.FromEntryModel(found));
                            }
                            //else not found
                        }
                    }).Start();
            }
        }

        public void Handle(DirectoryChangedEvent message)
        {
            if (message.NewModel != null)
            {
                SelectAsync(message.NewModel);
            }
        }

        #endregion

        #region Data

        private IEnumerable<IProfile> _profiles;
        private string _suggestedPath;
        private bool _showBreadcrumb = true;
        private IEnumerable<ISuggestSource> _suggestSources;
        private IEventAggregator _events;
        private FileExplorer.BaseControls.Switch _switch;
        private SuggestBoxBase _sbox;
        //private bool _updatingSuggestBox = false;

        #endregion

        #region Public Properties

        public ITreeSelector<IBreadcrumbItemViewModel, IEntryModel> Selection { get; set; }
        public IEntriesHelper<IBreadcrumbItemViewModel> Entries { get; set; }


        public bool ShowBreadcrumb
        {
            get { return _showBreadcrumb; }
            set { _showBreadcrumb = value; NotifyOfPropertyChange(() => ShowBreadcrumb); }
        }

        public IEnumerable<ISuggestSource> SuggestSources
        {
            get { return _suggestSources; }
            set { _suggestSources = value; NotifyOfPropertyChange(() => SuggestSources); }
        }

        public string SuggestedPath
        {
            get { return _suggestedPath; }
            set
            {
                _suggestedPath = value;
                NotifyOfPropertyChange(() => SuggestedPath);
                OnSuggestPathChanged();
            }
        }

        #endregion


    }
}
