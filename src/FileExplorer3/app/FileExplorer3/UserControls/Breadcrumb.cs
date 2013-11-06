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
                bread.SelectedPathValue =  bread.HierarchyHelper.GetPath(e.NewValue);
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

        public object SelectedPathValue
        {
            get { return GetValue(SelectedPathValueProperty); }
            set { SetValue(SelectedPathValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathValueProperty =
            DependencyProperty.Register("SelectedPathValue", typeof(object),
            typeof(Breadcrumb), new UIPropertyMetadata(null));



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
            typeof(Breadcrumb), new UIPropertyMetadata(new PathHierarchyHelper("Parent", "Value", "SubDirectories")));

       
        public ISuggestSource SuggestSource
        {
            get { return (ISuggestSource)GetValue(SuggestSourceProperty); }
            set { SetValue(SuggestSourceProperty, value); }
        }

        public static readonly DependencyProperty SuggestSourceProperty =
            DependencyProperty.Register("SuggestSource", typeof(ISuggestSource),
            typeof(Breadcrumb), new UIPropertyMetadata(new AutoSuggestSource()));

        #endregion
        #endregion
    }
}
