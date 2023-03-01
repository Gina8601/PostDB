using DBPost.Views;
using DBPost.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBPost.AddEditWindow
{
    /// <summary>
    /// Логика взаимодействия для subscriptions.xaml
    /// </summary>
    public partial class subscriptions : UserControl
    {
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private SubscriptionsView SubscriptionsView { get; set; }

        bool isAdding = true;
        string? idSubscription = string.Empty;
        public subscriptions (SubscriptionsView subscriptionsView)
        {
            InitializeComponent();
            SubscriptionsView = subscriptionsView;
            using (SqlConnection con = new SqlConnection(ConString))
            {
                con.Open();

                SqlDataAdapter adapter = new($"Select * from Subscribers", con);
                DataTable table = new("Subscriber");
                adapter.Fill(table);
                FKSubscriber.ItemsSource = table.DefaultView;
                FKSubscriber.DisplayMemberPath = "FIO";

                adapter = new($"Select * from Periodicals", con);
                table = new("Periodical");
                adapter.Fill(table);
                FKPeriodical.ItemsSource = table.DefaultView;
                FKPeriodical.DisplayMemberPath = "Title";

                con.Close();
            }
        }

        public subscriptions(SubscriptionsView subscriptionsView, DataRowView dataRowView)
        {
            InitializeComponent();
            isAdding = false;
            SubscriptionsView = subscriptionsView;
            this.idSubscription = dataRowView.Row["IDSubscription"].ToString();
            this.SubscriptionStart.Text = DateTime.Parse(dataRowView.Row["SubscriptionStart"].ToString()!).ToShortDateString();
            this.SubscriptionEnd.Text = DateTime.Parse(dataRowView.Row["SubscriptionEnd"].ToString()!).ToShortDateString();
            this.IssueDate.Text = DateTime.Parse(dataRowView.Row["IssueDate"].ToString()!).ToShortDateString();
            this.SubscriptionTerm.Text = dataRowView.Row["SubscriptionTerm"].ToString();
            this.Price.Text = dataRowView.Row["Price"].ToString();
            using (SqlConnection con = new SqlConnection(ConString))
            {
                con.Open();

                SqlDataAdapter adapter = new($"Select * from Subscribers", con);
                DataTable table = new("Subscriber");
                adapter.Fill(table);
                FKSubscriber.ItemsSource = table.DefaultView;
                FKSubscriber.DisplayMemberPath = "FIO";
                SqlCommand cmd = new("Select FIO from Subscribers where IDSubscriber=" + dataRowView.Row["FKSubscriber"].ToString(), con);
                this.FKSubscriber.Text = cmd.ExecuteScalar().ToString();


                adapter = new($"Select * from Periodicals", con);
                table = new("Periodical");
                adapter.Fill(table);
                FKPeriodical.ItemsSource = table.DefaultView;
                FKPeriodical.DisplayMemberPath = "Title";
                cmd = new("Select Title from Periodicals where IDPeriodical=" + dataRowView.Row["FKPeriodical"].ToString(), con);
                this.FKPeriodical.Text = cmd.ExecuteScalar().ToString();

                con.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            (FindResource("CloseMenu") as Storyboard)!.Completed += (s, a) => {
                (this.Parent as Grid)!.Children.Remove(this);
                SubscriptionsView.EnableWindow();
                if ((sender as Button) == AddButton)
                {
                   
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        con.Open();
                        SqlCommand findSubscriber = new($"Select IDSubscriber from Subscribers where FIO='{FKSubscriber.Text}'", con);
                        string idSub = findSubscriber.ExecuteScalar().ToString()!;
                        SqlCommand findPeriodical = new($"Select IDPeriodical from Periodicals where Title='{ FKPeriodical.Text}'", con);
                        string idPer = findPeriodical.ExecuteScalar().ToString()!;
                        SqlCommand cmd;
                        if (isAdding)
                        {
                            cmd = new ($"INSERT INTO Subscriptions (FKSubscriber, FKPeriodical, SubscriptionStart, SubscriptionEnd, IssueDate, SubscriptionTerm, Price) Values" +
                            $" ({idSub}, {idPer}, '{SubscriptionStart.Text}', '{SubscriptionEnd.Text}', '{IssueDate.Text}', {SubscriptionTerm.Text}, {Price.Text.Replace(',', '.')})", con);
                        }
                        else
                        {
                            cmd = new($"Update Subscriptions Set FKSubscriber={idSub}, FKPeriodical={idPer}, SubscriptionStart='{this.SubscriptionStart.Text}'" +
                                $", SubscriptionEnd='{this.SubscriptionEnd.Text}', IssueDate='{this.IssueDate.Text}', SubscriptionTerm={this.SubscriptionTerm.Text}, Price={Price.Text.Replace(',','.')} where IDSubscription={idSubscription}", con);
                        }
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    SubscriptionsView.FillDataGrid();
                }
            };
        }

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
        private void Date_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^0-9-]+"))
            {
                e.Handled = true;
            }
        }
        private void Digit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, "[^0-9.]+")) e.Handled = true;
        }

        private bool ValidateSubscription()
        {
            if (FKSubscriber.SelectedIndex < 0)
            {
                MessageWindow.Show("Ошибка ввода", "Выберите подписчика!", MessageBoxButton.OK);
                return false;
            }
            if (FKPeriodical.SelectedIndex < 0)
            {
                MessageWindow.Show("Ошибка ввода", "Выберите издание!!", MessageBoxButton.OK);
                return false;
            }

            DateTime issueDate = new();
            DateTime startDate = new();
            if (!DateTime.TryParse(SubscriptionStart.Text, out startDate))
            {
                MessageWindow.Show("Ошибка ввода", "Введите корректно начало подписки!", MessageBoxButton.OK);
                return false;
            }
            if (!DateTime.TryParse(SubscriptionEnd.Text, out issueDate))
            {
                MessageWindow.Show("Ошибка ввода", "Введите корректно конец подписки!", MessageBoxButton.OK);
                return false;
            }
            if (!DateTime.TryParse(IssueDate.Text, out issueDate))
            {
                MessageWindow.Show("Ошибка ввода", "Введите корректно дату оформления!", MessageBoxButton.OK);
                return false;
            }
            if (SubscriptionTerm.SelectedIndex < 0)
            {
                MessageWindow.Show("Ошибка ввода", "Выберите срок подписки!", MessageBoxButton.OK);
                return false;
            }
            if(issueDate.AddDays(10) < startDate)   
            {
                MessageWindow.Show("Ошибка ввода", "Дата оформления подписки должна быть не позднее 10 дней до ее начала!", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        private void AddButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ValidateSubscription()) (sender as Button)!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void SubscriptionTerm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubscriptionTerm.SelectedIndex >= 0)
            {
                DateTime start = new();
                if(DateTime.TryParse(SubscriptionStart.Text, out start))
                {
                   SubscriptionEnd.Text = start.AddMonths(int.Parse((SubscriptionTerm.SelectedItem as ComboBoxItem)!.Content.ToString()!)).ToShortDateString();
                }
                if (FKPeriodical.SelectedIndex >= 0)
                {
                    string term = string.Empty;
                    switch (int.Parse((SubscriptionTerm.SelectedItem as ComboBoxItem)!.Content.ToString()!))
                    {
                        case 1:
                            term = "PriceMonth";
                            break;
                        case 3:
                            term = "PriceThreeMonths";
                            break;
                        case 6:
                            term = "PriceSixMonths";
                            break;
                        case 12:
                            term = "PriceTwelveMonths";
                            break;
                        default:
                            break;
                    }
                    using (SqlConnection con = new SqlConnection(ConString))
                    {
                        con.Open();
                        SqlCommand findPricePeriodical = new($"Select {term} from Periodicals where Title='{FKPeriodical.Text}'", con);
                        Price.Text = findPricePeriodical.ExecuteScalar().ToString()!;
                        con.Close();
                    }
                }
            }
        }
    }
}
