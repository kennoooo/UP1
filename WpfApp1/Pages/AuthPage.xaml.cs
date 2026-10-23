using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        private string captcha = "";

        public AuthPage()
        {
            InitializeComponent();
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = TextBoxLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPass.Visibility = PassBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void CaptchaInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintCaptcha.Visibility = CaptchaInput.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TextBoxLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsLoginText(e.Text);
        }

        private void PassBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsPasswordText(e.Text);
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxLogin.Text) || string.IsNullOrEmpty(PassBox.Password))
            {
                Helper.ShowError("Введите логин и пароль!");
                return;
            }

            Users user;
            try
            {
                user = Helper.Authenticate(TextBoxLogin.Text.Trim(), PassBox.Password);
            }
            catch (Exception ex)
            {
                Helper.ShowError("Ошибка подключения к базе данных: " + ex.Message);
                return;
            }

            if (user == null)
            {
                failedAttempts++;
                Helper.ShowError("Неверный логин или пароль!");
                if (failedAttempts >= 3)
                    CaptchaSwitch(true);
                return;
            }

            failedAttempts = 0;
            if (user.Role == "Admin")
                NavigationService.Navigate(new AdminPage());
            else
                NavigationService.Navigate(new UserPage());
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChangePassPage());
        }

        private void NewCaptcha()
        {
            captcha = Helper.GenerateCaptcha();
            CaptchaText.Text = captcha;
            CaptchaInput.Clear();
        }

        private void CaptchaSwitch(bool show)
        {
            if (show)
            {
                NewCaptcha();
            }
            else
            {
                TextBoxLogin.Clear();
                PassBox.Clear();
            }
            LoginPanel.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            CaptchaPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ButtonCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (!Helper.CheckCaptcha(CaptchaInput.Text, captcha))
            {
                Helper.ShowError("Неверно введена капча!");
                NewCaptcha();
                return;
            }
            Helper.ShowInfo("Капча введена верно, можете продолжить авторизацию.");
            failedAttempts = 0;
            CaptchaSwitch(false);
        }

        private void CaptchaText_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
                e.Handled = true;
        }
    }
}
