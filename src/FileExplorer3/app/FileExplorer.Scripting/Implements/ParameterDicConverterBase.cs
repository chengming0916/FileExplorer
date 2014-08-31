using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public class ParameterDicConverterBase : IParameterDicConverter
    {
        private Func<IParameterDic, object[], object> _convertBackFunc;
        private Func<object, object[], IParameterDic> _convertFunc;
        private IParameterDicConverter _baseConverter;
        private IParameterDic _additionalParameters = new ParameterDic();

        public ParameterDicConverterBase(Func<object, object[], IParameterDic> convertFunc,
            Func<IParameterDic, object[], object> convertBackFunc, IParameterDicConverter baseConverter = null)
        {
            _convertFunc = convertFunc;
            _convertBackFunc = convertBackFunc;
            _baseConverter = baseConverter;
        }

        public void AddAdditionalParameters(IParameterDic pd)
        {
            _additionalParameters = ParameterDic.Combine(_additionalParameters, pd);
        }

        public IParameterDic Convert(object parameter, params object[] additionalParameters)
        {
            var retVal = _convertFunc(parameter, additionalParameters);
            if (_baseConverter != null)
            {
                var baseRetVal = _baseConverter.Convert(parameter, additionalParameters);
                foreach (var k in baseRetVal.Keys)
                    if (!retVal.ContainsKey(k))
                        retVal.Add(k, baseRetVal[k]);
                foreach (var k in _additionalParameters.Keys)
                    if (!retVal.ContainsKey(k))
                        retVal.Add(k, _additionalParameters[k]);
            }
            return retVal;
        }

        public object ConvertBack(IParameterDic pd, params object[] additionalParameters)
        {
            object retVal = _convertBackFunc(pd, additionalParameters);

            if (retVal == null && _baseConverter != null)
                return _baseConverter.ConvertBack(pd, additionalParameters);

            return retVal;
        }

    }
}
