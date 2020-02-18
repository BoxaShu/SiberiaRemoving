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
  static class XDataExtantion
  {
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
