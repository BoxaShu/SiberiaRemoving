using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
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
    }
    void IExtensionApplication.Terminate()
    {
    }
  }
}
