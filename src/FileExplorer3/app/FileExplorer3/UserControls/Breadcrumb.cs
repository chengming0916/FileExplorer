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
            tbox = this.Template.FindName("PART_TextBox", this) as SuggestBoxBase;
            toggle = this.Template.FindName("PART_Toggle", this) as ToggleButton;


            UpdateSelectedValue(RootItem);

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

            //Update Suggestions when text changed.
            tbox.AddHandler(TextBox.TextChangedEvent, (RoutedEventHandler)((o, e) =>
                {
                    if (tbox.IsEnabled)
                    {
                        var suggestSource = SuggestSource;
                        var hierarchyHelper = HierarchyHelper;
                        string text = tbox.Text;
                        object data = RootItem;
                        Task.Run(async () =>
                        {
                            return await suggestSource.SuggestAsync(data, text, hierarchyHelper);
                        }).ContinueWith(
                        (pTask) =>
                        {
                            if (!pTask.IsFaulted)
                                tbox.SetValue(SuggestBox.SuggestionsProperty, pTask.Result);
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }));

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


            this.AddValueChanged(RootItemProperty, OnRootItemChanged);
            OnRootItemChanged(this, EventArgs.Empty);
        }

        public void UpdateSelectedValue(object value)
        {
            if (bcore != null && value != null)
            {
                var hierarchy = HierarchyHelper.GetHierarchy(value, true).Reverse().ToList();
                bcore.SetValue(BreadcrumbCore.ItemsSourceProperty, hierarchy);
                SelectedValue = value;
                SelectedPathValue = HierarchyHelper.GetPath(value);
                bcore.SetValue(BreadcrumbCore.ShowDropDownProperty, SelectedPathValue != "");
            }
        }

        public void OnRootItemChanged(object sender, EventArgs args)
        {
            if (RootItem != null)
            {
                //bcore.RootItems = tbox.RootItems = this.HierarchyHelper.List(RootItem);
                //bcore.ShowDropDown = false;
            }
        }

        public static void OnSelectedValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            if (bread.bcore != null && (e.NewValue == null || !e.NewValue.Equals(e.OldValue)))
                bread.UpdateSelectedValue(e.NewValue);
        }

        public static void OnSelectedPathValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            if (bread.bcore != null &&
                (e.NewValue == null || !e.NewValue.Equals(bread.HierarchyHelper.GetPath(bread.SelectedValue))))
                bread.UpdateSelectedValue(bread.HierarchyHelper.GetItem(bread.RootItem, e.NewValue as string));
        }

        public static void OnHierarchyHelperPropChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            //If HierarchyHelper changed, update Parent/Value/Subentries path, vice versa.
            if (!bread._updatingHierarchyHelper)
            {
                bread._updatingHierarchyHelper = true;
                try
                {
                    if (e.Property.Equals(HierarchyHelperProperty))
                    {
                        if (bread.HierarchyHelper.ParentPath != bread.ParentPath)
                            bread.ParentPath = bread.HierarchyHelper.ParentPath;

                        if (bread.HierarchyHelper.ValuePath != bread.ValuePath)
                            bread.ValuePath = bread.HierarchyHelper.ValuePath;

                        if (bread.HierarchyHelper.SubentriesPath != bread.SubentriesPath)
                            bread.SubentriesPath = bread.HierarchyHelper.SubentriesPath;
                    }
                    else
                    {

                        bread.HierarchyHelper = new PathHierarchyHelper(bread.ParentPath, bread.ValuePath, bread.SubentriesPath);
                    }
                }
                finally
                {
                    bread._updatingHierarchyHelper = false;
                }
            }
        }

        #endregion

        #region Data

        bool _updatingHierarchyHelper = false;
        BreadcrumbCore bcore;
        SuggestBoxBase tbox;
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

        #region ProgressBar related - IsIndeterminate, IsProgressbarVisible, ProgressBarValue
        /// <summary>
        /// Toggle whether the progress bar is indertminate
        /// </summary>
        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool),
            typeof(Breadcrumb), new UIPropertyMetadata(true));

        /// <summary>
        /// Toggle whether Progressbar visible
        /// </summary>
        public bool IsProgressbarVisible
        {
            get { return (bool)GetValue(IsProgressbarVisibleProperty); }
            set { SetValue(IsProgressbarVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsProgressbarVisibleProperty =
            DependencyProperty.Register("IsProgressbarVisible", typeof(bool),
            typeof(Breadcrumb), new UIPropertyMetadata(false));

        /// <summary>
        /// Value of Progressbar.
        /// </summary>
        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(int),
            typeof(Breadcrumb), new UIPropertyMetadata(0));

        #endregion

        #region IsBreadcrumbVisible, DropDownHeight, DropDownWidth

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

        public static readonly DependencyProperty DropDownHeightProperty =
              BreadcrumbCore.DropDownHeightProperty.AddOwner(typeof(Breadcrumb));

        /// <summary>
        /// Is current dropdown (combobox) opened, this apply to the first &lt;&lt; button only
        /// </summary>
        public double DropDownHeight
        {
            get { return (double)GetValue(DropDownHeightProperty); }
            set { SetValue(DropDownHeightProperty, value); }
        }

        public static readonly DependencyProperty DropDownWidthProperty =
            BreadcrumbCore.DropDownWidthProperty.AddOwner(typeof(Breadcrumb));

        /// <summary>
        /// Is current dropdown (combobox) opened, this apply to the first &lt;&lt; button only
        /// </summary>
        public double DropDownWidth
        {
            get { return (double)GetValue(DropDownWidthProperty); }
            set { SetValue(DropDownWidthProperty, value); }
        }

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

        public static readonly DependencyProperty RootItemProperty =
         DependencyProperty.Register("RootItem", typeof(object), typeof(Breadcrumb),
         new PropertyMetadata(null));

        /// <summary>
        /// Root item of the breadcrumbnail
        /// </summary>
        public object RootItem
        {
            get { return (object)GetValue(RootItemProperty); }
            set { SetValue(RootItemProperty, value); }
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
            typeof(Breadcrumb), new PropertyMetadata(new PathHierarchyHelper("Parent", "Value", "SubEntries"), OnHierarchyHelperPropChanged));

        /// <summary>
        /// The path of view model to access parent.
        /// </summary>
        public string ParentPath
        {
            get { return (string)GetValue(ParentPathProperty); }
            set { SetValue(ParentPathProperty, value); }
        }

        public static readonly DependencyProperty ParentPathProperty =
            DependencyProperty.Register("Parent", typeof(string),
            typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

        /// <summary>
        /// The path of view model to access value.
        /// </summary>
        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public static readonly DependencyProperty ValuePathProperty =
            DependencyProperty.Register("Value", typeof(string),
            typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

        /// <summary>
        /// The path of view model to access sub entries.
        /// </summary>
        public string SubentriesPath
        {
            get { return (string)GetValue(SubentriesPathProperty); }
            set { SetValue(SubentriesPathProperty, value); }
        }

        public static readonly DependencyProperty SubentriesPathProperty =
           DependencyProperty.Register("SubentriesPath", typeof(string),
            typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

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
