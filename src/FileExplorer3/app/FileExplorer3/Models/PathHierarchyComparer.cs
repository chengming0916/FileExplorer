using FileExplorer.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class PathHierarchyComparer : IEntryHierarchyComparer
    {        
        private StringComparison _comparsion;

        public PathHierarchyComparer(StringComparison comparsion)
        {            
            _comparsion = comparsion;
        }

        public HierarchicalResult CompareHierarchy(IEntryModel value1, IEntryModel value2)
        {
            if (value1 == null || value2 == null)
                return HierarchicalResult.Unrelated;

            if (value1.FullPath.Equals(value2.FullPath, _comparsion))
                return HierarchicalResult.Current;

            if (value1.FullPath.StartsWith(value2.FullPath, _comparsion))
                return HierarchicalResult.Child;

            if (value2.FullPath.StartsWith(value1.FullPath, _comparsion))
                return HierarchicalResult.Parent;

            return HierarchicalResult.Unrelated;
        }
    }
}
