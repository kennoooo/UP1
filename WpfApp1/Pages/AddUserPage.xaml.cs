using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Pages
{
    public partial class AddUserPage : Page
    {
        private Users _user;

        public AddUserPage(Users selectedUser)
        {
            InitializeComponent();
            _user = selectedUser;
            CmbRole.ItemsSource = new string[] { "User", "Admin" };
            CmbRole.SelectedIndex = 0;

            if (_user == null)
            {
                Title = "Добавление пользователя";
                TextHeader.Text = "Добавление пользователя";
            }
            else
            {
                Title = "Редактирование пользователя";
                TextHeader.Text = "Редактирование пользователя";
                LabelPass.Text = "Новый пароль";
                PassBox.ToolTip = "Оставьте пустым, чтобы не менять пароль";
                TBLogin.Text = _user.Login;
                CmbRole.SelectedIndex = _user.Role == "Admin" ? 1 : 0;
                TBFio.Text = _user.FIO;
                TBPhoto.Text = _user.Photo;
            }
        }

        private void TBLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsLoginText(e.Text);
        }

        private void PassBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsPasswordText(e.Text);
        }

        private void TBFio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsFioText(e.Text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text.Trim();
            string fio = TBFio.Text.Trim();
            string password = PassBox.Password;
            StringBuilder errors = new StringBuilder();

            if (!Helper.IsLoginValid(login))
                errors.AppendLine("Логин должен содержать от 3 до 20 символов: латинские буквы, цифры и знак подчеркивания!");
            if (_user == null || password != "")
            {
                string passError = Helper.CheckPassword(password);
                if (passError != "")
                    errors.AppendLine(passError);
            }
            if (!Helper.IsFioValid(fio))
                errors.AppendLine("ФИО должно состоять из 2-3 слов и содержать только буквы и дефис!");
            if (errors.Length > 0)
            {
                Helper.ShowError(errors.ToString());
                return;
            }

            try
            {
                int id = _user == null ? 0 : _user.ID;
                if (Core.DB.Users.Any(u => u.Login == login && u.ID != id))
                {
                    Helper.ShowError("Пользователь с таким логином уже существует!");
                    return;
                }

                Users user = _user ?? new Users();
                user.Login = login;
                user.Role = CmbRole.SelectedItem.ToString();
                user.FIO = fio;
                user.Photo = string.IsNullOrWhiteSpace(TBPhoto.Text) ? null : TBPhoto.Text.Trim();
                if (password != "")
                    user.Password = Helper.GetHash(password);

                if (_user == null)
                    Core.DB.Users.Add(user);
                Core.DB.SaveChanges();
                Helper.ShowInfo("Данные успешно сохранены!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Helper.Refresh();
                Helper.ShowError("Не удалось сохранить данные: " + ex.Message);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Clear();
            PassBox.Clear();
            TBFio.Clear();
            TBPhoto.Clear();
            CmbRole.SelectedIndex = 0;
        }
    }
}
