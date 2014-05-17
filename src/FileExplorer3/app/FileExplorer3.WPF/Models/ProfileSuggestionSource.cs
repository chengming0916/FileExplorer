﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.WPF.BaseControls;
using FileExplorer.WPF.Models;

namespace FileExplorer.WPF.Models
{
    public class ProfileSuggestionSource : ISuggestSource
    {
       
        #region Constructor

        public ProfileSuggestionSource(IProfile profile)
        {
            _profile = profile;
        }


        #endregion

        #region Methods

        public static string GetDirectoryName(string path)
        {
            int idx = path.LastIndexOf('\\');
            if (idx == -1)
                return "";
            return path.Substring(0, idx);
        }

        public static string GetFileName(string path)
        {
            int idx = path.LastIndexOf('\\');
            if (idx == -1)
                return path;
            return path.Substring(idx + 1);
        }

        public async Task<IList<object>> SuggestAsync(object data, string input, IHierarchyHelper helper)
        {
            string dir = GetDirectoryName(input);
            string searchStr = GetFileName(input);
            if (String.IsNullOrEmpty(searchStr) && input.EndsWith("\\"))
                searchStr += "\\";

            if (dir == "" && input.EndsWith("\\"))
                dir = searchStr;
            var found = await _profile.ParseAsync(dir);
            List<object> retVal = new List<object>();

            if (found != null)
            {
                if (_cts != null)
                    _cts.Cancel();
                _cts = new CancellationTokenSource();
                foreach (var item in await _profile.ListAsync(found, _cts.Token, em => em.IsDirectory))
                {
                    if (item.FullPath.StartsWith(input, StringComparison.CurrentCultureIgnoreCase) &&
                        !item.FullPath.Equals(input, StringComparison.CurrentCultureIgnoreCase))
                        retVal.Add(item);
                }
            }

            return retVal;
        }

        #endregion

        #region Data
        private IProfile _profile;
        private CancellationTokenSource _cts;


        #endregion

        #region Public Properties

        #endregion

    }
}
