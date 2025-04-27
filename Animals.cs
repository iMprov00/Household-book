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
using System.Diagnostics;
using static Household_book.Main.Database_pl;
using static Household_book.Farming;
using static Household_book.Animals.Database_an;

namespace Household_book
{
    public partial class Animals : Form
    {
        private bool isDragging = false;
        private Point lastCursorPos;
        private List<AnimalRecord> allPeople = new List<AnimalRecord>(); // Поле класса

        public Animals()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.FormClosing += Animals_FormClosing;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            LoadAnimalsData();
        }

        public static class Database_an
        {
            private static string DatabaseFile = "household_book.db";
            public static string ConnectionString => $"Data Source={DatabaseFile};Version=3;";

            public class AnimalRecord
            {
                public int animal_id { get; set; }
                public int farm_id { get; set; }
                public string animal_type { get; set; }
                public int quantity { get; set; }
            }

            public static IEnumerable<AnimalRecord> GetAllAnimals()
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    return connection.Query<AnimalRecord>("SELECT * FROM animals");
                }
            }

            public static bool AddAnimal(AnimalRecord animal)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "INSERT INTO animals (farm_id, animal_type, quantity) VALUES (@farm_id, @animal_type, @quantity)",
                            animal);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            public static bool UpdateAnimal(AnimalRecord animal)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "UPDATE animals SET farm_id = @farm_id, animal_type = @animal_type, quantity = @quantity WHERE animal_id = @animal_id",
                            animal);
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            public static bool DeleteAnimal(int animalId)
            {
                try
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Execute(
                            "DELETE FROM animals WHERE animal_id = @animalId",
                            new { animalId });
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private void LoadAnimalsData()
        {
            try
            {
                bunifuDataGridView1.SuspendLayout();

                var animals = Database_an.GetAllAnimals().ToList();

                if (bunifuDataGridView1.Columns.Count == 0)
                {
                    AddGridColumns();
                }

                bunifuDataGridView1.DataSource = animals;
                allPeople = Database_an.GetAllAnimals().ToList();
                bunifuDataGridView1.DataSource = allPeople; // Первая загрузка
                ConfigureGridAppearance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                bunifuDataGridView1.ResumeLayout();
            }
        }

        private void AddGridColumns()
        {
            var columns = new[]
            {
                new { Name = "col_animal_id", Header = "ID животного", Width = 100, DataProperty = "animal_id" },
                new { Name = "col_farm_id", Header = "№ хозяйства", Width = 100, DataProperty = "farm_id" },
                new { Name = "col_type", Header = "Тип животного", Width = 200, DataProperty = "animal_type" },
                new { Name = "col_quantity", Header = "Количество", Width = 100, DataProperty = "quantity" }
            };

            foreach (var col in columns)
            {
                bunifuDataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = col.Name,
                    DataPropertyName = col.DataProperty,
                    HeaderText = col.Header,
                    Width = col.Width
                });
            }
        }

        private void ConfigureGridAppearance()
        {
            bunifuDataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.PapayaWhip;
            bunifuDataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            bunifuDataGridView1.BackgroundColor = Color.White;
            bunifuDataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGray;
            bunifuDataGridView1.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.DarkGray;
            bunifuDataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Goldenrod;
            bunifuDataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        }


        private void bunifuLabel2_Click(object sender, EventArgs e)
        {

        }

        private void bunifuLabel3_Click(object sender, EventArgs e)
        {

        }

        private void text_ge_TextChanged(object sender, EventArgs e)
        {

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

        private void Animals_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Вы точно хотите выйти?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Environment.Exit(Environment.ExitCode);

            }
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

        private void Animals_Load(object sender, EventArgs e)
        {
            
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





        private void button_ad_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(text_id_hoz.Text) || string.IsNullOrEmpty(text_type.Text) || string.IsNullOrEmpty(text_kol.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(text_id_hoz.Text, out int farmId) || !int.TryParse(text_kol.Text, out int quantity))
            {
                MessageBox.Show("ID хозяйства и количество должны быть числами!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newAnimal = new Database_an.AnimalRecord
            {
                farm_id = farmId,
                animal_type = text_type.Text,
                quantity = quantity
            };

            if (Database_an.AddAnimal(newAnimal))
            {
                MessageBox.Show("Запись успешно добавлена!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadAnimalsData();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении записи!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_ref_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(text_animal.Text))
            {
                MessageBox.Show("Не выбрана запись для изменения!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(text_id_hoz.Text) || string.IsNullOrEmpty(text_type.Text) || string.IsNullOrEmpty(text_kol.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы точно хотите изменить запись?", "Подтверждение изменения",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                var updatedAnimal = new Database_an.AnimalRecord
                {
                    animal_id = int.Parse(text_animal.Text),
                    farm_id = int.Parse(text_id_hoz.Text),
                    animal_type = text_type.Text,
                    quantity = int.Parse(text_kol.Text)
                };

                if (Database_an.UpdateAnimal(updatedAnimal))
                {
                    MessageBox.Show("Запись успешно обновлена!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAnimalsData();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении записи!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttond_del_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(text_animal.Text))
            {
                MessageBox.Show("Не выбрана запись для удаления!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int animalId = int.Parse(text_animal.Text);

                if (Database_an.DeleteAnimal(animalId))
                {
                    MessageBox.Show("Запись успешно удалена!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAnimalsData();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении записи!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void ClearFields()
        {
            text_animal.Text = "";
            text_id_hoz.Text = "";
            text_type.Text = "";
            text_kol.Text = "";
        }

        private void bunifuDataGridView1_SelectionChanged(object sender, EventArgs e)
        {

        }

        private void bunifuDataGridView1_SelectionChanged_1(object sender, EventArgs e)
        {
            if (bunifuDataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = bunifuDataGridView1.SelectedRows[0];

                text_animal.Text = selectedRow.Cells["col_animal_id"].Value?.ToString() ?? "";
                text_id_hoz.Text = selectedRow.Cells["col_farm_id"].Value?.ToString() ?? "";
                text_type.Text = selectedRow.Cells["col_type"].Value?.ToString() ?? "";
                text_kol.Text = selectedRow.Cells["col_quantity"].Value?.ToString() ?? "";
            }
        }

        private void Animals_Load_1(object sender, EventArgs e)
        {
            ApplyRoundedCorners(bunifuDataGridView1, 15);
        }

        private async void button_pl_Click(object sender, EventArgs e)
        {
            Main mainForm = new Main();
            await AnimateClose();

            await AnimateShow(mainForm);
        }

        private async void bunifuButton24_Click(object sender, EventArgs e)
        {
            Farming mainForm = new Farming();
            await AnimateClose();

            await AnimateShow(mainForm);
        }

        private async void button_tech_Click(object sender, EventArgs e)
        {
            Technic mainForm = new Technic();
            await AnimateClose();

            await AnimateShow(mainForm);
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

        private void bunifuTextBox1_TextChange(object sender, EventArgs e)
        {
            string searchText = bunifuTextBox1.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                bunifuDataGridView1.DataSource = allPeople; // Сброс
            }
            else
            {
                var filteredPeople = allPeople
                    .Where(p =>
                        (p.animal_id.ToString().Contains(searchText)) ||
                        (p.farm_id.ToString().Contains(searchText)) ||
                        (p.animal_type != null && p.animal_type.ToString().ToLower().Contains(searchText)) || // Дата
                        (p.quantity.ToString().Contains(searchText)) // Числовое поле (ID)
                                                                      // Добавьте другие поля по аналогии...
                    )
                    .ToList();

                bunifuDataGridView1.DataSource = filteredPeople;
            }
        }
    }
}
