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
using DBPost.AddEditWindow;
using DBPost.Windows;
using System.Text.RegularExpressions;

namespace DBPost.Views
{
    /// <summary>
    /// Логика взаимодействия для SubscriptionsView.xaml
    /// </summary> наслушался чего тут музыка не играет
    public partial class SubscriptionsView : UserControl
    {
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private IEnumerable? actSubscriptions;
        private IEnumerable? archiveSubscriptions;
        public SubscriptionsView()
        {
            InitializeComponent();

            CheckSubscriptions();

            FillDataGrid();

            FillDataGridArchive();
        }
        private void CheckSubscriptions()
        {
            bool isChanged = false;
            using (SqlConnection con = new SqlConnection(ConString))
            {

                SqlCommand cmd = new SqlCommand("SELECT * From Subscriptions", con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable subscriptionDataTable = new DataTable("Subscriptions");
                sda.Fill(subscriptionDataTable);
                foreach (DataRow row in subscriptionDataTable.Rows)
                {
                    if (DateTime.Parse(row["SubscriptionEnd"].ToString()!) < DateTime.Now)
                    {
                        string id = row["IDSubscription"].ToString()!;
                        string idSub = row["FKSubscriber"].ToString()!;
                        string idPer = row["FKPeriodical"].ToString()!;
                        string subStart = row["SubscriptionStart"].ToString()!;
                        string subEnd = row["SubscriptionEnd"].ToString()!;
                        string issueDate = row["IssueDate"].ToString()!;
                        string subTerm = row["SubscriptionTerm"].ToString()!;
                        string price = row["Price"].ToString()!;

                        con.Open();

                        cmd = new($"INSERT INTO Archive(FKSubscriber,FKPeriodical,SubscriptionStart ,SubscriptionEnd,IssueDate,SubscriptionTerm, Price)VALUES({idSub},{idPer},'{subStart}', '{subEnd}','{issueDate}',{subTerm}, {price.Replace(',','.')});", con);
                        cmd.ExecuteNonQuery();

                        cmd = new("Delete from Subscriptions where IDSubscription="+id, con);
                        cmd.ExecuteNonQuery();

                        isChanged = true;

                        con.Close();
                    }
                }
                if(isChanged)MessageWindow.Show("Перемещение", "Старые записи помещены в архив.", MessageBoxButton.OK);
            }
        }

        public void FillDataGrid()
        {
            using (SqlConnection con = new SqlConnection(ConString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * From Subscriptions", con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Subscriptions");
                sda.Fill(dt);
                SubscriptionsDataGrid.ItemsSource = dt.DefaultView;
                actSubscriptions = SubscriptionsDataGrid.ItemsSource;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text.Length > 0)
            {
                SearchTextBlock.Visibility = Visibility.Hidden;
                SearchIcon.Visibility = Visibility.Hidden;
                SubscriptionsDataGrid.ItemsSource = actSubscriptions!.Cast<object>().ToList().Where<object>(o => (o as DataRowView)!.Row["FKSubscriber"].ToString()!.ToLower().Contains(SearchTextBox.Text.ToLower()));
            }
            else
            {
                SearchTextBlock.Visibility = Visibility.Visible;
                SearchIcon.Visibility = Visibility.Visible;
                SubscriptionsDataGrid.ItemsSource = actSubscriptions;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDSubscription"].ToString()!;
                int rowsAffected;
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    SqlCommand cmd = new SqlCommand("Delete from Subscriptions where IDSubscription=" + id, con);
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

            subscriptions subscribtionWindow = new subscriptions(this, ((DataRowView)((Button)sender).Tag));
            subscribtionWindow.TitleTextBlock.Text = "Изменить";
            subscribtionWindow.AddButton.Content = "Сохранить";

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(subscribtionWindow);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            subscriptions postmanC = new subscriptions(this);

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(postmanC);
        }

        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }

        public void FillDataGridArchive()
        {
            using (SqlConnection con = new SqlConnection(ConString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * From Archive", con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Archive");
                sda.Fill(dt);
                SubscriptionsArchiveDataGrid.ItemsSource = dt.DefaultView;
                archiveSubscriptions = SubscriptionsArchiveDataGrid.ItemsSource;
            }
        }

        private void SearchTextBoxArc_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBoxArc.Text.Length > 0)
            {
                SearchTextBlockArc.Visibility = Visibility.Hidden;
                SearchIconArc.Visibility = Visibility.Hidden;
                SubscriptionsArchiveDataGrid.ItemsSource = archiveSubscriptions!.Cast<object>().ToList().Where<object>(o => (o as DataRowView)!.Row["FKSubscriber"].ToString()!.ToLower().Contains(SearchTextBoxArc.Text.ToLower()));
            }
            else
            {
                SearchTextBlockArc.Visibility = Visibility.Visible;
                SearchIconArc.Visibility = Visibility.Visible;
                SubscriptionsArchiveDataGrid.ItemsSource = archiveSubscriptions;
            }
        }

