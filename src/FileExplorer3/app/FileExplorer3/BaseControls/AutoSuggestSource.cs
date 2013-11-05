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
    public class AutoSuggestSource : FrameworkElement, ISuggestSource
    {
        #region Constructor
        public AutoSuggestSource(HierarchicalDataTemplate template, object rootData, string valuePath)
        {
            HeaderTemplate = template;
            if (rootData is IEnumerable)
                ItemsSource = rootData as IEnumerable;
            else RootItem = rootData;
            ValuePath = valuePath;
        }

        public AutoSuggestSource()
        {

        }

        #endregion

        #region Methods

        private static string getValuePath(string pathName, char Separator)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return "";
            else return pathName.Substring(0, pathName.LastIndexOf(Separator));
        }
        private static string getValueName(string pathName, char Separator)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return pathName;
            else return pathName.Substring(pathName.LastIndexOf(Separator) + 1);
        }

        private static IEnumerable<object> list(object item, DataTemplate HeaderTemplate)
        {
            var retVal = item is IEnumerable ? item as IEnumerable :
                    PropertyPathHelper.GetValue(item, (HeaderTemplate as HierarchicalDataTemplate).ItemsSource) as IEnumerable;
            return retVal.Cast<object>();
        }

        
        private static object lookup(string path, DataTemplate HeaderTemplate, 
            char Separator, string ValuePath, StringComparison StringComparisonOption,
            IEnumerable ItemsSource, object RootItem)
        {
            var queue = new Queue<string>(path.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries));
            object current = RootItem != null ? RootItem : ItemsSource;
            while (current != null && queue.Any())
            {
                var nextSegment = queue.Dequeue();
                object found = null;
                foreach (var item in list(current, HeaderTemplate))
                {
                    string valuePathName = PropertyPathHelper.GetValue(item, ValuePath) as string;
                    string value = getValueName(valuePathName, Separator); //Value may be full path, or just current value.
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
            string valuePath = getValuePath(input, separator);
            string valueName = getValueName(input, separator);
            if (String.IsNullOrEmpty(valueName) && input.EndsWith(Separator + ""))
                valueName += Separator;
            var found = lookup(valuePath, headerTemplate, separator, valuePath, stringComparisonOption, itemsSource, rootItem);
            List<object> retVal = new List<object>();

            if (found != null)
            {
                foreach (var item in list(found, headerTemplate))
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

        DataTemplate headerTemplate;
        char separator;
        string valuePath;
        StringComparison stringComparisonOption;
        IEnumerable itemsSource;
        object rootItem;

        #endregion

        #region Public Properties
        

        public static DependencyProperty SeparatorProperty = DependencyProperty.Register("Separator", typeof(char),
            typeof(AutoSuggestSource), new PropertyMetadata('\\'));

        public char Separator
        {
            get { return (char)GetValue(SeparatorProperty); }
            set { separator = value; SetValue(SeparatorProperty, value); }
        }

        public static DependencyProperty StringComparisonProperty = DependencyProperty.Register("StringComparisonOption", typeof(StringComparison),
            typeof(AutoSuggestSource), new PropertyMetadata(StringComparison.CurrentCultureIgnoreCase));

        public StringComparison StringComparisonOption
        {
            get { return (StringComparison)GetValue(StringComparisonProperty); }
            set { stringComparisonOption = value; SetValue(StringComparisonProperty, value); }
        }

        public static DependencyProperty ValuePathProperty = SuggestBox.ValuePathProperty.AddOwner(typeof(AutoSuggestSource));

        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { valuePath = value; SetValue(ValuePathProperty, value); }
        }

        public static DependencyProperty ItemsSourceProperty = BreadcrumbCore.ItemsSourceProperty.AddOwner(typeof(AutoSuggestSource));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { itemsSource = value; SetValue(ItemsSourceProperty, value); }
        }

        public static DependencyProperty RootItemProperty = DependencyProperty.Register("RootItem", typeof(object), 
            typeof(AutoSuggestSource), new PropertyMetadata(null));

        public object RootItem
        {
            get { return (object)GetValue(RootItemProperty); }
            set { rootItem = value; SetValue(RootItemProperty, value); }
        }

        public static DependencyProperty HeaderTemplateProperty = BreadcrumbCore.HeaderTemplateProperty
            .AddOwner(typeof(AutoSuggestSource));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { headerTemplate = value; SetValue(HeaderTemplateProperty, value); }
        }

        #endregion





    }
}
