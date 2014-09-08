﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using FileExplorer.WPF.Utils;
using System.Linq.Expressions;
using FileExplorer.Utils;

namespace FileExplorer.Script
{


    public static partial class ScriptCommands
    {
        /// <summary>
        /// Serializable, Assign a value converter (Func[Object,Object]) to destination variable.
        /// </summary>
        /// <param name="converterType"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand AssignValueConverter(ValueConverterType converterType = ValueConverterType.GetProperty,
            string destinationVariable = "{Converter}", IScriptCommand nextCommand = null,
            params object[] converterParameter)
        {
            return new AssignValueConverter()
            {
                VariableKey = destinationVariable,
                ConverterType = converterType,
                ConverterParameter = converterParameter,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Serializable, shortcut method for [AssignValueConverter], which a specific item from an array from a variable and assign to another variable.
        /// </summary>
        /// <param name="arrayVariable"></param>
        /// <param name="id"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand AssignArrayItem(string arrayVariable = "{Array}", int id = 0,
            string destinationVariable = "{Destination}", IScriptCommand nextCommand = null)
        {
            string valueConverterVariable = ParameterDic.CombineVariable(arrayVariable, "Converter");
            return AssignValueConverter(ValueConverterType.GetArrayItem, valueConverterVariable,
                ScriptCommands.Reassign(arrayVariable, valueConverterVariable,
                    destinationVariable, false, nextCommand), id);
        }

        /// <summary>
        /// Serializable, shortcut method for [AssignValueConverter], which obtains method result of a property from a variable and assign to another variable.
        /// </summary>
        /// <param name="sourceObjectVariable"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand AssignMethodResult(string sourceObjectVariable = "{Source}",
            string methodName = "Method", object[] parameters = null,
            string destinationVariable = "{Destination}", IScriptCommand nextCommand = null)
        {
            string valueConverterVariable = ParameterDic.CombineVariable(sourceObjectVariable, "Converter");

            List<object> methodParams = new List<object>();
            methodParams.Add(methodName);
            if (parameters != null)
                methodParams.AddRange(parameters);

            return AssignValueConverter(ValueConverterType.ExecuteMethod, valueConverterVariable,
                ScriptCommands.Reassign(sourceObjectVariable, valueConverterVariable,
                    destinationVariable, false, nextCommand), methodParams.ToArray());
        }