        private void ArchiveDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDSubscription"].ToString()!;
                int rowsAffected;
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    SqlCommand cmd = new SqlCommand("Delete from Archive where IDSubscription=" + id, con);
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
                FillDataGridArchive();
            }
        }

        private void QuerryButtonClick(object sender, RoutedEventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            {
                SqlCommand? cmd = null;
                switch ((sender as Button)!.Name)
                {
                    case "firstQueryButton":
                        cmd = new("SELECT Subscribers.*\r\nFROM Subscribers\r\nJOIN Subscriptions ON Subscribers.IDSubscriber = Subscriptions.IDSubscription\r\nJOIN Periodicals ON Subscriptions.IDSubscription = Periodicals.IDPeriodical\r\nWHERE Periodicals.title = 'Вечерний Минск';", con);
                        break;
                    case "secondQueryButton":
                        cmd = new("SELECT Periodicals.Title, COUNT(*) AS subscription_count\r\nFROM Subscriptions\r\nJOIN Periodicals ON Subscriptions.FKPeriodical = Periodicals.IDPeriodical\r\nGROUP BY Periodicals.Title;", con);
                        break;
                    case "thirdQueryButton":
                        cmd = new("SELECT FIO, Address, COUNT(*) AS subscription_count\r\nFROM (\r\nSELECT IDSubscriber AS id, FIO, Address\r\nFROM Subscribers\r\nUNION ALL\r\nSELECT IDPostmen AS id, FIO, Address\r\nFROM Postmen\r\n) AS people\r\nJOIN Subscriptions ON people.id = Subscriptions.FKSubscriber\r\nGROUP BY people.id, people.FIO, people.Address;", con);
                        break;
                    case "fourthQueryButton":
                        cmd = new("SELECT\r\nDATEPART(QUARTER, s.IssueDate) as Quarter,\r\nSUM(p.PriceThreeMonths * s.SubscriptionTerm) as QuarterSum\r\nFROM\r\nSubscriptions s\r\nJOIN Periodicals p ON s.FKPeriodical = p.IDPeriodical\r\nGROUP BY\r\nDATEPART(QUARTER, s.IssueDate)", con);
                        break;
                    case "fifthQueryButton":
                        cmd = new("SELECT\r\ns.FIO as SubscriberName,\r\np.FIO as PostmanName,\r\nper.Title as PeriodicalTitle,\r\nsub.SubscriptionStart,\r\nsub.SubscriptionEnd,\r\nsub.IssueDate\r\nFROM\r\nSubscribers s\r\nJOIN Subscriptions sub ON s.IDSubscriber = sub.FKSubscriber\r\nJOIN Periodicals per ON sub.FKPeriodical = per.IDPeriodical\r\nJOIN Postmen p ON s.FKPostmen = p.IDPostmen\r\nWHERE\r\nMONTH(sub.SubscriptionStart) = MONTH(GETDATE()) OR MONTH(sub.IssueDate) = MONTH(GETDATE())", con);
                        break;
                    case "sixthQueryButton":
                        if (FuncSubCount.Text.Length < 1)
                        {
                            MessageWindow.Show("Ошибка ввода", "Введите количество подписок!", MessageBoxButton.OK);
                            return;
                        }
                        if(FuncYear.Text.Length < 4)
                        {
                            MessageWindow.Show("Ошибка ввода", "Введите корректный год!", MessageBoxButton.OK);
                            return;
                        }
                        cmd = new($"SELECT * FROM [dbo].[GetSubscribersByYear] ({FuncYear.Text},{FuncSubCount.Text})", con);
                        break;
                }
                if (cmd != null)
                {
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("Querry");
                    sda.Fill(dt);
                    QueriesDataGrid.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void QueriesDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
            {
                DataGridTextColumn? dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = "dd-MM-yyyy";
                }
            }
        }
        private void Digit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, "[^0-9]+")) e.Handled = true;
        }
    }
}
