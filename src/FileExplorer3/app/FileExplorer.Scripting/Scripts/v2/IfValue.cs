using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class ScriptCommands
    {
        /// <summary>
        /// Serializable, Use Expression to compare two value in ParameterDic, and run different command based on the result.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="variable1"></param>
        /// <param name="variable2"></param>
        /// <param name="trueCommand"></param>
        /// <param name="otherwiseCommand"></param>
        /// <returns></returns>
        public static IScriptCommand IfValue(ComparsionOperator op, string variable1 = "{Variable1}", 
            string variable2 = "{Variable2}", IScriptCommand trueCommand = null, IScriptCommand otherwiseCommand = null)
        {
            return new IfValue()
            {
                Variable1Key = variable1,
                Variable2Key = variable2,
                Operator = op,
                NextCommand = (ScriptCommandBase)trueCommand,
                OtherwiseCommand = (ScriptCommandBase)otherwiseCommand,
            };
        }

        /// <summary>
        /// Serializable, Run IfValue comparsion based on the length of an array in ParameterDic.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="arrayVariable"></param>
        /// <param name="valueVariable"></param>
        /// <param name="trueCommand"></param>
        /// <param name="otherwiseCommand"></param>
        /// <returns></returns>
        public static IScriptCommand IfArrayLength(ComparsionOperator op, string arrayVariable = "{array}", 
            string valueVariable = "{value}",
            IScriptCommand trueCommand = null, IScriptCommand otherwiseCommand = null)
        {
            return ScriptCommands.AssignValueConverter(ValueConverterType.GetProperty, "{GetPropertyConverter}", 
                ScriptCommands.Reassign(arrayVariable, "{GetPropertyConverter}", "{ArrayLength}", false, 
                  ScriptCommands.PrintLogger(MetroLog.LogLevel.Debug, "Length of array is {ArrayLength}",  
                  ScriptCommands.IfValue(op, "{ArrayLength}", valueVariable, trueCommand, otherwiseCommand))), "Length");
        }

        public static IScriptCommand IfArrayLength(ComparsionOperator op = ComparsionOperator.GreaterThanOrEqual, 
            string arrayVariable = "{array}", int value = 1,
            IScriptCommand trueCommand = null, IScriptCommand otherwiseCommand = null)
        {
            return 
                ScriptCommands.Assign("{ArrayLengthValue}", value, false, 
                  IfArrayLength(op, arrayVariable, "{ArrayLengthValue}", trueCommand, otherwiseCommand));
        }
    }

    public enum ComparsionOperator {  Equals, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual }
    public class IfValue : ScriptCommandBase
    {
        public ScriptCommandBase OtherwiseCommand { get; set; }

        public string Variable1Key { get; set; }

        public ComparsionOperator Operator { get; set; }

        public string Variable2Key { get; set; }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            object value1 = pm.GetValue<Object>(Variable1Key);
            object value2 = pm.GetValue<Object>(Variable2Key);

            bool result = false;
            if (value1 != null && value2 != null)
            {
                var left = Expression.Constant(value1);
                var right = Expression.Constant(value2);
                Expression expression;

                switch (Operator)
                {
                    case ComparsionOperator.Equals: expression = Expression.Equal(left, right); break;
                    case ComparsionOperator.GreaterThan: expression = Expression.GreaterThan(left, right); break;
                    case ComparsionOperator.GreaterThanOrEqual: expression = Expression.GreaterThanOrEqual(left, right); break;
                    case ComparsionOperator.LessThan: expression = Expression.LessThan(left, right); break;
                    case ComparsionOperator.LessThanOrEqual: expression = Expression.LessThanOrEqual(left, right); break;
                    default:
                        throw new NotSupportedException(Operator.ToString());
                }

                result = Expression.Lambda<Func<bool>>(expression).Compile().Invoke();

            }

            return result ? NextCommand : OtherwiseCommand;                
        }

    }
}
