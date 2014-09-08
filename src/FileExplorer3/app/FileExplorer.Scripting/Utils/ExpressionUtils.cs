using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Utils
{
    public static class ExpressionUtils
    {
        //http://www.yoda.arachsys.com/csharp/genericoperators.html
        public static T Add<T>(T a, T b)
        {
            // declare the parameters
            ParameterExpression paramA = System.Linq.Expressions.Expression.Parameter(typeof(T), "a"),
                paramB = System.Linq.Expressions.Expression.Parameter(typeof(T), "b");
            // add the parameters together
            BinaryExpression body;

            body = System.Linq.Expressions.Expression.Add(paramA, paramB);

            // compile it
            Func<T, T, T> add = System.Linq.Expressions.Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
            // call it
            return add(a, b);
        }

        public static T Subtract<T>(T a, T b)
        {
            // declare the parameters
            ParameterExpression paramA = System.Linq.Expressions.Expression.Parameter(typeof(T), "a"),
                paramB = System.Linq.Expressions.Expression.Parameter(typeof(T), "b");
            // add the parameters together
            BinaryExpression body = System.Linq.Expressions.Expression.Subtract(paramA, paramB);
            // compile it
            Func<T, T, T> subtract = System.Linq.Expressions.Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
            // call it
            return subtract(a, b);
        }
    }
}
