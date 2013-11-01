﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace FileExplorer.ViewModels
{
    public class ViewModeViewModel : PropertyChangedBase
    {
        #region Cosntructor

        public ViewModeViewModel(string viewMode)
        {
            _view = viewMode;
            _viewModeIcon = new Lazy<ImageSource>(
            () =>
                {
                    try
                    {
                        Stream imgStream = Application.GetResourceStream(
                            new Uri(String.Format(iconPathMask, ViewMode.ToLower()))).Stream;
                        BitmapDecoder decoder = IconBitmapDecoder.Create(imgStream, BitmapCreateOptions.None, BitmapCacheOption.None);
                        return decoder.Frames[0];
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
            });
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            return (obj is ViewModeViewModel) && (obj as ViewModeViewModel).View == View;
        }

        #endregion

        #region Data

        public static string iconPathMask = 
            "pack://application:,,,/FileExplorer3;component/Themes/Resources/ViewMode/{0}_16.png";
        private string _view;
        private Lazy<ImageSource> _viewModeIcon;

        #endregion

        #region Public Properties

        public string View { get { return _view; } set { _view = value; NotifyOfPropertyChange(() => View); } }
        public string ViewMode { get { return _view.Replace("View", ""); } }
        public Lazy<ImageSource> ViewModeIcon { get { return _viewModeIcon; } }


        #endregion
    }
}
