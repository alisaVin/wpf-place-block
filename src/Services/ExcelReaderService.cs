using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using place_block_wpf_ares.src.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace place_block_wpf_ares.src.Services
{
    public class ExcelReaderService
    {
        public List<BlockDataModel> ReadInputData(string coordPathFile, string blockName, string etage)
        {
            List<BlockDataModel> blockData = new List<BlockDataModel>();
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(coordPathFile, true))
            {
                WorkbookPart workbookPart = doc.WorkbookPart;
                var sheet = workbookPart.Workbook.Descendants<Sheet>()
                                    .First(s => s.Name == "Technische Anlage"); // Textbox dafür eingeben falls nötig

                WorksheetPart worksheetPart = workbookPart.GetPartById(sheet.Id) as WorksheetPart;
                var rows = worksheetPart.Worksheet.GetFirstChild<SheetData>()
                                                    .Elements<Row>();
                var dataRows = rows.Skip(1);
                var clearRows = dataRows.Where(row => !row.Elements<Cell>()
                                       .Any(cell => cell.DataType?.Value == CellValues.Error))
                                       .ToList();

                foreach (Row row in clearRows)
                {
                    BlockDataModel blockRecord = new BlockDataModel();

                    var cells = row.Elements<Cell>();
                    var sst = workbookPart.SharedStringTablePart?
                                                  .SharedStringTable
                                                  .Elements<SharedStringItem>()
                                                  .ToList();

                    var nameCell = cells.FirstOrDefault(c => string.Compare(GetColumnName(c.CellReference), "O", true) == 0);
                    if (nameCell == null) continue;

                    var nameValue = GetCellText(nameCell, workbookPart);
                    if (nameValue != blockName) continue;

                    var etCell = cells.FirstOrDefault(c => GetColumnName(c.CellReference) == "L");
                    string etageInp = GetCellText(etCell, workbookPart);
                    blockRecord.Etage = etageInp;

                    if (etageInp == etage)
                    {
                        var bezCell = cells.FirstOrDefault(c => GetColumnName(c.CellReference) == "B");
                        var bezValue = GetCellText(bezCell, workbookPart);
                        blockRecord.TABezeichnung = bezValue;

                        var xCell = cells.FirstOrDefault(c => GetColumnName(c.CellReference) == "I");
                        var xValue = GetCellText(xCell, workbookPart);
                        if (xValue != "-1" && xValue != null)
                        {
                            double xCoord = double.Parse(xValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                            blockRecord.X = xCoord;
                        }

                        var yCell = cells.FirstOrDefault(c => GetColumnName(c.CellReference) == "J");
                        var yValue = GetCellText(yCell, workbookPart);
                        if (yValue != "-1" && yValue != null)
                        {
                            double yCoord = double.Parse(yValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                            blockRecord.Y = yCoord;
                        }
                    }
                    blockData.Add(blockRecord);
                }
            }
            return blockData;
        }

        private static string GetColumnName(string cellReference)
        {
            return new string(cellReference
                .TakeWhile(c => char.IsLetter(c))
                .ToArray());
        }

        private string GetCellText(Cell cell, WorkbookPart wbPart)
        {
            if (cell.CellValue == null)
                return string.Empty;

            string raw = cell.CellValue.Text;

            // SharedString-Lookup
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                // Index parsen
                if (int.TryParse(raw, out int ssid))
                {
                    var sstPart = wbPart.GetPartsOfType<SharedStringTablePart>()
                                        .FirstOrDefault();
                    if (sstPart?.SharedStringTable != null)
                    {
                        return sstPart.SharedStringTable
                                      .ElementAt(ssid)
                                      .InnerText;
                    }
                }
            }
            return raw;
        }
    }
}
