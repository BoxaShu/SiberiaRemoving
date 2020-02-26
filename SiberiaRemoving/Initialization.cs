using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;


namespace SiberiaRemoving
{
    public class Initialization : IExtensionApplication
  {
    void IExtensionApplication.Initialize()
    {
      Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("\nДанная программа предназначена на данный момент");
      stringBuilder.AppendLine("только для удаления невидимых объектов, некоей \"Siberia\"");
      stringBuilder.AppendLine("(предположительно, Autodesk СПДС, без наличия оного)");
      stringBuilder.AppendLine("ПРИМЕНЕНИЕ: команда SiberiaRemove");
      stringBuilder.AppendLine("\nКопирайт: ООО \"НСК-Проект\"");
      editor.WriteMessage(stringBuilder.ToString());


      //Application.DocumentManager.DocumentCreated += new DocumentCollectionEventHandler(this.DocumentManager_DocumentCreated);

    }
    void IExtensionApplication.Terminate()
    {
    }


    public const string AppName = "Siberia";

    /// <summary>
    /// Ректрор на создание/ открытие нового чертежа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DocumentManager_DocumentCreated(object sender, DocumentCollectionEventArgs e)
    {
      //Application.DocumentManager.MdiActiveDocument.CommandWillStart += new CommandEventHandler(this.MdiActiveDocument_CommandWillStart);

      Database database = Application.DocumentManager.MdiActiveDocument.Database;

      using (DBDictionary dBDictionary = database.NamedObjectsDictionaryId.Open(OpenMode.ForRead) as DBDictionary)
      {
        if (dBDictionary.Contains(AppName))
        {
          ObjectId SiberiaDicId = (ObjectId)dBDictionary[AppName];

          using (DBDictionary SiberiaDic = SiberiaDicId.Open(OpenMode.ForRead) as DBDictionary)
          {
            //SiberiaDic.Modified += new EventHandler
          }

        }
      }
    }



    private void OnObjectModified(object sender, ObjectEventArgs e)
    {

    }

    ///// <summary>
    ///// Реактор на начало команды и если эта команда MIRROR, то действуем!!
    ///// Включаем отслеживание создаваемых объектов, для того, что бы по завершении команды привести их в порядок
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    //private void MdiActiveDocument_CommandWillStart(object sender, CommandEventArgs e)
    //{
    //  ids.Clear();
    //  if (e.GlobalCommandName == "MIRROR")
    //  {
    //    App.Application.DocumentManager.MdiActiveDocument.Database.ObjectModified += new Db.ObjectEventHandler(this.Database_ObjectModified);
    //    App.Application.DocumentManager.MdiActiveDocument.CommandCancelled += new App.CommandEventHandler(this.MdiActiveDocument_CommandCancelled);
    //    App.Application.DocumentManager.MdiActiveDocument.CommandEnded += new App.CommandEventHandler(this.MdiActiveDocument_CommandEnded);
    //    App.Application.DocumentManager.MdiActiveDocument.CommandFailed += new App.CommandEventHandler(this.MdiActiveDocument_CommandFailed);
    //  }
    //}


  }
}
