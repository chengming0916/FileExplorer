using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    //public static partial class ScriptCommands
    //{
    //    public static IScriptCommand AssignArray(string variable = "{Variable}", 
    //        string arrayTypeVariable = "System.String", object[] values = null, 
    //        bool skipIfExists = false, IScriptCommand nextCommand = null)
    //    {
    //        values = values ?? new object[] { };
    //        return new AssignArray()
    //        {
    //            VariableKey = variable,
    //            ArrayTypeKey = arrayTypeVariable,
    //            SkipIfExists = skipIfExists,
    //            Values = values,
    //            NextCommand = (ScriptCommandBase)nextCommand
    //        };
    //    }

    //}

    //public class AssignArray : ScriptCommandBase
    //{
    //    /// <summary>
    //    /// Variable name to set to, default = "Variable".
    //    /// </summary>
    //    public string VariableKey { get; set; }

    //    /// <summary>
    //    /// TypeName of the array, it's passed to Type.GetType() to get Type.
    //    /// </summary>
    //    public string ArrayTypeKey { get; set; }

    //    /// <summary>
    //    /// The actual value, if a value is string and startWith "{" and endWith "}", it will be lookup from ParameterDic.
    //    /// </summary>
    //    public object[] Values { get; set; }

    //       /// <summary>
    //    /// Whether skip (or override) if key already in dictionary, default = false.
    //    /// </summary>
    //    public bool SkipIfExists { get; set; }

    //       private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<AssignArray>();

    //    public AssignArray()
    //        : base("AssignArray")
    //    {
    //        VariableKey = "{Variable}";
    //        Values = new object[] {};
    //        SkipIfExists = false;
    //    }

    //    public override IScriptCommand Execute(ParameterDic pm)
    //    { 
    //        string typeName = pm.ReplaceVariableInsideBracketed(ArrayTypeKey);
    //        Type type = Type.GetType(typeName, false);

    //        if (type == null)
    //            return ResultCommand.Error(new ArgumentException(typeName as string));

    //        Array retVal = Array.CreateInstance(type, Values.Length);
    //        int idx = 0;
    //        foreach (var v in Values)
    //        {
    //            object value = v is string && (v as string).StartsWith("{") && (v as string).EndsWith("}") ?
    //                pm.GetValue(v as string) : v;
    //            retVal.SetValue(v, idx++);
    //        }

    //        return ScriptCommands.Assign(VariableKey, retVal, SkipIfExists);
    //    }
    //}
}
