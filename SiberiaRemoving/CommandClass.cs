using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
namespace SiberiaRemoving
{
  class CommandClass
  {
    [CommandMethod("bargSiberiaRemove")]
    public void SiberiaRemove()
    {
      string text = "Siberia";
      Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
      Database database = mdiActiveDocument.Database;
      Editor editor = mdiActiveDocument.Editor;
      using (Transaction transaction = database.TransactionManager.StartTransaction())
      {
        RegAppTable regAppTable = transaction.GetObject(database.RegAppTableId, 0) as RegAppTable;
        if (regAppTable.Has(text))
        {
          regAppTable.UpgradeOpen();
          RegAppTableRecord regAppTableRecord = transaction.GetObject(regAppTable[text], OpenMode.ForWrite) as RegAppTableRecord;
          if (regAppTableRecord != null)
          {
            editor.WriteMessage("\nЧистим таблицу приложений от " + text);
            regAppTableRecord.Erase(true);
          }
        }
        DBDictionary dBDictionary = transaction.GetObject(database.NamedObjectsDictionaryId, 0) as DBDictionary;
        if (dBDictionary.Contains(text))
        {
          dBDictionary.UpgradeOpen();
          dBDictionary.Remove(text);
          editor.WriteMessage("\nЧистим NOD от " + text);
        }
        transaction.Commit();
      }
    }
  }
}
