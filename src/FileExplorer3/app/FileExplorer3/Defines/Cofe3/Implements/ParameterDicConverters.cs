using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FileExplorer.ViewModels;
using FileExplorer.UserControls.InputProcesor;

namespace Cofe.Core.Script
{

    public static class ParameterDicConverters
    {
        public static IParameterDicConverter ConvertParameterOnly =
            new ParameterDicConverterBase((p, p2) => new ParameterDic() { { "Parameter", p } },
                (pd, p2) => pd.ContainsKey("Parameter") ? pd["Parameter"] : null);

        /// <summary>
        /// For CommandViewModel, Convert Sender, EventName, EventArgs to ParameterDic
        /// </summary>
        public static IParameterDicConverter ConvertUIParameter =
            new ParameterDicConverterBase((p, p2) =>
                {
                    if (p2 == null || p2.Length < 3)
                        return ConvertParameterOnly.Convert(p, p2);

                    string eventName = p2[0] as string;
                    object sender = p2[1];
                    RoutedEventArgs eventArgs = p2[2] as RoutedEventArgs;

                    return new FileExplorer.BaseControls.UIParameterDic()
                    {
                        Sender = sender,
                        EventName = eventName,
                        EventArgs = eventArgs
                    };
                }, null, ParameterDicConverters.ConvertParameterOnly);


        /// <summary>
        /// For UIEventHub's UIInput, Convert Sender, Input, InputProcessors to ParameterDic
        /// </summary>
        public static IParameterDicConverter ConvertUIInputParameter =
            new ParameterDicConverterBase((p, p2) =>
            {
                if (p2 == null || p2.Length < 3)
                    return ConvertParameterOnly.Convert(p, p2);

                string eventName = p2[0] as string;
                IUIInput input = p2[1] as IUIInput;
                UIInputManager inputProcManager = p2[2] as UIInputManager;

                return new FileExplorer.BaseControls.UIParameterDic()
                {
                    Sender = input.Sender,
                    EventName = eventName,
                    EventArgs = input.EventArgs,
                    Input = input,
                    InputProcessors = inputProcManager.Processors.ToList()
                };
            }, null, ParameterDicConverters.ConvertParameterOnly);

        /// <summary>
        /// Convert ConvertUIParameter +  parameters specified in viewModelProperties
        /// </summary>
        /// <param name="viewModelProperties"></param>
        /// <returns></returns>
        public static IParameterDicConverter ConvertVMParameter(params Tuple<string, object>[] viewModelProperties)
        {
            return new ParameterDicConverterBase((p, p2) =>
                {
                    var retVal = new VMParameterDic();
                    foreach (var pp in viewModelProperties)
                        retVal.Add(pp.Item1, pp.Item2);
                    return retVal;
                }, null, ParameterDicConverters.ConvertUIParameter);
        }
    }

    public class ParameterDicConverterBase : IParameterDicConverter
    {
        private Func<ParameterDic, object[], object> _convertBackFunc;
        private Func<object, object[], ParameterDic> _convertFunc;
        private IParameterDicConverter _baseConverter;

        public ParameterDicConverterBase(Func<object, object[], ParameterDic> convertFunc,
            Func<ParameterDic, object[], object> convertBackFunc, IParameterDicConverter baseConverter = null)
        {
            _convertFunc = convertFunc;
            _convertBackFunc = convertBackFunc;
            _baseConverter = baseConverter;
        }

        public ParameterDic Convert(object parameter, params object[] additionalParameters)
        {
            var retVal = _convertFunc(parameter, additionalParameters);
            if (_baseConverter != null)
            {
                var baseRetVal = _baseConverter.Convert(parameter, additionalParameters);
                foreach (var k in baseRetVal.Keys)
                    if (!retVal.ContainsKey(k))
                        retVal.Add(k, baseRetVal[k]);
            }
            return retVal;
        }

        public object ConvertBack(ParameterDic pd, params object[] additionalParameters)
        {
            object retVal = _convertBackFunc(pd, additionalParameters);

            if (retVal == null && _baseConverter != null)
                return _baseConverter.ConvertBack(pd, additionalParameters);

            return retVal;
        }
    }


}
