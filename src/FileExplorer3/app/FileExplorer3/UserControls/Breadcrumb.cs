using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        public Breadcrumb()
        {
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            bcore = this.Template.FindName("PART_BreadcrumbCore", this) as BreadcrumbCore;
            tbox = this.Template.FindName("PART_TextBox", this) as SuggestBox;
            toggle = this.Template.FindName("PART_Toggle", this) as ToggleButton;


            UpdateSelectedValue(DataContext);

            #region BreadcrumbCore related handlers
            //When Breadcrumb select a value, update it.
            AddHandler(BreadcrumbCore.SelectedValueChangedEvent, (RoutedEventHandler)((o, e) =>
            {
                UpdateSelectedValue(bcore.SelectedValue);
            }));
            #endregion

            #region SuggestBox related handlers.
            //When click empty space, switch to text box
            AddHandler(Breadcrumb.MouseDownEvent, (RoutedEventHandler)((o, e) =>
            {
                toggle.SetValue(ToggleButton.IsCheckedProperty, false); //Hide Breadcrumb
            }));
            //When text box is visible, call SelectAll
            toggle.AddValueChanged(ToggleButton.IsCheckedProperty,
                (o, e) =>
                {
                    tbox.Focus();
                    tbox.SelectAll();
                });
            //When changed selected (path) value, hide textbox.
            AddHandler(SuggestBox.ValueChangedEvent, (RoutedEventHandler)((o, e) =>
                {
                    toggle.SetValue(ToggleButton.IsCheckedProperty, true); //Show Breadcrumb
                }));
            this.AddValueChanged(Breadcrumb.SelectedPathValueProperty, (o, e) =>
            {
                toggle.SetValue(ToggleButton.IsCheckedProperty, true); //Show Breadcrumb
            });
            this.AddValueChanged(Breadcrumb.SelectedValueProperty, (o, e) =>
            {
                toggle.SetValue(ToggleButton.IsCheckedProperty, true); //Show Breadcrumb
            });
            
            #endregion

            
            this.AddValueChanged(DataContextProperty, OnDataContextChanged);
            OnDataContextChanged(this, EventArgs.Empty);
        }

        public void UpdateSelectedValue(object value)
        {
            if (bcore != null && value != null)
            {
                var hierarchy = HierarchyHelper.GetHierarchy(value, true).Reverse().ToList();
                bcore.SetValue(BreadcrumbCore.ItemsSourceProperty, hierarchy);                
                SelectedPathValue = HierarchyHelper.GetPath(value);
            }
        }

        public void OnDataContextChanged(object sender, EventArgs args)
        {
            if (DataContext != null)
                bcore.RootItems = this.HierarchyHelper.List(DataContext);
        }

        public static void OnSelectedValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            if (bread.bcore != null && !e.NewValue.Equals(e.OldValue))
                bread.UpdateSelectedValue(e.NewValue);
        }

        public static void OnSelectedPathValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            if (bread.bcore != null && !e.NewValue.Equals(e.OldValue))
                bread.UpdateSelectedValue(bread.HierarchyHelper.GetItem(bread.DataContext, e.NewValue as string));
        }

         public static void OnHierarchyHelperPropChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            bread.HierarchyHelper = new PathHierarchyHelper(bread.ParentPath, bread.ValuePath, bread.SubEntriesPath);
        }

        #endregion

        #region Data

        BreadcrumbCore bcore;
        SuggestBox tbox;
        ToggleButton toggle;

        #endregion

        #region Public Properties

        #region SelectedValue, SelectedPathValue

        /// <summary>
        /// Selected value object, it's path is retrieved from HierarchyHelper.GetPath(), not bindable at this time
        /// </summary>
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object),
            typeof(Breadcrumb), new UIPropertyMetadata(null, OnSelectedValueChanged));

        /// <summary>
        /// Path value of the SelectedValue object, bindable.
        /// </summary>
        public string SelectedPathValue
        {
            get { return (string)GetValue(SelectedPathValueProperty); }
            set { SetValue(SelectedPathValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathValueProperty =
            DependencyProperty.Register("SelectedPathValue", typeof(string),
            typeof(Breadcrumb), new UIPropertyMetadata(null, OnSelectedPathValueChanged));

        #endregion

        #region IsBreadcrumbVisible
        
        /// <summary>
        /// Toggle whether Breadcrumb (or SuggestBox) visible
        /// </summary>
        public bool IsBreadcrumbVisible
        {
            get { return (bool)GetValue(IsBreadcrumbVisibleProperty); }
            set { SetValue(IsBreadcrumbVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsBreadcrumbVisibleProperty =
            DependencyProperty.Register("IsBreadcrumbVisible", typeof(bool),
            typeof(Breadcrumb), new UIPropertyMetadata(true));

        #endregion

        #region Header/Icon Template, DisplayMemberPath
        public static readonly DependencyProperty HeaderTemplateProperty =
                    BreadcrumbCore.HeaderTemplateProperty.AddOwner(typeof(Breadcrumb));

        /// <summary>
        /// DataTemplate define the header text, (see also IconTemplate)
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty IconTemplateProperty =
           DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(Breadcrumb), new PropertyMetadata(null));

        /// <summary>
        /// DataTemplate define the icon.
        /// </summary>
        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }

        #endregion

        #region HierarchyHelper, ParentPath, ValuePath, SubEntriesPath, SuggestSource

        /// <summary>
        /// Uses to navigate the hierarchy, one can also set the ParentPath/ValuePath and SubEntriesPath instead.
        /// </summary>
        public IHierarchyHelper HierarchyHelper
        {
            get { return (IHierarchyHelper)GetValue(HierarchyHelperProperty); }
            set { SetValue(HierarchyHelperProperty, value); }
        }

        public static readonly DependencyProperty HierarchyHelperProperty =
            DependencyProperty.Register("HierarchyHelper", typeof(IHierarchyHelper),
            typeof(Breadcrumb), new PropertyMetadata(new PathHierarchyHelper("Parent", "Value", "SubEntries")));

        /// <summary>
        /// The path of view model to access parent.
        /// </summary>
        public string ParentPath
        {
            get { return (string)GetValue(ParentPathProperty); }
            set { SetValue(ParentPathProperty, value); }
        }

        public static readonly DependencyProperty ParentPathProperty =
             SuggestBox.ParentPathProperty.AddOwner(typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

        /// <summary>
        /// The path of view model to access value.
        /// </summary>
        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public static readonly DependencyProperty ValuePathProperty =
            SuggestBox.ValuePathProperty.AddOwner(typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

        /// <summary>
        /// The path of view model to access sub entries.
        /// </summary>
        public string SubEntriesPath
        {
            get { return (string)GetValue(SubEntriesPathProperty); }
            set { SetValue(SubEntriesPathProperty, value); }
        }

        public static readonly DependencyProperty SubEntriesPathProperty =
           SuggestBox.SubEntriesPathProperty.AddOwner(typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

        /// <summary>
        /// Uses by SuggestBox to suggest options.
        /// </summary>
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
