using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using System.Configuration;

using System.Data;

using System.Web;

using System.Web.Security;

using System.Web.UI;

using System.Web.UI.HtmlControls;

using System.Web.UI.WebControls;

using System.Web.UI.WebControls.WebParts;

using System.Data.OleDb;


//using Excel;

using System.Reflection;

using System.Runtime.InteropServices;// For COMException 

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;




namespace mmc_production
{
    class ExcelHandler
    {
        /*private const string COLUMN_MAC = "A";
        private const string COLUMN_HW = "B";
        private const string COLUMN_MODEL = "C";
        private const string COLUMN_SN = "D";
        private const string COLUMN_TIME = "E";
        private const string COLUMN_STATUS = "F";
        private const string COLUMN_MES_STATUS = "G";

        private const string COLUMN_MAC_NAME = "Mac";
        private const string COLUMN_HW_NAME = "HW version";
        private const string COLUMN_MODEL_NAME = "Model";
        private const string COLUMN_SN_NAME = "SN";
        private const string COLUMN_TIME_NAME = "Time";
        private const string COLUMN_STATUS_NAME = "Status";
        private const string COLUMN_MES_STATUS_NAME = "MES status";*/
        private const int COLUMN_NUMBER = 7;
        private static readonly string[] COLUMN_INDEX_ARRAY = { "A", "B", "C", "D", "E", "F", "G" };
        private static readonly string[] COLUMN_NAME_ARRAY = { "Mac", "HW version", "Model", "SN", "Time", "Status", "Mes status" };

        private const string LOG_DIR = "log";

        private const string WORK_SHEET_NAME = "MMC";

        public static SpreadsheetDocument document;
        public static Worksheet worksheet;

        public static uint index = 1; //index is from 1 to 9999


        public static readonly int NO_ERROR = 0;
        public static readonly int ERROR_EXCEL_OPEN = -1;
        public static readonly int ERROR_EXCEL_CREATE = -2;
        public static readonly int ERROR_EXCEL_INSERT_HEAD = -3;
        public static readonly int ERROR_EXCEL_INSERT_INFO = -4;

        public static int openExcel()
        {
            string name = LOG_DIR + @"\" + System.DateTime.Now.ToString("yyyyMMdd") + "_" + ProdDataHandler.work_loc + ".xlsx";
            if (document != null)
            {
                document.Close();
            }
            if (File.Exists(name))
            {
                try
                {
                    document = SpreadsheetDocument.Open(name, true);
                    IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == WORK_SHEET_NAME);
                    if (sheets.Count() == 0)
                    {
                        // The specified worksheet does not exist.
                        return ERROR_EXCEL_OPEN;
                    }

                    WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);
                    worksheet = worksheetPart.Worksheet;
                    index = getLastIndex(worksheet);
                    return NO_ERROR;
                }catch(Exception e)
                {
                    return ERROR_EXCEL_OPEN;
                }
            }
            else
            {
                try
                {
                    if (Directory.Exists(LOG_DIR) == false)
                    {
                        Directory.CreateDirectory(LOG_DIR);
                    }
                    document = SpreadsheetDocument.Create(name, SpreadsheetDocumentType.Workbook);
                    // Add a WorkbookPart to the document.
                    WorkbookPart workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart.
                    WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());
                    worksheet = worksheetPart.Worksheet;

                    // Add Sheets to the Workbook.
                    Sheets sheets = document.WorkbookPart.Workbook.
                        AppendChild<Sheets>(new Sheets());

                    // Append a new worksheet and associate it with the workbook.
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.
                        GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = WORK_SHEET_NAME
                    };
                    sheets.Append(sheet);

                    workbookpart.Workbook.Save();

                    insertHeadRow(worksheet);

