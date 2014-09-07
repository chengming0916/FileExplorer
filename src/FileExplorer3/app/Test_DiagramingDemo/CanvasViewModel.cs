﻿using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.UIEventHub;
using FileExplorer.Script;
using FileExplorer;
using System.Collections;

namespace DiagramingDemo
{
    public class CanvasViewModel : NotifyPropertyChanged, ISupportDrag, IChildInfo
    {
        private ObservableCollection<ItemViewModel> _items  = new ObservableCollection<ItemViewModel>();

        public ObservableCollection<ItemViewModel> Items { get { return _items; } }
        public dynamic Commands { get; private set; } 

        public CanvasViewModel()
        {
            Commands = new DynamicRelayCommandDictionary()
            {
                ParameterDicConverter = ParameterDicConverters.FromParameterDic(
                new ParameterDic()
                {
                    { "CanvasVM", this }
                })
            };

            Commands.UnselectAll = new SimpleScriptCommand("UnselectAll",
                pd =>
                {
                    pd.GetValue<CanvasViewModel>("{CanvasVM}").UnselectAll();
                    return ResultCommand.NoError;
                });
            for (int i = 1; i < 20; i++)
                Items.Add(new ItemViewModel("Test" + i));            
        }

        public void UnselectAll()
        {
            foreach (var item in Items)
                item.IsSelected = false;
        }

        #region ISupportDrag
       

        public IEnumerable<IDraggable> GetDraggables()
        {
            return from i in Items where i.IsSelected select i;
        }

        public bool HasDraggables
        {
            get { return Items.Any(i => i.IsSelected); }
        }

        public void OnDragCompleted(IEnumerable<IDraggable> draggables, System.Windows.DragDropEffects effect)
        {
           
        }

        public System.Windows.DragDropEffects QueryDrag(IEnumerable<IDraggable> draggables)
        {
            return System.Windows.DragDropEffects.Move;
        }
        #endregion

        #region IChildInfo
        public System.Windows.Rect GetChildRect(int itemIndex)
        {
            var item = Items[itemIndex];
            return new System.Windows.Rect(item.Position.X, item.Position.Y, 50, 50);
        }
        #endregion 
    }
}
