using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface IPathHelper
    {
        string Combine(string path1, params string[] paths);
        string GetDirectoryName(string path);
        string GetFileName(string path);
        string GetExtension(string path);
    }

    public class PathHelper : IPathHelper
    {
        public static PathHelper Disk = new PathHelper('\\');
        public static PathHelper Web = new PathHelper('/');

        #region Constructor

        public PathHelper(char separator)
        {
            _separator = separator;
        }

        #endregion

        #region Methods
        public string Combine(string path1, params string[] paths)
        {
            string retVal = path1;

            foreach (var p in paths)
            {
                retVal = retVal.TrimEnd(_separator) + _separator + p.TrimStart(_separator);
            }
            return retVal.TrimStart(_separator);
        }

        public string GetDirectoryName(string path)
        {
            if (path.EndsWith(_separator + ""))
                path = path.Substring(0, path.Length - 1); //Remove ending slash.

            int idx = path.LastIndexOf(_separator);
            if (idx == -1)
                return "";
            return path.Substring(0, idx);
        }

        public string GetFileName(string path)
        {
            int idx = path.LastIndexOf(_separator);
            if (idx == -1)
                return path;
            return path.Substring(idx + 1);
        }

        public string GetExtension(string path)
        {
            return System.IO.Path.GetExtension(GetFileName(path));
        }

        #endregion

        #region Data

        private char _separator;

        #endregion

        #region Public Properties

        #endregion



    }
}
