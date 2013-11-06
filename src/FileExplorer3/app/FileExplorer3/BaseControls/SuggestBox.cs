using System;
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
        Task<IList<object>> SuggestAsync(string input);
    }

    public class SuggestBox : TextBox
    {
        #region Constructor

        static SuggestBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SuggestBox),
                new FrameworkPropertyMetadata(typeof(SuggestBox)));
        }

        public SuggestBox()
        {
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _popup = this.Template.FindName("PART_Popup", this) as Popup;
            _itemList = this.Template.FindName("PART_ItemList", this) as ListBox;
            _host = this.Template.FindName("PART_ContentHost", this) as ScrollViewer;
            _textBoxView = LogicalTreeHelper.GetChildren(_host).OfType<UIElement>().First();
            _root = this.Template.FindName("root", this) as Grid;

            this.GotKeyboardFocus += (o, e) => { this.popupIfSuggest(); IsHintVisible = false; };
            this.LostKeyboardFocus += (o, e) => { if (!IsKeyboardFocusWithin) this.hidePopup(); 
                IsHintVisible = String.IsNullOrEmpty(Text); };

            //09-04-09 Based on SilverLaw's approach 
            _popup.CustomPopupPlacementCallback += new CustomPopupPlacementCallback(
                (popupSize, targetSize, offset) => new CustomPopupPlacement[] {
                new CustomPopupPlacement(new Point((0.01 - offset.X),
                    (_root.ActualHeight - offset.Y)), PopupPrimaryAxis.None) });

            #region _itemList event handlers - MouseDblClick, PreviewMouseUp, PreviewKeyDown

            _itemList.MouseDoubleClick += (o, e) => { updateValueFromListBox(); };

            _itemList.PreviewMouseUp += (o, e) =>
                {
                    if (_itemList.SelectedValue != null)
                        updateValueFromListBox();
                };

            _itemList.PreviewKeyDown += (o, e) =>
            {
                if (e.OriginalSource is ListBoxItem)
                {

                    ListBoxItem lbItem = e.OriginalSource as ListBoxItem;

                    e.Handled = true;
                    switch (e.Key)
                    {
                        case Key.Enter:
                            //Handle in OnPreviewKeyDown
                            break;
                        case Key.Oem5:
                            updateValueFromListBox(false);
                            SetValue(TextProperty, Text + "\\");
                            break;
                        case Key.Escape:
                            this.Focus();
                            hidePopup();
                            break;
                        default: e.Handled = false; break;
                    }

                    if (e.Handled)
                    {
                        Keyboard.Focus(this);
                        hidePopup();
                        this.Select(Text.Length, 0); //Select last char
                    }
                }
            };
            #endregion

            #region Hide popup when switch to another window

            Window parentWindow = UITools.FindLogicalAncestor<Window>(this);
            if (parentWindow != null)
            {
                parentWindow.Deactivated += delegate { _prevState = IsPopupOpened; IsPopupOpened = false; };
                parentWindow.Activated += delegate { IsPopupOpened = _prevState; };
            }
            #endregion

        }

        #region Utils - Update Bindings

        private void updateValueFromListBox(bool updateSrc = true)
        {
            var bindingExpr = _itemList.GetBindingExpression(ListBox.SelectedValueProperty);
            if (bindingExpr != null)
                bindingExpr.UpdateSource();

            if (updateSrc)
                updateSource();
            hidePopup();
        }

        private void updateSource()
        {
            var txtBindingExpr = this.GetBindingExpression(TextBox.TextProperty);
            if (txtBindingExpr != null)
                txtBindingExpr.UpdateSource();
        }

        #endregion

        #region Utils - Popup show / hide
        private void popupIfSuggest()
        {
            if (this.IsFocused)
                if (Suggestions != null && Suggestions.Count > 0)
                    IsPopupOpened = true;
                else IsPopupOpened = false;
        }

        private void hidePopup()
        {
            IsPopupOpened = false;
        }
        #endregion

        private static string getDirectoryName(string path)
        {
            if (path.EndsWith("\\"))
                return path;
            //path = path.Substring(0, path.Length - 1); //Remove ending slash.

            int idx = path.LastIndexOf('\\');
            if (idx == -1)
                return "";
            return path.Substring(0, idx);
        }

        #region OnEventHandler

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);


            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Prior:
                case Key.Next:
                    if (Suggestions != null && Suggestions.Count > 0 && !(e.OriginalSource is ListBoxItem))
                    {
                        popupIfSuggest();
                        _itemList.Focus();
                        _itemList.SelectedIndex = 0;
                        ListBoxItem lbi = _itemList.ItemContainerGenerator
                            .ContainerFromIndex(0) as ListBoxItem;
                        lbi.Focus();
                        e.Handled = true;
                    }
                    break;
                case Key.Return:
                    if (_itemList.IsKeyboardFocusWithin)
                        updateValueFromListBox();
                    hidePopup();
                    updateSource();
                    e.Handled = true;
                    break;

                case Key.Back:
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        if (Text.EndsWith("\\"))
                            Text = Text.Substring(0, Text.Length - 1);
                        else
                            Text = getDirectoryName(Text) + "\\";

                        this.Select(Text.Length, 0);
                        e.Handled = true;
                    }
                    break;

            }

        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            var suggestSource = SuggestSource;
            string text = Text;
            IsHintVisible = String.IsNullOrEmpty(text);
            if (suggestSource != null)
                Task.Run(async () =>
                    {
                        return await suggestSource.SuggestAsync(text);
                    }).ContinueWith(
                    (pTask) =>
                    {
                        if (!pTask.IsFaulted)
                            this.SetValue(SuggestionsProperty, pTask.Result);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                //Dispatcher.InvokeAsync(async () =>
                //{
                //    var retVal = await suggestSource.SuggestAsync(text);
                //    this.SetValue(SuggestionsProperty, retVal);
                //});
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

        Popup _popup;
        ListBox _itemList;
        Grid _root;
        ScrollViewer _host;
        UIElement _textBoxView;
        bool _prevState;

        #endregion

        #region Public Properties

        public static readonly DependencyProperty SuggestSourceProperty = DependencyProperty.Register(
            "SuggestSource", typeof(ISuggestSource), typeof(SuggestBox), new PropertyMetadata(null));

        public ISuggestSource SuggestSource
        {
            get { return (ISuggestSource)GetValue(SuggestSourceProperty); }
            set { SetValue(SuggestSourceProperty, value); }
        }

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register(
            "Suggestions", typeof(IList<object>), typeof(SuggestBox), new PropertyMetadata(null, OnSuggestionsChanged));

        public IList<object> Suggestions
        {
            get { return (IList<object>)GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
        }

        public static readonly DependencyProperty HeaderTemplateProperty =
            HeaderedItemsControl.HeaderTemplateProperty.AddOwner(typeof(SuggestBox));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }


        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
            "DisplayMemberPath", typeof(string), typeof(SuggestBox), new PropertyMetadata("Header"));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty ValuePathProperty = DependencyProperty.Register(
            "ValuePath", typeof(string), typeof(SuggestBox), new PropertyMetadata("Value"));

        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public bool IsPopupOpened
        {
            get { return (bool)GetValue(IsPopupOpenedProperty); }
            set { SetValue(IsPopupOpenedProperty, value); }
        }

        public static readonly DependencyProperty IsPopupOpenedProperty =
            DependencyProperty.Register("IsPopupOpened", typeof(bool),
            typeof(SuggestBox), new UIPropertyMetadata(false));


        public object DropDownPlacementTarget
        {
            get { return (object)GetValue(DropDownPlacementTargetProperty); }
            set { SetValue(DropDownPlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty DropDownPlacementTargetProperty =
            DependencyProperty.Register("DropDownPlacementTarget", typeof(object), typeof(SuggestBox));

        public string Hint
        {
            get { return (string)GetValue(HintProperty); }
            set { SetValue(HintProperty, value); }
        }

        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(SuggestBox), new PropertyMetadata(""));


        public bool IsHintVisible
        {
            get { return (bool)GetValue(IsHintVisibleProperty); }
            set { SetValue(IsHintVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsHintVisibleProperty =
            DependencyProperty.Register("IsHintVisible", typeof(bool), typeof(SuggestBox), new PropertyMetadata(true));


        #endregion
    }
}
