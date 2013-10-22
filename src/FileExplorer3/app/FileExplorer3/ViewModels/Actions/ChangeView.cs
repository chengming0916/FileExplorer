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
        public static string EntryList_ItemTemplate = "{0}ItemTemplate";

        #region Cosntructor

        public ChangeView(FileListViewModel flistModel, string viewMode)
        {
            _fileListViewModel = flistModel;
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

                    var view = found.TryFindResource(viewKey);
                    //Use view if specific view is found.
                    //if ((view as FileExplorer.UserControls.VirtualWrapPanelView).ItemTemplate.VisualTree == null)
                    //   throw new Exception();
                    if (view != null)
                    {
                        found.SetValue(ListView.ViewProperty, view);

                        //if (found.GetBindingExpression(ListView.ItemContainerStyleProperty) == null)
                        //    throw new Exception();
                        //if (found.GetBindingExpression(ListView.ItemTemplateProperty) == null)
                        //    throw new Exception();
                    }
                    else
                    {
                        var panelTemplate = found.TryFindResource(String.Format(EntryList_PanelTemplate, _viewMode));
                        var itemContainerStyle = found.TryFindResource(itemContainerStyleKey);


                        if (itemContainerStyle != null)
                            found.SetValue(ListView.ItemContainerStyleProperty, itemContainerStyle);
                        if (panelTemplate != null)
                            found.SetValue(ListView.ItemsPanelProperty, panelTemplate);

                        //found.SetValue(View.ContextProperty, _viewMode);
                    }

                    //var itemTemplate = found.TryFindResource(String.Format(EntryList_ItemTemplate, _viewMode));
                    //if (itemTemplate != null)
                    //    found.SetValue(ListView.ItemTemplateProperty, itemTemplate);
                    
                    _fileListViewModel.ViewMode = _viewMode;
                    break;

                }
                ele = ele.Parent as FrameworkElement;
            }

            if (ele == null)
                ex = new Exception("Cannot find ListView named Items in FileList control");

            Completed(this, new ResultCompletionEventArgs() { Error = ex });
        }

        #endregion

        #region Data

        FileListViewModel _fileListViewModel;
        string _viewMode;

        #endregion

        #region Public Properties

        

        #endregion
    }
}
