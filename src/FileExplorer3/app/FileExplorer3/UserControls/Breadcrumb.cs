using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls
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
    }

    public class Breadcrumb : ItemsControl
    {
        #region Constructor

        static Breadcrumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Breadcrumb),
                new FrameworkPropertyMetadata(typeof(Breadcrumb)));
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            bcore = this.Template.FindName("PART_BreadcrumbCore", this) as BreadcrumbCore;
            tbox = this.Template.FindName("PART_TextBox", this) as SuggestBox;
        }

        public static void OnSelectedValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            if (bread.bcore != null && e.NewValue != null && !e.NewValue.Equals(e.OldValue))
            {
                var hierarchy = bread.HierarchyHelper.GetHierarchy(e.NewValue, true).Reverse().ToList();                
                bread.bcore.SetValue(BreadcrumbCore.ItemsSourceProperty, hierarchy);                
                //bread.tbox.SetValue(TextBlock.TextProperty, 
            }
        }
        
        #endregion

        #region Data

        BreadcrumbCore bcore;
        SuggestBox tbox;
        
        #endregion

        #region Public Properties

        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object),
            typeof(Breadcrumb), new UIPropertyMetadata(null, OnSelectedValueChanged));


        public bool IsBreadcrumbVisible
        {
            get { return (bool)GetValue(IsBreadcrumbVisibleProperty); }
            set { SetValue(IsBreadcrumbVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsBreadcrumbVisibleProperty =
            DependencyProperty.Register("IsBreadcrumbVisible", typeof(bool),
            typeof(Breadcrumb), new UIPropertyMetadata(true));

        public static readonly DependencyProperty HeaderTemplateProperty = 
            BreadcrumbCore.HeaderTemplateProperty.AddOwner(typeof(Breadcrumb));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        #region HierarchyHelper, SuggestSource

        public IHierarchyHelper HierarchyHelper
        {
            get { return (IHierarchyHelper)GetValue(HierarchyHelperProperty); }
            set { SetValue(HierarchyHelperProperty, value); }
        }

        public static readonly DependencyProperty HierarchyHelperProperty =
            DependencyProperty.Register("HierarchyHelper", typeof(IHierarchyHelper),
            typeof(Breadcrumb), new UIPropertyMetadata(new AutoHierarchyHelper("Parent", "Value", "SubDirectories")));

       
        public ISuggestSource SuggestSource
        {
            get { return (ISuggestSource)GetValue(SuggestSourceProperty); }
            set { SetValue(SuggestSourceProperty, value); }
        }

        public static readonly DependencyProperty SuggestSourceProperty =
            DependencyProperty.Register("SuggestSource", typeof(ISuggestSource),
            typeof(Breadcrumb), new UIPropertyMetadata(new AutoSuggestSource("SubDirectories", "Value")));

        #endregion
        #endregion
    }
}
