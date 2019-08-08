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

            Database database = mdiActiveDocument.Database;
            Editor editor = mdiActiveDocument.Editor;

            ObjectIdCollection forPurge = new ObjectIdCollection();

            int erase = 0;
            using (DocumentLock dl = mdiActiveDocument.LockDocument())
            {
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
                                ObjectIdCollection m_hardPointerIds = new ReferencedBy(regTablId).m_hardPointerIds;

                                if (m_hardPointerIds.Count == 0)
                                {
                                    editor.WriteMessage("\nЧистим таблицу приложений от " + AppName);
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
                        editor.WriteMessage($"\nЧистим NOD от {AppName}");
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

                database.Purge(forPurge);
                editor.WriteMessage($"\nУдалено {erase} записей");
                //Правим ошибки с выводом в консоль
                editor.WriteMessage($"\nПроводим аудит файла");
                database.Audit(true, true);
            }
        }


        public static string GetXData(this DBObject ent)
        {
            string json = string.Empty;
            using (ResultBuffer rb = ent.GetXDataForApplication(CommandClass.AppName))
            {
                if (rb != null)
                    try
                    {
                        TypedValue[] vals = rb.AsArray();
                        if (vals.Length >= 4)
                        {
                            json = vals[3].Value.ToString();
                        }
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.
                        WriteMessage($"\nError: CommandClass - GetXData:{ex.Message}");
                    }
            }
            return json;
        }
    }
}
