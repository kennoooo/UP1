using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace WpfApp1
{
    internal static class Helper
    {
        public static bool IsDark;

        private static string ThemeFile
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MuseumTicketsTheme.txt");
            }
        }

        public static string GetHash(string password)
        {
            using (var sha = SHA1.Create())
            {
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        public static Users Authenticate(string login, string password)
        {
            string hash = GetHash(password);
            Users user = Core.DB.Users.FirstOrDefault(u => u.Login == login && u.Password == hash);
            Core.CurrentUser = user;
            return user;
        }

        public static string GenerateCaptcha()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            Random random = new Random();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 6; i++)
                result.Append(chars[random.Next(chars.Length)]);
            return result.ToString();
        }

        public static bool CheckCaptcha(string entered, string captcha)
        {
            return entered == captcha;
        }

        public static string CheckPassword(string password)
        {
            if (password.Length < 6)
                return "Пароль должен содержать минимум 6 символов!";
            bool hasDigit = false;
            foreach (char c in password)
            {
                if (c >= '0' && c <= '9')
                    hasDigit = true;
                else if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                    return "Используйте только английскую раскладку (буквы и цифры)!";
            }
            if (!hasDigit)
                return "Добавьте хотя бы одну цифру!";
            return "";
        }

        public static bool IsLoginValid(string login)
        {
            return Regex.IsMatch(login, "^[A-Za-z0-9_]{3,20}$");
        }

        public static bool IsFioValid(string fio)
        {
            return Regex.IsMatch(fio, @"^[A-Za-zА-Яа-яЁё\-]+( [A-Za-zА-Яа-яЁё\-]+){1,2}$");
        }

        public static bool IsCityValid(string city)
        {
            return Regex.IsMatch(city, @"^[A-Za-zА-Яа-яЁё\- ]{2,50}$");
        }

        public static bool IsLoginText(string text)
        {
            return text.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_');
        }

        public static bool IsPasswordText(string text)
        {
            return text.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
        }

        public static bool IsFioText(string text)
        {
            return text.All(c => char.IsLetter(c) || c == ' ' || c == '-');
        }

        public static bool IsDigitsText(string text)
        {
            return text.All(c => c >= '0' && c <= '9');
        }

        public static void ShowInfo(string text)
        {
            MessageBox.Show(text, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool Confirm(string text)
        {
            return MessageBox.Show(text, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static void Refresh()
        {
            foreach (var entry in Core.DB.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
        }

        public static void LoadTheme()
        {
            IsDark = File.Exists(ThemeFile) && File.ReadAllText(ThemeFile) == "dark";
            ApplyTheme();
        }

        public static void SwitchTheme()
        {
            IsDark = !IsDark;
            ApplyTheme();
            File.WriteAllText(ThemeFile, IsDark ? "dark" : "light");
        }

        private static void ApplyTheme()
        {
            string file = IsDark ? "DarkDictionary.xaml" : "Dictionary.xaml";
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            dictionaries.Clear();
            dictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/" + file, UriKind.Absolute) });
        }
    }
}
