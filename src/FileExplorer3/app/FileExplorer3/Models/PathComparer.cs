using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    public class PathComparer : IEntryHierarchyComparer
    {
        #region Cosntructor

        public static PathComparer Default = new PathComparer('\\', StringComparison.InvariantCultureIgnoreCase);

        public PathComparer(char separator, StringComparison comparsion)
        {
            _separator = separator;
            _stringComparsion = comparsion;
        }

        #endregion

        #region Methods

        public HierarchicalResult CompareHierarchy(IEntryModel a, IEntryModel b)
        {
            if (a.FullPath.Equals(b.FullPath, _stringComparsion))
                return HierarchicalResult.Current;

            var aSplit = a.FullPath.Split(_separator);
            var bSplit = b.FullPath.Split(_separator);

            for (int i = 0; i < Math.Min(aSplit.Length, bSplit.Length); i++)
            {
                if (!(aSplit[i].Equals(bSplit[i], _stringComparsion)))
                    return HierarchicalResult.Unrelated;
            }

            if (aSplit.Length > bSplit.Length)
                return HierarchicalResult.Parent;
            else return HierarchicalResult.Child;
        }

        #endregion

        #region Data

        private StringComparison _stringComparsion = StringComparison.InvariantCultureIgnoreCase;
        private char _separator = '\\';

        #endregion

        #region Public Properties

        #endregion

    }
}
