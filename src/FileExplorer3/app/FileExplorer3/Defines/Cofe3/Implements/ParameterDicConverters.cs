using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.BaseControls;

namespace Cofe.Core.Script
{

    public static class ParameterDicConverters
    {
        public static IParameterDicConverter ConvertParameterOnly =
            new ParameterDicConverterBase(p => new ParameterDic() { { "Parameter", p } },
                pd => pd.ContainsKey("Parameter") ? pd["Parameter"] : null);

        //public static IParameterDicConverter ConvertUIParameter = 
        //    new ParameterDicConverterBase(p =>
        //        {
        //            var tup = p as Tuple<string, object, EventArgs>; //Event name
        //            return new UIParameterDic() { Sender = tup.Item1, EventArgs = tup.Item2, 
        //        }
    }

    public class ParameterDicConverterBase : IParameterDicConverter
    {
        private Func<ParameterDic, object> _convertBackFunc;
        private Func<object, ParameterDic> _convertFunc;
        private IParameterDicConverter _baseConverter;

        public ParameterDicConverterBase(Func<object, ParameterDic> convertFunc,
            Func<ParameterDic, object> convertBackFunc, IParameterDicConverter baseConverter = null)
        {
            _convertFunc = convertFunc;
            _convertBackFunc = convertBackFunc;
            _baseConverter = baseConverter;
        }

        public ParameterDic Convert(object parameter)
        {
            var retVal = _convertFunc(parameter);
            if (_baseConverter != null)
            {
                var baseRetVal = _baseConverter.Convert(parameter);
                foreach (var k in baseRetVal.Keys)
                    if (!retVal.ContainsKey(k))
                        retVal.Add(k, baseRetVal[k]);
            }
            return retVal;
        }

        public object ConvertBack(ParameterDic pd)
        {
            object retVal =  _convertBackFunc(pd);

            if (retVal == null && _baseConverter != null)
                return _baseConverter.ConvertBack(pd);

            return retVal;
        }
    }


}
