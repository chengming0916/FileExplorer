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
    public class DragDropItemViewModel : NotifyPropertyChanged, ISupportDrag, ISupportDrop, IDraggable
    {
        public static string Format_DragDropItem = "DragDropItemVM";
        #region Constructor

        public DragDropItemViewModel(int value, bool isDroppable, bool isChildDroppable)
        {
            IsDroppable = isDroppable;
            IsChildDroppable = isChildDroppable;
            Value = value;
            UnselectAllCommand = new SimpleCommand()
            {
                ExecuteDelegate = (param) =>
                {
                    foreach (var item in Items)
                        item.IsSelected = false;
                }
            };
        }


        public DragDropItemViewModel(int startId, int count, bool isDroppable, bool isChildDroppable)
            : this(-1, isDroppable, isChildDroppable)
        {
            for (int i = startId; i < startId + count; i++)
                _items.Add(new DragDropItemViewModel(i, isChildDroppable, isChildDroppable));


        }

        #endregion

        #region Methods



        public bool HasDraggables
        {
            get { return GetDraggables().Any(); }
        }

        public bool IsDroppable
        {
            get;
            set;
        }





        public bool IsChildDroppable { get; set; }

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
                foreach (var item in Items.Where(i => i.IsSelected).ToList())
                    Items.Remove(item);
            }
        }


        public DragDropEffects QueryDrop(IDataObject da)
        {
            if (da.GetDataPresent(Format_DragDropItem))
            {
                var data = da.GetData(Format_DragDropItem) as int[];
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == this.Value)
                        return DragDropEffects.None;
                return DragDropEffects.Move;
            }
            return DragDropEffects.None;
        }

        public IEnumerable<IDraggable> QueryDropDraggables(IDataObject da)
        {
            if (da.GetDataPresent(Format_DragDropItem))
            {
                var data = da.GetData(Format_DragDropItem) as int[];
                for (int i = 0; i < data.Length; i++)
                    yield return new DragDropItemViewModel(data[i], IsChildDroppable, IsChildDroppable);
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
                Items.Insert(i, new DragDropItemViewModel(data[i], IsChildDroppable, IsChildDroppable));

            return DragDropEffects.Move;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        #region Data

        private bool _isSelected = false;
        private ObservableCollection<DragDropItemViewModel> _items = new ObservableCollection<DragDropItemViewModel>();

        #endregion

        #region Public Properties

        public ICommand UnselectAllCommand { get; set; }
        public ObservableCollection<DragDropItemViewModel> Items { get { return _items; } }

        public int Value { get; set; }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyOfPropertyChanged(() => IsSelected); }
        }

        #endregion





    }



}
