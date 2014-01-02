﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cinch;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels.Helpers
{
    public class ListSelector<VM, T> : NotifyPropertyChanged, IListSelector<VM, T>
        where VM : IEntryViewModel
    {
        #region Constructor

        public ListSelector(IEntriesHelper<VM> entryHelper)
        {
            EntryHelper = entryHelper;
            EntryHelper.EntriesChanged += (o, e) => { notifySelectionChanged(); };
            
            UnselectAllCommand = new SimpleCommand() { UICommand = FileListCommands.UnselectAll, ExecuteDelegate = (param) => UnselectAll() };
            SelectAllCommand = new SimpleCommand() { UICommand = ApplicationCommands.SelectAll, ExecuteDelegate = (param) => SelectAll() };
        }

        #endregion

        #region Methods

        private void notifySelectionChanged()
        {
            NotifyOfPropertyChanged(() => SelectedItems);
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        public void UnselectAll()
        {
            foreach (var e in EntryHelper.AllNonBindable.ToList())
                e.IsSelected = false;
            notifySelectionChanged();
        }

        public void SelectAll()
        {
            if (SelectedItems.Count() == EntryHelper.AllNonBindable.Count())
                UnselectAll();
            else
            {
                foreach (var e in EntryHelper.AllNonBindable.ToList())
                    e.IsSelected = true;
                notifySelectionChanged();
            }
        }

        public void ReportChildSelected(VM viewModel)
        {
            notifySelectionChanged();
        }

        public void ReportChildUnSelected(VM viewModel)
        {
            notifySelectionChanged();
        }

        public IEnumerable<VM> getSelectedItems()
        {
            if (EntryHelper.AllNonBindable != null)
                foreach (var item in EntryHelper.AllNonBindable.ToList())
                    if (item.IsSelected)
                        yield return item;
        }

        public void OnSelectionChanged(IList selectedItems)
        {
            //SelectedItems = selectedItems.Cast<VM>().Distinct().ToList();
        }


        #endregion

        #region Data

        IList<VM> _selectedVms;

        #endregion

        #region Public Properties

        public IEntriesHelper<VM> EntryHelper { get; private set; }

        public IList<VM> SelectedItems
        {
            get { return getSelectedItems().ToList(); }
            //get { return _selectedVms; }
            //set
            //{
            //    _selectedVms = value;
            //    NotifyOfPropertyChanged(() => SelectedItems);
            //}
        }


        public event EventHandler SelectionChanged;

        public ICommand UnselectAllCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }

        public IEnumerable<ICommand> ExportedCommands
        {
            get { return new [] { UnselectAllCommand, SelectAllCommand }; }
        }

        #endregion




        
    }
}