        /// <summary>
        /// Serializable, AssignMethodResult but no return variable.
        /// </summary>
        /// <param name="sourceObjectVariable"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand ExecuteMethod(string sourceObjectVariable = "{Source}",
            string methodName = "Method", object[] parameters = null, IScriptCommand nextCommand = null)
        {
            return AssignMethodResult(sourceObjectVariable, methodName, parameters, null, nextCommand);
        }

        /// <summary>
        /// Serializable, shortcut method for [AssignValueConverter], which obtains value of a property from a variable and assign to another variable.
        /// </summary>
        /// <param name="sourceObjectVariable"></param>
        /// <param name="propertyName"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand AssignProperty(string sourceObjectVariable = "{Source}",
            string propertyName = "Property",
            string destinationVariable = "{Destination}", IScriptCommand nextCommand = null)
        {
            string valueConverterVariable = ParameterDic.CombineVariable(sourceObjectVariable, "Converter");
            return AssignValueConverter(ValueConverterType.GetProperty, valueConverterVariable,
                ScriptCommands.Reassign(sourceObjectVariable, valueConverterVariable,
                    destinationVariable, false, nextCommand), propertyName);
        }

        public static IScriptCommand SetProperty(string sourceObjectVariable = "{Source}",
           string propertyName = "Property",
           string valueVariable = "{Value}", IScriptCommand nextCommand = null)
        {
            string valueConverterVariable = ParameterDic.CombineVariable(sourceObjectVariable, "Converter");
            return AssignValueConverter(ValueConverterType.SetProperty, valueConverterVariable,
                ScriptCommands.Reassign(sourceObjectVariable, valueConverterVariable,
                    valueVariable, false, nextCommand), propertyName);
        }

        public static IScriptCommand SetProperty<T>(string sourceObjectVariable = "{Source}",
         string propertyName = "Property",
         T value = default(T), IScriptCommand nextCommand = null)
        {
            string valueConverterVariable = ParameterDic.CombineVariable(sourceObjectVariable, "Converter");
            string valueVariable = ParameterDic.CombineVariable(sourceObjectVariable, "Value");
            return
                Assign(valueVariable, value, false,
                AssignValueConverter(ValueConverterType.SetProperty, valueConverterVariable,
                ScriptCommands.Reassign(sourceObjectVariable, valueConverterVariable,
                    valueVariable, false, nextCommand), propertyName));
        }

        /// <summary>
        /// Add variables (using Expression) to destination.
        /// </summary>
        /// <param name="sourceObjectVariable"></param>
        /// <param name="addValues"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand Add(string sourceObjectVariable = "{Source}",
            object[] addValues = null,
            string destinationVariable = "{Destination}", IScriptCommand nextCommand = null)
        {
            string valueConverterVariable = ParameterDic.CombineVariable(sourceObjectVariable, "Converter");
            return AssignValueConverter(ValueConverterType.AddValue, valueConverterVariable,
                ScriptCommands.Reassign(sourceObjectVariable, valueConverterVariable,
                    destinationVariable, false, nextCommand),  addValues);
        }


        /// <summary>
        /// Concat array to destination
        /// </summary>
        /// <param name="sourceObjectVariable"></param>
        /// <param name="addValues"></param>
        /// <param name="destinationVariable"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand ConcatArray(string sourceObjectVariable = "{Source}",
           object[] addValues = null,
           string destinationVariable = "{Destination}", IScriptCommand nextCommand = null)
        {
            string valueConverterVariable = ParameterDic.CombineVariable(sourceObjectVariable, "Converter");
            return AssignValueConverter(ValueConverterType.ConcatArray, valueConverterVariable,
                ScriptCommands.Reassign(sourceObjectVariable, valueConverterVariable,
                    destinationVariable, false, nextCommand), addValues);
        }

    }

    public enum ValueConverterType
    {
        /// <summary>
        /// Given an Array, get specific item.
        /// </summary>
        GetArrayItem, 
        GetProperty, 
        ExecuteMethod, 
        SetProperty, 
        AddValue, 
        ConcatArray
    }

    public class AssignValueConverter : Assign
    {

        /// <summary>
        /// Specify which converter to use, default = GetProperty
        /// </summary>
        public ValueConverterType ConverterType { get; set; }

        /// <summary>
        /// Parameter used by the converter, default = null.
        /// </summary>
        public object[] ConverterParameter { get; set; }


        public AssignValueConverter()
            : base("AssignValueConverter")
        {
            ConverterType = ValueConverterType.GetProperty;
            ConverterParameter = null;

            VariableKey = "{Converter}";
            Value = null;
            SkipIfExists = false;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
             Func<object, object> checkParameters = p =>
                        {
                            if (p is string)
                            {
                                string pString = p as string;
                                if (pString.StartsWith("{") && pString.EndsWith("}"))
                                    return pm.GetValue(pString);
                            }
                            return p;
                        };

            switch (ConverterType)
            {
                case ValueConverterType.GetArrayItem:
                    Int32 id = ConverterParameter.Count() > 0 ? Int32.Parse(ConverterParameter.First().ToString()) : -1;
                    if (id == -1)
                        return ResultCommand.Error(new KeyNotFoundException());
                    Value = (Func<Object, Object>)(v => (v as Array).GetValue(id));
                    break;

                case ValueConverterType.GetProperty:
                    string property = ConverterParameter.Count() > 0 ? ConverterParameter.First() as string : null;
                    Func<Object, Object> retVal = String.IsNullOrEmpty(property) ?
                        (Func<Object, Object>)(v => v) :
                        (Func<Object, Object>)(v =>
                            {
                                var typeInfo = v is Array ? typeof(Array).GetTypeInfo() : v.GetType().GetTypeInfo();
                                var propertyInfo = typeInfo.GetPropertyInfoRecursive(property);
                                if (propertyInfo == null)
                                {
                                    var fieldInfo = typeInfo.GetFieldInfoRecursive(property);

                                    if (fieldInfo == null)
                                        throw new KeyNotFoundException(String.Format("{0} cannot be found in {1}", property, v.GetType()));
                                    else return fieldInfo.GetValue(v);
                                }
                                else return propertyInfo.GetValue(v);
                            });
                    Value = retVal;


                    break;
                case ValueConverterType.SetProperty:
                    string property1 = ConverterParameter.Count() > 0 ? ConverterParameter.First() as string : null;
                    Action<Object, Object> retVal1 = String.IsNullOrEmpty(property1) ?
                        (Action<Object, Object>)((o, v) => { }) :
                        (Action<Object, Object>)((o, v) =>
                            {
                                var typeInfo = o is Array ? typeof(Array).GetTypeInfo() : o.GetType().GetTypeInfo();
                                var propertyInfo = typeInfo.GetPropertyInfoRecursive(property1);
                                if (propertyInfo == null)
                                {
                                    var fieldInfo = typeInfo.GetFieldInfoRecursive(property1);

                                    if (fieldInfo == null)
                                        throw new KeyNotFoundException(String.Format("{0} cannot be found in {1}", property1, o.GetType()));

                                    fieldInfo.SetValue(o, v);
                                }
                                else propertyInfo.SetValue(o, v);
                            });
                    Value = retVal1;
                    break;
                case ValueConverterType.ExecuteMethod:
                    string methodName = ConverterParameter.Count() > 0 ? ConverterParameter.First() as string : null;
                   

                    object[] methodParameters = ConverterParameter.Skip(1)
                        .Select(param => checkParameters(param))
                        .ToArray();
                    Func<Object, Object> retVa1l = String.IsNullOrEmpty(methodName) ?
                        (Func<Object, Object>)(v => v) :
                        (Func<Object, Object>)(v =>
                            {
                                var typeInfo = v is Array ? typeof(Array).GetTypeInfo() : v.GetType().GetTypeInfo();
                                var methodInfo = typeInfo.DeclaredMethods
                                  .FirstOrDefault(pi => pi.Name.Equals(
                                      methodName, StringComparison.CurrentCultureIgnoreCase));
                                if (methodInfo == null)
                                    throw new KeyNotFoundException(methodName);

                                else return methodInfo.Invoke(v, methodParameters);
                            });
                    Value = retVa1l;

                    break;

                case ValueConverterType.AddValue:
                    Func<Object, Object> retVa12 = (Func<Object, Object>)(v => 
                        {
                              var mInfo = typeof(FileExplorer.Utils.ExpressionUtils)
                                .GetRuntimeMethods().First(m => m.Name == "Add")
                                .MakeGenericMethod(v.GetType());
                                foreach (var addItem in ConverterParameter.Select(p => checkParameters(p)).ToArray())
                                    v = mInfo.Invoke(null, new object[] { v, addItem});
                                return v;
                        });                                      
                    Value = retVa12;
                    break;

                case ValueConverterType.ConcatArray :

                     Func<Object, Object> retVa13 = (Func<Object, Object>)(v => 
                        {
                            var parameters = ConverterParameter.Select(p => checkParameters(p)).ToArray();
                            if (v == null)
                                v = Array.CreateInstance(parameters.First().GetType(), new int[] { 0 });

                            List<object> items2Add = new List<object>();
                            foreach (var item in parameters)
                                if (item is Array)
                                    items2Add.AddRange((item as Array).Cast<object>());
                                else items2Add.Add(item);

                            Array array = v as Array;
                            Type arrayType = array.GetType().GetElementType();
                            Array newArray = Array.CreateInstance(arrayType, array.Length + items2Add.Count());
                            Array.Copy(array, 0, newArray, 0, array.Length);
                            Array.Copy(items2Add.ToArray(), 0, newArray, array.Length, items2Add.Count());
                            return newArray;
                        });                                      
                    Value = retVa13;

                    break;
                default: return ResultCommand.Error(new NotSupportedException(ConverterType.ToString()));

            }

            return base.Execute(pm);
        }


        //private static object add(object obj1, params object[] addItems)
        //{
        //    if (obj1 == null)
        //    {
        //        return add(addItems[0], addItems.Skip(0).ToArray());
        //    }

        //    if (obj1 is Array)
        //    {
        //        List<object> items2Add = new List<object>();
        //        foreach (var item in addItems)
        //            if (item is Array)
        //                items2Add.AddRange((item as Array).Cast<object>());
        //            else items2Add.Add(item);

        //        Array array = obj1 as Array;
        //        Type arrayType = array.GetType().GetElementType();
        //        Array newArray = Array.CreateInstance(arrayType, array.Length + items2Add.Count());
        //        Array.Copy(array, 0, newArray, 0, array.Length);
        //        Array.Copy(items2Add.ToArray(), 0, newArray, array.Length, items2Add.Count());
        //        return newArray;
        //    }
        //    else
        //    {
                
        //        var mInfo = typeof(FileExplorer.Utils.ExpressionUtils)
        //        .GetRuntimeMethods().First(m => m.Name == "Add")
        //        .MakeGenericMethod(typeof(int));
        //        foreach (var addItem in addItems)
        //            obj1 = mInfo.Invoke(null, new object[] {  obj1, addItem});
        //        return obj1;
        //    }
        //}
    }


}
