﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.UIEventHub
{
    /// <summary>
    /// Indicate the item is a view model participate in UIEventHub's processing.
    /// </summary>
    public interface IUIAware
    {
        string DisplayName { get;  }
    }
}
