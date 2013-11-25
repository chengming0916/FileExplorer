﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Defines;

namespace FileExplorer.Models
{
    public interface IMetadata : INotifyPropertyChanged
    {
        /// <summary>
        /// Display the metadata in header when expanded.
        /// </summary>
        bool IsHeader { get; }

        /// <summary>
        /// Specify how this item to be displayed.
        /// </summary>
        DisplayType DisplayType { get; }

        /// <summary>
        /// Title of this metadata.
        /// </summary>
        string HeaderText { get; }

        /// <summary>
        /// Value of this metadata.
        /// </summary>
        object Content { get; }
    }
}