using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class FileExplorer3Commands
    {

    }

    public class ScriptCommandSerializer : XmlSerializer
    {
        public ScriptCommandSerializer(params Type[] commandTypes)
            : base(typeof(ScriptCommandBase), commandTypes)
        {

        }

        public Stream SerializeScriptCommand(IScriptCommand command)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter myWriter = new StreamWriter(ms);
            Serialize(myWriter, command);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public IScriptCommand DeserializeScriptCommand(Stream stream)
        {
            return Deserialize(stream) as ScriptCommandBase;
        }
    }   
}
