using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class ScriptCommands
    {
        public static IScriptCommand Reassign(string sourceVariableKey = "{SourceVariable}", string valueConverterKey = null,
            string destinationVariableKey = "{DestinationVariable}", bool skipIfExists = false, IScriptCommand nextCommand = null)
        {
            return new Reassign()
            {
                SourceVariableKey = sourceVariableKey,
                ValueConverterKey = valueConverterKey,
                DestinationVariableKey = destinationVariableKey,
                SkipIfExists = skipIfExists,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }    
    }

    /// <summary>
    /// Serializable, assign value of a variable in ParameterDic to another variable.
    /// </summary>
    public class Reassign : ScriptCommandBase
    {
        /// <summary>
        /// Variable name to obtain from, default = "SourceVariable".
        /// </summary>
        public string SourceVariableKey { get; set; }

        /// <summary>
        /// Func[object,object] to convert SourceVariable to DestinationVariable, default = null.
        /// </summary>
        public string ValueConverterKey { get; set; }

        /// <summary>
        /// Variable name to set to, default = "DestinationVariable".
        /// </summary>
        public string DestinationVariableKey { get; set; }

        /// <summary>
        /// Whether skip (or override) if key already in dictionary, default = false.
        /// </summary>
        public bool SkipIfExists { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<Reassign>();

        public Reassign()
            : base("Reassign")
        {
            SourceVariableKey = "SourceVariable";
            ValueConverterKey = null;
            DestinationVariableKey = "DestinationVariable";
            SkipIfExists = false;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            object value = pm.ReplaceVariableInsideBracketed(SourceVariableKey);

            if (ValueConverterKey != null)
            {
                Func<object, object> valueConverter = pm.GetValue<Func<object, object>>(ValueConverterKey);
                if (valueConverter != null)
                    value = valueConverter(value);
            }

            pm.SetValue(DestinationVariableKey, value, SkipIfExists);

            return base.Execute(pm);
        }
    }


}
