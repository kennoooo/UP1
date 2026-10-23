using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            CmbRole.ItemsSource = new string[] { "User", "Admin" };
            CmbRole.SelectedIndex = 0;
        }

        private void TBLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = TBLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassFirst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPassFirst.Visibility = PassFirst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassSecond_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPassSecond.Visibility = PassSecond.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TBFio_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintFio.Visibility = TBFio.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TBLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsLoginText(e.Text);
        }

        private void Pass_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsPasswordText(e.Text);
        }

        private void TBFio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsFioText(e.Text);
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = TBLogin.Text.Trim();
            string fio = TBFio.Text.Trim();

            if (login == "" || fio == "" || PassFirst.Password == "" || PassSecond.Password == "")
            {
                Helper.ShowError("Заполните все поля!");
                return;
            }
            if (!Helper.IsLoginValid(login))
            {
                Helper.ShowError("Логин должен содержать от 3 до 20 символов: латинские буквы, цифры и знак подчеркивания!");
                return;
            }
            string passError = Helper.CheckPassword(PassFirst.Password);
            if (passError != "")
            {
                Helper.ShowError(passError);
                return;
            }
            if (PassFirst.Password != PassSecond.Password)
            {
                Helper.ShowError("Пароли не совпадают!");
                return;
            }
            if (!Helper.IsFioValid(fio))
            {
                Helper.ShowError("ФИО должно состоять из 2-3 слов и содержать только буквы и дефис!");
                return;
            }

            try
            {
                if (Core.DB.Users.Any(u => u.Login == login))
                {
                    Helper.ShowError("Пользователь с таким логином уже существует!");
                    return;
                }

                Users user = new Users
                {
                    Login = login,
                    Password = Helper.GetHash(PassFirst.Password),
                    Role = CmbRole.SelectedItem.ToString(),
                    FIO = fio
                };
                Core.DB.Users.Add(user);
                Core.DB.SaveChanges();
                Helper.ShowInfo("Пользователь успешно зарегистрирован!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Helper.Refresh();
                Helper.ShowError("Не удалось зарегистрировать пользователя: " + ex.Message);
            }
        }
    }
}
