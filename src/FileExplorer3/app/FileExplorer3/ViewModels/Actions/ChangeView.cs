using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// Change the View of a ListView.
    /// </summary>
    public class ChangeView : IResult
    {
        //public static string Entry_ItemTemplate = "{0}EntryTemplate"; //Where {0} is View
        public static string Entry_ItemContainerStyle = "{0}ItemContainerStyle";
        public static string EntryList_View = "{0}View";
        public static string EntryList_PanelTemplate = "{0}PanelTemplate";

        #region Cosntructor

        public ChangeView(string viewMode)
        {
            _viewMode = viewMode;
        }

        #endregion

        #region Methods


        public event EventHandler<ResultCompletionEventArgs> Completed;

        public void Execute(ActionExecutionContext context)
        {
            FrameworkElement ele = context.Source as FrameworkElement;
            Exception ex = null;
            while (ele != null)
            {
                var found = UITools.FindVisualChildByName<ListView>(ele, "Items");
                if (found != null)
                {
                    string viewKey = String.Format(EntryList_View, _viewMode);
                    string itemContainerStyleKey = String.Format(Entry_ItemContainerStyle, _viewMode);
                    var panelTemplate = found.FindResource(String.Format(EntryList_PanelTemplate, _viewMode));

                    if (_viewMode.Equals("Grid"))
                    {
                        var view = found.TryFindResource(viewKey);
                        found.SetValue(ListView.ViewProperty, view);
                    }
                    else
                    {
                        var itemContainerStyle = found.TryFindResource(itemContainerStyleKey);
                        if (itemContainerStyle == null)
                            ex = new Exception("Resource not found: " + itemContainerStyle);
                        else found.SetValue(ListView.ItemContainerStyleProperty, itemContainerStyle);
                    
                        found.SetValue(ListView.ItemsPanelProperty, panelTemplate);
                        found.SetValue(View.ContextProperty, _viewMode);
                    }
                    break; 
                   
                    //Debug.WriteLine(found.View.ToString());
                    //break;

                    ////string itemTemplateKey = String.Format(Entry_ItemTemplate, _viewMode);
                    ////

                    ////if (!itemTemplateKey.Equals(found.ItemTemplate.DataTemplateKey))                        
                    ////{
                    ////    var itemTemplate = found.TryFindResource(itemTemplateKey);
                    ////    if (itemTemplate == null)
                    ////        ex = new Exception("Resource not found: " + itemTemplateKey);
                    ////    else found.SetValue(ListView.ItemTemplateProperty, itemTemplate);
                    ////}
;

                    //break;
                }
                ele = ele.Parent as FrameworkElement;
            }

            if (ele == null)
                ex = new Exception("Cannot find ListView named Items in FileList control");           

            Completed(this, new ResultCompletionEventArgs() { Error = ex });
        }

        #endregion

        #region Data

        string _viewMode;

        #endregion

        #region Public Properties

        #endregion
    }
}
