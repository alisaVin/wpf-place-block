using place_block_wpf_ares.Properties;
using place_block_wpf_ares.src.Views;
using Teigha.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.EditorInput;
using Teigha.Runtime;
using app = Teigha.ApplicationServices.Application;

[assembly: CommandClass(typeof(place_block_wpf_ares.src.Commands.ARESCommands))]

namespace place_block_wpf_ares.src.Commands
{
    public class ARESCommands
    {
        Document doc;
        Database db;
        Editor ed;
        string coordPath;
        string blockPath;
        string blockName;
        string etageInput;

        public ARESCommands()
        {
            doc = app.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
            coordPath = Settings.Default.LastCoordinatesPath;
            blockPath = Settings.Default.LastBlockPath;
            blockName = Settings.Default.LastBlockNameInput;
            etageInput = Settings.Default.LastEtageInput;
        }

        [CommandMethod("PLACEBLOCKFORM")]
        public void ModelessWpfDialogCmd()
        {
            var dialog = new ModaelssWpfDialog();
            var result = app.ShowModalWindow(dialog);
        }


    }
}
