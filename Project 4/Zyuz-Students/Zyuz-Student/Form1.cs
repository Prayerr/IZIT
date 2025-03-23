using System;
using System.Windows.Forms;
using System.Linq;
using Zyuz_Student.Models;
using Zyuz_Student.Services;
using System.ComponentModel;

namespace Zyuz_Student
{
    public partial class Form1 : Form
    {
        private readonly StudentService _studentService;
        private Student _currentStudent;

        public Form1()
        {
            InitializeComponent();
            _studentService = new StudentService();
            InitializeControls();
            LoadStudents();
        }

        private void InitializeControls()
        {
            // Настройка DataGridView
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.RowHeadersVisible = false;
            
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LastName",
                Name = "LastName",
                HeaderText = "Фамилия",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FirstName",
                Name = "FirstName",
                HeaderText = "Имя",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "MiddleName",
                Name = "MiddleName",
                HeaderText = "Отчество",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Course",
                Name = "Course",
                HeaderText = "Курс",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Group",
                Name = "Group",
                HeaderText = "Группа",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "BirthDate",
                Name = "BirthDate",
                HeaderText = "Дата рождения",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Email",
                Name = "Email",
                HeaderText = "Email",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Настройка DateTimePicker
            dateTimePicker1.MinDate = new DateTime(1992, 1, 1);
            dateTimePicker1.MaxDate = DateTime.Today;

            // Настройка ComboBox для курса
            for (int i = 1; i <= 6; i++)
            {
                comboBoxCourse.Items.Add(i);
            }
            comboBoxCourse.SelectedIndex = 0;

            // Настройка ComboBox для фильтрации по курсу
            comboBoxFilterCourse.Items.Add("Все");
            for (int i = 1; i <= 6; i++)
            {
                comboBoxFilterCourse.Items.Add(i);
            }
            comboBoxFilterCourse.SelectedIndex = 0;
        }

        private void LoadStudents()
        {
            try
            {
                dataGridView1.SelectionChanged -= dataGridView1_SelectionChanged;
                var students = _studentService.GetAllStudents();
                if (students == null || students.Count == 0)
                {
                    MessageBox.Show("Список студентов пуст", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = new BindingList<Student>(students);
            }
            finally
            {
                dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var student = new Student
                {
                    LastName = txtLastName.Text,
                    FirstName = txtFirstName.Text,
                    MiddleName = txtMiddleName.Text,
                    Course = (int)comboBoxCourse.SelectedItem,
                    Group = txtGroup.Text,
                    BirthDate = dateTimePicker1.Value,
                    Email = txtEmail.Text
                };

                if (!student.ValidateEmail())
                {
                    MessageBox.Show("Email должен быть зарегистрирован на одном из доменов: yandex.ru, gmail.com, icloud.com",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _studentService.AddStudent(student);
                LoadStudents();
                ClearInputs();
                
                // Автоматически сохраняем после добавления
                _studentService.SaveToJson();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении студента: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_currentStudent == null || !ValidateInput())
                return;

            try
            {
                _currentStudent.LastName = txtLastName.Text;
                _currentStudent.FirstName = txtFirstName.Text;
                _currentStudent.MiddleName = txtMiddleName.Text;
                _currentStudent.Course = (int)comboBoxCourse.SelectedItem;
                _currentStudent.Group = txtGroup.Text;
                _currentStudent.BirthDate = dateTimePicker1.Value;
                _currentStudent.Email = txtEmail.Text;

                if (!_currentStudent.ValidateEmail())
                {
                    MessageBox.Show("Email должен быть зарегистрирован на одном из доменов: yandex.ru, gmail.com, icloud.com",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _studentService.UpdateStudent(_currentStudent);
                _studentService.SaveToJson(); // Автоматически сохраняем после обновления
                LoadStudents();
                ClearInputs();
                
                MessageBox.Show("Студент успешно обновлен", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении студента: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_currentStudent == null)
                return;

            try
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этого студента?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Сохраняем ID студента для удаления
                    int studentId = _currentStudent.Id;
                    
                    // Очищаем текущего студента и поля ввода перед удалением
                    _currentStudent = null;
                    ClearInputs();
                    
                    // Удаляем студента и обновляем данные
                    _studentService.DeleteStudent(studentId);
                    _studentService.SaveToJson();
                    LoadStudents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении студента: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null || 
                    dataGridView1.CurrentRow.Index < 0 || 
                    dataGridView1.CurrentRow.DataBoundItem == null)
                {
                    _currentStudent = null;
                    ClearInputs();
                    return;
                }

                _currentStudent = (Student)dataGridView1.CurrentRow.DataBoundItem;
                DisplayStudentDetails();
            }
            catch
            {
                _currentStudent = null;
                ClearInputs();
            }
        }

        private void DisplayStudentDetails()
        {
            if (_currentStudent == null)
            {
                ClearInputs();
                return;
            }

            try
            {
                txtLastName.Text = _currentStudent.LastName;
                txtFirstName.Text = _currentStudent.FirstName;
                txtMiddleName.Text = _currentStudent.MiddleName;
                comboBoxCourse.SelectedItem = _currentStudent.Course;
                txtGroup.Text = _currentStudent.Group;
                dateTimePicker1.Value = _currentStudent.BirthDate;
                txtEmail.Text = _currentStudent.Email;
            }
            catch
            {
                ClearInputs();
            }
        }

        private void ClearInputs()
        {
            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            comboBoxCourse.SelectedIndex = 0;
            txtGroup.Clear();
            dateTimePicker1.Value = DateTime.Today;
            txtEmail.Clear();
            _currentStudent = null;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMiddleName.Text))
            {
                MessageBox.Show("Введите отчество", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtMiddleName.Focus();
                return false;
            }

            if (comboBoxCourse.SelectedItem == null)
            {
                MessageBox.Show("Выберите курс", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBoxCourse.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtGroup.Text))
            {
                MessageBox.Show("Введите группу", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtGroup.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Введите email", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmail.Focus();
                return false;
            }

            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _studentService.SaveToJson();
            MessageBox.Show("Данные успешно сохранены", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _studentService.ImportFromCsv(openFileDialog.FileName);
                        LoadStudents();
                        MessageBox.Show("Данные успешно импортированы", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _studentService.ExportToCsv(saveFileDialog.FileName);
                        MessageBox.Show("Данные успешно экспортированы", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_studentService.HasUnsavedChanges())
            {
                var result = MessageBox.Show("У вас есть несохраненные изменения. Хотите сохранить их перед выходом?",
                    "Предупреждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                switch (result)
                {
                    case DialogResult.Yes:
                        _studentService.SaveToJson();
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            int? selectedCourse = null;
            if (comboBoxFilterCourse.SelectedIndex > 0) // Пропускаем "Все"
            {
                selectedCourse = (int)comboBoxFilterCourse.SelectedItem;
            }

            var filteredStudents = _studentService.FilterStudents(
                course: selectedCourse,
                group: txtFilterGroup.Text,
                lastName: txtSearch.Text
            );
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = filteredStudents;
        }
    }
}
