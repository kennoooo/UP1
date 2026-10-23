using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Pages
{
    public partial class UserPage : Page
    {
        private List<Bookings> _all;

        public UserPage()
        {
            InitializeComponent();
            Title = "Мои бронирования — " + Core.CurrentUser.FIO;
            TextWelcome.Text = "Добро пожаловать, " + Core.CurrentUser.FIO + "!";

            Helper.Refresh();
            int userId = Core.CurrentUser.ID;
            _all = Core.DB.Bookings.Include(b => b.Museums).Where(b => b.UserID == userId).ToList();

            CmbSort.ItemsSource = new string[]
            {
                "Дата: сначала ранние",
                "Дата: сначала поздние",
                "Сумма: по возрастанию",
                "Сумма: по убыванию"
            };

            List<Museums> museums = _all.Select(b => b.Museums).Distinct().OrderBy(m => m.Name).ToList();
            museums.Insert(0, new Museums { ID = 0, Name = "Все музеи" });
            CmbMuseum.ItemsSource = museums;

            CmbSort.SelectedIndex = 0;
            CmbMuseum.SelectedIndex = 0;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintSearch.Visibility = SearchBox.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
            UpdateList();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            CmbMuseum.SelectedIndex = 0;
            CmbSort.SelectedIndex = 0;
        }

        private void UpdateList()
        {
            if (!IsInitialized || _all == null)
                return;

            IEnumerable<Bookings> list = _all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                list = list.Where(b => b.Name.ToLower().Contains(search));

            Museums museum = CmbMuseum.SelectedItem as Museums;
            if (museum != null && museum.ID != 0)
                list = list.Where(b => b.MuseumID == museum.ID);

            switch (CmbSort.SelectedIndex)
            {
                case 1:
                    list = list.OrderByDescending(b => b.Date);
                    break;
                case 2:
                    list = list.OrderBy(b => b.Total);
                    break;
                case 3:
                    list = list.OrderByDescending(b => b.Total);
                    break;
                default:
                    list = list.OrderBy(b => b.Date);
                    break;
            }

            DataGridBookings.ItemsSource = list.ToList();
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!Helper.Confirm("Вы действительно хотите выйти?"))
                return;
            Core.CurrentUser = null;
            NavigationService.Navigate(new AuthPage());
        }
    }
}
