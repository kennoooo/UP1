using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp1.Pages
{
    public partial class AddMuseumPage : Page
    {
        private Museums _museum;

        public AddMuseumPage(Museums selectedMuseum)
        {
            InitializeComponent();
            _museum = selectedMuseum;

            if (_museum == null)
            {
                Title = "Добавление музея";
                TextHeader.Text = "Добавление музея";
            }
            else
            {
                Title = "Редактирование музея";
                TextHeader.Text = "Редактирование музея";
                TBName.Text = _museum.Name;
                TBCity.Text = _museum.City;
            }
        }

        private void TBCity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helper.IsFioText(e.Text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string name = TBName.Text.Trim();
            string city = TBCity.Text.Trim();
            StringBuilder errors = new StringBuilder();

            if (name.Length < 2)
                errors.AppendLine("Название музея должно содержать минимум 2 символа!");
            if (!Helper.IsCityValid(city))
                errors.AppendLine("Город: от 2 до 50 символов, только буквы, пробел и дефис!");
            if (errors.Length > 0)
            {
                Helper.ShowError(errors.ToString());
                return;
            }

            try
            {
                Museums museum = _museum ?? new Museums();
                museum.Name = name;
                museum.City = city;

                if (_museum == null)
                    Core.DB.Museums.Add(museum);
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
            TBName.Clear();
            TBCity.Clear();
        }
    }
}
