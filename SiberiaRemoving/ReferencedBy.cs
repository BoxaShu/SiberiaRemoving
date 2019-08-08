// Copy from: ADN-DevTech / MgdDbg
// Link: https://github.com/ADN-DevTech/MgdDbg/blob/master/Snoop/Data/ReferencedBy.cs
// Autor: Augusto Goncalves : 
// augusto.goncalves@autodesk.com  and http://developer.autodesk.com/
//
//
//
// (C) Copyright 2006 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

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

      using (var trHelp = database.TransactionManager.StartTransaction())
      {

        m_count = 0;
      m_skipped = 0;

      // since we aren't calculating this in the destructor, we have to re-init every time they
      // do the drill-down.
      m_hardPointerIds.Clear();

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
    }
  }
}
