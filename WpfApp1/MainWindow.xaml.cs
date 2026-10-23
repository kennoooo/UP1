using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using WpfApp1.Pages;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString(); };
            timer.Start();
            DateTimeNow.Text = DateTime.Now.ToString();
            UpdateThemeButton();
            MainFrame.Navigate(new AuthPage());
        }

        private void MainFrame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Page page = e.Content as Page;
            if (page != null)
            {
                PageTitle.Text = page.Title;
                Title = "Музеи и выставки — " + page.Title;
            }
            if (e.Content is AuthPage || e.Content is UserPage || e.Content is AdminPage)
            {
                while (MainFrame.CanGoBack)
                    MainFrame.RemoveBackEntry();
            }
            ButtonBack.IsEnabled = MainFrame.CanGoBack;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            Helper.SwitchTheme();
            UpdateThemeButton();
        }

        private void UpdateThemeButton()
        {
            ButtonTheme.Content = Helper.IsDark ? "Светлая тема" : "Тёмная тема";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!Helper.Confirm("Вы действительно хотите выйти?"))
                e.Cancel = true;
        }
    }
}
