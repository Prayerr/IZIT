using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Zyuz_Student.Models
{
    [DataContract]
    public class Student
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        public string LastName { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        public string FirstName { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Отчество обязательно для заполнения")]
        public string MiddleName { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Курс обязателен для заполнения")]
        [Range(1, 6, ErrorMessage = "Курс должен быть от 1 до 6")]
        public int Course { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Группа обязательна для заполнения")]
        public string Group { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Дата рождения обязательна для заполнения")]
        public DateTime BirthDate { get; set; }

        [DataMember]
        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; }

        public bool ValidateEmail()
        {
            if (string.IsNullOrEmpty(Email))
                return false;

            var parts = Email.Split('@');
            if (parts.Length != 2 || parts[0].Length < 3)
                return false;

            string[] allowedDomains = { "yandex.ru", "gmail.com", "icloud.com" };
            return Array.Exists(allowedDomains, domain => domain.Equals(parts[1], StringComparison.OrdinalIgnoreCase));
        }

        public bool ValidateBirthDate()
        {
            var minDate = new DateTime(1992, 1, 1);
            return BirthDate >= minDate && BirthDate <= DateTime.Today;
        }
    }
} 