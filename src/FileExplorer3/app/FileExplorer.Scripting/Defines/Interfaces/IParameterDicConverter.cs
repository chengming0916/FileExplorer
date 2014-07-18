using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    /// <summary>
    /// Convert object parameter to ParameterDic
    /// </summary>
    public interface IParameterDicConverter
    {
        ParameterDic Convert(object parameter, params object[] additionalParameters);
        object ConvertBack(ParameterDic pd, params object[] additionalParameters);
    }
}
