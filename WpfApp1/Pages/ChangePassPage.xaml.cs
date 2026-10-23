using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void TBLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = TBLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassOld_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPassOld.Visibility = PassOld.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPassNew.Visibility = PassNew.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPassConfirm.Visibility = PassConfirm.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TBLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsLoginText(e.Text);
        }

        private void Pass_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsPasswordText(e.Text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (TBLogin.Text.Trim() == "" || PassOld.Password == "" || PassNew.Password == "" || PassConfirm.Password == "")
            {
                Helper.ShowError("Все поля обязательны к заполнению!");
                return;
            }

            try
            {
                string login = TBLogin.Text.Trim();
                string oldHash = Helper.GetHash(PassOld.Password);
                Users user = Core.DB.Users.FirstOrDefault(u => u.Login == login && u.Password == oldHash);
                if (user == null)
                {
                    Helper.ShowError("Логин или текущий пароль неверный!");
                    return;
                }

                string passError = Helper.CheckPassword(PassNew.Password);
                if (passError != "")
                {
                    Helper.ShowError(passError);
                    return;
                }
                if (PassNew.Password != PassConfirm.Password)
                {
                    Helper.ShowError("Пароли не совпадают!");
                    return;
                }
                if (PassNew.Password == PassOld.Password)
                {
                    Helper.ShowError("Новый пароль должен отличаться от текущего!");
                    return;
                }

                user.Password = Helper.GetHash(PassNew.Password);
                Core.DB.SaveChanges();
                Helper.ShowInfo("Пароль успешно изменен!");
                NavigationService.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                Helper.Refresh();
                Helper.ShowError("Не удалось сменить пароль: " + ex.Message);
            }
        }
    }
}
