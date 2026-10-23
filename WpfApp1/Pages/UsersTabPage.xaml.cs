using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1.Pages
{
    public partial class UsersTabPage : Page
    {
        private List<Users> _all;

        public UsersTabPage()
        {
            InitializeComponent();
            CmbRole.ItemsSource = new string[] { "Все роли", "Admin", "User" };
            CmbSort.ItemsSource = new string[] { "ФИО: от А до Я", "ФИО: от Я до А" };
            CmbRole.SelectedIndex = 0;
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
            _all = Core.DB.Users.ToList();
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
            CmbRole.SelectedIndex = 0;
            CmbSort.SelectedIndex = 0;
        }

        private void UpdateList()
        {
            if (!IsInitialized || _all == null)
                return;

            IEnumerable<Users> list = _all;

            string search = SearchBox.Text.Trim().ToLower();
            if (search != "")
                list = list.Where(u => u.FIO.ToLower().Contains(search) || u.Login.ToLower().Contains(search));

            if (CmbRole.SelectedIndex > 0)
            {
                string role = CmbRole.SelectedItem.ToString();
                list = list.Where(u => u.Role == role);
            }

            if (CmbSort.SelectedIndex == 1)
                list = list.OrderByDescending(u => u.FIO);
            else
                list = list.OrderBy(u => u.FIO);

            DataGridUsers.ItemsSource = list.ToList();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddUserPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Users user = DataGridUsers.SelectedItem as Users;
            if (user == null)
            {
                Helper.ShowError("Выберите пользователя в таблице!");
                return;
            }
            NavigationService.Navigate(new AddUserPage(user));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            Users user = DataGridUsers.SelectedItem as Users;
            if (user == null)
            {
                Helper.ShowError("Выберите пользователя в таблице!");
                return;
            }
            if (user.ID == Core.CurrentUser.ID)
            {
                Helper.ShowError("Нельзя удалить свою учетную запись!");
                return;
            }
            if (!Helper.Confirm("Удалить пользователя " + user.FIO + "? Все его бронирования также будут удалены."))
                return;

            try
            {
                Core.DB.Users.Remove(user);
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
