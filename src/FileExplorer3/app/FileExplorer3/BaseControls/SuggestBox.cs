using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FileExplorer.UserControls;

namespace FileExplorer.BaseControls
{
    public interface ISuggestSource
    {
        Task<IList<object>> SuggestAsync(object data, string input, IHierarchyHelper helper);
    }

    /// <summary>
    /// Uses ISuggestSource and HierarchyHelper to suggest automatically.
    /// </summary>
    public class SuggestBox : SuggestBoxBase
    {
        #region Constructor

        public SuggestBox()
        {
        }

        #endregion

        #region Methods

      

        #region Utils - Update Bindings

       
        protected override void updateSource()
        {
            var txtBindingExpr = this.GetBindingExpression(TextBox.TextProperty);
            if (txtBindingExpr == null)
                return;
            var value = HierarchyHelper.GetItem(RootItem, Text);
            if (value != null)
            {
                if (txtBindingExpr != null)
                    txtBindingExpr.UpdateSource();
                RaiseEvent(new RoutedEventArgs(ValueChangedEvent));                
            }
            else Validation.MarkInvalid(txtBindingExpr,                
                new ValidationError(new PathExistsValidationRule(), txtBindingExpr, 
                    "Path not exists.", null));
        }

        #endregion

   

        #region OnEventHandler

   
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            var suggestSource = SuggestSource;
            var hierarchyHelper = HierarchyHelper;
            string text = Text;
            object data = RootItem;
            IsHintVisible = String.IsNullOrEmpty(text);

            if (IsEnabled && suggestSource != null)
                Task.Run(async () =>
                    {
                        return await suggestSource.SuggestAsync(data, text, hierarchyHelper);
                    }).ContinueWith(
                    (pTask) =>
                    {
                        if (!pTask.IsFaulted)
                            this.SetValue(SuggestionsProperty, pTask.Result);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        protected static void OnSuggestionsChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            SuggestBox sbox = sender as SuggestBox;
            if (args.OldValue != args.NewValue)
                sbox.popupIfSuggest();
        }

        #endregion

        #endregion

        #region Data

        #endregion

        #region Public Properties


        #region HierarchyHelper, SuggestSource

        public IHierarchyHelper HierarchyHelper
        {
            get { return (IHierarchyHelper)GetValue(HierarchyHelperProperty); }
            set { SetValue(HierarchyHelperProperty, value); }
        }

        public static readonly DependencyProperty HierarchyHelperProperty =
            DependencyProperty.Register("HierarchyHelper", typeof(IHierarchyHelper),
            typeof(SuggestBox), new UIPropertyMetadata(new PathHierarchyHelper("Parent", "Value", "SubEntries")));

        public static readonly DependencyProperty SuggestSourceProperty = DependencyProperty.Register(
            "SuggestSource", typeof(ISuggestSource), typeof(SuggestBox), new PropertyMetadata(new AutoSuggestSource()));

        public ISuggestSource SuggestSource
        {
            get { return (ISuggestSource)GetValue(SuggestSourceProperty); }
            set { SetValue(SuggestSourceProperty, value); }
        }
        #endregion


        public static readonly DependencyProperty RootItemProperty = DependencyProperty.Register("RootItem",
            typeof(object), typeof(SuggestBox), new PropertyMetadata(null));

        /// <summary>
        /// Assigned by Breadcrumb
        /// </summary>
        public object RootItem
        {
            get { return GetValue(RootItemProperty); }
            set { SetValue(RootItemProperty, value); }
        }


        #endregion
    }
}
