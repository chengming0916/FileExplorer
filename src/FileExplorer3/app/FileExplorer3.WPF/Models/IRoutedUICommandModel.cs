﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.Models
{
    public interface IRoutedCommandModel : ICommandModel
    {
        ICommand RoutedCommand { get; set; }
    }
}
