﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using FileExplorer.Utils;

namespace FileExplorer.BaseControls
{
    /// <summary>
    /// Suggest based on sub entries of specified data.
    /// </summary>
    public class AutoSuggestSource : ISuggestSource
    {
        #region Constructor

        public AutoSuggestSource()
        {
        }

        
        #endregion

        #region Methods

   
        public Task<IList<object>> SuggestAsync(object data, string input, IHierarchyHelper helper)
        {
            string valuePath = helper.ExtractPath(input);
            string valueName = helper.ExtractName(input);
            if (String.IsNullOrEmpty(valueName) && input.EndsWith(helper.Separator + ""))
                valueName += helper.Separator;

            if (valuePath == "" && input.EndsWith("" + helper.Separator))
                valuePath = valueName;
            var found = helper.GetItem(data, valuePath);
            List<object> retVal = new List<object>();

            if (found != null)
            {
                foreach (var item in helper.List(found))
                {
                    string valuePathName = helper.GetPath(item) as string;
                    if (valuePathName.StartsWith(input, helper.StringComparisonOption) &&
                        !valuePathName.Equals(input, helper.StringComparisonOption))
                        retVal.Add(item);
                }
            }



            return Task.FromResult<IList<object>>(retVal);

        }
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties
        
        #endregion
    }
}
