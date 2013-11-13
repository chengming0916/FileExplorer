using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.BaseControls;

namespace FileExplorer.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="VM">Main ViewModel type, e.g. BreadcrumbItemViewModel</typeparam>
    public class BreadcrumbHierarchyHelper : PathHierarchyHelper
    {
        #region Constructor

        public BreadcrumbHierarchyHelper(BreadcrumbViewModel rootModel)
            : base("ParentNode", "Value", "SubEntries")
        {
            _rootModel = rootModel;
        }
        

        #endregion

        #region Methods

        public override string ExtractPath(string pathName)
        {
            if (pathName.EndsWith(":\\")) //Drive
                return "";
            else return base.ExtractPath(pathName);
        }

        public override string ExtractName(string pathName)
        {
            if (pathName.EndsWith(":\\"))  //Drive
                return pathName;
            else return base.ExtractName(pathName);
        }
        
        #endregion

        #region Data

        private BreadcrumbViewModel _rootModel;
       
        #endregion

        #region Public Properties

        protected override object getParent(object item)
        {
            if (item is BreadcrumbViewModel)
                return null;

            BreadcrumbItemViewModel itemVM = item as BreadcrumbItemViewModel;
            if (itemVM == null) return null;
            else if (itemVM.ParentNode == null) return _rootModel; //Return root if parent is null.
            else return itemVM.ParentNode;
        }

        protected override IEnumerable getSubEntries(object item)
        {
            if (item is BreadcrumbViewModel)
                return (item as BreadcrumbViewModel).Subdirectories;

            BreadcrumbItemViewModel itemVM = item as BreadcrumbItemViewModel;
            if (itemVM == null)
                return null;
            else
            {
                return from subitemVM in itemVM.ListAsync(m => m.IsDirectory).Result
                    select itemVM.CreateSubmodel(subitemVM);                
            }
        }

        protected override string getValuePath(object item)
        {
            //if (item is BreadcrumbViewModel)
            //    return "";
            BreadcrumbItemViewModel itemVM = item as BreadcrumbItemViewModel;
            if (itemVM == null)
                return "";
            return itemVM.Value;
        }

        #endregion

    }
}
