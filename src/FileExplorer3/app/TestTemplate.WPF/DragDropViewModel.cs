using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Cinch;
using FileExplorer.UserControls;
using FileExplorer.ViewModels.Helpers;

namespace TestTemplate.WPF
{
    public class DragDropViewModel : NotifyPropertyChanged, ISupportDrag, ISupportDrop
    {
        #region Constructor

        public DragDropViewModel(int startId, int count)
        {
            for (int i = startId; i < startId + count; i++)
                _items.Add(new DragDropItemViewModel(i));

            UnselectAllCommand = new SimpleCommand() { ExecuteDelegate = (param) =>
                {
                    foreach (var item in Items)
                        item.IsSelected = false;
                }};
        }

        #endregion

        #region Methods

        

        public bool HasDraggables
        {
            get { throw new NotImplementedException(); }
        }

        public Task<IDraggable> GetDraggables()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data

        private ObservableCollection<DragDropItemViewModel> _items = new ObservableCollection<DragDropItemViewModel>();

        #endregion

        #region Public Properties

        public ICommand UnselectAllCommand { get; set; }
        public ObservableCollection<DragDropItemViewModel> Items { get { return _items; } }

        #endregion





        public DragDropEffects QueryDrop(IDraggable draggable)
        {
            return DragDropEffects.Move;
        }

        public bool Drop(IDraggable draggable)
        {
            throw new NotImplementedException();
        }

    }

    public class DragDropItemViewModel : NotifyPropertyChanged, IDraggable
    {
        #region Constructor

        public DragDropItemViewModel(int value)
        {
            Value = value;
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private bool _isSelected = false, _isSelecting = false;

        #endregion

        #region Public Properties

        public int Value { get; set; }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyOfPropertyChanged(() => IsSelected); }
        }
        public bool IsSelecting
        {
            get { return _isSelecting; }
            set { _isSelecting = value; NotifyOfPropertyChanged(() => IsSelecting); }
        }
        #endregion


        public DragDropEffects SupportedEffects
        {
            get { return DragDropEffects.Move; }
        }

        public DataObject GetDataObject()
        {
            throw new NotImplementedException();
        }
    }
}
