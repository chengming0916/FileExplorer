using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FileExplorer.UserControls
{
    #region Enums and Interfaces

    public enum ViewMode : int
    {
        vmTile = 13,
        vmGrid = 14,
        vmList = 15,
        vmSmallIcon = 16,
        vmIcon = 48,
        vmLargeIcon = 80,
        vmExtraLargeIcon = 120,
        vmViewer = 256
    }

    #endregion

    #region Classes

    internal class fileListItemStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FileList flist = UITools.FindAncestor<FileList>(container);
            if (flist != null)
            {
                if (!(flist.View is GridView))
                    return (Style)flist.FindResource(FileList.GridFileListItem_Style);
            }
            return base.SelectStyle(item, container);
        }
    }

    #endregion

    /// <summary>
    /// This is Refactored version of FileList2.
    /// </summary>
    public class FileList : ListView, IVirtualListView
    {
        public static string GridFileListItem_Style = "FileListItem_Style";        
        public static string DefaultHeader_Template = "NormHeaderTemplate";
        public static string AscendingHeader_Template = "AscHeaderTemplate";
        public static string DecendingHeader_Template = "DecHeaderTemplate";
       
        public static string PART_ListView = "PART_ListView";
        public static Dictionary<ViewMode, string> View_Template = new Dictionary<ViewMode, string>()
        {
            { ViewMode.vmTile, "TileView"},
            { ViewMode.vmGrid, "GridView"},
            { ViewMode.vmList, "ListView"},
            { ViewMode.vmSmallIcon, "SmallIconView"},
            { ViewMode.vmIcon , "IconView" },
            { ViewMode.vmLargeIcon , "IconView" },
            { ViewMode.vmExtraLargeIcon , "IconView" },
            { ViewMode.vmViewer , "ViewerView" },

        };

        #region Cosntructor


        public FileList()
        {
            this.ItemContainerStyleSelector = new fileListItemStyleSelector();

            this.AddHandler(FileList.SelectionChangedEvent, (RoutedEventHandler)((o, e) =>
            {
                BindableSelectedItems = SelectedItems;

            }));

            this.AddHandler(UIElement.KeyDownEvent, (KeyEventHandler)delegate(object sender, KeyEventArgs args)
            {
                switch (args.Key)
                {
                    case Key.F2:
                        if (RenameCommand != null && RenameCommand.CanExecute(DataContext))
                            RenameCommand.Execute(DataContext);
                        break;
                }

            });


            this.AddHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)delegate(object sender, RoutedEventArgs args)
            {
                if (args.OriginalSource is GridViewColumnHeader)
                {
                    GridViewColumnHeader header = (GridViewColumnHeader)args.OriginalSource;
                    if (header.Column != null)
                    {
                        string columnName = GetSortPropertyName(header.Column);

                        //if (string.IsNullOrEmpty(columnName))
                        //    return;

                        if (SortBy != columnName)
                            SetValue(SortByProperty, columnName);
                        else
                            SortAscending = !SortAscending;

                        UpdateCollumnHeader();
                    }
                }
            });
        }

        #endregion

        #region Methods


        protected override DependencyObject GetContainerForItemOverride()
        {
            ListViewItem item = new ListViewItem();

            if (ViewMode == ViewMode.vmGrid)
                item.Style = (Style)this.FindResource(GridFileListItem_Style);
            return item;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SelectionHelper.SetEnableSelection(this, true);

            UpdateCollumnHeader();


            #region Update scroll position if scroll bar disappear
            ScrollViewer scrollView = UITools.FindVisualChild<ScrollViewer>(this);

            DependencyPropertyDescriptor vertScrollbarVisibilityDescriptor =
                DependencyPropertyDescriptor.FromProperty(
                ScrollViewer.ComputedVerticalScrollBarVisibilityProperty, typeof(ScrollViewer));

            vertScrollbarVisibilityDescriptor.AddValueChanged
                      (scrollView, delegate
                      {
                          if (scrollView.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Collapsed)
                          {
                              VirtualizingPanel panel = UITools.FindVisualChild<VirtualizingPanel>(this);
                              if (panel is IScrollInfo)
                              {
                                  (panel as IScrollInfo).SetVerticalOffset(0);
                              }
                          }
                      });

            DependencyPropertyDescriptor horzScrollbarVisibilityDescriptor =
                DependencyPropertyDescriptor.FromProperty(
                ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty, typeof(ScrollViewer));

            horzScrollbarVisibilityDescriptor.AddValueChanged
                      (scrollView, delegate
                      {
                          if (scrollView.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Collapsed)
                          {
                              VirtualizingPanel panel = UITools.FindVisualChild<VirtualizingPanel>(this);
                              if (panel is IScrollInfo)
                              {
                                  (panel as IScrollInfo).SetHorizontalOffset(0);
                              }
                          }
                      });


            #endregion

            #region ContextMenu

            Point mouseDownPt = new Point(-100, -100);

            this.AddHandler(TreeViewItem.PreviewMouseRightButtonDownEvent, new MouseButtonEventHandler(
                 (MouseButtonEventHandler)delegate(object sender, MouseButtonEventArgs args)
                 {
                     mouseDownPt = args.GetPosition(this);
                 }));
            this.AddHandler(TreeViewItem.MouseRightButtonUpEvent, new MouseButtonEventHandler(
                (MouseButtonEventHandler)delegate(object sender, MouseButtonEventArgs args)
                {
                    Point mouseUpPt = args.GetPosition(this);

                    if ((Math.Abs(mouseDownPt.X - mouseUpPt.X) < 5) &&
                        (Math.Abs(mouseDownPt.Y - mouseUpPt.Y) < 5))
                    {
                        args.Handled = true;
                        if (ContextMenuCommand != null && ContextMenuCommand.CanExecute(this.DataContext))
                            if (SelectedValue != null)
                                ContextMenuCommand.Execute(this.DataContext);
                    }
                }));

            #endregion

            //Memory Leak
            //Unloaded += (o, e) =>
            //{
            //    SelectionHelper.SetEnableSelection(o as FileList2, false);
            //    (o as FileList2).View = null;

            //};


        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);


        }

        public void UpdateCollumnHeader()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            GridViewColumnCollection collection = null;
            if (View is GridView)
                collection = (View as GridView).Columns;
            else
                if (View is VirtualWrapPanelView)
                    collection = (View as VirtualWrapPanelView).Columns;
                else if (View is VirtualStackPanelView)
                    collection = (View as VirtualStackPanelView).Columns;

            if (collection != null)
                foreach (GridViewColumn col in collection)
                {
                    string colHeader = GetSortPropertyName(col);
                    if (colHeader != SortBy)
                        col.HeaderTemplate = this.TryFindResource(DefaultHeader_Template) as DataTemplate;
                    else
                        if (SortAscending)
                            col.HeaderTemplate = this.TryFindResource(AscendingHeader_Template) as DataTemplate;
                        else col.HeaderTemplate = this.TryFindResource(DecendingHeader_Template) as DataTemplate;
                }
        }


        public static void OnViewModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {

            if (!args.NewValue.Equals(args.OldValue))
            {
                FileList fl = (FileList)sender;
                ViewMode viewMode = (ViewMode)args.NewValue;
                if (viewMode == 0)
                    return;

                string viewName = FileList.View_Template[viewMode];
                ViewBase view = (ViewBase)(fl.TryFindResource(viewName)); ;

                if (fl.ViewSize < (int)viewMode)
                    fl.ViewSize = (int)viewMode;

                if (view != null)
                    fl.View = view;
                fl.UpdateCollumnHeader();
            }
        }



        public static void OnViewSizeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            FileList flist = sender as FileList;
            int newSize = (int)args.NewValue;

            ViewMode newViewMode = flist.ViewMode;
            switch (newSize)
            {
                case 13: newViewMode = ViewMode.vmTile; break;
                case 14: newViewMode = ViewMode.vmGrid; break;
                case 15: newViewMode = ViewMode.vmList; break;
                case 16: newViewMode = ViewMode.vmSmallIcon; break;
                case 256: newViewMode = ViewMode.vmViewer; break;
                default:
                    if (newSize <= 16) newViewMode = ViewMode.vmSmallIcon;
                    else
                        if (newSize <= 48)
                            newViewMode = ViewMode.vmIcon;
                        else if (newSize <= 80)
                            newViewMode = ViewMode.vmLargeIcon;
                        else newViewMode = ViewMode.vmExtraLargeIcon;
                    break;
            }

            if (newViewMode != flist.ViewMode)
                flist.ViewMode = newViewMode;


        }

        #endregion

        #region Data

        #endregion

        #region Public Properties


        #region SortPropertyName (attached property)
        public static readonly DependencyProperty SortPropertyNameProperty = DependencyProperty.RegisterAttached("SortPropertyName",
            typeof(string), typeof(FileList));

        public static string GetSortPropertyName(DependencyObject sender)
        {
            return (string)sender.GetValue(SortPropertyNameProperty);
        }

        public static void SetSortPropertyName(DependencyObject sender, string value)
        {
            sender.SetValue(SortPropertyNameProperty, value);
        }
        #endregion

        #region IsEditing (attached property)
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.RegisterAttached("IsEditing", typeof(bool), typeof(FileList));

        public static bool GetIsEditing(DependencyObject sender)
        {
            return (bool)sender.GetValue(IsEditingProperty);
        }

        public static void SetIsEditing(DependencyObject sender, bool value)
        {
            sender.SetValue(IsEditingProperty, value);
        }
        #endregion


        #region SortBy, SortDirection
        public static readonly DependencyProperty SortByProperty =
         DependencyProperty.Register("SortBy", typeof(string), typeof(FileList),
         new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SortBy
        {
            get { return (string)GetValue(SortByProperty); }
            set { SetValue(SortByProperty, value); }
        }

        public static readonly DependencyProperty SortAscendingProperty =
          DependencyProperty.Register("SortAscending", typeof(bool), typeof(FileList),
          new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool SortAscending
        {
            get { return (bool)GetValue(SortAscendingProperty); }
            set { SetValue(SortAscendingProperty, value); }
        }
        #endregion


        #region ViewMode, ViewSize
        public static readonly DependencyProperty ViewModeProperty =
          DependencyProperty.Register("ViewMode", typeof(ViewMode), typeof(FileList),
          new FrameworkPropertyMetadata(ViewMode.vmGrid, new PropertyChangedCallback(OnViewModeChanged)));

        public ViewMode ViewMode
        {
            get { return (ViewMode)GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        public static readonly DependencyProperty ViewSizeProperty =
            DependencyProperty.Register("ViewSize", typeof(int), typeof(FileList),
            new FrameworkPropertyMetadata(16, new PropertyChangedCallback(OnViewSizeChanged)));

        public int ViewSize
        {
            get { return (int)GetValue(ViewSizeProperty); }
            set { SetValue(ViewSizeProperty, value); }
        }
        #endregion

        #region BindableSelectedItems

        public IList BindableSelectedItems
        {
            get { return (IList)GetValue(BindableSelectedItemsProperty); }
            set { SetValue(BindableSelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BindableSelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindableSelectedItemsProperty =
            DependencyProperty.Register("BindableSelectedItems", typeof(IList), typeof(FileList));
        #endregion




        public ICommand RenameCommand
        {
            get { return (ICommand)GetValue(RenameCommandProperty); }
            set { SetValue(RenameCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RenameCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RenameCommandProperty =
            DependencyProperty.Register("RenameCommand", typeof(ICommand), typeof(FileList), new UIPropertyMetadata(null));



        public ICommand ContextMenuCommand
        {
            get { return (ICommand)GetValue(ContextMenuCommandProperty); }
            set { SetValue(ContextMenuCommandProperty, value); }
        }

        public static readonly DependencyProperty ContextMenuCommandProperty =
            DependencyProperty.Register("ContextMenuCommand", typeof(ICommand), typeof(FileList), new UIPropertyMetadata(null));


        public ICommand UnSelectAllCommand
        {
            get { return (ICommand)GetValue(UnSelectAllCommandProperty); }
            set { SetValue(UnSelectAllCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnSelectAllCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnSelectAllCommandProperty =
            DependencyProperty.Register("UnSelectAllCommand", typeof(ICommand), typeof(FileList), new UIPropertyMetadata(null));

        void IVirtualListView.UnselectAll()
        {
            if (UnSelectAllCommand != null)
                UnSelectAllCommand.Execute(this);
            this.SelectedItems.Clear();
        }

        #endregion


    }
}
