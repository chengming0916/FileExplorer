using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Defines
{
    public class QueryDropResult
    {
        public QueryDropResult(DragDropResult supportedEffects, DragDropResult preferredEffects)
        {
            SupportedEffects = supportedEffects;
            PreferredEffect = preferredEffects;
        }

        public static QueryDropResult None = new QueryDropResult(DragDropResult.None, DragDropResult.None);

        public static QueryDropResult CreateNew(DragDropResult supportedEffects, DragDropResult preferredEffects)
        {
            return new QueryDropResult(supportedEffects, preferredEffects);
        }

        public static QueryDropResult CreateNew(DragDropResult supportedEffects)
        {
            return new QueryDropResult(supportedEffects, supportedEffects);
        }


        public DragDropResult SupportedEffects { get; set; }
        public DragDropResult PreferredEffect { get; set; }
    }
}
