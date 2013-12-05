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
        public static string Format_DragDropItem = "DragDropItemVM";
        #region Constructor

        public DragDropViewModel(int startId, int count)
        {
            for (int i = startId; i < startId + count; i++)
                _items.Add(new DragDropItemViewModel(i));

            UnselectAllCommand = new SimpleCommand()
            {
                ExecuteDelegate = (param) =>
                    {
                        foreach (var item in Items)
                            item.IsSelected = false;
                    }
            };
        }

        #endregion

        #region Methods



        public bool HasDraggables
        {
            get { return GetDraggables().Any(); }
        }

        public IEnumerable<IDraggable> GetDraggables()
        {
            return Items.Where(i => i.IsSelected).Cast<IDraggable>();
        }

        public Tuple<IDataObject, DragDropEffects> GetDataObject()
        {
            DataObject da = new DataObject(Format_DragDropItem,
                (from i in Items where i.IsSelected select i.Value).ToArray());

            return new Tuple<IDataObject, DragDropEffects>(da, DragDropEffects.Move);
        }


        public void OnDataObjectDropped(IDataObject da, DragDropEffects effect)
        {
            if (effect == DragDropEffects.Move)
            {
                for (int i = Items.Count() - 1; i >= 0; i--)
                    if (Items[i].IsSelected)
                        Items.RemoveAt(i);
            }
        }


        public DragDropEffects QueryDrop(IDataObject da)
        {
            if (da.GetDataPresent(Format_DragDropItem))
                return DragDropEffects.Move;
            return DragDropEffects.None;
        }

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            if (da.GetDataPresent(Format_DragDropItem))
            {
                var data = da.GetData(Format_DragDropItem) as int[];
                for (int i = 0; i < data.Length; i++)
                    yield return new DragDropItemViewModel(data[i]);
            }
        }

        public DragDropEffects Drop(IDataObject da, DragDropEffects allowedEffects)
        {
            if (!(allowedEffects.HasFlag(DragDropEffects.Move)))
                return DragDropEffects.None;

            if (!(da.GetDataPresent(Format_DragDropItem)))
                return DragDropEffects.None;

            var data = da.GetData(Format_DragDropItem) as int[];
            for (int i = 0; i < data.Length; i++)
                Items.Insert(i, new DragDropItemViewModel(data[i]));

            return DragDropEffects.Move;
        }


        #endregion

        #region Data

        private ObservableCollection<DragDropItemViewModel> _items = new ObservableCollection<DragDropItemViewModel>();

        #endregion

        #region Public Properties

        public ICommand UnselectAllCommand { get; set; }
        public ObservableCollection<DragDropItemViewModel> Items { get { return _items; } }

        #endregion






        public DragDropEffects GetSupportedEffects
        {
            get { throw new NotImplementedException(); }
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

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        #region Data

        private bool _isSelected = false;

        #endregion

        #region Public Properties

        public int Value { get; set; }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyOfPropertyChanged(() => IsSelected); }
        }

        #endregion
    }
}
