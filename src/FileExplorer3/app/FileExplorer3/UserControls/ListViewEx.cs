using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls
{
    public class ListViewEx : ListView, IVirtualListView
    {
        #region Cosntructor

        public ListViewEx()
        {

        }


        #endregion

        #region Methods


        private ListViewItemEx getSelectedContainer()
        {
            if (this.SelectedIndex != -1)
                return this.ItemContainerGenerator.ContainerFromIndex(this.SelectedIndex) as ListViewItemEx;
            return null;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.HandleScrollBarInvisible();
            this.AddHandler(ListViewEx.LoadedEvent, (RoutedEventHandler)((o, e) => OnLoaded()));
            
        }

        public void OnLoaded()
        {
            ListViewColumnUtils.RegisterFilterEvent(this);
            ListViewColumnUtils.UpdateFilterPanel(this, Columns, ColumnFilters);
            ColumnInfo sortColumn = Columns.Find(SortBy);
            if (sortColumn != null)
                ListViewColumnUtils.UpdateSortSymbol(this, sortColumn, SortDirection);
        }


        #region OnPropertyChanged

        public static void OnColumnsFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ListViewEx lv = (ListViewEx)sender;
            if (lv.ColumnFilters != null)
                ListViewColumnUtils.UpdateFilterPanel(lv, lv.Columns, lv.ColumnFilters);
        }

        public static void OnColumnsVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ListViewEx lv = (ListViewEx)sender;

            if (args.OldValue != args.NewValue)
                if ((Visibility)args.NewValue == Visibility.Visible)
                    lv.AddHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)lv.columnClickedHandler);
                else
                    lv.RemoveHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)lv.columnClickedHandler);
        }

        private void columnClickedHandler(object sender, RoutedEventArgs args)
        {
            if (!(args.OriginalSource is GridViewColumnHeader))
                return;
            GridViewColumnHeader header = (GridViewColumnHeader)args.OriginalSource;
            if (header.Column != null)
            {
                ColumnInfo sortColumn = Columns.Find((string)header.Column.Header);

                if (SortBy != sortColumn.Header && SortBy != sortColumn.ValuePath)
                    SetValue(SortByProperty, sortColumn.ValuePath);
                else
                    if (SortDirection == ListSortDirection.Ascending)
                        SetValue(SortDirectionProperty, ListSortDirection.Descending);
                    else SetValue(SortDirectionProperty, ListSortDirection.Ascending);
            }

            var fl = sender as ListViewEx;
        }

        public static void OnSelectionModeExChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ListViewEx fl = (ListViewEx)sender;
            SelectionModeEx selMode = (SelectionModeEx)args.NewValue;
            switch (selMode)
            {
                case SelectionModeEx.Single:
                case SelectionModeEx.Multiple:
                case SelectionModeEx.Extended:
                    SelectionHelper.SetEnableSelection(fl, false);
                    fl.SetValue(SelectionModeProperty, (SelectionMode)(int)selMode);
                    break;
                case SelectionModeEx.SelectionHelper:
                    SelectionHelper.SetEnableSelection(fl, true);
                    break;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListViewItemEx();
        }

        public static void OnViewModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ListViewEx fl = (ListViewEx)sender;

            ViewBase view = fl.View;
            if (args.Property.Equals(ViewModeProperty))
            {
                string viewMode = (string)args.NewValue;
                if (String.IsNullOrEmpty(viewMode))
                    return;
                string viewResourceName = viewMode + "View";
                view = (ViewBase)(fl.TryFindResource(viewResourceName));
                if (view != null)
                {
                    fl.View = view;
                }
                else Debug.WriteLine(String.Format("ListViewEx - {0} not found.", viewResourceName));
            }

            if (fl.View != null)
            {
                //Only update columns if View is updated
                if (args.Property.Equals(ColumnsProperty) || args.Property.Equals(ViewModeProperty))
                {
                    ListViewColumnUtils.UpdateColumn(fl, fl.Columns);
                    fl.Dispatcher.Invoke(() =>
                        {
                            ListViewColumnUtils.UpdateFilterPanel(fl, fl.Columns, fl.ColumnFilters);
                        },
                         System.Windows.Threading.DispatcherPriority.Input);
                }

                ////always update sort column
                ColumnInfo sortColumn = fl.Columns.Find(fl.SortBy);
                if (sortColumn != null)
                    ListViewColumnUtils.UpdateSortSymbol(fl, sortColumn, fl.SortDirection);
            }
            else Debug.WriteLine(String.Format("ListViewEx - No view defined."));
        }

        void IVirtualListView.UnselectAll()
        {
            this.RaiseEvent(new RoutedEventArgs(UnselectAllRequiredEvent, this));
            base.UnselectAll();
        }

        #endregion

        #endregion

        #region Data

        #endregion

        #region Public Properties


        #region ViewMode, ItemAnimationDuration, ItemSize property

        public static readonly DependencyProperty ViewModeProperty =
         DependencyProperty.Register("ViewMode", typeof(string), typeof(ListViewEx),
         new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnViewModeChanged)));

        /// <summary>
        /// Setting the view mode, e.g. SmallIcon, Icon and Grid.
        /// FileList try to find {ViewMode}View and apply it to the file list.
        /// </summary>
        public string ViewMode
        {
            get { return (string)GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        public static readonly DependencyProperty ItemAnimationDurationProperty =
         DependencyProperty.Register("ItemAnimationDuration", typeof(TimeSpan), typeof(ListViewEx),
         new FrameworkPropertyMetadata(TimeSpan.FromSeconds(0), new PropertyChangedCallback(OnViewModeChanged)));

        /// <summary>
        /// Time for an item to move when re-arrange item.
        /// </summary>
        public TimeSpan ItemAnimationDuration
        {
            get { return (TimeSpan)GetValue(ItemAnimationDurationProperty); }
            set { SetValue(ItemAnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty ItemSizeProperty =
            DependencyProperty.Register("ItemSize", typeof(double), typeof(ListViewEx),
            new FrameworkPropertyMetadata(60.0d));

        /// <summary>
        /// Applied to some view mode, let you specify the icon size.
        /// </summary>
        public double ItemSize
        {
            get { return (int)GetValue(ItemSizeProperty); }
            set { SetValue(ItemSizeProperty, value); }
        }


        #endregion

        #region Columns, and ColumnsVisibility property, FilterChanged event

        public static readonly DependencyProperty ColumnsProperty =
         DependencyProperty.Register("Columns", typeof(ColumnInfo[]), typeof(ListViewEx),
         new FrameworkPropertyMetadata(new ColumnInfo[] { }, new PropertyChangedCallback(OnViewModeChanged)));

        /// <summary>
        /// If the Panel is GridView, VirtualStack/WrapPanelView, one can change the column headers.
        /// </summary>
        public ColumnInfo[] Columns
        {
            get { return (ColumnInfo[])GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnFiltersProperty =
      DependencyProperty.Register("ColumnFilters", typeof(ColumnFilter[]), typeof(ListViewEx),
      new FrameworkPropertyMetadata(new ColumnFilter[] { }, new PropertyChangedCallback(OnColumnsFilterChanged)));

        /// <summary>
        /// Filters of column, display as dropdown box.
        /// </summary>
        public ColumnFilter[] ColumnFilters
        {
            get { return (ColumnFilter[])GetValue(ColumnFiltersProperty); }
            set { SetValue(ColumnFiltersProperty, value); }
        }

        public static readonly DependencyProperty ColumnsVisibilityProperty =
        DependencyProperty.Register("ColumnsVisibility", typeof(Visibility), typeof(ListViewEx),
            new FrameworkPropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(OnColumnsVisibilityChanged)));

        /// <summary>
        /// Whether the column header is visible, you may want to implement your own sorting mechanism.
        /// </summary>
        public Visibility ColumnsVisibility
        {
            get { return (Visibility)GetValue(ColumnsVisibilityProperty); }
            set { SetValue(ColumnsVisibilityProperty, value); }
        }

        public static readonly RoutedEvent FilterChangedEvent = EventManager.RegisterRoutedEvent("FilterChanged",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ListViewEx));

        public event RoutedEventHandler FilterChanged
        {
            add { AddHandler(FilterChangedEvent, value); }
            remove { RemoveHandler(FilterChangedEvent, value); }
        }

        public static readonly DependencyProperty ContentBelowHeaderProperty =
            DependencyProperty.Register("ContentBelowHeader", typeof(object), typeof(ListViewEx));
        public object ContentBelowHeader
        {
            get { return (object)GetValue(ContentBelowHeaderProperty); }
            set { SetValue(ContentBelowHeaderProperty, value); }
        }


        #endregion

        #region SortBy, SortDirection property

        public static readonly DependencyProperty SortByProperty =
        DependencyProperty.Register("SortBy", typeof(string), typeof(ListViewEx),
        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            new PropertyChangedCallback(OnViewModeChanged)));

        /// <summary>
        /// Specify which column to be labeled as sorted, 
        /// </summary>
        public string SortBy
        {
            get { return (string)GetValue(SortByProperty); }
            set { SetValue(SortByProperty, value); }
        }

        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register("SortDirection", typeof(ListSortDirection), typeof(ListViewEx),
            new FrameworkPropertyMetadata(ListSortDirection.Ascending, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnViewModeChanged)));

        /// <summary>
        /// Specify direction of sorting.
        /// </summary>
        public ListSortDirection SortDirection
        {
            get { return (ListSortDirection)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        #endregion

        public static readonly DependencyProperty IsCheckBoxVisibleProperty =
          DependencyProperty.Register("IsCheckBoxVisible", typeof(bool), typeof(ListViewEx),
          new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Whether show a checkbox symobl (symbol only) on ListViewItemEx.
        /// </summary>
        public bool IsCheckBoxVisible
        {
            get { return (bool)GetValue(IsCheckBoxVisibleProperty); }
            set { SetValue(IsCheckBoxVisibleProperty, value); }
        }


        #region SelectionMode property, UnselectAllRequired event.
        public static readonly DependencyProperty SelectionModeExProperty =
       DependencyProperty.Register("SelectionModeEx", typeof(SelectionModeEx), typeof(ListViewEx),
       new FrameworkPropertyMetadata(SelectionModeEx.Single, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
           new PropertyChangedCallback(OnSelectionModeExChanged)));

        /// <summary>
        /// Specify which column to be labeled as sorted, 
        /// </summary>
        public SelectionModeEx SelectionModeEx
        {
            get { return (SelectionModeEx)GetValue(SelectionModeExProperty); }
            set { SetValue(SelectionModeExProperty, value); }
        }


        public static readonly RoutedEvent UnselectAllRequiredEvent = EventManager.RegisterRoutedEvent("UnselectAllRequired",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ListViewEx));

        /// <summary>
        /// The ListView unable to deselect the items, request view model do the de-selection.
        /// </summary>
        public event RoutedEventHandler UnselectAllRequired
        {
            add { AddHandler(UnselectAllRequiredEvent, value); }
            remove { RemoveHandler(UnselectAllRequiredEvent, value); }
        }


        #endregion

        public static readonly DependencyProperty ColumnHeaderSortDirectionProperty =
            DependencyProperty.RegisterAttached("ColumnHeaderSortDirection", typeof(int), typeof(ListViewEx),
            new PropertyMetadata(0));

        public static void SetColumnHeaderSortDirection(DependencyObject obj, int value)
        {
            obj.SetValue(ColumnHeaderSortDirectionProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(GridViewColumnHeader))]
        public static int GetColumnHeaderSortDirection(DependencyObject obj)
        {
            return (int)obj.GetValue(ColumnHeaderSortDirectionProperty);
        }

        //public static readonly DependencyProperty ColumnHeaderValueProperty =
        //    DependencyProperty.RegisterAttached("ColumnHeaderValue", typeof(string), typeof(FileList),
        //    new PropertyMetadata(0));

        //public static void SetColumnHeaderValue(DependencyObject obj, int value)
        //{
        //    obj.SetValue(ColumnHeaderSortDirectionProperty, value);
        //}

        //[AttachedPropertyBrowsableForType(typeof(GridViewColumnHeader))]
        //public static string GetColumnHeaderValue(DependencyObject obj)
        //{
        //    return (string)obj.GetValue(ColumnHeaderSortDirectionProperty);
        //}

        //public static readonly DependencyProperty SelectedItemsProperty =
        //    DependencyProperty.Register("SelectedItems", typeof(IList), typeof(FileList));
        //public IList SelectedItems
        //{
        //    get { return (IList)GetValue(SelectedItemsProperty); }
        //    set { SetValue(SelectedItemsProperty, value); }
        //}


        #region Commands

        public ICommand RenameCommand
        {
            get { return (ICommand)GetValue(RenameCommandProperty); }
            set { SetValue(RenameCommandProperty, value); }
        }

        public static readonly DependencyProperty RenameCommandProperty =
            DependencyProperty.Register("RenameCommand", typeof(ICommand), typeof(ListViewItemEx), new UIPropertyMetadata(null));

        #endregion





        #endregion

    }


    public class ListViewItemEx : ListViewItem
    {
        #region Cosntructor

        public ListViewItemEx()
        {

        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public static readonly DependencyProperty IsCheckBoxVisibleProperty =
            ListViewEx.IsCheckBoxVisibleProperty.AddOwner(typeof(ListViewItemEx));

        /// <summary>
        /// Whether show a checkbox symobl (symbol only) on ListViewItemEx.
        /// </summary>
        public bool IsCheckBoxVisible
        {
            get { return (bool)GetValue(IsCheckBoxVisibleProperty); }
            set { SetValue(IsCheckBoxVisibleProperty, value); }
        }


        #endregion




    }
}
