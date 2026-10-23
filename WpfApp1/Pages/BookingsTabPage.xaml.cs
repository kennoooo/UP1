using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Pages
{
    public partial class BookingsTabPage : Page
    {
        private List<Bookings> _all;

        public BookingsTabPage()
        {
            InitializeComponent();
            CmbSort.ItemsSource = new string[]
            {
                "Дата: сначала ранние",
                "Дата: сначала поздние",
                "Сумма: по возрастанию",
                "Сумма: по убыванию"
            };
            CmbSort.SelectedIndex = 0;
            LoadData();
            IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                LoadData();
        }

        private void LoadData()
        {
            Helper.Refresh();
            _all = Core.DB.Bookings.Include(b => b.Users).Include(b => b.Museums).ToList();

            List<Museums> museums = Core.DB.Museums.OrderBy(m => m.Name).ToList();
            museums.Insert(0, new Museums { ID = 0, Name = "Все музеи" });
            CmbMuseum.ItemsSource = museums;
            CmbMuseum.SelectedIndex = 0;

            UpdateList();
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
                list = list.Where(b => b.Name.ToLower().Contains(search) || b.Users.FIO.ToLower().Contains(search));

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

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddBookingPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Bookings booking = DataGridBookings.SelectedItem as Bookings;
            if (booking == null)
            {
                Helper.ShowError("Выберите бронирование в таблице!");
                return;
            }
            NavigationService.Navigate(new AddBookingPage(booking));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            Bookings booking = DataGridBookings.SelectedItem as Bookings;
            if (booking == null)
            {
                Helper.ShowError("Выберите бронирование в таблице!");
                return;
            }
            if (!Helper.Confirm("Удалить бронирование «" + booking.Name + "» на " + booking.Date.ToString("dd.MM.yyyy") + "?"))
                return;

            try
            {
                Core.DB.Bookings.Remove(booking);
                Core.DB.SaveChanges();
                Helper.ShowInfo("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                Helper.ShowError("Не удалось удалить данные: " + ex.Message);
            }
            LoadData();
        }
    }
}
