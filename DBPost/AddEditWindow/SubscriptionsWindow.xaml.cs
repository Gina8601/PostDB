using DBPost.Views;
using DBPost.Windows;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DBPost.AddEditWindow
{
    public partial class subscriptions : UserControl
    {
        // Строка подключения к базе данных из файла конфигурации
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private SubscriptionsView SubscriptionsView { get; set; }

        bool isAdding = true; // Флаг, указывающий, добавляем ли мы новую запись
        string? idSubscription = string.Empty;

        // Конструктор для добавления новой подписки
        public subscriptions (SubscriptionsView subscriptionsView)
        {
            InitializeComponent();
            SubscriptionsView = subscriptionsView;
            // Устанавливаем текущую дату как дату оформления
            IssueDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
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

        // Конструктор для редактирования подписки
        public subscriptions(SubscriptionsView subscriptionsView, DataRowView dataRowView)
        {
            InitializeComponent();
            isAdding = false;

            SubscriptionsView = subscriptionsView;
            this.idSubscription = dataRowView.Row["IDSubscription"].ToString();
            this.SubscriptionStart.Text = DateTime.Parse(dataRowView.Row["SubscriptionStart"].ToString()!).ToString("dd-MM-yyyy");
            this.SubscriptionEnd.Text = DateTime.Parse(dataRowView.Row["SubscriptionEnd"].ToString()!).ToString("dd-MM-yyyy");
            this.IssueDate.Text = DateTime.Parse(dataRowView.Row["IssueDate"].ToString()!).ToString("dd-MM-yyyy");
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

        // Обработка кнопки "Сохранить" (добавить или обновить)
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
                            cmd = new SqlCommand("INSERT INTO Subscriptions (FKSubscriber, FKPeriodical, SubscriptionStart, SubscriptionEnd, IssueDate, SubscriptionTerm, Price) " +
                                                 "VALUES (@FKSubscriber, @FKPeriodical, @Start, @End, @Issue, @Term, @Price)", con);
                        }
                        else
                        {
                            cmd = new SqlCommand("UPDATE Subscriptions SET FKSubscriber = @FKSubscriber, FKPeriodical = @FKPeriodical, " +
                                                 "SubscriptionStart = @Start, SubscriptionEnd = @End, IssueDate = @Issue, " +
                                                 "SubscriptionTerm = @Term, Price = @Price WHERE IDSubscription = @ID", con);
                        }

                        // Преобразуем строки из TextBox в DateTime и другие типы
                        cmd.Parameters.AddWithValue("@FKSubscriber", idSub);
                        cmd.Parameters.AddWithValue("@FKPeriodical", idPer);

                        // ВАЖНО: DateTime.ParseExact по нужному формату
                        cmd.Parameters.AddWithValue("@Start", DateTime.ParseExact(SubscriptionStart.Text, "dd-MM-yyyy", CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@End", DateTime.ParseExact(SubscriptionEnd.Text, "dd-MM-yyyy", CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@Issue", DateTime.ParseExact(IssueDate.Text, "dd-MM-yyyy", CultureInfo.InvariantCulture));

                        cmd.Parameters.AddWithValue("@Term", int.Parse(SubscriptionTerm.Text));
                        cmd.Parameters.AddWithValue("@Price", decimal.Parse(Price.Text.Replace(',', '.'), CultureInfo.InvariantCulture));

                        if (!isAdding)
                            cmd.Parameters.AddWithValue("@ID", idSubscription);

                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    SubscriptionsView.FillDataGrid();
                }
            };
        }

        // Анимация открытия формы
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

        // Ограничение ввода: только цифры и дефис
        private void Date_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^0-9-]+"))
            {
                e.Handled = true;
            }
        }

        // Ограничение ввода: только цифры и точка
        private void Digit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, "[^0-9.]+")) e.Handled = true;
        }

        // Валидация формы перед сохранением
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

        // Вызывается при нажатии на кнопку добавления
        private void AddButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ValidateSubscription()) (sender as Button)!.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        // Обновление даты окончания и цены при смене срока подписки
        private void SubscriptionTerm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SubscriptionTerm.SelectedIndex >= 0)
            {
                DateTime start = new();
                if(DateTime.TryParse(SubscriptionStart.Text, out start))
                {
                   SubscriptionEnd.Text = start.AddMonths(int.Parse((SubscriptionTerm.SelectedItem as ComboBoxItem)!.Content.ToString()!)).ToString("dd-MM-yyyy");
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

        // Автоматический пересчет даты окончания при изменении даты начала
        private void SubscriptionStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DateTime.TryParse(SubscriptionStart.Text, out DateTime startDate))
            {
                if (SubscriptionTerm.SelectedItem is ComboBoxItem selectedItem &&
                    int.TryParse(selectedItem.Content.ToString(), out int months))
                {
                    DateTime endDate = startDate.AddMonths(months);
                    SubscriptionEnd.Text = endDate.ToString("dd-MM-yyyy");
                }
            }
        }
    }
}
