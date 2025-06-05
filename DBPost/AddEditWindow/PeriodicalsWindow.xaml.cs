using DBPost.Views;
using DBPost.Windows;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;


namespace DBPost.AddEditWindow
{
    public partial class periodicals : UserControl
    {
        // Ссылка на родительское окно/представление с таблицей изданий
        private PeriodicalsView PeriodicalsView { get; set; }
        // Флаг, указывающий режим: добавление (true) или редактирование (false)
        bool isAdding = true;
        string? idPeriodical = string.Empty;
        public periodicals(PeriodicalsView periodicalsView)
        {
            InitializeComponent();
            PeriodicalsView = periodicalsView;
        }

        // Конструктор для редактирования существующего издания с заполнением полей
        public periodicals(PeriodicalsView periodicalsView, DataRowView dataRowView)
        {
            InitializeComponent();
            isAdding = false; // Режим редактирования
            PeriodicalsView = periodicalsView;
            idPeriodical = dataRowView.Row["IDPeriodical"].ToString();
            this.Title.Text = dataRowView.Row["Title"].ToString();
            this.PriceMonth.Text = dataRowView.Row["PriceMonth"].ToString();
            this.PriceThreeMonths.Text = dataRowView.Row["PriceThreeMonths"].ToString();
            this.PriceSixMonths.Text = dataRowView.Row["PriceSixMonths"].ToString();
            this.PriceTwelveMonths.Text = dataRowView.Row["PriceTwelveMonths"].ToString();
        }

        // Обработчик нажатия кнопки Добавить/Сохранить
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false; // Блокируем управление во время анимации закрытия
            (FindResource("CloseMenu") as Storyboard)!.Completed += (s, a) => {
                // После завершения анимации закрываем окно и удаляем контрол из родителя
                (this.Parent as Grid)!.Children.Remove(this);
                PeriodicalsView.EnableWindow();
                if ((sender as Button) == AddButton)
                {
                    string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        con.Open();
                        SqlCommand cmd;
                        if (isAdding)
                        {
                            // Вставляем новую запись
                            cmd = new SqlCommand($"INSERT INTO Periodicals (Title, PriceMonth, PriceThreeMonths, PriceSixMonths, PriceTwelveMonths) Values ('{Title.Text}',{PriceMonth.Text},{PriceThreeMonths.Text}, {PriceThreeMonths.Text}, {PriceTwelveMonths.Text})", con);
                        }
                        else
                        {
                            // Обновляем существующую запись по ID
                            cmd = new SqlCommand($"Update Periodicals Set Title='{Title.Text}', PriceMonth={PriceMonth.Text}, PriceThreeMonths={PriceThreeMonths.Text}, PriceSixMonths={PriceSixMonths.Text}, PriceTwelveMonths={PriceTwelveMonths.Text} where IDPeriodical={idPeriodical}", con);
                        }
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    PeriodicalsView.FillDataGrid();
                }
            };
        }

        // Обработчик события обновления разметки, запускающий анимацию открытия меню
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            this.IsEnabled = false;
            (FindResource("OpenMenu") as Storyboard)!.Completed += (s, ev) =>
            {
                this.LayoutUpdated -= UserControl_LayoutUpdated!;
                this.IsEnabled = true;
            };
            (FindResource("OpenMenu") as Storyboard)!.Begin();
        }

        // Ограничение ввода в поля с ценами — разрешаем только цифры и точку
        private void Digit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, "[^0-9.]+")) e.Handled = true;
        }

        // Валидация данных перед сохранением
        private bool ValidatePeriodical()
        {
            if (Title.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите название издания!", MessageBoxButton.OK);
                return false;
            }
            if (PriceMonth.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите цену за месяц!", MessageBoxButton.OK);
                return false;
            }
            if (PriceThreeMonths.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите цену за 3 месяца!", MessageBoxButton.OK);
                return false;
            }
            if (PriceSixMonths.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите цену за 6 месяцев!", MessageBoxButton.OK);
                return false;
            }
            if (PriceTwelveMonths.Text.Length < 1)
            {
                MessageWindow.Show("Ошибка ввода", "Введите цену за 12 месяцев!", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        // Обработчик нажатия мыши по кнопке добавления, запускающий проверку и событие Click
        private void AddButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ValidatePeriodical()) (sender as Button)!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
    }
}
