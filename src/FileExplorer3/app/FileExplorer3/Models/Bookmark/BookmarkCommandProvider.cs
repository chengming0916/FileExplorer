using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models.Bookmark
{
    public class BookmarkCommandProvider : ICommandProvider
    {
        private BookmarkProfile _profile;

        public BookmarkCommandProvider(BookmarkProfile profile)
        {
            _profile = profile;
        }

        public IEnumerable<ICommandModel> GetCommandModels()
        {
            yield return new CommandModel(new NewBookmarkFolder(_profile))
                {
                    Header = "New Folder"
                };
        }
    }

    public class NewBookmarkFolder : ScriptCommandBase
    {
        private BookmarkProfile _profile;
        public NewBookmarkFolder(BookmarkProfile profile)
            : base("NewBookmarkFolder")
        {
            _profile = profile;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            IEntryModel[] selectedModels = pm.GetValue<IEntryModel[]>("{Parameter}");
            if (selectedModels.Length == 1)
            {
                BookmarkModel bm = (selectedModels[0] as BookmarkModel);
                bm.AddFolder("New Folder");
            }

            return NextCommand;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            IEntryModel[] selectedModels = pm.GetValue<IEntryModel[]>("{Parameter}");
            return selectedModels.Length == 1 && selectedModels[0] is BookmarkModel &&
                ((selectedModels[0] as BookmarkModel).Type == BookmarkModel.BookmarkEntryType.Directory ||
                    (selectedModels[0] as BookmarkModel).Type == BookmarkModel.BookmarkEntryType.Root);
        }
    }

    
    
}
