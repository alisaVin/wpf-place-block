using place_block_wpf_ares.src.Services;
using System.Collections.Generic;
using System.Linq;
using Teigha.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;

namespace place_block_wpf_ares.src.Model
{
    public class AresOperations
    {
        //string coordPath;
        //string blockPath;
        //string blockName;
        //string etageInput;
        ExcelReaderService excel = new ExcelReaderService();

        public void InsertBlocks(Document targetDoc, Database targetDb, string coordPath, string blockPath, string blockName, string etageInput)
        {
            var edit = targetDoc.Editor;
            var blockData = excel.ReadInputData(coordPath, blockName, etageInput);
            var validBlocks = blockData.Where(b => b.X > 0 && b.Y > 0 && b.Etage == etageInput)
                                        .ToList();

            var sourceDb = new Database(false, true);
            sourceDb.ReadDwgFile(blockPath, FileOpenMode.OpenForReadAndReadShare, true, string.Empty);
            if (sourceDb is null) return;

            var layerName = "techAnlage_" + blockName;
            var blIds = new ObjectIdCollection();
            var layIds = new ObjectIdCollection();

            using (targetDoc.LockDocument())
            {
                using (sourceDb)
                {
                    #region copy block into dwg
                    using (var tr = sourceDb.TransactionManager.StartOpenCloseTransaction())
                    {
                        var layerTable = tr.GetObject(sourceDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                        if (!layerTable.Has(layerName)) return;
                        var blLayId = layerTable[layerName];
                        layIds.Add(blLayId);

                        var bt = tr.GetObject(sourceDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                        var btr = tr.GetObject(bt[blockName], OpenMode.ForRead) as BlockTableRecord;
                        if (!(btr.IsLayout || btr.IsFromExternalReference || btr.IsFromOverlayReference || btr.IsDependent) && bt.Has(blockName))
                        {
                            blIds.Add(btr.Id);
                        }

                        if (blIds.Count != 0 && layIds.Count != 0)
                        {
                            var mapping = new IdMapping();
                            sourceDb.WblockCloneObjects(layIds, targetDb.LayerTableId, mapping, DuplicateRecordCloning.Replace, false);

                            var idMapping = new IdMapping();
                            sourceDb.WblockCloneObjects(blIds, targetDb.BlockTableId, idMapping, DuplicateRecordCloning.Replace, false);
                        }
                        else
                        {
                            edit.WriteMessage("\nErrors caused by copying");
                            return;
                        }
                    }
                    #endregion

                    #region set attributes to copied blocks
                    List<Point3d> insertPoints = new List<Point3d>();
                    List<AttributesModel> lstAttrData = new List<AttributesModel>();
                    foreach (var b in validBlocks)
                    {
                        insertPoints.Add(new Point3d(b.X, b.Y, 0));
                        lstAttrData.Add(

                            //new AttributesModel { Name = "PUNKTNUMMER", Value = b.PunktNum },
                            //new AttributesModel { Name = "TA_ID", Value = b.TAId },
                            new AttributesModel { Name = "TA_BEZEICHNUNG", Value = b.TABezeichnung }
                            //new AttributesModel { Name = "TA_GRUPPE", Value = b.TAGruppe }
                            //new AttributesModel { Name = "Geschoss", Value = b.Etage }
                        );
                    }

                    List<Point3d> transformPoints = TransformCoordinatesService.Rotate(insertPoints);
                    using (Transaction tr = targetDb.TransactionManager.StartTransaction())
                    {
                        //tr.TransactionManager.QueueForGraphicsFlush();
                        var blBtrID = AcadUtilsService.GetBlockDef(targetDb, blockName);
                        var bt = tr.GetObject(targetDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                        var ms = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        var layerTable = tr.GetObject(targetDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                        try
                        {
                            if (blBtrID.IsNull) return;
                            for (int i = 0; i < insertPoints.Count; i++)
                            {
                                var newBr = new BlockReference(transformPoints[i], blBtrID);
                                newBr.Layer = layerName;
                                targetDb.Clayer = layerTable[layerName];
                                ms.AppendEntity(newBr);
                                tr.AddNewlyCreatedDBObject(newBr, true);

                                using (var blDef = tr.GetObject(blBtrID, OpenMode.ForRead) as BlockTableRecord)
                                {
                                    if (blDef == null || !blDef.HasAttributeDefinitions)
                                        return;

                                    SetAttributeData(tr, blDef, newBr, lstAttrData);
                                }
                            }
                            #endregion
                            ms.Dispose();
                            tr.Commit();

                        }
                        catch (System.Exception ex)
                        {
                            edit.WriteMessage($"\n Exception: {ex.Message} \n{ex.StackTrace}");
                        }
                    }
                    return;
                }
            }
        }

        private void SetAttributeData(Transaction tr, BlockTableRecord bd, BlockReference bRef, List<AttributesModel> lstAttrData)
        {
            if ((bd == null) || !bd.HasAttributeDefinitions)
                return;

            if (bRef != null)
            {
                Teigha.DatabaseServices.AttributeCollection attrColl = bRef.AttributeCollection;
                foreach (ObjectId adId in bd)
                {
                    var adObj = tr.GetObject(adId, OpenMode.ForWrite); //!!!
                    AttributeDefinition ad = adObj as AttributeDefinition;
                    if (ad != null)
                    {
                        using (var attrRef = new AttributeReference())
                        {
                            attrRef.SetAttributeFromBlock(ad, bRef.BlockTransform);
                            var modelEntity = lstAttrData.FirstOrDefault(b => b.Name == "TA_BEZEICHNUNG");
                            if (modelEntity != null && attrRef.Tag == "ATTR1")
                            {
                                attrRef.Tag = modelEntity.Name;
                                attrRef.TextString = modelEntity.Value;
                                lstAttrData.Remove(modelEntity);
                            }
                            else
                            {
                                continue;
                            }
                            bRef.AttributeCollection.AppendAttribute(attrRef);
                            tr.AddNewlyCreatedDBObject(attrRef, true);
                        }
                    }
                }
            }
        }
    }
}
