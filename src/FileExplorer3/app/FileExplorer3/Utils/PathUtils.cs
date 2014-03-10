using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Utils
{
    public static class PathUtils
    {
        //Andre, Richard Ev on http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        public static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        //Dour High Arch on http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        public static string SanitizePath(string path, char replaceChar)
        {
            int filenamePos = path.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            var sb = new System.Text.StringBuilder();
            sb.Append(path.Substring(0, filenamePos));
            for (int i = filenamePos; i < path.Length; i++)
            {
                char filenameChar = path[i];
                foreach (char c in Path.GetInvalidFileNameChars())
                    if (filenameChar.Equals(c))
                    {
                        filenameChar = replaceChar;
                        break;
                    }

                sb.Append(filenameChar);
            }

            return sb.ToString();
        }
    }
}
