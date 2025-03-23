using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Zyuz_Student.Models;

namespace Zyuz_Student.Services
{
    public class StudentService
    {
        private List<Student> _students;
        private readonly string _jsonFilePath = "students.json";
        private bool _hasChanges;

        public StudentService()
        {
            _students = new List<Student>();
            _hasChanges = false;
            LoadFromJson();
        }

        public List<Student> GetAllStudents()
        {
            return _students;
        }

        public void AddStudent(Student student)
        {
            student.Id = _students.Count > 0 ? _students.Max(s => s.Id) + 1 : 1;
            _students.Add(student);
            _hasChanges = true;
        }

        public void UpdateStudent(Student student)
        {
            var index = _students.FindIndex(s => s.Id == student.Id);
            if (index != -1)
            {
                _students[index] = student;
                _hasChanges = true;
            }
        }

        public void DeleteStudent(int id)
        {
            var student = _students.FirstOrDefault(s => s.Id == id);
            if (student != null)
            {
                _students.Remove(student);
                _hasChanges = true;
            }
        }

        public List<Student> FilterStudents(int? course = null, string group = null, string lastName = null)
        {
            var query = _students.AsEnumerable();

            if (course.HasValue)
                query = query.Where(s => s.Course == course);

            if (!string.IsNullOrEmpty(group))
                query = query.Where(s => s.Group.IndexOf(group, StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(lastName))
                query = query.Where(s => s.LastName.IndexOf(lastName, StringComparison.OrdinalIgnoreCase) >= 0);

            return query.ToList();
        }

        public void SaveToJson()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<Student>));
                using (var fs = new FileStream(_jsonFilePath, FileMode.Create))
                {
                    serializer.WriteObject(fs, _students);
                }
                _hasChanges = false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        public void LoadFromJson()
        {
            if (!File.Exists(_jsonFilePath))
            {
                _students = new List<Student>();
                return;
            }

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<Student>));
                using (var fs = new FileStream(_jsonFilePath, FileMode.Open))
                {
                    _students = (List<Student>)serializer.ReadObject(fs) ?? new List<Student>();
                }
            }
            catch (Exception ex)
            {
                _students = new List<Student>();
                throw new Exception($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        public void ImportFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines.Skip(1)) // Пропускаем заголовок
            {
                var values = line.Split(',');
                if (values.Length >= 7)
                {
                    var student = new Student
                    {
                        LastName = values[0],
                        FirstName = values[1],
                        MiddleName = values[2],
                        Course = int.Parse(values[3]),
                        Group = values[4],
                        BirthDate = DateTime.Parse(values[5]),
                        Email = values[6]
                    };
                    AddStudent(student);
                }
            }
        }

        public void ExportToCsv(string filePath)
        {
            var lines = new List<string>
            {
                "Фамилия,Имя,Отчество,Курс,Группа,Дата рождения,Email"
            };

            foreach (var student in _students)
            {
                lines.Add($"{student.LastName},{student.FirstName},{student.MiddleName}," +
                         $"{student.Course},{student.Group},{student.BirthDate:dd.MM.yyyy},{student.Email}");
            }

            File.WriteAllLines(filePath, lines);
        }

        public bool HasUnsavedChanges()
        {
            return _hasChanges;
        }
    }
} 