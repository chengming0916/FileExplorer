using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.UserControls.InputProcesor
{
    public interface IInputProcessorManager
    {
        IEnumerable<IInputProcessor> Processors { get; }
        void Update(IInput input);
    }

    public class InputProcessorManager : IInputProcessorManager
    {
        #region Constructors

        public InputProcessorManager(params IInputProcessor[] processors)
        {
            Processors = processors.ToList();
        }

        #endregion

        #region Methods

        public void Update(IInput input)
        {
            foreach (var p in Processors)
                p.Update(input);
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IEnumerable<IInputProcessor> Processors { get; private set; }

        #endregion

       
    }
}
