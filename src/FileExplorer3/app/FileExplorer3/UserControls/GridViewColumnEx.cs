﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.UserControls
{
    public class GridViewColumnEx : GridViewColumn
    {
        #region Cosntructor

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public static readonly DependencyProperty ColumnIdProperty =
            DependencyProperty.Register("ColumnId", typeof(string), typeof(GridViewColumnEx), new PropertyMetadata(""));

        /// <summary>
        /// Used to identify a GridViewColumn
        /// </summary>
        public string ColumnId
        {
            get { return (string)GetValue(ColumnIdProperty); }
            set { SetValue(ColumnIdProperty, value); }
        }

        #endregion

    }
}
