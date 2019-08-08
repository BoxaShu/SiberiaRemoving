using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App = Autodesk.AutoCAD.ApplicationServices;
using AcDb = Autodesk.AutoCAD.DatabaseServices;

namespace SiberiaRemoving
{
  class ReferencedBy 
  {

    public AcDb.ObjectIdCollection m_hardPointerIds = new AcDb.ObjectIdCollection();
    //public AcDb.ObjectIdCollection m_softPointerIds = new AcDb.ObjectIdCollection();
    //public AcDb.ObjectIdCollection m_hardOwnershipIds = new AcDb.ObjectIdCollection();
    //public AcDb.ObjectIdCollection m_softOwnershipIds = new AcDb.ObjectIdCollection();

    protected AcDb.ObjectId m_val;
    protected int m_count = 0;
    protected int m_skipped = 0;

    public ReferencedBy( AcDb.ObjectId val)
    {
      m_val = val;
      BruteForceFindReferences();
    }


    private void
    BruteForceFindReferences()
    {
      App.Document mdiActiveDocument = App.Application.DocumentManager.MdiActiveDocument;
      if (mdiActiveDocument == null) return;

      AcDb.Database database = mdiActiveDocument.Database;
      //Editor editor = mdiActiveDocument.Editor;


      using (var trHelp = database.TransactionManager.StartTransaction())
      {

        m_count = 0;
      m_skipped = 0;

      // since we aren't calculating this in the destructor, we have to re-init every time they
      // do the drill-down.
      m_hardPointerIds.Clear();
      //m_softPointerIds.Clear();
      //m_hardOwnershipIds.Clear();
      //m_softOwnershipIds.Clear();

      AcDb.Database db = m_val.Database;

      // pass in all the root objects
      ProcessObject(trHelp, m_val, db.NamedObjectsDictionaryId);
      ProcessObject(trHelp, m_val, db.BlockTableId);
      ProcessObject(trHelp, m_val, db.DimStyleTableId);
      ProcessObject(trHelp, m_val, db.LayerTableId);
      ProcessObject(trHelp, m_val, db.LinetypeTableId);
      ProcessObject(trHelp, m_val, db.RegAppTableId);
      ProcessObject(trHelp, m_val, db.TextStyleTableId);
      ProcessObject(trHelp, m_val, db.UcsTableId);
      ProcessObject(trHelp, m_val, db.ViewportTableId);
      ProcessObject(trHelp, m_val, db.ViewTableId);

        //string str = string.Format("Visited: {0:d}, Skipped: {1:d}, DB Approx: {2:d}", m_count, m_skipped, db.ApproxNumObjects);
        //MessageBox.Show(str);

        trHelp.Commit();

      }

    }


    private void
    ProcessObject(AcDb.Transaction trHelp, AcDb.ObjectId lookForObjId, AcDb.ObjectId curObjId)
    {
      AcDb.DBObject tmpObj = trHelp.GetObject(curObjId, AcDb.OpenMode.ForRead);
      if (tmpObj != null)
      {
        m_count++;
       ReferenceFiler filer = new ReferenceFiler();
        tmpObj.DwgOut(filer);     // find out who this object owns

        RecordReferences(lookForObjId, tmpObj, filer); // record references for this object

        // now recursively visit all the objects this one owns
        for (int i = 0; i < filer.m_hardOwnershipIds.Count; i++)
          ProcessObject(trHelp, lookForObjId, filer.m_hardOwnershipIds[i]);

        for (int i = 0; i < filer.m_softOwnershipIds.Count; i++)
          ProcessObject(trHelp, lookForObjId, filer.m_softOwnershipIds[i]);
      }
      else
        m_skipped++;
    }

    private void
    RecordReferences(AcDb.ObjectId lookForObjId, AcDb.DBObject objToCheck, ReferenceFiler filer)
    {
      // now see if we showed up in any of the lists
      if (filer.m_hardPointerIds.Contains(lookForObjId))
        m_hardPointerIds.Add(objToCheck.ObjectId);

      //if (filer.m_softPointerIds.Contains(lookForObjId))
      //  m_softPointerIds.Add(objToCheck.ObjectId);

      //if (filer.m_hardOwnershipIds.Contains(lookForObjId))
      //  m_hardOwnershipIds.Add(objToCheck.ObjectId);

      //if (filer.m_softOwnershipIds.Contains(lookForObjId))
      //  m_softOwnershipIds.Add(objToCheck.ObjectId);
    }
  }
}
