using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileExplorer.Defines;

namespace FileExplorer.UserControls
{
    public class FileList : ListView
    {
        #region Cosntructor

        #endregion

        #region Methods


        public static void OnColumnsVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            FileList fl = (FileList)sender;

            if (args.OldValue != args.NewValue)
                if ((Visibility)args.NewValue == Visibility.Visible)
                    fl.AddHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)fl.columnClickedHandler);
                else
                    fl.RemoveHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)fl.columnClickedHandler);
        }

        private void columnClickedHandler(object sender, RoutedEventArgs args)
        {
            GridViewColumnHeader header = (GridViewColumnHeader)args.OriginalSource;
            if (header.Column != null)
            {
                ListViewColumnInfo sortColumn = Columns.FirstOrDefault(
                    c => c.Header == (string)header.Column.Header);

                if (SortBy != sortColumn.Header && SortBy != sortColumn.ValuePath)
                    SetValue(SortByProperty, sortColumn.ValuePath);
                else
                    if (SortDirection == Defines.SortDirection.Ascending)
                        SetValue(SortDirectionProperty, Defines.SortDirection.Descending);
                    else SetValue(SortDirectionProperty, Defines.SortDirection.Ascending);
            }
        }

        public static void OnViewModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            FileList fl = (FileList)sender;
            
            ViewBase  view = fl.View;
            if (args.Property.Equals(ViewModeProperty))
            {
                string viewMode = (string)args.NewValue;
                if (String.IsNullOrEmpty(viewMode))
                    return;                        
                string viewResourceName = viewMode + "View";
                view = (ViewBase)(fl.TryFindResource(viewResourceName));
                if (view != null)
                    fl.View = view;
                else Debug.WriteLine(String.Format("FileList - {0} not found.", viewResourceName));
            }
            
            if (fl.View != null)
            {       
                //Only update columns if View is updated
                if (args.Property.Equals(ColumnsProperty) || args.Property.Equals(ViewModeProperty))
                    ListViewColumnUtils.UpdateColumn(fl, fl.Columns);                 
                
                //always update sort column
                ListViewColumnInfo sortColumn = fl.Columns.FirstOrDefault(
                    c => c.Header == fl.SortBy || c.ValuePath == fl.SortBy);
                if (sortColumn != null)
                    ListViewColumnUtils.UpdateCollumnHeader(fl, sortColumn, fl.SortDirection);                     
            }
            else Debug.WriteLine(String.Format("FileList - No view defined."));
        }

 

        #endregion

        #region Data

        #endregion

        #region Public Properties


        #region ViewMode, ItemAnimationDuration, ItemSize property

        public static readonly DependencyProperty ViewModeProperty =
         DependencyProperty.Register("ViewMode", typeof(string), typeof(FileList),
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
         DependencyProperty.Register("ItemAnimationDuration", typeof(TimeSpan), typeof(FileList),
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
            DependencyProperty.Register("ItemSize", typeof(double), typeof(FileList),
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

        #region Columns, and ColumnsVisibility property

        public static readonly DependencyProperty ColumnsProperty =
         DependencyProperty.Register("Columns", typeof(ListViewColumnInfo[]), typeof(FileList),
         new FrameworkPropertyMetadata(new ListViewColumnInfo[] { }, new PropertyChangedCallback(OnViewModeChanged)));

        /// <summary>
        /// If the Panel is GridView, VirtualStack/WrapPanelView, one can change the column headers.
        /// </summary>
        public ListViewColumnInfo[] Columns
        {
            get { return (ListViewColumnInfo[])GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsVisibilityProperty =
        DependencyProperty.Register("ColumnsVisibility", typeof(Visibility), typeof(FileList),
            new FrameworkPropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(OnColumnsVisibilityChanged)));

        /// <summary>
        /// Whether the column header is visible, you may want to implement your own sorting mechanism.
        /// </summary>
        public Visibility ColumnsVisibility
        {
            get { return (Visibility)GetValue(ColumnsVisibilityProperty); }
            set { SetValue(ColumnsVisibilityProperty, value); }
        }
        #endregion

        #region SortBy, SortDirection property

        public static readonly DependencyProperty SortByProperty =
        DependencyProperty.Register("SortBy", typeof(string), typeof(FileList),
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
            DependencyProperty.Register("SortDirection", typeof(SortDirection), typeof(FileList),
            new FrameworkPropertyMetadata(SortDirection.Ascending, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                new PropertyChangedCallback(OnViewModeChanged)));

        /// <summary>
        /// Specify direction of sorting.
        /// </summary>
        public SortDirection SortDirection
        {
            get { return (SortDirection)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        #endregion



        #endregion

    }
}
