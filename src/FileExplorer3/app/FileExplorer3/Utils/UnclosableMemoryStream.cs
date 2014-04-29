using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Utils
{
    public class UnclosableMemoryStream : MemoryStream
    {

        public override void Close()
        {
            //base.Close();
        }
    }
}
