using System;
using System.Drawing;
using System.Windows.Forms;
using OfficeOpenXml;
using System.Data.SQLite;
using System.IO;
using Bunifu.UI.WinForms;
using ClosedXML.Excel;
using Dapper;
using System.Collections.Generic;
using System.Linq;

using static Household_book.Farming.Database_farm;


namespace Household_book
{
    public partial class Report : Form
    {
        private BindingSource bindingSource = new BindingSource();
        private bool isDragging = false;
        private Point lastCursorPos;
        private List<Database_farm.FarmRecord> allPeople = new List<Database_farm.FarmRecord>();


        public Report()
        {
            InitializeComponent();
 
            this.FormClosing += Report_FormClosing;
            LoadFarmsData();

        }

        private void Report_Load(object sender, EventArgs e)
        {
            ApplyRoundedCorners(bunifuDataGridView1, 15);
        }

        private void ApplyRoundedCorners(BunifuDataGridView dgv, int radius)
        {
/*            // Создаем GraphicsPath с закругленными углами
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, dgv.Width, dgv.Height);

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Верхний левый угол
            path.AddArc(rect.Width - radius, rect.Y, radius, radius, 270, 90); // Верхний правый угол
            path.AddArc(rect.Width - radius, rect.Height - radius, radius, radius, 0, 90); // Нижний правый угол
            path.AddArc(rect.X, rect.Height - radius, radius, radius, 90, 90); // Нижний левый угол
            path.CloseFigure();

            // Применяем регион к DataGridView
            dgv.Region = new Region(path);*/
        }

