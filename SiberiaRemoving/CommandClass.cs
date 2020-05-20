using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Win = System.Windows.Forms;
//using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Diagnostics;
//using Autodesk.AutoCAD.Windows;

[assembly: CommandClass(typeof(SiberiaRemoving.CommandClass))]

namespace SiberiaRemoving
{
  public static class CommandClass
  {

    /// <summary>
    /// Имя приложения для записи XData
    /// </summary>
    public const string AppName = "Siberia";
    [CommandMethod("SiberiaRemove")]
    public static void bxSiberiaRemove()
    {
      Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
      if (mdiActiveDocument == null) return;

      string str = Remover(mdiActiveDocument.Database);
      mdiActiveDocument.Editor.WriteMessage(str);

      Application.ShowAlertDialog("Очистка закончена.");
    }

    //[CommandMethod("PB")]
    //public static void ProgressBarManaged()
    //{
    //  ProgressMeter pm = new ProgressMeter();
    //  pm.Start("Testing Progress Bar");
    //  pm.SetLimit(100);
    //  // Now our lengthy operation
    //  for (int i = 0; i <= 100; i++)
    //  {
    //    System.Threading.Thread.Sleep(50);
    //    // Increment Progress Meter...
    //    pm.MeterProgress();
    //    // This allows AutoCAD to repaint
    //    Win.Application.DoEvents();
    //  }
    //  pm.Stop();
    //}

    [CommandMethod("SiberiaRemoveBatch")]
    public static void bxSiberiaRemoveBatch()
    {
      List<string> raport = new List<string>();
      raport.Add($"\n\n__________Raport:____________");

      Document mdiActiveDocument = Application.DocumentManager.MdiActiveDocument;
      if (mdiActiveDocument == null) return;

      //List<string> filePaths = new List<string>();

      Win.OpenFileDialog OPF = new Win.OpenFileDialog();
      OPF.Multiselect = true;
      OPF.Filter = "Файлы dwg|*.dwg";

      //Выбираем файлы и из полученного списка берем только те 
      // которые доступны для записи
      if (OPF.ShowDialog() == Win.DialogResult.OK)
      {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        int lim = OPF.FileNames.Length;
        int cur = 0;
        //ProgressMeter pm = new ProgressMeter();
        //pm.SetLimit(lim );
        //pm.Start("Cleanup file progress bar");


        foreach (string file in OPF.FileNames)
        {
          cur++;
          mdiActiveDocument.Editor.WriteMessage($"\n {file} // {cur}/{lim} ({GetTik(stopWatch)}");

          System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);

          //TODO тут надо бы резервное копирование предусмотреть... но нафиг надо
          //System.IO.File.Copy(file, $"{file}.");

          if (!fileInfo.IsReadOnly)
          {
            try
            {
              using (Database db = new Database(false, true))
              {
                // https://www.theswamp.org/index.php?topic=42016.0
                //Database db = HostApplicationServices.WorkingDatabase;
                //Document doc = Application.DocumentManager.GetDocument(db);
                //doc.CloseAndSave(file);
                db.ReadDwgFile(file, FileOpenMode.OpenForReadAndAllShare, false, null);
                db.CloseInput(true);
                raport.Add($"\nClean file:{file}");
                raport.Add(Remover(db));

                db.SaveAs(file, true, DwgVersion.Current, db.SecurityParameters);
                db.CloseInput(true);

                //db.Save();
                //db.CloseInput(true);
              }
            }
            catch (System.Exception ex)
            {
              mdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }
          }

          mdiActiveDocument.Editor.WriteMessage($"-{GetTik(stopWatch)})");
          //pm.MeterProgress();
          //Win.Application.DoEvents();

        }

        //pm.Stop();
        stopWatch.Stop();
      }

      mdiActiveDocument.Editor.WriteMessage(String.Join("\n", raport.ToArray()));
      Application.ShowAlertDialog("Очистка закончена.");
    }

