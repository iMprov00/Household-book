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
using System.Diagnostics;
using static Household_book.Main.Database_pl;
using static Household_book.Farming.Database_farm;

namespace Household_book
{
    public partial class Farming : Form
    {
        private bool isDragging = false;
        private Point lastCursorPos;
        private List<FarmRecord> allPeople = new List<FarmRecord>(); // Поле класса
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
                            "UPDATE farming SET area = @area WHERE farm_id = @farmId",
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
            public static bool DeleteFarm(int farmId)
            {
                using (var connection = new SQLiteConnection(Database_farm.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Удаляем записи из animals для этого farm_id
                            connection.Execute(
                                "DELETE FROM animals WHERE farm_id = @farmId",
                                new { farmId },
                                transaction: transaction);

                            // 2. Удаляем записи из technic для этого farm_id
                            connection.Execute(
                                "DELETE FROM technic WHERE farm_id = @farmId",
                                new { farmId },
                                transaction: transaction);

                            // 3. Удаляем саму запись из farming
                            connection.Execute(
                                "DELETE FROM farming WHERE farm_id = @farmId",
                                new { farmId },
                                transaction: transaction);

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Debug.WriteLine($"Ошибка при удалении хозяйства: {ex.Message}");
                            return false;
                        }
                    }
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

        private void button_ref_Click(object sender, EventArgs e)
        {
            // Проверка выбранной записи
            if (string.IsNullOrEmpty(text_hoz.Text))
            {
                MessageBox.Show("Не выбрана запись для изменения!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(text_id.Text) || string.IsNullOrEmpty(text_ge.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка корректности числовых значений
            if (!int.TryParse(text_id.Text, out int personId))
            {
                MessageBox.Show("ID жителя должен быть целым числом!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(text_ge.Text, out double area))
            {
                MessageBox.Show("Площадь должна быть числом!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Подтверждение действия
            var result = MessageBox.Show("Вы точно хотите изменить запись?", "Подтверждение изменения",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int farmId = int.Parse(text_hoz.Text);

                    // Обновляем запись (используем farmId как идентификатор)
                    if (Database_farm.UpdateFarm(0, farmId, personId, area))
                    {
                        MessageBox.Show("Запись успешно обновлена!", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadFarmsData(); // Обновляем данные в таблице
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении записи!", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearFields()
        {
            text_id.Text = "";
            text_hoz.Text = "";
            text_ge.Text = "";
            text_fio.Text = "";
        }

        private void buttond_del_Click(object sender, EventArgs e)
        {
            // Удаление записи
            if (string.IsNullOrEmpty(text_hoz.Text))
            {
                MessageBox.Show("Не выбрано хозяйство для удаления!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить это хозяйство?\nЭто также удалит все связанные записи о животных и технике.",
                                       "Подтверждение",
                                       MessageBoxButtons.YesNo,
                                       MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                int farmId = int.Parse(text_hoz.Text);

                if (Database_farm.DeleteFarm(farmId))
                {
                    MessageBox.Show("Хозяйство и связанные данные успешно удалены!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFarmsData(); // Обновляем данные в таблице
                    ClearFields(); // Очищаем поля
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении хозяйства!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button_ad_Click(object sender, EventArgs e)
        {
            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(text_id.Text) || string.IsNullOrEmpty(text_ge.Text))
            {
                MessageBox.Show("Пожалуйста, заполните ID жителя и площадь!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка корректности числовых значений
            if (!int.TryParse(text_id.Text, out int personId))
            {
                MessageBox.Show("ID жителя должен быть целым числом!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(text_ge.Text, out double area))
            {
                MessageBox.Show("Площадь должна быть числом!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Создаем диалог выбора действия
            var choiceDialog = MessageBox.Show($"Добавить животных/технику к хозяйству \"{text_hoz.Text}\"?",
                                             "Выбор действия",
                                             MessageBoxButtons.YesNoCancel,
                                             MessageBoxIcon.Question);

            if (choiceDialog == DialogResult.Cancel)
            {
                return; // Отмена операции
            }

            if (choiceDialog == DialogResult.No)
            {
                // Просто добавляем новую ферму
                try
                {
                    using (var connection = new SQLiteConnection(Database_farm.ConnectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Получаем максимальный farm_id и увеличиваем на 1
                                int maxFarmId = connection.ExecuteScalar<int>(
                                    "SELECT COALESCE(MAX(farm_id), 0) FROM farming",
                                    transaction: transaction);

                                int newFarmId = maxFarmId + 1;

                                // Добавляем хозяйство
                                connection.Execute(
                                    "INSERT INTO farming (farm_id, person_id, area) VALUES (@farmId, @personId, @area)",
                                    new { farmId = newFarmId, personId, area },
                                    transaction: transaction);

                                transaction.Commit();

                                MessageBox.Show("Новое хозяйство успешно добавлено!", "Успех",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadFarmsData();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Ошибка при добавлении хозяйства: {ex.Message}", "Ошибка",
                                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (choiceDialog == DialogResult.Yes)
            {
                // Создаем форму для добавления животных/техники
                Form addForm = new Form()
                {
                    Width = 400,
                    Height = 250,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = "Добавление животных/техники",
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.Moccasin,
                    Font = new Font("Century Gothic", 12)
                };


                Label labelCategory = new Label()
                {
                    Text = "Категория:",
                    Left = 20,
                    Top = 120,
                    Width = 100,
                    Height = 30
                };

                ComboBox comboBoxCategory = new ComboBox()
                {
                    Left = 130,
                    Top = 120,
                    Width = 240,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                comboBoxCategory.Items.AddRange(new string[] { "Техника", "Животные" });
                comboBoxCategory.SelectedIndex = 0;

                Button confirmation = new Button()
                {
                    Text = "Добавить",
                    Left = 150,
                    Top = 170,
                    Width = 150,
                    Height = 40,
                    BackColor = Color.Goldenrod,
                    DialogResult = DialogResult.OK
                };

                // Элементы формы
                Label labelType = new Label()
                {
                    Text = "Тип:",
                    Left = 20,
                    Top = 20,
                    Width = 100,
                    Height = 30
                };

                TextBox textBoxType = new TextBox()
                {
                    Left = 130,
                    Top = 20,
                    Width = 240
                    
                };

                Label labelQuantity = new Label()
                {
                    Text = "Количество:",
                    Left = 20,
                    Top = 70,
                    Width = 100,
                    Height = 30
                };

                TextBox textBoxQuantity = new TextBox()
                {
                    Left = 130,
                    Top = 70,
                    Width = 240
                };


                confirmation.Click += (sender2, e2) => { addForm.Close(); };

                addForm.Controls.Add(labelCategory);
                addForm.Controls.Add(comboBoxCategory);
                addForm.Controls.Add(labelType);
                addForm.Controls.Add(textBoxType);
                addForm.Controls.Add(labelQuantity);
                addForm.Controls.Add(textBoxQuantity);
                addForm.Controls.Add(confirmation);
                addForm.AcceptButton = confirmation;

                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrEmpty(textBoxType.Text) ||
                        !int.TryParse(textBoxQuantity.Text, out int quantity) ||
                        quantity <= 0)
                    {
                        MessageBox.Show("Пожалуйста, заполните все поля корректно!", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int farmId = int.Parse(text_hoz.Text);
                    string type = textBoxType.Text;

                    try
                    {
                        using (var connection = new SQLiteConnection(Database_farm.ConnectionString))
                        {
                            connection.Open();
                            using (var transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    if (comboBoxCategory.SelectedItem.ToString() == "Техника")
                                    {
                                        connection.Execute(
                                            "INSERT INTO technic (farm_id, technic_type, quantity) VALUES (@farmId, @type, @quantity)",
                                            new { farmId, type, quantity },
                                            transaction: transaction);
                                    }
                                    else
                                    {
                                        connection.Execute(
                                            "INSERT INTO animals (farm_id, animal_type, quantity) VALUES (@farmId, @type, @quantity)",
                                            new { farmId, type, quantity },
                                            transaction: transaction);
                                    }

                                    transaction.Commit();
                                    MessageBox.Show("Данные успешно добавлены!", "Успех",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void bunifuButton24_Click(object sender, EventArgs e)
        {

        }

        private async void button_anim_Click(object sender, EventArgs e)
        {
            Animals mainForm = new Animals();
            await AnimateClose();

            await AnimateShow(mainForm);
        }

        private async void button_tech_Click(object sender, EventArgs e)
        {
            Technic mainForm = new Technic();
            await AnimateClose();

            await AnimateShow(mainForm);
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
                        (p.full_name != null && p.full_name.ToLower().Contains(searchText)) ||
                        (p.farm_id.ToString().Contains(searchText)) ||
                        (p.area != null && p.area.ToString().ToLower().Contains(searchText)) || // Дата
                        (p.person_id.ToString().Contains(searchText)) // Числовое поле (ID)
                                                                      // Добавьте другие поля по аналогии...
                    )
                    .ToList();

                bunifuDataGridView1.DataSource = filteredPeople;
            }
        }

        private async void button_rep_Click(object sender, EventArgs e)
        {
            Report mainForm = new Report();
            await AnimateClose();

            await AnimateShow(mainForm);
        }
    }
    
}