                    //index is from 1 to 9999
                    index = 1;
                    // Close the document.
                    //closeExcel();
                    return NO_ERROR;
                }catch(Exception e)
                {
                    return ERROR_EXCEL_CREATE;
                }
            }
        }


        private static int insertHeadRow(Worksheet worksheet)
        {
            try
            {
                //WorkbookPart workbookpart = document.AddWorkbookPart();
                //WorksheetPart worksheetpart = workbookpart.WorksheetParts.
                if (worksheet == null)
                {
                    return ERROR_EXCEL_INSERT_HEAD;
                }
                SheetData sheetData = worksheet.GetFirstChild<SheetData>();
                //IEnumerable<SheetData> data = worksheet.Elements<SheetData>();

                Row row = new Row()
                {
                    RowIndex = (uint)1
                };
                //worksheet.Append(row);
                sheetData.Append(row);

                for (int i = 0; i < COLUMN_NUMBER; ++i)
                {
                    string cellReference = COLUMN_INDEX_ARRAY[i] + row.RowIndex;

                    Cell newCell = new Cell() { CellReference = cellReference };
                    newCell.CellValue = new CellValue(COLUMN_NAME_ARRAY[i]);
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    //row.InsertBefore(newCell, refCell);
                    row.Append(newCell);
                }

                worksheet.Save();
                //document.Close();
                return NO_ERROR;
            }catch(Exception e)
            {
                return ERROR_EXCEL_INSERT_HEAD;
            }
        }


        private static uint getLastIndex(Worksheet worksheet)
        { 
            if (worksheet != null)
            {
                SheetData sheetData = worksheet.GetFirstChild<SheetData>();
                Row lastRow = (Row)sheetData.LastChild;
                index = lastRow.RowIndex;
                if (lastRow.RowIndex > 1)
                {
                    string reference = COLUMN_INDEX_ARRAY[3] + lastRow.RowIndex;
                    IEnumerable<Cell> cells = worksheet.Descendants<Cell>().Where(c => c.CellReference == reference);
                    //index: 当前最大的行数，同时检查最后一行sn 的值，如果sn中的index更大，把index设为sn中的index
                    if (cells.Count() != 0)
                    {
                        Cell sn_cell = cells.First();
                        string sn = sn_cell.InnerText;
                        uint sn_index = uint.Parse(sn.Substring(5, 4));
                        if (sn_index > index)
                        {
                            index = sn_index;
                        }
                    }
                }
                //SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            }
            return index;
        }

        public static int insertDeviceInfo(string mac, string hw, string model, string sn, string status, string mes_status)
        {
            int res = openExcel();
            if (res != NO_ERROR) return res;

            try
            {
                if (worksheet != null)
                {
                    SheetData sheetData = worksheet.GetFirstChild<SheetData>();
                    //IEnumerable<SheetData> data = worksheet.Elements<SheetData>();
                    Row lastRow = (Row)sheetData.LastChild;
                    Row row = new Row();
                    //row.RowIndex = index + 1;
                    {
                        //row.RowIndex = (uint)index + 1
                    };
                    row.RowIndex = lastRow.RowIndex + 1;

                    string[] infoArray = { mac, hw, model, sn, System.DateTime.Now.ToString("yyyyMMdd HH:mm:ss"), status, mes_status };
                    sheetData.Append(row);
                    for (int i = 0; i < COLUMN_NUMBER; ++i)
                    {
                        string cellReference = COLUMN_INDEX_ARRAY[i] + row.RowIndex;

                        Cell newCell = new Cell() { CellReference = cellReference };
                        newCell.CellValue = new CellValue(infoArray[i]);
                        //newCell.CellValue = new CellValue(COLUMN_NAME_ARRAY[i]);
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                        //row.InsertBefore(newCell, refCell);
                        row.Append(newCell);
                    }

                    index++;

                    worksheet.Save();
                }
            }catch(Exception e)
            {
                res = ERROR_EXCEL_INSERT_INFO;
            }
            closeExcel();
            return res;
        }

        public static void closeExcel()
        {
            if (document != null)
            {
                document.Close();
                document = null;
                worksheet = null;
            }
        }
    }
}
