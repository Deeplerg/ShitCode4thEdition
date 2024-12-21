using System.IO;
using OfficeOpenXml;

namespace AlgorithmVizualizer.Desktop;

 public class SortingTable
 {
     private int _numberSorting;
        
        async public Task NaturalMergeSortAlgorithm(string inputFile, string tempFileB, string tempFileC, 
            int columnIndex)
        {
            bool sorted = false;
            while (!sorted)
            {
                NaturalSplitExcelFile(inputFile, tempFileB, tempFileC, columnIndex);
                sorted = MergeExcelFiles(inputFile, tempFileB, tempFileC, columnIndex);

                // Задержка для визуализации этапов
                //Thread.Sleep(1000); // 1 секунда
            }

        }

        async public Task StreightMergeSortAlgorithm(string inputFile, string tempFileB, string tempFileC,
            int columnIndex)
        {
            bool sorted = false;
            _numberSorting = 1;
            while (!sorted)
            {
               StraightSplitExcelFile(inputFile, tempFileB, tempFileC);
                sorted = MergeExcelFiles(inputFile, tempFileB, tempFileC, columnIndex);

                // Задержка для визуализации этапов
                //Thread.Sleep(1000); // 1 секунда
            }
        }

       public void NaturalSplitExcelFile(string inputFile, string tempFileB, string tempFileC, int columnIndex)
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

            int start = 1;
            while (start <= rowCount)
            {
                int end = start;
                while (end < rowCount && 
                       string.Compare(
                           worksheet.Cells[end + 1, columnIndex].GetValue<string>() ?? "", 
                           worksheet.Cells[end, columnIndex].GetValue<string>() ?? "", 
                           StringComparison.CurrentCulture) >= 0)
                {
                    end++;
                }

                if (writeToB)
                {
                    for (int i = start; i <= end; i++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            worksheetB.Cells[rowB, col].Value = worksheet.Cells[i, col].Value;
                        }
                        rowB++;
                    }
                }
                else
                {
                    for (int i = start; i <= end; i++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            worksheetC.Cells[rowC, col].Value = worksheet.Cells[i, col].Value;
                        }
                        rowC++;
                    }
                }

                writeToB = !writeToB;
                start = end + 1;
            }

            packageB.Save();
            packageC.Save();
            package.Save();
        }
    }
}
       
       public void StraightSplitExcelFile(string inputFile, string tempFileB, string tempFileC)
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

            int start = 1;
            while (start <= rowCount)
            {
                int end = (int)Math.Pow(2,_numberSorting);
                if (end > rowCount)
                {
                    end = rowCount;
                }

                if (writeToB)
                {
                    for (int i = start; i <= end; i++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            worksheetB.Cells[rowB, col].Value = worksheet.Cells[i, col].Value;
                        }
                        rowB++;
                    }
                }
                else
                {
                    for (int i = start; i <= end; i++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            worksheetC.Cells[rowC, col].Value = worksheet.Cells[i, col].Value;
                        }
                        rowC++;
                    }
                }

                writeToB = !writeToB;
                start = end + 1;
            }

            _numberSorting++;
            packageB.Save();
            packageC.Save();
            package.Save();
        }
    }
}


public bool MergeExcelFiles(string outputFile, string tempFileB, string tempFileC, int columnIndex)
{
    using (var packageB = new ExcelPackage(new FileInfo(tempFileB)))
    using (var packageC = new ExcelPackage(new FileInfo(tempFileC)))
    using (var packageOutput = new ExcelPackage(new FileInfo(outputFile)))
    {
        DeleteExistingWorksheets(tempFileB);
        DeleteExistingWorksheets(tempFileC);
        

        var worksheetB = packageB.Workbook.Worksheets["Sheet1"]; // <--- Исправлено
        var worksheetC = packageC.Workbook.Worksheets["Sheet1"]; // <--- Исправлено
        var worksheetOutput = packageOutput.Workbook.Worksheets[0];
        
        worksheetOutput.Cells.Clear();
        
        int rowB = 1, rowC = 1, rowOutput = 1;
        int rowCountB = worksheetB.Dimension?.Rows ?? 0;
        int rowCountC = worksheetC.Dimension?.Rows ?? 0;
        int colCount = worksheetB.Dimension?.Columns ?? worksheetC.Dimension?.Columns ?? 0;

        bool sorted = true;

        while (rowB <= rowCountB || rowC <= rowCountC)
        {


            while (rowB <= rowCountB && rowC <= rowCountC)
            {
                var valueB = worksheetB.Cells[rowB, columnIndex].GetValue<string>();
                var valueC = worksheetC.Cells[rowC, columnIndex].GetValue<string>();

                if (string.Compare(valueB, valueC, StringComparison.CurrentCulture) <= 0)
                {
                    // Копируем строку из B
                    CopyRow(worksheetB, worksheetOutput, rowB, rowOutput, colCount);
                    rowB++;
                    rowOutput++;
                }
                else
                {
                    // Копируем строку из C
                    CopyRow(worksheetC, worksheetOutput, rowC, rowOutput, colCount);
                    rowC++;
                    rowOutput++;
                    sorted = false; // Отмечаем, что файлы не отсортированы
                }
            }

            // Копируем оставшиеся элементы из B или C
            while (rowB <= rowCountB)
            {
                CopyRow(worksheetB, worksheetOutput, rowB, rowOutput, colCount);
                rowB++;
                rowOutput++;
            }
            while (rowC <= rowCountC)
            {
                CopyRow(worksheetC, worksheetOutput, rowC, rowOutput, colCount);
                rowC++;
                rowOutput++;
            }

        }
        
        packageOutput.Save();
        return sorted;
    }
}


// Вспомогательный метод для копирования строки
        private void CopyRow(ExcelWorksheet source, ExcelWorksheet destination, int sourceRow, int destRow, int colCount)
        {
            // Проверяем, что листы не null
            if (source == null || destination == null)
            {
                throw new ArgumentNullException("Source or destination worksheet is null.");
            }

            for (int col = 1; col <= colCount; col++)
            {
                // Проверяем, что исходная ячейка не выходит за пределы
                if (source.Dimension == null || sourceRow > source.Dimension.Rows || col > source.Dimension.Columns)
                {
                    continue; // Пропускаем, если ячейка не существует
                }

                var cellValue = source.Cells[sourceRow, col]?.Value;

                // Проверяем, что значение ячейки не null
                if (cellValue != null)
                {
                    if (destination.Dimension == null || destRow > destination.Dimension.Rows || col > destination.Dimension.Columns)
                    {
                        // Создаём ячейку назначения при необходимости
                        destination.Cells[destRow, col].Value = cellValue;
                    }
                    else
                    {
                        destination.Cells[destRow, col].Value = cellValue;
                    }
                }
            }
        }

        public List<List<string>> ReadExcelFile(string filePath)
        {
            var result = new List<List<string>>();

            if (!File.Exists(filePath))
                return result;

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            int colCount = worksheet.Dimension?.Columns ?? 0;

            for (int row = 1; row <= rowCount; row++)
            {
                var rowData = new List<string>();
                for (int col = 1; col <= colCount; col++)
                {
                    rowData.Add(worksheet.Cells[row, col].Text);
                }
                result.Add(rowData);
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