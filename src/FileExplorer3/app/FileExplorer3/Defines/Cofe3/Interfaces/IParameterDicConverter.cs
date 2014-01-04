using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofe.Core.Script
{
    /// <summary>
    /// Convert object parameter to ParameterDic
    /// </summary>
    public interface IParameterDicConverter
    {
        ParameterDic Convert(object parameter);
        object ConvertBack(ParameterDic pd);
    }
}
