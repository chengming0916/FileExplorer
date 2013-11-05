using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using FileExplorer.Utils;

namespace FileExplorer.BaseControls
{
    public class AutoSuggestSource : DependencyObject, ISuggestSource
    {
        #region Constructor
        public AutoSuggestSource(HierarchicalDataTemplate template, object rootData, string valuePath)
        {
            HeaderTemplate = template;
            ItemsSource = rootData as IEnumerable;
            ValuePath = valuePath;
        }

        public AutoSuggestSource()
        {

        }

        #endregion

        #region Methods

        private string getValuePath(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return "";
            else return pathName.Substring(0, pathName.LastIndexOf(Separator));
        }
        private string getValueName(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return pathName;
            else return pathName.Substring(pathName.LastIndexOf(Separator) + 1);
        }

        private IEnumerable<object> list(object item)
        {
            var retVal = item is IEnumerable ? item as IEnumerable :
                    PropertyPathHelper.GetValue(item, (HeaderTemplate as HierarchicalDataTemplate).ItemsSource) as IEnumerable;
            return retVal.Cast<object>();
        }

        
        private object lookup(string path)
        {
            var queue = new Queue<string>(path.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries));
            object current = ItemsSource;
            while (current != null && queue.Any())
            {
                var nextSegment = queue.Dequeue();
                object found = null;
                //var itemList = 
                //    current is IEnumerable ? current as IEnumerable : 
                //    PropertyPathHelper.GetValue(current, _template.ItemsSource) as IEnumerable;
                foreach (var item in list(current))
                {
                    string valuePathName = PropertyPathHelper.GetValue(item, ValuePath) as string;
                    string value = getValueName(valuePathName); //Value may be full path, or just current value.
                    if (value.Equals(nextSegment, StringComparisonOption))
                    {
                        found = item;
                        break;
                    }
                }
                current = found;
            }
            return current;
        }

        public Task<IList<object>> SuggestAsync(string input)
        {
            string valuePath = getValuePath(input);
            string valueName = getValueName(input);
            if (String.IsNullOrEmpty(valueName) && input.EndsWith(Separator + ""))
                valueName += Separator;
            var found = lookup(valuePath);
            List<object> retVal = new List<object>();

            if (found != null)
            {
                foreach (var item in list(found))
                {
                    string valuePathName = PropertyPathHelper.GetValue(item, ValuePath) as string;
                    if (valuePathName.StartsWith(input, StringComparisonOption) &&
                        !valuePathName.Equals(input, StringComparisonOption))
                        retVal.Add(item);
                }
            }



            return Task.FromResult<IList<object>>(retVal);

        }

        #endregion

        #region Data



        #endregion

        #region Public Properties
        

        public static DependencyProperty SeparatorProperty = DependencyProperty.Register("Separator", typeof(char),
            typeof(AutoSuggestSource), new PropertyMetadata('\\'));

        public char Separator
        {
            get { return (char)GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
        }

        public static DependencyProperty StringComparisonProperty = DependencyProperty.Register("StringComparisonOption", typeof(StringComparison),
            typeof(AutoSuggestSource), new PropertyMetadata(StringComparison.CurrentCultureIgnoreCase));

        public StringComparison StringComparisonOption
        {
            get { return (StringComparison)GetValue(StringComparisonProperty); }
            set { SetValue(StringComparisonProperty, value); }
        }

        public static DependencyProperty ValuePathProperty = SuggestBox.ValuePathProperty.AddOwner(typeof(AutoSuggestSource));

        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public static DependencyProperty ItemsSourceProperty = BreadcrumbCore.ItemsSourceProperty.AddOwner(typeof(AutoSuggestSource));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static DependencyProperty HeaderTemplateProperty = BreadcrumbCore.HeaderTemplateProperty
            .AddOwner(typeof(AutoSuggestSource));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        #endregion





    }
}
