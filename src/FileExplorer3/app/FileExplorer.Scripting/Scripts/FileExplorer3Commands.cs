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
    [XmlInclude(typeof(Print))]
    [XmlInclude(typeof(ForEach))]
    [XmlInclude(typeof(IfValue))]
    [XmlInclude(typeof(AssignValueConverter))]
    public class BaseScriptCommands
    {

    }
}
