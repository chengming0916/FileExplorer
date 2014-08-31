using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer
{
    public interface IParameterDic : IDictionary<string, object>
    {
        bool HasValue<T>(string variableKey);
        bool HasValue(string variableKey);
        T GetValue<T>(string variableKey, T defaultValue);
        T GetValue<T>(string variableKey);
        object GetValue(string variableKey);
        bool ClearValue(string variableKey);
        bool SetValue<T>(string variableKey, T value, bool skipIfExists = false);
        IParameterDic Clone();
        Exception Error { get; set; }
        bool IsHandled { get; set; }
        object Parameter { get; set; }
        CancellationToken CancellationToken { get; set; }
    }
}
