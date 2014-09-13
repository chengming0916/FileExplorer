using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileExplorer.WPF.Utils
{
    public static class TypeInfoUtils
    {
        public static object GetPropertyOrMethod(object obj, params string[] pathSplit)
        {
            for (int i = 0; i < pathSplit.Length; i++)
                obj = GetPropertyOrMethod(obj, pathSplit[i]);
            return obj;
        }

        public static void SetProperty(object obj, string name, object value)
        {
            var propertyInfo = obj.GetType().GetTypeInfo().GetPropertyInfoRecursive(name);
            if (propertyInfo == null)
                throw new KeyNotFoundException(name);

            propertyInfo.SetValue(obj, value);            
        }

        public static object GetPropertyOrMethod(object obj, string name)
        {
            var match = Regex.Match(name, RegexPatterns.ParseArrayCounterPattern);
            name = match.Groups["variable"].Value;
            int idx = match.Groups["counter"].Success ? Int32.Parse(match.Groups["counter"].Value) : -1;


            if (name.EndsWith("()"))
            {
                var methodInfo = obj.GetType().GetTypeInfo().GetMethodInfoRecursive(name.TrimEnd('(', ')'));
                if (methodInfo == null)
                    throw new KeyNotFoundException(name);
                else return methodInfo.Invoke(obj, new object[] { });
            }
            else
            {
                var propertyInfo = obj.GetType().GetTypeInfo().GetPropertyInfoRecursive(name);
                if (propertyInfo == null)
                    throw new KeyNotFoundException(name);
                else
                {
                    object retVal = propertyInfo.GetValue(obj);
                    if (retVal is Array && idx != -1)
                        return (retVal as Array).GetValue(idx);
                    else return retVal;
                }
            }
        }

        public static FieldInfo GetFieldInfoRecursive(this TypeInfo typeInfo, string fieldName)
        {
            var retVal = typeInfo.DeclaredFields.FirstOrDefault(pi => pi.Name.Equals(fieldName));
            if (retVal == null && typeInfo.BaseType != null)
            {
                return GetFieldInfoRecursive(typeInfo.BaseType.GetTypeInfo(), fieldName);
            }
            return retVal;
        }

        public static PropertyInfo GetPropertyInfoRecursive(this TypeInfo typeInfo, string propertyName)
        {
            var retVal = typeInfo.DeclaredProperties.FirstOrDefault(pi => pi.Name.Equals(propertyName));
            if (retVal == null && typeInfo.BaseType != null)
            {
                return GetPropertyInfoRecursive(typeInfo.BaseType.GetTypeInfo(), propertyName);
            }
            return retVal;
        }

        public static MethodInfo GetMethodInfoRecursive(this TypeInfo typeInfo, string methodName)
        {
            var retVal = typeInfo.DeclaredMethods.FirstOrDefault(pi => pi.Name.Equals(methodName));
            if (retVal == null && typeInfo.BaseType != null)
            {
                return GetMethodInfoRecursive(typeInfo.BaseType.GetTypeInfo(), methodName);
            }
            return retVal;
        }
    }
}
