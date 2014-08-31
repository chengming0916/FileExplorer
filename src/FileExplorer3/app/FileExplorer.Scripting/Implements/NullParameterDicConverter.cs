using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public class NullParameterDicConverter : IParameterDicConverter
    {

        public void AddAdditionalParameters(IParameterDic pd)
        {
            
        }

        public IParameterDic Convert(object parameter, params object[] additionalParameters)
        {
            return new ParameterDic();
        }

        public object ConvertBack(IParameterDic pd, params object[] additionalParameters)
        {
            return null;
        }
    }
}