        private void Report_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы точно хотите выйти?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Environment.Exit(Environment.ExitCode);

            }
        }

        public class ReportGenerator
        {
            private const string ConnectionString = "Data Source=household_book.db;Version=3;";

            public void GenerateExcelReport(string filePath)
            {
                using (var workbook = new XLWorkbook())
                {
                    // Создаем лист с общей статистикой
                    var statsSheet = workbook.Worksheets.Add("Общая статистика");
                    GenerateStatsSheet(statsSheet);

                    // Создаем лист с детализацией по хозяйствам
                    var detailsSheet = workbook.Worksheets.Add("Детализация");
                    GenerateDetailsSheet(detailsSheet);

                    workbook.SaveAs(filePath);
                }
            }

            private void GenerateStatsSheet(IXLWorksheet worksheet)
            {
                // Заголовки
                worksheet.Cell(1, 1).Value = "Всего жителей";
                worksheet.Cell(2, 1).Value = "Всего хозяйств";
                worksheet.Cell(3, 1).Value = "Всего животных";
                worksheet.Cell(4, 1).Value = "Всего техники";

                // Получаем данные из БД
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Всего жителей
                    var command = new SQLiteCommand("SELECT COUNT(*) FROM people", connection);
                    worksheet.Cell(1, 2).Value = Convert.ToInt32(command.ExecuteScalar());

                    // Всего хозяйств
                    command.CommandText = "SELECT COUNT(*) FROM farming";
                    worksheet.Cell(2, 2).Value = Convert.ToInt32(command.ExecuteScalar());

                    // Всего животных
                    command.CommandText = "SELECT SUM(quantity) FROM animals";
                    worksheet.Cell(3, 2).Value = Convert.ToInt32(command.ExecuteScalar());

                    // Всего техники
                    command.CommandText = "SELECT SUM(quantity) FROM technic";
                    worksheet.Cell(4, 2).Value = Convert.ToInt32(command.ExecuteScalar());
                }

                // Форматирование
                worksheet.Range(1, 1, 4, 2).Style.Font.Bold = true;
                worksheet.Range(1, 1, 4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 15;
            }

            private void GenerateDetailsSheet(IXLWorksheet worksheet)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Заголовки
                    worksheet.Cell(1, 1).Value = "ФИО жителя";
                    worksheet.Cell(1, 2).Value = "Дата рождения";
                    worksheet.Cell(1, 3).Value = "Адрес";
                    worksheet.Cell(1, 4).Value = "Кол-во хозяйств";
                    worksheet.Cell(1, 5).Value = "ID хозяйства";
                    worksheet.Cell(1, 6).Value = "Площадь хозяйства";
                    worksheet.Cell(1, 7).Value = "Тип животного";
                    worksheet.Cell(1, 8).Value = "Кол-во животных";
                    worksheet.Cell(1, 9).Value = "Тип техники";
                    worksheet.Cell(1, 10).Value = "Кол-во техники";

                    // Получаем данные о жителях
                    var command = new SQLiteCommand(
                        @"SELECT p.person_id, p.full_name, p.birth_date, p.address, 
                         COUNT(f.farm_id) as farm_count
                  FROM people p
                  LEFT JOIN farming f ON p.person_id = f.person_id
                  GROUP BY p.person_id, p.full_name, p.birth_date, p.address
                  ORDER BY p.full_name", connection);

                    using (var reader = command.ExecuteReader())
                    {
                        int row = 2;
                        while (reader.Read())
                        {
                            int personId = reader.GetInt32(0);
                            string fullName = reader.GetString(1);
                            string birthDate = reader.GetString(1);
                            string address = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            int farmCount = reader.GetInt32(4);

                            // Основная информация о жителе
                            worksheet.Cell(row, 1).Value = fullName;
                            worksheet.Cell(row, 2).Value = birthDate;
                            worksheet.Cell(row, 3).Value = address;
                            worksheet.Cell(row, 4).Value = farmCount;

                            // Получаем данные о хозяйствах этого жителя
                            GetFarmDetails(worksheet, personId, ref row);

                            row++;
                        }
                    }
                }

                // Форматирование
                worksheet.Range(1, 1, 1, 10).Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.SheetView.Freeze(1, 0); // Заменяет FreezePanes
                worksheet.Columns().AdjustToContents();
            }

            private void GetFarmDetails(IXLWorksheet worksheet, int personId, ref int startRow)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Получаем все хозяйства жителя
                    var farmCommand = new SQLiteCommand(
                        @"SELECT f.farm_id, f.area
                  FROM farming f
                  WHERE f.person_id = @personId
                  ORDER BY f.farm_id", connection);
                    farmCommand.Parameters.AddWithValue("@personId", personId);

                    using (var farmReader = farmCommand.ExecuteReader())
                    {
                        while (farmReader.Read())
                        {
                            int farmId = farmReader.GetInt32(0);
                            decimal area = farmReader.GetDecimal(1);

                            // Информация о хозяйстве
                            worksheet.Cell(startRow, 5).Value = farmId;
                            worksheet.Cell(startRow, 6).Value = area;

                            // Получаем животных в этом хозяйстве
                            GetAnimals(worksheet, farmId, startRow);

                            // Получаем технику в этом хозяйстве
                            GetTechnics(worksheet, farmId, startRow);

                            startRow++;
                        }
                    }
                }
            }

            private void GetAnimals(IXLWorksheet worksheet, int farmId, int row)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(
                        @"SELECT animal_type, quantity 
                  FROM animals 
                  WHERE farm_id = @farmId
                  ORDER BY animal_type", connection);
                    command.Parameters.AddWithValue("@farmId", farmId);

                    using (var reader = command.ExecuteReader())
                    {
                        int col = 7;
                        while (reader.Read())
                        {
                            worksheet.Cell(row, col).Value = reader.GetString(0);
                            worksheet.Cell(row, col + 1).Value = reader.GetInt32(1);
                            col += 2;
                        }
                    }
                }
            }

            private void GetTechnics(IXLWorksheet worksheet, int farmId, int row)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(
                        @"SELECT technic_type, quantity 
                  FROM technic 
                  WHERE farm_id = @farmId
                  ORDER BY technic_type", connection);
                    command.Parameters.AddWithValue("@farmId", farmId);

                    using (var reader = command.ExecuteReader())
                    {
                        int col = 9;
                        while (reader.Read())
                        {
                            worksheet.Cell(row, col).Value = reader.GetString(0);
                            worksheet.Cell(row, col + 1).Value = reader.GetInt32(1);
                            col += 2;
                        }
                    }
                }
            }
        }



        private void bunifuTextBox1_TextChange(object sender, EventArgs e)
        {
/*            string searchText = bunifuTextBox1.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                bunifuDataGridView1.DataSource = allPeople; // Сброс
            }
            else
            {
                var filteredPeople = allPeople
                    .Where(p =>
                        (p.full_name != null && p.full_name.ToLower().Contains(searchText)) ||
                        (p.address != null && p.address.ToLower().Contains(searchText)) ||
                        (p.birth_date != null && p.birth_date.ToString().ToLower().Contains(searchText)) || // Дата
                        (p.person_id.ToString().Contains(searchText)) // Числовое поле (ID)
                                                                      // Добавьте другие поля по аналогии...
                    )
                    .ToList();

                bunifuDataGridView1.DataSource = filteredPeople;
            }*/
        }

        private void bunifuButton22_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            saveFileDialog.Title = "Сохранить отчет";
            saveFileDialog.FileName = "Отчет по хозяйствам от" + DateTime.Now.ToString("dd MMMM yyyy г.") + ".xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var reportGenerator = new ReportGenerator();
                    reportGenerator.GenerateExcelReport(saveFileDialog.FileName);
                    MessageBox.Show("Отчет успешно сформирован!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static class Database_farm
        {
            private static string DatabaseFile = "household_book.db";
            public static string ConnectionString => $"Data Source={DatabaseFile};Version=3;";

            // Класс для представления записи из таблицы farming
            public class FarmRecord
            {
                public int farm_id { get; set; }    // ID подсобного хозяйства
                public int person_id { get; set; } // ID владельца (связь с people)
                public string full_name { get; set; } // ФИО владельца (из таблицы people)
                public double area { get; set; }    // Площадь
            }

            // Получение всех записей из farming с ФИО владельцев
            public static IEnumerable<FarmRecord> GetAllFarms()
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    return connection.Query<FarmRecord>(
                        @"SELECT f.farm_id, f.person_id, p.full_name, f.area 
                  FROM farming f
                  LEFT JOIN people p ON f.person_id = p.person_id");
                }
            }


            // Получение конкретной записи по ID
            public static FarmRecord GetFarmById(int rowid)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    return connection.QueryFirstOrDefault<FarmRecord>(
                        @"SELECT f.rowid, f.farm_id, f.person_id, p.full_name, f.area 
                  FROM farming f
                  LEFT JOIN people p ON f.person_id = p.person_id
                  WHERE f.rowid = @rowid",
                        new { rowid });
                }
            }

            // Получение списка хозяйств по person_id
            public static IEnumerable<FarmRecord> GetFarmsByPersonId(int personId)
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    return connection.Query<FarmRecord>(
                        @"SELECT f.rowid, f.farm_id, f.person_id, p.full_name, f.area 
                  FROM farming f
                  LEFT JOIN people p ON f.person_id = p.person_id
                  WHERE f.person_id = @personId",
                        new { personId });
                }

            }
        }

            private void LoadFarmsData()
        {
            try
            {
                // Получаем данные из БД
                var people = Database_farm.GetAllFarms().ToList();

                 
                // Добавляем колонки только если их нет
                if (bunifuDataGridView1.Columns.Count == 0)
                {
                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "f_id", // Добавляем Name для обращения
                        DataPropertyName = "farm_id",
                        HeaderText = "№ Хозяйства",
                        Width = 50
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "f_pers",
                        DataPropertyName = "person_id",
                        HeaderText = "ID жителя",
                        Width = 100
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "f_name",
                        DataPropertyName = "full_name",
                        HeaderText = "ФИО",
                        Width = 100
                    });

                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "f_ar",
                        DataPropertyName = "area",
                        HeaderText = "Площадь (га)",
                        Width = 100
                    });

                }

                // Устанавливаем источник данных
                bunifuDataGridView1.DataSource = people;
                allPeople = Database_farm.GetAllFarms().ToList();
                bunifuDataGridView1.DataSource = allPeople; // Первая загрузка



               // Настройка цветов
                bunifuDataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.PapayaWhip; // Цвет четных строк
                bunifuDataGridView1.RowsDefaultCellStyle.BackColor = Color.White; // Цвет нечетных строк
                bunifuDataGridView1.BackgroundColor = Color.White; // Цвет фона

                // Цвет выделенной строки
                bunifuDataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGray;
                bunifuDataGridView1.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.DarkGray;

                // Цвет заголовков
                bunifuDataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Goldenrod;
                bunifuDataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
            private void bunifuDataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (bunifuDataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = bunifuDataGridView1.SelectedRows[0];

                // Используем имена колонок, которые задали в LoadPeopleData
                text_hoz.Text = selectedRow.Cells["f_id"].Value?.ToString() ?? "";
            }
        }

        private void bunifuLabel2_Click(object sender, EventArgs e)
        {

        }

        private void button_ref_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(text_hoz.Text))
            {
                MessageBox.Show("Выберите хозяйство из списка", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(text_hoz.Text, out int farmId))
            {
                MessageBox.Show("Некорректный ID хозяйства", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx";
            saveFileDialog.Title = "Сохранить справку";
            saveFileDialog.FileName = $"Справка хозяйства №{farmId} от {DateTime.Now:dd MMMM yyyy г.}.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GenerateFarmReport(farmId, saveFileDialog.FileName);
                    MessageBox.Show("Справка успешно сформирована!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при формировании справки: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void GenerateFarmReport(int farmId, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                // Создаем лист с справкой
                var worksheet = workbook.Worksheets.Add("Справка");

                // Получаем данные о хозяйстве
                var farmData = GetFarmData(farmId);
                var personData = GetPersonData(farmData.person_id);
                var animalsCount = GetAnimalCount(farmId);
                var technicsCount = GetTechnicCount(farmId);

                // Форматируем справку
                worksheet.Columns().AdjustToContents();
                worksheet.Style.Font.FontName = "Times New Roman";
                worksheet.Style.Font.FontSize = 12;

                // Заголовок
                var titleCell = worksheet.Cell(1, 1);
                titleCell.Value = "Справка";
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.FontSize = 16;
                titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(1, 1, 1, 3).Merge();

                // Подзаголовок
                worksheet.Cell(2, 1).Value = "Из хозяйственной книги жителя";
                worksheet.Range(2, 1, 2, 3).Merge();

                // Нормативный акт
                worksheet.Cell(3, 1).Value = "уп. приказом Федеральной регистрационной службы";
                worksheet.Cell(4, 1).Value = "от 28 августа 2006 г. №148";
                worksheet.Cell(5, 1).Value = "зарег. в Министр России 30 августа 2006 г. рег. № 5183";

                // Место и дата выдачи
                worksheet.Cell(7, 1).Value = $"Место выдачи: Администрация Березовского сельсовета";
                worksheet.Cell(8, 1).Value = $"Дата выдачи: {DateTime.Now:dd MMMM yyyy г.}";

                // Глава администрации
                worksheet.Cell(10, 1).Value = "Глава Администрации: Герасимова Яна Сергеевна";
                worksheet.Cell(11, 1).Value = "Подпись, печать";

                // Основной текст
                worksheet.Cell(13, 1).Value = $"Настоящая справка из хозяйственной книги подтверждает, что граждан(ка)";
                worksheet.Cell(14, 1).Value = personData.full_name;
                worksheet.Cell(15, 1).Value = $"дата рождения: {personData.birth_date:dd.MM.yyyy}";
                worksheet.Cell(16, 1).Value = $"проживает по адресу: {personData.address}";
                worksheet.Cell(17, 1).Value = $"имеет во владении недвижимость площадью: {farmData.area} га";
                worksheet.Cell(18, 1).Value = $"количество животных: {animalsCount}";
                worksheet.Cell(19, 1).Value = $"количество техники: {technicsCount}";

                // Дата
                var dateCell = worksheet.Cell(21, 3);
                dateCell.Value = $"{DateTime.Now:dd MMMM yyyy г.}";
                dateCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Сохраняем файл
                workbook.SaveAs(filePath);
            }
        }

        // Методы для получения данных
        private FarmRecord GetFarmData(int farmId)
        {
            using (var connection = new SQLiteConnection(Database_farm.ConnectionString))
            {
                return connection.QueryFirstOrDefault<FarmRecord>(
                    "SELECT * FROM farming WHERE farm_id = @farmId",
                    new { farmId });
            }
        }

        private PersonRecord GetPersonData(int personId)
        {
            using (var connection = new SQLiteConnection(Database_farm.ConnectionString))
            {
                return connection.QueryFirstOrDefault<PersonRecord>(
                    "SELECT * FROM people WHERE person_id = @personId",
                    new { personId });
            }
        }

        private int GetAnimalCount(int farmId)
        {
            using (var connection = new SQLiteConnection(Database_farm.ConnectionString))
            {
                var result = connection.ExecuteScalar<int?>(
                    "SELECT SUM(quantity) FROM animals WHERE farm_id = @farmId",
                    new { farmId });
                return result ?? 0;
            }
        }

        private int GetTechnicCount(int farmId)
        {
            using (var connection = new SQLiteConnection(Database_farm.ConnectionString))
            {
                var result = connection.ExecuteScalar<int?>(
                    "SELECT SUM(quantity) FROM technic WHERE farm_id = @farmId",
                    new { farmId });
                return result ?? 0;
            }
        }

        // Классы для данных
        public class PersonRecord
        {
            public int person_id { get; set; }
            public string full_name { get; set; }
            public string birth_date { get; set; }
            public string address { get; set; }
        }

    }
    
}
