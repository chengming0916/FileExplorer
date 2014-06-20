using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Return the first directory found from the input. 
        /// (E.g. ExtractFirstDir("\temp\1\2",0) will return "Temp")
        /// </summary>
        public static string ExtractFirstDir(this IPathHelper pathHelper, string input, Int32 startIndex)
        {
            if (input.Length < startIndex)
                return "";

            Int32 idx = input.IndexOf(pathHelper.Separator, startIndex);
            if (idx == -1)
                return input.Substring(startIndex);
            return input.Substring(startIndex, idx - startIndex);
        }

         /// <summary>
        /// Add a slash "\" to end of input if not exists.
        /// </summary>
        public static string AppendSlash(this IPathHelper pathHelper, string input)
        {
            if (input.EndsWith(pathHelper.Separator + "")) { return input; }
            else
            { return input + pathHelper.Separator; }
        }

        /// <summary>
        /// Add a slash "\" to front of input if not exists.
        /// </summary>
        public static string AppendFrontSlash(this IPathHelper pathHelper, string input)
        {
            if (input.StartsWith(pathHelper.Separator + "")) { return input; }
            else
            { return pathHelper.Separator + input; }
        }
    }
}
