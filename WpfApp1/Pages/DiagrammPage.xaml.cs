using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Charting = System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace WpfApp1.Pages
{
    public partial class DiagrammPage : Page
    {
        private Charting.Chart _chart = new Charting.Chart();
        private Charting.SeriesChartType[] _types =
        {
            Charting.SeriesChartType.Column,
            Charting.SeriesChartType.Pie,
            Charting.SeriesChartType.Line
        };
        private List<Bookings> _bookings;
        private List<Museums> _museums;

        public DiagrammPage()
        {
            InitializeComponent();

            Helper.Refresh();
            _bookings = Core.DB.Bookings.Include(b => b.Users).Include(b => b.Museums).ToList();
            _museums = Core.DB.Museums.OrderBy(m => m.Name).ToList();

            _chart.ChartAreas.Add(new Charting.ChartArea("Main"));
            _chart.ChartAreas[0].AxisX.Interval = 1;
            _chart.Legends.Add(new Charting.Legend("Default"));
            _chart.Series.Add(new Charting.Series("Сумма бронирований, руб.") { IsValueShownAsLabel = true });
            ChartHost.Child = _chart;

            CmbUser.ItemsSource = Core.DB.Users.OrderBy(u => u.FIO).ToList();
            CmbType.ItemsSource = new string[] { "Столбчатая", "Круговая", "Линейная" };
            CmbType.SelectedIndex = 0;
            CmbUser.SelectedIndex = 0;
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            Users user = CmbUser.SelectedItem as Users;
            if (user == null || CmbType.SelectedIndex < 0 || _bookings == null)
                return;

            Charting.Series series = _chart.Series[0];
            series.ChartType = _types[CmbType.SelectedIndex];
            series.Points.Clear();

            foreach (Museums museum in _museums)
            {
                decimal sum = _bookings.Where(b => b.UserID == user.ID && b.MuseumID == museum.ID).Sum(b => b.Total);
                if (sum > 0)
                    series.Points.AddXY(museum.Name, sum);
            }
        }

        private string GetFilePath()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(folder, "Bookings_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        private void ButtonWord_Click(object sender, RoutedEventArgs e)
        {
            List<Bookings> bookings = _bookings.OrderBy(b => b.Date).ToList();
            string path = GetFilePath();

            try
            {
                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                Word.Paragraph titleParagraph = document.Paragraphs.Add();
                Word.Range titleRange = titleParagraph.Range;
                titleRange.Text = "Отчет по бронированию билетов";
                titleRange.Font.Name = "Times New Roman";
                titleRange.Font.Size = 16;
                titleRange.Font.Bold = 1;
                titleRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                titleRange.InsertParagraphAfter();
                document.Paragraphs.Add();

                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table table = document.Tables.Add(tableRange, bookings.Count + 1, 6);
                table.Borders.InsideLineStyle = table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                table.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                table.Range.Font.Name = "Times New Roman";
                table.Range.Font.Size = 12;

                string[] headers = { "Дата", "Пользователь", "Музей", "Выставка", "Билетов", "Сумма, руб." };
                for (int i = 0; i < headers.Length; i++)
                    table.Cell(1, i + 1).Range.Text = headers[i];
                table.Rows[1].Range.Font.Bold = 1;
                table.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                decimal total = 0;
                for (int i = 0; i < bookings.Count; i++)
                {
                    Bookings booking = bookings[i];
                    table.Cell(i + 2, 1).Range.Text = booking.Date.ToString("dd.MM.yyyy");
                    table.Cell(i + 2, 2).Range.Text = booking.Users.FIO;
                    table.Cell(i + 2, 3).Range.Text = booking.Museums.Name;
                    table.Cell(i + 2, 4).Range.Text = booking.Name;
                    table.Cell(i + 2, 5).Range.Text = booking.Num.ToString();
                    table.Cell(i + 2, 6).Range.Text = booking.Total.ToString("N2");
                    total += booking.Total;
                }

                document.Paragraphs.Add();
                Word.Paragraph totalParagraph = document.Paragraphs.Add();
                Word.Range totalRange = totalParagraph.Range;
                totalRange.Text = "Общая сумма бронирований: " + total.ToString("N2") + " руб.";
                totalRange.Font.Name = "Times New Roman";
                totalRange.Font.Size = 14;
                totalRange.Font.Bold = 1;

                foreach (Word.Section section in document.Sections)
                {
                    section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                    section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Text = DateTime.Now.ToString("dd.MM.yyyy");
                }

                document.SaveAs2(path + ".docx");
                document.ExportAsFixedFormat(path + ".pdf", Word.WdExportFormat.wdExportFormatPDF);
                application.Visible = true;
                Helper.ShowInfo("Отчет сохранен:\n" + path + ".docx\n" + path + ".pdf");
            }
            catch (Exception ex)
            {
                Helper.ShowError("Не удалось создать отчет Word: " + ex.Message);
            }
        }

        private void ButtonExcel_Click(object sender, RoutedEventArgs e)
        {
            List<Bookings> bookings = _bookings.OrderBy(b => b.Date).ToList();
            string path = GetFilePath() + ".xlsx";

            try
            {
                var application = new Excel.Application();
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                worksheet.Name = "Бронирования";

                string[] headers = { "Дата", "Пользователь", "Музей", "Выставка", "Билетов", "Цена", "Сумма" };
                for (int i = 0; i < headers.Length; i++)
                    worksheet.Cells[1, i + 1] = headers[i];
                Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 7]];
                headerRange.Font.Bold = true;
                headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                int row = 2;
                foreach (Bookings booking in bookings)
                {
                    worksheet.Cells[row, 1] = booking.Date.ToString("dd.MM.yyyy");
                    worksheet.Cells[row, 2] = booking.Users.FIO;
                    worksheet.Cells[row, 3] = booking.Museums.Name;
                    worksheet.Cells[row, 4] = booking.Name;
                    worksheet.Cells[row, 5] = booking.Num;
                    worksheet.Cells[row, 6] = booking.Price;
                    ((Excel.Range)worksheet.Cells[row, 7]).Formula = "=E" + row + "*F" + row;
                    row++;
                }

                worksheet.Cells[row, 6] = "ИТОГО:";
                ((Excel.Range)worksheet.Cells[row, 7]).Formula = "=SUM(G2:G" + (row - 1) + ")";
                Excel.Range totalRange = worksheet.Range[worksheet.Cells[row, 6], worksheet.Cells[row, 7]];
                totalRange.Font.Bold = true;
                totalRange.Font.Color = Excel.XlRgbColor.rgbRed;

                Excel.Range allRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[row, 7]];
                allRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Columns.AutoFit();

                workbook.SaveAs(path, Excel.XlFileFormat.xlOpenXMLWorkbook);
                application.Visible = true;
                Helper.ShowInfo("Отчет сохранен:\n" + path);
            }
            catch (Exception ex)
            {
                Helper.ShowError("Не удалось создать отчет Excel: " + ex.Message);
            }
        }
    }
}
