using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


using App = Autodesk.AutoCAD.ApplicationServices;
using cad = Autodesk.AutoCAD.ApplicationServices.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Gem = Autodesk.AutoCAD.Geometry;
using Rtm = Autodesk.AutoCAD.Runtime;


namespace SiberiaRemoving
{
  public class Initialization : Rtm.IExtensionApplication
  {



    /// <summary>
    /// Загрузка библиотеки
    /// http://through-the-interface.typepad.com/through_the_interface/2007/03/getting_the_lis.html
    /// </summary>
    #region 
    public void Initialize()
    {
      String assemblyFileFullName = GetType().Assembly.Location;
      String assemblyName = System.IO.Path.GetFileName(
                                                GetType().Assembly.Location);

      // Just get the commands for this assembly
      App.DocumentCollection dm = App.Application.DocumentManager;
      Assembly asm = Assembly.GetExecutingAssembly();

      // Сообщаю о том, что произведена загрузка сборки 
      //и указываю полное имя файла,
      // дабы было видно, откуда она загружена

      Ed.Editor editor = App.Application.DocumentManager.MdiActiveDocument.Editor;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"\n Assembly, {assemblyName}, Loaded");
      stringBuilder.AppendLine($"\nAssembly File:{ assemblyFileFullName}");
      stringBuilder.AppendLine("\nДанная программа предназначена, на данный момент,");
      stringBuilder.AppendLine("только для удаления невидимых объектов, некоей \"Siberia\"");
      stringBuilder.AppendLine("(предположительно, Autodesk СПДС, без наличия оного)");
      stringBuilder.AppendLine("\nCopyright © ООО \'НСК-Проект\' written by Владимир Шульжицкий, 01.2020");
      editor.WriteMessage(stringBuilder.ToString());

      //Вывожу список комманд определенных в библиотеке
      App.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
        ("\nСписок команд реализованных в библиотеке: \n\n");

      string[] cmds = GetCommands(asm, false);
      foreach (string cmd in cmds)
        App.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
          (cmd + "\n");

      App.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage
        ("\n\nКонец списка.\n");
    }

    public void Terminate()
    {
      Console.WriteLine("finish!");
    }

    /// <summary>
    /// Получение списка комманд определенных в сборке
    /// </summary>
    /// <param name="asm"></param>
    /// <param name="markedOnly"></param>
    /// <returns></returns>
    private static string[] GetCommands(Assembly asm, bool markedOnly)
    {
      List<string> result = new List<string>();
      object[] objs =
        asm.GetCustomAttributes(typeof(Rtm.CommandClassAttribute), true);
      Type[] tps;
      int numTypes = objs.Length;
      if (numTypes > 0)
      {
        tps = new Type[numTypes];
        for (int i = 0; i < numTypes; i++)
        {
          Rtm.CommandClassAttribute cca =
            objs[i] as Rtm.CommandClassAttribute;
          if (cca != null)
          {
            tps[i] = cca.Type;
          }
        }
      }
      else
      {
        // If we're only looking for specifically
        // marked CommandClasses, then use an
        // empty list
        if (markedOnly)
          tps = new Type[0];
        else
          tps = asm.GetExportedTypes();
      }
      foreach (Type tp in tps)
      {
        MethodInfo[] meths = tp.GetMethods();
        foreach (MethodInfo meth in meths)
        {
          objs = meth.GetCustomAttributes(typeof(Rtm.CommandMethodAttribute), true);
          foreach (object obj in objs)
          {
            Rtm.CommandMethodAttribute attb =
                (Rtm.CommandMethodAttribute)obj; result.Add(attb.GlobalName);
          }
        }
      }
      //string[] ret = new string[result.Count];
      //result.CopyTo(ret, 0);

      return result.ToArray();
    }
    #endregion


    //void IExtensionApplication.Initialize()
    //{
    //  Editor editor = Application.DocumentManager.MdiActiveDocument.Editor;
    //  StringBuilder stringBuilder = new StringBuilder();
    //  stringBuilder.AppendLine("\nДанная программа предназначена на данный момент");
    //  stringBuilder.AppendLine("только для удаления невидимых объектов, некоей \"Siberia\"");
    //  stringBuilder.AppendLine("(предположительно, Autodesk СПДС, без наличия оного)");
    //  stringBuilder.AppendLine("ПРИМЕНЕНИЕ: команда SiberiaRemove");
    //  stringBuilder.AppendLine("\nКопирайт: ООО \"НСК-Проект\"");
    //  editor.WriteMessage(stringBuilder.ToString());


    //  //Application.DocumentManager.DocumentCreated += new DocumentCollectionEventHandler(this.DocumentManager_DocumentCreated);

    //}

  }
}
