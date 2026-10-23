using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Pages
{
    public partial class AddBookingPage : Page
    {
        private Bookings _booking;

        public AddBookingPage(Bookings selectedBooking)
        {
            InitializeComponent();
            _booking = selectedBooking;

            CmbUser.ItemsSource = Core.DB.Users.OrderBy(u => u.FIO).ToList();
            CmbMuseum.ItemsSource = Core.DB.Museums.OrderBy(m => m.Name).ToList();
            DPDate.DisplayDateStart = new DateTime(2020, 1, 1);
            DPDate.DisplayDateEnd = new DateTime(2035, 12, 31);

            if (_booking == null)
            {
                Title = "Добавление бронирования";
                TextHeader.Text = "Добавление бронирования";
            }
            else
            {
                Title = "Редактирование бронирования";
                TextHeader.Text = "Редактирование бронирования";
                CmbUser.SelectedValue = _booking.UserID;
                CmbMuseum.SelectedValue = _booking.MuseumID;
                TBName.Text = _booking.Name;
                DPDate.SelectedDate = _booking.Date;
                TBNum.Text = _booking.Num.ToString();
                TBPrice.Text = ((int)_booking.Price).ToString();
            }
        }

        private void Digits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsDigitsText(e.Text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            string name = TBName.Text.Trim();
            int num;
            int price;

            if (CmbUser.SelectedValue == null)
                errors.AppendLine("Выберите пользователя!");
            if (CmbMuseum.SelectedValue == null)
                errors.AppendLine("Выберите музей!");
            if (name.Length < 2)
                errors.AppendLine("Название выставки должно содержать минимум 2 символа!");
            if (DPDate.SelectedDate == null)
                errors.AppendLine("Выберите дату посещения!");
            else if (_booking == null && DPDate.SelectedDate.Value < DateTime.Today)
                errors.AppendLine("Дата посещения не может быть в прошлом!");
            if (!int.TryParse(TBNum.Text, out num) || num < 1 || num > 10)
                errors.AppendLine("Количество билетов должно быть числом от 1 до 10!");
            if (!int.TryParse(TBPrice.Text, out price) || price < 1 || price > 100000)
                errors.AppendLine("Цена билета должна быть числом от 1 до 100000!");
            if (errors.Length > 0)
            {
                Helper.ShowError(errors.ToString());
                return;
            }

            try
            {
                Bookings booking = _booking ?? new Bookings();
                booking.UserID = (int)CmbUser.SelectedValue;
                booking.MuseumID = (int)CmbMuseum.SelectedValue;
                booking.Name = name;
                booking.Date = DPDate.SelectedDate.Value;
                booking.Num = num;
                booking.Price = price;

                if (_booking == null)
                    Core.DB.Bookings.Add(booking);
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
            CmbUser.SelectedIndex = -1;
            CmbMuseum.SelectedIndex = -1;
            TBName.Clear();
            DPDate.SelectedDate = null;
            TBNum.Clear();
            TBPrice.Clear();
        }
    }
}
