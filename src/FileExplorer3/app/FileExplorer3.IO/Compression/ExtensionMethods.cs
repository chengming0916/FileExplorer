using FileExplorer.IO.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    public static partial class ExtensionMethods
    {
        public static Task<bool> CompressOne(this ICompressorWrapper wrapper, string type, Stream stream, string fileName, Stream fileStream)
        {
            return wrapper.CompressMultiple(type, stream, new Dictionary<string, Stream>() { { fileName, fileStream } });
        }

    }
}
