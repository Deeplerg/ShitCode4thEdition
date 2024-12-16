using System.IO;
using OfficeOpenXml;

namespace AlgorithmVizualizer.Desktop;

 public class SortingTable
    {
        public void NaturalMergeSortAlgorithm(string inputFile, string tempFileB, string tempFileC, int columnIndex)
        {
            bool sorted = false;

            while (!sorted)
            {
                SplitExcelFile(inputFile, tempFileB, tempFileC, columnIndex);
                sorted = MergeExcelFiles(inputFile, tempFileB, tempFileC, columnIndex);

                // Задержка для визуализации этапов
                System.Threading.Thread.Sleep(1000); // 1 секунда
            }
        }


        public void SplitExcelFile(string inputFile, string tempFileB, string tempFileC, int columnIndex)
        {
            using (var package = new ExcelPackage(new FileInfo(inputFile)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int colCount = worksheet.Dimension?.Columns ?? 0;

                DeleteExistingWorksheets(tempFileB);
                DeleteExistingWorksheets(tempFileC);

                using (var packageB = new ExcelPackage(new FileInfo(tempFileB)))
                using (var packageC = new ExcelPackage(new FileInfo(tempFileC)))
                {
                    var worksheetB = packageB.Workbook.Worksheets.Add("Sheet1");
                    var worksheetC = packageC.Workbook.Worksheets.Add("Sheet1");

                    int rowB = 1, rowC = 1;
                    bool writeToB = true;

                    for (int i = 1; i <= rowCount; i++)
                    {
                        if (writeToB)
                        {
                            for (int col = 1; col <= colCount; col++)
                            {
                                worksheetB.Cells[rowB, col].Value = worksheet.Cells[i, col].Value;
                            }
                            rowB++;
                        }
                        else
                        {
                            for (int col = 1; col <= colCount; col++)
                            {
                                worksheetC.Cells[rowC, col].Value = worksheet.Cells[i, col].Value;
                            }
                            rowC++;
                        }

                        writeToB = !writeToB;
                    }

                    packageB.Save();
                    packageC.Save();
                }
            }
        }


        public bool MergeExcelFiles(string outputFile, string tempFileB, string tempFileC, int columnIndex)
{
    using (var packageB = new ExcelPackage(new FileInfo(tempFileB)))
    using (var packageC = new ExcelPackage(new FileInfo(tempFileC)))
    using (var packageOutput = new ExcelPackage(new FileInfo(outputFile)))
    {
        var worksheetB = packageB.Workbook.Worksheets[0];
        var worksheetC = packageC.Workbook.Worksheets[0];

        // Проверяем наличие листа с именем "Sheet1" и удаляем его, если он существует
        var existingWorksheet = packageOutput.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Sheet1");
        if (existingWorksheet != null)
        {
            packageOutput.Workbook.Worksheets.Delete("Sheet1");
        }

        // Добавляем новый лист
        var worksheetOutput = packageOutput.Workbook.Worksheets.Add("Sheet1");

        int rowB = 1, rowC = 1, rowOutput = 1;
        int rowCountB = worksheetB.Dimension?.Rows ?? 0;
        int rowCountC = worksheetC.Dimension?.Rows ?? 0;
        int colCount = worksheetB.Dimension?.Columns ?? worksheetC.Dimension?.Columns ?? 0;

        bool sorted = true;

        while (rowB <= rowCountB || rowC <= rowCountC)
        {
            var valueB = rowB <= rowCountB ? worksheetB.Cells[rowB, columnIndex].Value : null;
            var valueC = rowC <= rowCountC ? worksheetC.Cells[rowC, columnIndex].Value : null;

            if (valueB != null && valueC != null)
            {
                if (Convert.ToInt32(valueB) <= Convert.ToInt32(valueC))
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        worksheetOutput.Cells[rowOutput, col].Value = worksheetB.Cells[rowB, col].Value;
                    }
                    rowB++;
                }
                else
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        worksheetOutput.Cells[rowOutput, col].Value = worksheetC.Cells[rowC, col].Value;
                    }
                    rowC++;
                    sorted = false;
                }
                rowOutput++;
            }
            else if (valueB != null)
            {
                for (int col = 1; col <= colCount; col++)
                {
                    worksheetOutput.Cells[rowOutput, col].Value = worksheetB.Cells[rowB, col].Value;
                }
                rowB++;
                rowOutput++;
            }
            else if (valueC != null)
            {
                for (int col = 1; col <= colCount; col++)
                {
                    worksheetOutput.Cells[rowOutput, col].Value = worksheetC.Cells[rowC, col].Value;
                }
                rowC++;
                rowOutput++;
            }
        }

        packageOutput.Save();
        return sorted;
    }
}




        public void GenerateExcelFile(string filePath, int[] numbers)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // Удаляем все существующие листы
                while (package.Workbook.Worksheets.Count > 0)
                {
                    package.Workbook.Worksheets.Delete(0);
                }

                // Создаем новый лист с данными
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                for (int i = 0; i < numbers.Length; i++)
                {
                    worksheet.Cells[i + 1, 1].Value = numbers[i];
                }

                package.Save();
            }
        }

        public List<List<object>> ReadExcelFile(string filePath)
        {
            var result = new List<List<object>>();

            if (!File.Exists(filePath))
                return result;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension?.Rows ?? 0;
                int colCount = worksheet.Dimension?.Columns ?? 0;

                for (int row = 1; row <= rowCount; row++)
                {
                    var rowData = new List<object>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        rowData.Add(worksheet.Cells[row, col].Value);
                    }
                    result.Add(rowData);
                }
            }

            return result;
        }


        private void DeleteExistingWorksheets(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    while (package.Workbook.Worksheets.Count > 0)
                    {
                        package.Workbook.Worksheets.Delete(0);
                    }

                    // Убедимся, что хотя бы один лист существует перед сохранением
                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        package.Workbook.Worksheets.Add("DefaultSheet");
                    }

                    package.Save();
                }
            }
        }

    }