using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    
    /// <summary>
    /// Used by DirectoryTree and Breadcrumb to identify the location of appropriate model.
    /// </summary>
    public interface IEntryHierarchyComparer
    {
        HierarchicalResult CompareHierarchy(IEntryModel a, IEntryModel b);
    }
}
