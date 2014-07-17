using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileExplorer.Script
{
    [XmlInclude(typeof(Assign))]
    [XmlInclude(typeof(RunCommands))]
    [XmlInclude(typeof(ResultCommand))]
    [XmlInclude(typeof(ParsePath))]
    [XmlInclude(typeof(DiskCreatePath))]
    [XmlInclude(typeof(DiskOpenStream))]
    [XmlInclude(typeof(CopyStream))]
    [XmlInclude(typeof(Download))]
    [XmlInclude(typeof(Print))]
    [XmlInclude(typeof(ForEach))]
    [XmlInclude(typeof(NotifyChanged))]
    public class FileExplorer3Commands
    {

    }
}
