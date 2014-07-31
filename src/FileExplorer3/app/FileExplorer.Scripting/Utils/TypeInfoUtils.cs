using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Utils
{
    public static class TypeInfoUtils
    {
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
