using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FileExplorer.Defines;

namespace FileExplorer.UserControls
{
    public class DisplayTemplateSelector : DataTemplateSelector
    {
        #region Cosntructor

        #endregion

        #region Methods

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var parentItem = UITools.FindAncestor<StatusbarItemEx>(container);
            DataTemplate retVal = null;
            if (parentItem != null)
            {
                DisplayType type = (DisplayType)parentItem.GetValue(StatusbarItemEx.TypeProperty);
                if (type == DisplayType.Auto)
                {
                    if (item is string && (item as string).Any(c => c.Equals('.') || c.Equals('\\'))) type = DisplayType.Filename;
                    else if (item is Int16) type = DisplayType.Percent;
                    else if (item is Int32 || item is Int64) type = DisplayType.Number;
                    else if (item is Boolean) type = DisplayType.Boolean;
                    else type = DisplayType.Text;
                }

                switch (type)
                {
                    case DisplayType.Text: retVal = TextTemplate; break;
                    case DisplayType.Number: retVal = NumberTemplate; break;
                    case DisplayType.Percent: retVal = PercentTemplate; break;
                    case DisplayType.Filename: retVal = FilenameTemplate; break;
                    case DisplayType.Boolean: retVal = BooleanTemplate; break;
                }
            }
            return retVal ?? base.SelectTemplate(item, container);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public DataTemplate TextTemplate { get; set; }
        public DataTemplate NumberTemplate { get; set; }
        public DataTemplate PercentTemplate { get; set; }
        public DataTemplate FilenameTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }

        #endregion
    }
}
