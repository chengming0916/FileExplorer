using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using FileExplorer.Defines;

namespace FileExplorer.UserControls.DragDrop
{
    public interface INotifyDropped
    {
        void NotifyDropped(string format);
    }

    public class VirtualDataObject : System.Windows.Forms.DataObject
    {

        #region Constructor

        public VirtualDataObject(INotifyDropped notifyDropped)
        {
            _notifyDropped = notifyDropped;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overrided GetData to implement dragloop monitoring.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public override object GetData(String format)
        {            
            if (!inDragLoop() && !_notifyDropCalled)
            {
                string s;
                try
                {
                    _notifyDropped.NotifyDropped(format);                    
                    _notifyDropCalled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Drag and Drop failed.");
                    _notifyDropCalled = true;                    
                }                
            }
            return base.GetData(format);
        }

        /// <summary>
        /// return false when if user droped the item, or canceled
        /// </summary>
        /// <returns></returns>
        private bool inDragLoop()
        {
            return (0 != (int)GetData(ShellClipboardFormats.CFSTR_INDRAGLOOP));
        }

        #endregion

        #region Data

        bool _notifyDropCalled = false;
        INotifyDropped _notifyDropped;

        #endregion

        #region Public Properties

        #endregion

    }
}
