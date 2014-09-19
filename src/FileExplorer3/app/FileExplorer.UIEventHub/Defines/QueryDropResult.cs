using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UIEventHub
{

    public class QueryDropResult
    {
        public QueryDropResult(DragDropEffects supportedEffects, DragDropEffects preferredEffects)
        {
            SupportedEffects = supportedEffects;
            PreferredEffect = preferredEffects;
        }

        public static QueryDropResult None = new QueryDropResult(DragDropEffects.None, DragDropEffects.None);

        public static QueryDropResult CreateNew(DragDropEffects supportedEffects, DragDropEffects preferredEffects)
        {
            return new QueryDropResult(supportedEffects, preferredEffects);
        }

        public static QueryDropResult CreateNew(DragDropEffects supportedEffects)
        {
            return new QueryDropResult(supportedEffects, supportedEffects);
        }


        public DragDropEffects SupportedEffects { get; set; }
        public DragDropEffects PreferredEffect { get; set; }
    }

  
}
