using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using DBPost.AddEditWindow;
using DBPost.Windows;

namespace DBPost.Views
{
    // Класс для отображения и управления представлением "Периодические издания"
    public partial class PeriodicalsView : UserControl
    {
        // Строка подключения к базе данных, полученная из конфигурационного файла
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private IEnumerable? periodicals;
        public PeriodicalsView()
        {
            InitializeComponent();
            FillDataGrid(); // Загрузка данных при инициализации компонента
        }

        // Метод для заполнения DataGrid данными из базы данных
        public void FillDataGrid()
        {
            string CmdString = string.Empty;
            using (SqlConnection con = new SqlConnection(ConString)) { 
                SqlCommand cmd = new SqlCommand("SELECT * From Periodicals", con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Periodicals");
                sda.Fill(dt);
                PeriodicalsDataGrid.ItemsSource = dt.DefaultView;
                periodicals = PeriodicalsDataGrid.ItemsSource;
            }
        }

        // Обработка поиска по тексту в поле SearchTextBox
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length > 0)
            {
                SearchTextBlock.Visibility = Visibility.Hidden;
                SearchIcon.Visibility = Visibility.Hidden;
                PeriodicalsDataGrid.ItemsSource = periodicals!.Cast<object>().ToList().Where<object>(o => (o as DataRowView)!.Row["Title"].ToString()!.ToLower().Contains(SearchTextBox.Text.ToLower()));
            }
            else
            {
                SearchTextBlock.Visibility = Visibility.Visible;
                SearchIcon.Visibility = Visibility.Visible;
                PeriodicalsDataGrid.ItemsSource = periodicals;
            }
        }

        // Удаление выбранного периодического издания
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                bool hasSubsc = false; // Флаг наличия подписок
                string id = ((DataRowView)((Button)sender).Tag).Row["IDPeriodical"].ToString()!;
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    SqlCommand cmd = new SqlCommand($"SELECT * From Subscriptions where FKPeriodical = {id}", con);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("Subscriptions");
                    sda.Fill(dt);
                    hasSubsc = dt.DefaultView.Count > 0;
                }
                

                if (hasSubsc)
                    MessageWindow.Show("Информационное окно", "Перед удалением периодического издания необходимо удалить все подписки на него.", MessageBoxButton.OK);
                else
                {
                    int rowsAffected;
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        SqlCommand cmd = new SqlCommand("Delete from Periodicals where IDPeriodical=" + id, con);
                        con.Open();
                        rowsAffected = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    if (rowsAffected > 0)
                    {
                        MessageWindow.Show("Информационное окно", "Успешно удалено", MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageWindow.Show("Информационное окно", "Произошла ошибка, попробуйте снова", MessageBoxButton.OK);
                    }
                    FillDataGrid();
                }
            }
        }

        // Открытие окна редактирования выбранного издания
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            periodicals periodicalsWindow = new periodicals(this, ((DataRowView)((Button)sender).Tag));
            periodicalsWindow.TitleTextBlock.Text = "Изменить";
            periodicalsWindow.AddButton.Content = "Сохранить";

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(periodicalsWindow);
        }

        // Обработка нажатия кнопки добавления нового издания
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            periodicals periodocalsC = new periodicals(this);

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(periodocalsC);
        }

        // Метод для повторного включения окна и меню после закрытия дочернего окна
        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }
    }
}
