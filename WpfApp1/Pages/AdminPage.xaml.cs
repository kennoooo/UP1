using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            TextWelcome.Text = "Администратор: " + Core.CurrentUser.FIO;
        }

        private void BtnTab1_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UsersTabPage());
        }

        private void BtnTab2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MuseumsTabPage());
        }

        private void BtnTab3_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BookingsTabPage());
        }

        private void BtnTab4_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DiagrammPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!Helper.Confirm("Вы действительно хотите выйти?"))
                return;
            Core.CurrentUser = null;
            NavigationService.Navigate(new AuthPage());
        }
    }
}
