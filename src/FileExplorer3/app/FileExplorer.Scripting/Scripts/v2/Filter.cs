using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{

    public static partial class ScriptCommands
    {
        public static IScriptCommand FilterArray(string sourceArrayVariable = "{SourceArray}", string property = "Property",
             ComparsionOperator op = ComparsionOperator.Equals, string valueVariable = "{Value}",
             string destinationArrayVariable = "{DestinationArray}", IScriptCommand nextCommand = null)
        {
            string currentItemVariable = ParameterDic.CombineVariable(sourceArrayVariable, "Current");
            string currentPropertyVariable = ParameterDic.CombineVariable(sourceArrayVariable, "Current" +
                (property == null ? "" : "." + property));
            string tempArrayVariable = ParameterDic.CombineVariable(sourceArrayVariable, "Temp");

            return ForEach(sourceArrayVariable, currentItemVariable,
                        IfValue(op, currentPropertyVariable, valueVariable,
                            ConcatArray(tempArrayVariable, new object[] { currentItemVariable }, tempArrayVariable)),
                        Assign(destinationArrayVariable, tempArrayVariable, false, nextCommand));
        }



        public static IScriptCommand FilterArray<T>(string sourceArrayVariable = "{SourceArray}", string property = "Property",
               ComparsionOperator op = ComparsionOperator.Equals, T value = default(T),
               string destinationArrayVariable = "{DestinationArray}", IScriptCommand nextCommand = null)
        {
            string valueVariable = ParameterDic.CombineVariable(sourceArrayVariable, "ValueCompare");
            return Assign(valueVariable, value, false, 
                FilterArray(sourceArrayVariable, property, op, valueVariable, destinationArrayVariable, nextCommand));
        }
    }
}
