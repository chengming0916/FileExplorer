///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// LYCJ (c) 2010  - http://www.quickzip.org/components                                                            //
// Release under MIT license.                                                                                   //
//                                                                                                               //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;

namespace FileExplorer.UserControls
{
    public class BreadcrumbCore : ItemsControl
    {
        #region Constructor

        static BreadcrumbCore()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbCore),
                new FrameworkPropertyMetadata(typeof(BreadcrumbCore)));
        }

        public BreadcrumbCore()
        {
            
        }

        #endregion

        #region Methods        

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbItem() { HeaderTemplate = this.HeaderTemplate };
        }

        private void updateToggle()
        {
            if (this.Items.Count > 0)
            {
                BreadcrumbItem firstItem = this.ItemContainerGenerator.ContainerFromIndex(0) as BreadcrumbItem;
                if (firstItem != null)
                    firstItem.ShowCaption = this.Items.Count == 1;
            }
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            //updateToggle();            

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //updateToggle();
        }


        //public BreadcrumbItem AddItem(object item)
        //{
        //    Items.Add(item);
        //    return this.ItemContainerGenerator.ContainerFromItem(item) as BreadcrumbItem;
        //}

        //public BreadcrumbItem AddItem(object parentItem, object item)
        //{
        //    int idx = Items.IndexOf(parentItem);
        //    Debug.Assert(idx != -1);
        //    ClearAfterIndex(idx);
        //    return AddItem(item);
        //}


        //public void ClearAll()
        //{
        //    Items.Clear();
        //}

        //public void ClearAfterIndex(int index)
        //{
        //    for (int i = Items.Count - 1; i >= index; i--)
        //    {
        //        Items.RemoveAt(i);
        //    }
        //}


        //public BreadcrumbItem GetBreadcrumbItem(int index)
        //{
        //    return Items.Count > index ? 
        //        ItemContainerGenerator.ContainerFromIndex(index) as BreadcrumbItem : null;
        //}


        public static void RootChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //BreadcrumbCore bcv = (BreadcrumbCore)obj;
            //bcv.RootItem.DataContext = e.NewValue;    
            
        }

        //public object getCurrent()
        //{
        //    if (Items.Count == 0)
        //        return Root;
        //    else return Items[Items.Count - 1];
        //}

        public static void OnLastNonVisibleIndexChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            BreadcrumbCore bcore = sender as BreadcrumbCore;

            List<Object> overflowedItems = new List<object>();
            for (int i = 0; i < Math.Min((int)args.NewValue + 1, bcore.Items.Count) ; i++)
            {
                overflowedItems.Add(bcore.Items[i]);
            }

            bcore.SetValue(OverflowedItemsProperty, overflowedItems);
        }

        #endregion

        #region Dependency properties

        //public object Current
        //{
        //    get { return getCurrent(); }
        //}
        public static DependencyProperty OverflowedItemsProperty = DependencyProperty.Register("OverflowedItems",
           typeof(IList<Object>), typeof(BreadcrumbCore), new PropertyMetadata(null));

        public IList<Object> OverflowedItems
        {
            get { return (IList<Object>)GetValue(OverflowedItemsProperty); }
            set { SetValue(OverflowedItemsProperty, value); }
        }

        public static DependencyProperty LastNonVisibleIndexProperty = DependencyProperty.Register("LastNonVisibleIndex",
           typeof(int), typeof(BreadcrumbCore), new PropertyMetadata(0, OnLastNonVisibleIndexChanged));

        public int LastNonVisible
        {
            get { return (int)GetValue(LastNonVisibleIndexProperty); }
            set { SetValue(LastNonVisibleIndexProperty, value); }
        }

        public static readonly DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof(object), typeof(BreadcrumbCore),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(RootChanged)));

        public object Root
        {
            get { return GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }


        public static readonly DependencyProperty HeaderTemplateProperty = HeaderedItemsControl.HeaderTemplateProperty.AddOwner(typeof(BreadcrumbCore));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }
        #endregion
    }
}
