using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;


[assembly: CommandClass(typeof(SiberiaRemoving.CommandClass))]

namespace SiberiaRemoving
{
  class CommandClass
  {
    [CommandMethod("SiberiaRemove")]
   public void bxSiberiaRemove()
    {
      string text = "Siberia";
      Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
      if (mdiActiveDocument == null) return;

      Database database = mdiActiveDocument.Database;
      Editor editor = mdiActiveDocument.Editor;

      int erase = 0;
      using (DocumentLock dl = mdiActiveDocument.LockDocument())
      {

        using (RegAppTable regAppTable = database.RegAppTableId.Open(OpenMode.ForRead) as RegAppTable)
        {
          if (regAppTable.Has(text))
          {
            ObjectId regTablId = regAppTable[text];
            using (RegAppTableRecord regAppTableRecord = regTablId.Open(OpenMode.ForWrite) as RegAppTableRecord)
            {
              if (regAppTableRecord != null)
              {

                //Получаем список объектов Siberia 
                ObjectIdCollection m_hardPointerIds = new ReferencedBy(regAppTableRecord.ObjectId).m_hardPointerIds;

                editor.WriteMessage("\nЧистим таблицу приложений от " + text);
                //regAppTableRecord.Erase();
                //erase++;


                //erased.Add(regTablId);
              }
            }
          }
        }

        //using (DBDictionary dBDictionary = database.NamedObjectsDictionaryId.Open(OpenMode.ForWrite) as DBDictionary)
        //{
        //  editor.WriteMessage($"\nЧистим NOD от {text}");
        //  ObjectId SiberiaDicId = (ObjectId)dBDictionary[text];
        //  using (DBDictionary SiberiaDic = SiberiaDicId.Open(OpenMode.ForWrite) as DBDictionary)
        //  {
        //    foreach (DBDictionaryEntry id in SiberiaDic)
        //    {
        //      using (DBDictionary SiberiaDic0 = id.Value.Open(OpenMode.ForWrite) as DBDictionary)
        //      {
        //        foreach (DBDictionaryEntry idd in SiberiaDic0)
        //        {
        //          SiberiaDic0.Remove(idd.Value);
        //          erase++;
        //        }
        //      }
        //      SiberiaDic.Remove(id.Value);
        //      erase++;
        //    }
        //  }
        //}
        //editor.WriteMessage($"\nУдалено {erase} записей");
        ////Правим ошибки с выводом в консоль
        //editor.WriteMessage($"\nПроводим аудит файла");
        //database.Audit(true, true);
      }
    }
  }
}
