using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileExplorer.Script
{
    [XmlInclude(typeof(DiskTransfer))]  
    [XmlInclude(typeof(DiskRun))]
    public class FileExplorer3IOCommands
    {

    }
}
