using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using FileExplorer.UserControls;

namespace FileExplorer.ViewModels
{
    public class ChangeViewMode : IResult
    {
        public static string Entry_ItemTemplate = "{0}EntryTemplate"; //Where {0} is View
        public static string Entry_ItemContainerStyle = "{0}ItemContainerStyle";


        #region Cosntructor

        public ChangeViewMode(string viewMode)
        {
            _viewMode = viewMode;
        }

        #endregion

        #region Methods


        public event EventHandler<ResultCompletionEventArgs> Completed;

        public void Execute(ActionExecutionContext context)
        {
            FrameworkElement view = context.Source as FrameworkElement;
            Exception ex = null;
            while (view != null)
            {
                var found = UITools.FindVisualChildByName<ListView>(view, "Items");
                if (found != null)
                {                    
                    string itemTemplateKey = String.Format(Entry_ItemTemplate, _viewMode);
                    string itemContainerStyleKey = String.Format(Entry_ItemContainerStyle, _viewMode);

                    if (!itemTemplateKey.Equals(found.ItemTemplate.DataTemplateKey))                        
                    {
                        var itemTemplate = found.TryFindResource(itemTemplateKey);
                        if (itemTemplate == null)
                            ex = new Exception("Resource not found: " + itemTemplateKey);
                        else found.SetValue(ListView.ItemTemplateProperty, itemTemplate);
                    }

                    var itemContainerStyle = found.TryFindResource(itemContainerStyleKey);
                    if (itemContainerStyle == null)
                        ex = new Exception("Resource not found: " + itemContainerStyle);
                    else found.SetValue(ListView.ItemContainerStyleProperty, itemContainerStyle);

                    break;
                }
                view = view.Parent as FrameworkElement;
            }

            if (view == null)
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
