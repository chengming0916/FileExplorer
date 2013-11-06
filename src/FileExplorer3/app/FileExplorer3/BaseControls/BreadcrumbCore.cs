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
using System.Collections;

namespace FileExplorer.BaseControls
{
    public interface IHierarchyHelper
    {
        /// <summary>
        /// Used to generate ItemsSource for BreadcrumbCore.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        IEnumerable<object> GetHierarchy(object item, bool includeCurrent);

        /// <summary>
        /// Generate Path from an item;
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        string GetPath(object item);

        /// <summary>
        /// Get Item from path.
        /// </summary>
        /// <param name="rootItem">RootItem or ItemSource which can be used to lookup from.</param>
        /// <param name="path"></param>
        /// <returns></returns>
        object GetItem(object rootItem, string path);

        IEnumerable List(object item);

        string ExtractPath(string pathName);

        string ExtractName(string pathName);

        char Separator { get; }
        StringComparison StringComparisonOption { get; }
    }

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
            AddHandler(BreadcrumbItem.SelectedEvent, (RoutedEventHandler)((o, e) =>
            {                
                SelectedItem = e.OriginalSource as BreadcrumbItem;
                if (SelectedItem is BreadcrumbItem)
                    SelectedValue = (SelectedItem as BreadcrumbItem).DataContext;
                RaiseEvent(new RoutedEventArgs(SelectedValueChangedEvent));
            }));
        }

        #endregion

        #region Methods        

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbItem() { HeaderTemplate = this.HeaderTemplate, IconTemplate = this.IconTemplate, IsTopLevel=true };
        }

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

        public static readonly RoutedEvent SelectedValueChangedEvent = EventManager.RegisterRoutedEvent("SelectedValueChanged",
          RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbCore));

        public event RoutedEventHandler SelectedValueChanged
        {
            add { AddHandler(SelectedValueChangedEvent, value); }
            remove { RemoveHandler(SelectedValueChangedEvent, value); }
        }               

        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
          typeof(Object), typeof(BreadcrumbCore), new PropertyMetadata(null));

        public Object SelectedItem
        {
            get { return (Object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue",
         typeof(Object), typeof(BreadcrumbCore), new PropertyMetadata(null));

        public Object SelectedValue
        {
            get { return (Object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

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

        public static readonly DependencyProperty HeaderTemplateProperty = HeaderedItemsControl.HeaderTemplateProperty.AddOwner(typeof(BreadcrumbCore));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty IconTemplateProperty = 
            DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(BreadcrumbCore));

        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }
        #endregion
    }
}
