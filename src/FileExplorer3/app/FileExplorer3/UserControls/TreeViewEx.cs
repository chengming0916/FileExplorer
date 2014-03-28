using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FileExplorer.BaseControls;


namespace FileExplorer.UserControls
{
    //TreeViewEx and TreeViewItemEx

    public class TreeViewEx : TreeView
    {
        #region Cosntructor

        public TreeViewEx()
        {

        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public static readonly DependencyProperty ContentBelowScrollViewerProperty =
        DependencyProperty.Register("ContentBelowScrollViewer", typeof(object), typeof(TreeViewEx));
        public object ContentBelowScrollViewer
        {
            get { return (object)GetValue(ContentBelowScrollViewerProperty); }
            set { SetValue(ContentBelowScrollViewerProperty, value); }
        }

        #endregion
    }

    public class TreeViewItemEx : TreeViewItem
    {
        #region Cosntructor

        public TreeViewItemEx()
        {
            this.AddHandler(TreeViewItem.SelectedEvent, new RoutedEventHandler(
                 (RoutedEventHandler)delegate(object obj, RoutedEventArgs args)
                 {
                     if (args.OriginalSource is TreeViewItem)
                         (args.OriginalSource as TreeViewItem).BringIntoView();

                 }));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.AddValueChanged(TreeViewItemEx.IsDraggingOverProperty, (o,e) =>
                {
                    TreeViewItemEx tvItem = o as TreeViewItemEx;
                    if (tvItem.IsDraggingOver && ExpandIfDragOver)
                    {
                        DispatcherTimer dispatcherTimer = new DispatcherTimer();
                        EventHandler onTick = null;
                        onTick = (o1, e1) =>
                        {
                            if (tvItem.IsDraggingOver)
                                tvItem.SetValue(TreeViewItem.IsExpandedProperty, true);
                            dispatcherTimer.Tick -= onTick;
                            dispatcherTimer.Stop();
                        };
                        dispatcherTimer.Tick += onTick;
                        dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                        dispatcherTimer.Start();
                    }
                });
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);


        }
        //override hittestvi

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public bool IsItemUpdateRequired
        {
            get { return (bool)GetValue(IsItemUpdateRequiredProperty); }
            set { SetValue(IsItemUpdateRequiredProperty, value); }
        }

        public static readonly DependencyProperty IsItemUpdateRequiredProperty =
            DependencyProperty.Register("IsItemUpdateRequired", typeof(bool),
            typeof(TreeViewItemEx), new UIPropertyMetadata(false));

        public bool IsDraggingOver
        {
            get { return (bool)GetValue(IsDraggingOverProperty); }
            set { SetValue(IsDraggingOverProperty, value); }
        }

        public static readonly DependencyProperty IsDraggingOverProperty =
            DependencyProperty.Register("IsDraggingOver", typeof(bool),
            typeof(TreeViewItemEx), new UIPropertyMetadata(false));

        public bool ExpandIfDragOver
        {
            get { return (bool)GetValue(ExpandIfDragOverProperty); }
            set { SetValue(ExpandIfDragOverProperty, value); }
        }

        public static readonly DependencyProperty ExpandIfDragOverProperty =
            DependencyProperty.Register("ExpandIfDragOver", typeof(bool),
            typeof(TreeViewItemEx), new UIPropertyMetadata(true));

        #endregion
    }
}
