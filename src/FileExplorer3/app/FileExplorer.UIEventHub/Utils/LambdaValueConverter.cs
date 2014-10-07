using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FileExplorer.UIEventHub
{
    public class LambdaValueConverter<T, T1> : IValueConverter
    {
        private Func<T, Type, object, T1> _convertFunc;
        private Func<T1, Type, object, T> _convertBackFunc;

        public LambdaValueConverter(Func<T, Type, object, T1> convertFunc, 
            Func<T1, Type, object, T> convertBackFunc)
        {
            _convertFunc = convertFunc;
            _convertBackFunc = convertBackFunc;
        }

        public LambdaValueConverter(Func<T, T1> convertFunc, 
            Func<T1, T> convertBackFunc)
            : this((t,_, __) => convertFunc(t), (t1, _, __) => convertBackFunc(t1))
        {

        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return _convertFunc((T)value, targetType, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return _convertBackFunc((T1)value, targetType, parameter);
        }
    }
}