    //private static void ProgressBarRedraw(ProgressMeter pm)
    //{
    //  //for (int i = 0; i < 5; i++)
    //  //{
    //    //System.Threading.Thread.Sleep(1);
    //    pm.MeterProgress();
    //    Win.Application.DoEvents();
    //  //}
    //}

    private static string GetTik(Stopwatch stopWatch)
    {
      // Get the elapsed time as a TimeSpan value.
      TimeSpan ts = stopWatch.Elapsed;

      // Format and display the TimeSpan value.
      string elapsedTime = String.Format("_{0:00}:{1:00}:{2:00}.{3:00}",
          ts.Hours, ts.Minutes, ts.Seconds,
          ts.Milliseconds / 10);

      return elapsedTime;
    }

    public static string Remover(Database database)
    {
      StringBuilder result = new StringBuilder();
      ObjectIdCollection forPurge = new ObjectIdCollection();
      int erase = 0;

      //using (DocumentLock dl = mdiActiveDocument.LockDocument())
      //{
      List<string> siberiaStylesUsed = new List<string>();

      using (RegAppTable regAppTable = database.RegAppTableId.Open(OpenMode.ForRead) as RegAppTable)
      {
        if (regAppTable.Has(AppName))
        {
          ObjectId regTablId = regAppTable[AppName];
          using (RegAppTableRecord regAppTableRecord = regTablId.Open(OpenMode.ForWrite) as RegAppTableRecord)
          {
            if (regAppTableRecord != null)
            {

              //Получаем список объектов Siberia 
              ObjectIdCollection m_hardPointerIds = new ReferencedBy(database, regTablId).m_hardPointerIds;

              if (m_hardPointerIds.Count == 0)
              {
                result.AppendLine("Чистим таблицу приложений от " + AppName);
                regAppTableRecord.Erase();
                forPurge.Add(regTablId);
                erase++;
              }
              else
              {
                foreach (ObjectId id in m_hardPointerIds)
                {
                  using (DBObject obj = id.Open(OpenMode.ForRead) as DBObject)
                  {
                    //Ищем используемые стили
                    string json = obj.GetXData();

                    if (!string.IsNullOrEmpty(json) && !siberiaStylesUsed.Contains(json))
                    {
                      siberiaStylesUsed.Add(json);
                    }
                  }
                }
              }

              //erased.Add(regTablId);
            }
          }
        }
      }


      using (DBDictionary dBDictionary = database.NamedObjectsDictionaryId.Open(OpenMode.ForWrite) as DBDictionary)
      {
        if (dBDictionary.Contains(AppName))
        {
          result.AppendLine($"Чистим NOD от {AppName}");
          ObjectId SiberiaDicId = (ObjectId)dBDictionary[AppName];

          using (DBDictionary SiberiaDic = SiberiaDicId.Open(OpenMode.ForWrite) as DBDictionary)
          {
            foreach (DBDictionaryEntry id in SiberiaDic)
            {

              int dicCount = 0;

              using (DBDictionary SiberiaDic0 = id.Value.Open(OpenMode.ForWrite) as DBDictionary)
              {
                foreach (DBDictionaryEntry idd in SiberiaDic0)
                {
                  if (!siberiaStylesUsed.Contains(idd.Key))
                  {
                    //Удаляем только то, на что нет ссылок в объектах в зарегестрированных приложениях
                    SiberiaDic0.Remove(idd.Value);
                    forPurge.Add(idd.Value);
                    erase++;
                  }
                }
                dicCount = SiberiaDic0.Count;
              }

              if (dicCount == 0)
              {
                SiberiaDic.Remove(id.Value);
                erase++;
              }

            }
          }
        }

      }


      result.AppendLine($"Удалено {erase} записей");
      database.Purge(forPurge);

      //Правим ошибки с выводом в консоль
#if (!ACAD2014)
      //result.AppendLine($"Проводим аудит файла");
      database.Audit(true, false);
#else
      result.AppendLine($"Поведите очистку и аудит файла!!!");
#endif

      //}
      return result.ToString();

    }
  }
}
