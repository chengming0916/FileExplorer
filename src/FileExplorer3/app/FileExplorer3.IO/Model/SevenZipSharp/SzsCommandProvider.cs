using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models.SevenZipSharp
{
    public class SzsCommandProvider : StaticCommandProvider
    {
        public SzsCommandProvider()
            : base(


            )
        {

        }

    }

    /// <summary>
    /// Transfer from a folder (Source:IEntryModel) to dest folder (from destPathFunc)
    /// </summary>
    public class ExtractDirectoryCommandModel : DirectoryCommandModel
    {
        private Func<IEntryModel, string> _destPathFunc;
        public ExtractDirectoryCommandModel(Func<ParameterDic, IEntryModel[]> srcModelFunc)
            : base(ResultCommand.NoError)
        {
            IsHeaderVisible = true;
            Header = "Extract";
            //Func<IEntryModel, string> destPathFunc, 
            //_destPathFunc = destPathFunc;
        }

        public override void NotifySelectionChanged(IEntryModel[] appliedModels)
        {
            List<ICommandModel> subCommands = new List<ICommandModel>();

            if (appliedModels.Length >= 1)
            {

                if (appliedModels.Length == 1 && appliedModels[0] is SzsRootModel)
                {
                    SzsRootModel root = appliedModels[0] as SzsRootModel;

                    //Extract to parent
                    subCommands.Add(new CommandModel(
                        ScriptCommands.List(root, null, true, ems =>
                        ScriptCommands.ForEach(ems, em => IOScriptCommands.Transfer(em, root.Parent)))) { Header = "Here...", IsEnabled = true });
                }

                
                SubCommands = subCommands;
            }

            IsVisibleOnToolbar = IsEnabled = SubCommands.Count > 0;
        }


    }

    //public class ExtractToCommandModel : CommandModel
    //{
    //    private Func<IEntryModel, string> _destPathFunc;
    //    public ExtractToCommandModel(Func<ParameterDic, IEntryModel[]> srcModelFunc)
    //        : base(ResultCommand.NoError)
    //    {

    //        //Func<IEntryModel, string> destPathFunc, 
    //        //_destPathFunc = destPathFunc;
    //    }

        

    //}
}
