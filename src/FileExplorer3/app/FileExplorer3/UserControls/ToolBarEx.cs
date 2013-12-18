using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileExplorer.BaseControls;

namespace FileExplorer.UserControls
{
    public class ToolbarEx : Menu
    {
        #region Constructor

        static ToolbarEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarEx), new FrameworkPropertyMetadata(typeof(ToolbarEx)));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolbarItemEx();
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion

    }

    public enum ToolbarItemType { Button, MenuButton, Range }


    public class ToolbarItemEx : MenuItem
    {
        #region Constructor

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //this.AddValueChanged(MenuItem.RoleProperty, (o, e) =>
            //    {

            //    });
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties


        /// <summary>
        /// Lookup from http://www.adamdawes.com/windows8/win8_segoeuisymbol.html
        /// </summary>
        public string Symbol
        {
            get { return (string)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(string),
            typeof(ToolbarItemEx), new PropertyMetadata(""));


        public static readonly DependencyProperty IsSeparatorProperty =
            DependencyProperty.Register("IsSeparator", typeof(bool), typeof(ToolbarItemEx),
            new UIPropertyMetadata(false));

        public bool IsSeparator
        {
            get { return (bool)GetValue(IsSeparatorProperty); }
            set { SetValue(IsSeparatorProperty, value); }
        }

        #region Slider related

        public static readonly DependencyProperty IsSliderEnabledProperty =
            DependencyProperty.Register("IsSliderEnabled", typeof(bool), typeof(ToolbarItemEx),
            new UIPropertyMetadata(false));

        public bool IsSliderEnabled
        {
            get { return (bool)GetValue(IsSliderEnabledProperty); }
            set { SetValue(IsSliderEnabledProperty, value); }
        }

        public static readonly DependencyProperty SliderMaximumProperty =
            DependencyProperty.Register("SliderMaximum", typeof(double), typeof(ToolbarItemEx),
            new UIPropertyMetadata(1000.0d));

        public double SliderMaximum
        {
            get { return (double)GetValue(SliderMaximumProperty); }
            set { SetValue(SliderMaximumProperty, value); }
        }

        public static readonly DependencyProperty SliderMinimumProperty =
            DependencyProperty.Register("SliderMinimum", typeof(double), typeof(ToolbarItemEx),
            new UIPropertyMetadata(0.0d));

        public double SliderMinimum
        {
            get { return (double)GetValue(SliderMinimumProperty); }
            set { SetValue(SliderMinimumProperty, value); }
        }

        public static readonly DependencyProperty SliderValueProperty =
                    DependencyProperty.Register("SliderValue", typeof(double), typeof(ToolbarItemEx),
                    new UIPropertyMetadata(new PropertyChangedCallback(delegate { /*Debug.WriteLine("Changed-ToolbarMenuItem");*/ })));


        public static readonly DependencyProperty StepsProperty = 
            DependencyProperty.RegisterAttached("Steps", typeof(ObservableCollection<Step>), typeof(ToolbarItemEx));

        public static ObservableCollection<Step> GetSelectionAdorner(DependencyObject target)
        {
            return (ObservableCollection<Step>)target.GetValue(StepsProperty);
        }

        public static void SetSelectionAdorner(DependencyObject target, ObservableCollection<Step> value)
        {
            target.SetValue(StepsProperty, value);
        }

        public double SliderValue
        {
            get { return (double)GetValue(SliderValueProperty); }
            set { SetValue(SliderValueProperty, value); }
        }

        public static readonly DependencyProperty SliderStepProperty =
                     DependencyProperty.Register("SliderStep", typeof(double), typeof(ToolbarItemEx),
                     new UIPropertyMetadata(0.0d));

        public double SliderStep
        {
            get { return (double)GetValue(SliderStepProperty); }
            set { SetValue(SliderStepProperty, value); }
        }

        public static readonly DependencyProperty ItemHeightProperty =
                     DependencyProperty.Register("ItemHeight", typeof(double), typeof(ToolbarItemEx),
                     new UIPropertyMetadata());


        public static readonly DependencyProperty IsStepStopProperty =
                    DependencyProperty.Register("IsStepStop", typeof(bool), typeof(ToolbarItemEx),
                    new UIPropertyMetadata(false));

        public bool IsStepStop
        {
            get { return (bool)GetValue(IsStepStopProperty); }
            set { SetValue(IsStepStopProperty, value); }
        }

        #endregion









        public ToolbarItemType ContentType
        {
            get { return (ToolbarItemType)GetValue(ContentTypeProperty); }
            set { SetValue(ContentTypeProperty, value); }
        }

        public static readonly DependencyProperty ContentTypeProperty =
            DependencyProperty.Register("ContentType", typeof(ToolbarItemType),
            typeof(ToolbarItemEx), new PropertyMetadata(ToolbarItemType.Button));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius),
            typeof(ToolbarItemEx), new PropertyMetadata(new CornerRadius(0)));


        #endregion
    }

}
