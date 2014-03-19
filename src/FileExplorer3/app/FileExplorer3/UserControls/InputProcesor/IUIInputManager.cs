using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorer.UserControls.InputProcesor
{
    public interface IUIInputManager
    {
        IEnumerable<IUIInputProcessor> Processors { get; }
        IUIInput Update(IUIInput input);
    }

    public class UIInputManager : IUIInputManager
    {
        #region Constructors

        public UIInputManager(params IUIInputProcessor[] processors)
        {
            Processors = processors.ToList();
        }

        #endregion

        #region Methods

        public IUIInput Update(IUIInput input)
        {
            foreach (var p in Processors)
                if (p.ProcessAllEvents || p.ProcessEvents.Contains(input.EventArgs.RoutedEvent))
                    input = p.Update(input);
            return input;
        }

        //public void AddPostionAdjust(Type type, Func<Point, FrameworkElement, Point> func)
        //{
        //    if (_positionAdjustDictionary.ContainsKey(type))
        //        _positionAdjustDictionary[type] = func;
        //    else _positionAdjustDictionary.Add(type, func);
        //}

        //public Func<Point, Point> GetPostionAdjust(Type type)
        //{
        //    if (_positionAdjustDictionary.ContainsKey(type))
        //        return _positionAdjustDictionary[type] ;
        //    else return pt => pt;
        //}

        //public Point AdjustPosition(Point pt, FrameworkElement element)

        #endregion

        #region Data

        //private Dictionary<Type, Func<Point, FrameworkElement, Point>> _positionAdjustDictionary
        //     = new Dictionary<Type,Func<Point,FrameworkElement, Point>>();

        #endregion

        #region Public Properties

        //public Dictionary<Type, Func<Point, FrameworkElement, Point>> PositionAdjustDictionary { get { return _positionAdjustDictionary;  } }
        public IEnumerable<IUIInputProcessor> Processors { get; private set; }

        #endregion


    }
}
