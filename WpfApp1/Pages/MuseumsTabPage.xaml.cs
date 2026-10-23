using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Pages
{
    public partial class MuseumsTabPage : Page
    {
        private List<Museums> _all;

        public MuseumsTabPage()
        {
            InitializeComponent();
            CmbSort.ItemsSource = new string[] { "Название: от А до Я", "Название: от Я до А" };
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
            _all = Core.DB.Museums.ToList();

            List<string> cities = _all.Select(m => m.City).Distinct().OrderBy(c => c).ToList();
            cities.Insert(0, "Все города");
            CmbCity.ItemsSource = cities;
            CmbCity.SelectedIndex = 0;

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
            CmbCity.SelectedIndex = 0;
            CmbSort.SelectedIndex = 0;
        }

        private void UpdateList()
        {
            if (!IsInitialized || _all == null)
                return;

            IEnumerable<Museums> list = _all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                list = list.Where(m => m.Name.ToLower().Contains(search));

            if (CmbCity.SelectedIndex > 0)
            {
                string city = CmbCity.SelectedItem.ToString();
                list = list.Where(m => m.City == city);
            }

            if (CmbSort.SelectedIndex == 1)
                list = list.OrderByDescending(m => m.Name);
            else
                list = list.OrderBy(m => m.Name);

            DataGridMuseums.ItemsSource = list.ToList();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddMuseumPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Museums museum = DataGridMuseums.SelectedItem as Museums;
            if (museum == null)
            {
                Helper.ShowError("Выберите музей в таблице!");
                return;
            }
            NavigationService.Navigate(new AddMuseumPage(museum));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            Museums museum = DataGridMuseums.SelectedItem as Museums;
            if (museum == null)
            {
                Helper.ShowError("Выберите музей в таблице!");
                return;
            }
            if (!Helper.Confirm("Удалить музей " + museum.Name + "? Все бронирования в этот музей также будут удалены."))
                return;

            try
            {
                Core.DB.Museums.Remove(museum);
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
