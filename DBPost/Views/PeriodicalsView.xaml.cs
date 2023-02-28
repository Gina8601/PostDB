using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Windows.Controls.Primitives;
using DBPost.AddEditWindow;
using DBPost.Windows;

namespace DBPost.Views
{
    /// <summary>
    /// Логика взаимодействия для PeriodicalsView.xaml
    /// </summary>
    public partial class PeriodicalsView : UserControl
    {
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private IEnumerable? periodicals;
        public PeriodicalsView()
        {
            InitializeComponent();
            FillDataGrid();
        }

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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDPeriodical"].ToString()!;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            periodicals periodocalsC = new periodicals(this);

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(periodocalsC);
        }

        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }
    }
}
