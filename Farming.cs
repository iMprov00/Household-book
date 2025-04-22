using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using Dapper;
using System.IO;
using static Household_book.Authorization;
using Bunifu.UI.WinForms;
using System.Drawing.Drawing2D;
using static Household_book.Main;

namespace Household_book
{
    public partial class Farming : Form
    {
        private bool isDragging = false;
        private Point lastCursorPos;
        public Farming()
        {
            InitializeComponent();
            LoadFarmsData();
            this.FormBorderStyle = FormBorderStyle.None;
            this.FormClosing += Farming_FormClosing;
        }

        private void Farming_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы точно хотите выйти?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Environment.Exit(Environment.ExitCode);

            }
        }

        public static class Database_farm
        {
            private static string DatabaseFile = "household_book.db";
            private static string ConnectionString => $"Data Source={DatabaseFile};Version=3;";

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

            // Добавление новой записи в farming
            public static bool AddFarm(int farmId, int personId, double area)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "INSERT INTO farming (farm_id, person_id, area) VALUES (@farmId, @personId, @area)",
                            new { farmId, personId, area });
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            // Обновление записи в farming
            public static bool UpdateFarm(int rowid, int farmId, int personId, double area)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "UPDATE farming SET farm_id = @farmId, person_id = @personId, area = @area WHERE rowid = @rowid",
                            new { rowid, farmId, personId, area });
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            // Удаление записи из farming
            public static bool DeleteFarm(int rowid)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "DELETE FROM farming WHERE rowid = @rowid",
                            new { rowid });
                        return true;
                    }
                }
                catch
                {
                    return false;
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


                /*
                 *                 @"SELECT f.rowid, f.farm_id, f.person_id, p.full_name, f.area 
                  FROM farming f
                  LEFT JOIN people p ON f.person_id = p.person_id");
                 */
                // Добавляем колонки только если их нет
                if (bunifuDataGridView1.Columns.Count == 0)
                {
                    bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                    {
                        Name = "f_id", // Добавляем Name для обращения
                        DataPropertyName = "farm_id",
                        HeaderText = "№ Фермы",
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


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

        private async Task AnimateClose()
        {
            int duration = 300;
            int steps = 10;
            float opacityStep = 1f / steps;
            int yStep = this.Height / steps;

            for (int i = 0; i < steps; i++)
            {
                this.Opacity -= opacityStep;
                this.Top -= yStep;
                await Task.Delay(duration / steps);
            }

            this.Hide();
        }

        private async Task AnimateShow(Form form)
        {
            form.Opacity = 0;
            form.Show();

            int duration = 300;
            int steps = 20;
            float opacityStep = 1f / steps;
            int yStep = form.Height / steps;

            // Начальная позиция (форма появляется снизу)
            form.Top += yStep * steps / 2;

            for (int i = 0; i < steps; i++)
            {
                form.Opacity += opacityStep;
                form.Top -= yStep / 2;
                await Task.Delay(duration / steps);
            }

            form.Opacity = 1;
            form.Top = (Screen.PrimaryScreen.WorkingArea.Height - form.Height) / 2;
        }
        private async void button_pl_Click(object sender, EventArgs e)
        {
            Main mainForm = new Main();
            await AnimateClose();

            await AnimateShow(mainForm);
        }

        private void bunifuButton21_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Farming_Load(object sender, EventArgs e)
        {
            ApplyRoundedCorners(bunifuDataGridView1, 15);
        }

        private void ApplyRoundedCorners(BunifuDataGridView dgv, int radius)
        {
            // Создаем GraphicsPath с закругленными углами
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, dgv.Width, dgv.Height);

            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Верхний левый угол
            path.AddArc(rect.Width - radius, rect.Y, radius, radius, 270, 90); // Верхний правый угол
            path.AddArc(rect.Width - radius, rect.Height - radius, radius, radius, 0, 90); // Нижний правый угол
            path.AddArc(rect.X, rect.Height - radius, radius, radius, 90, 90); // Нижний левый угол
            path.CloseFigure();

            // Применяем регион к DataGridView
            dgv.Region = new Region(path);
        }

        private void menuStrip1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastCursorPos = e.Location;
            }
        }

        private void menuStrip1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - lastCursorPos.X;
                this.Top += e.Y - lastCursorPos.Y;
            }
        }

        private void menuStrip1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void bunifuDataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (bunifuDataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = bunifuDataGridView1.SelectedRows[0];

                // Используем имена колонок, которые задали в LoadPeopleData
                text_hoz.Text = selectedRow.Cells["f_id"].Value?.ToString() ?? "";
                text_id.Text = selectedRow.Cells["f_pers"].Value?.ToString() ?? "";
                text_fio.Text = selectedRow.Cells["f_name"].Value?.ToString() ?? "";
                text_ge.Text = selectedRow.Cells["f_ar"].Value?.ToString() ?? "";
            }
        }

        private async void button_exit_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы точно хотите выйти из текущей учетной записи?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Authorization mainForm = new Authorization();
                await AnimateClose();

                await AnimateShow(mainForm);
            }
        }
    }
}
