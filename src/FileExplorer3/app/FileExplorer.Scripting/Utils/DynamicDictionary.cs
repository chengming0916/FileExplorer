using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Utils
{
    //http://reyrahadian.wordpress.com/2012/02/01/creating-a-dynamic-dictionary-with-c-4-dynamic/
    //http://blog.lab49.com/archives/3893
    public class DynamicDictionary<TValue> : DynamicObject, INotifyPropertyChanged
    {
        private Dictionary<string, TValue> _dictionary;
        protected IEqualityComparer<string> _comparer;

        public Dictionary<string, TValue> Dictionary { get { return _dictionary; } }
        

        public DynamicDictionary(IEqualityComparer<string> comparer)
        {
            _dictionary = new Dictionary<string, TValue>(comparer);
            _comparer = comparer;
        }

        public DynamicDictionary()
            : this(StringComparer.CurrentCulture)
        {

        }

        public TValue this[string key] { get { return _dictionary[key]; }
            set
            {
                if (_dictionary.ContainsKey(key))
                    _dictionary[key] = value;
                else _dictionary.Add(key, value);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            TValue data;
            if (!_dictionary.TryGetValue(binder.Name, out data))
            {
                throw new KeyNotFoundException("There's no key by that name");
            }
            result = (TValue)data;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dictionary.ContainsKey(binder.Name))
            {
                _dictionary[binder.Name] = (TValue)value;
            }
            else
            {
                _dictionary.Add(binder.Name, (TValue)value);
            }
            FirePropertyChanged(binder.Name);
            return true;
        }

        public void FirePropertyChanged(string propName)
        {
            var propChange = PropertyChanged;
            if (propChange != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

         public event PropertyChangedEventHandler PropertyChanged;
    }
}
