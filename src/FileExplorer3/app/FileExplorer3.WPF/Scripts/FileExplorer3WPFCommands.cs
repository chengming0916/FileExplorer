using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileExplorer.Script
{
    [XmlInclude(typeof(TabExplorerCloseTab))]
    [XmlInclude(typeof(TabExplorerNewTab))]
    [XmlInclude(typeof(ExplorerGoTo))]
    [XmlInclude(typeof(ExplorerSetParameters))]    
    public class FileExplorer3WPFCommands
    {

    }
}
