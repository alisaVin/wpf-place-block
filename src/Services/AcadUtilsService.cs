using System;
using System.IO;
using Teigha.ApplicationServices;
using Teigha.DatabaseServices;
using aresApp = Teigha.ApplicationServices.Application;

namespace place_block_wpf_ares.src.Services
{
    public class AcadUtilsService
    {
        public static Database OpenDb(string filePath)
        {
            var db = new Database(false, true);
            try
            {
                db.ReadDwgFile(filePath, FileShare.Read/*FileOpenMode.OpenForReadAndAllShare/*FileShare.ReadWrite*/, true, null);
                return db;
            }
            catch (System.Exception ex)
            {

                //reporter?.ClearText();
                //if (reporter != null)
                //    reporter.ReportExeption(ex);

                //reporter.WriteText($"\nUnable to read drawing file - {filePath}");
                return null;
            }
        }

        public static Document OpenDocument(string sFileName, bool setActive)
        {
            try
            {
                //if (!File.Exists(sFileName))
                //    reporter.WriteText($"Datei: {sFileName} ist nicht vorhanden.");

                var docMng = aresApp.DocumentManager;
                Document returnDoc = docMng.Open(sFileName, false);
                if (setActive)
                {
                    if (docMng.MdiActiveDocument != null && returnDoc != docMng.MdiActiveDocument)
                        docMng.MdiActiveDocument = returnDoc;
                }
                return returnDoc;
            }
            catch (System.Exception ex)
            {
                //if (reporter != null)
                //{
                //    reporter.ClearText();
                //    reporter.ReportExeption(ex);
                //    reporter.WriteText($"\nUnable to read drawing file - {sFileName}");
                //}
                return null;
            }
        }

        public static ObjectIdCollection GetBlockDefIds(Database db)
        {
            ObjectIdCollection newBlDefIds = new ObjectIdCollection();
            try
            {
                using (Transaction trSource = db.TransactionManager.StartTransaction())
                {
                    var bt = trSource.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    foreach (ObjectId id in bt)
                    {
                        var obj = trSource.GetObject(id, OpenMode.ForRead) as DBObject;
                        var bd = obj as BlockTableRecord;
                        if ((bd != null) && !bd.IsAnonymous && bd.IsLayout)
                            newBlDefIds.Add(id);
                        obj.Dispose();
                    }
                    trSource.Commit();
                    bt.Dispose();
                }
            }
            catch (System.Exception ex)
            {
                //if (reporter != null)
                //    reporter.ReportExeption(ex);
            }
            return newBlDefIds;
        }

        //Id
        public static ObjectId GetBlockDef(Database db, string sBlockName)
        {
            ObjectId retId = ObjectId.Null;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                foreach (ObjectId id in bt)
                {
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                    var bd = obj as BlockTableRecord;
                    if (bd == null) continue;
                    if (String.Compare(bd.Name, sBlockName) == 0)
                    {
                        bd.Dispose();
                        retId = id;
                        break;
                    }
                    bd.Dispose();
                }
                bt.Dispose();
            }
            return retId;
        }

        public static ObjectId GetLayerId(Database db, string sLayerName, short colorIndex)
        {
            ObjectId layId = new ObjectId();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                layId = lt[sLayerName];

                if (layId.IsNull)
                {
                    var ltr = new LayerTableRecord();
                    ltr.Name = sLayerName;
                    ltr.Color = Teigha.Colors.Color.FromColorIndex(Teigha.Colors.ColorMethod.None, colorIndex);
                    lt.DisableUndoRecording(true);
                    lt.UpgradeOpen();
                    layId = lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                    lt.DisableUndoRecording(false);
                }
                tr.Commit();
            }
            return layId;
        }
    }
}

