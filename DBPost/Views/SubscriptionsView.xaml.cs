using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using DBPost.AddEditWindow;
using DBPost.Windows;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;

namespace DBPost.Views
{
    public partial class SubscriptionsView : UserControl
    {
        // Строка подключения к БД из конфигурации
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private IEnumerable? actSubscriptions;
        private IEnumerable? archiveSubscriptions;
        public SubscriptionsView()
        {
            InitializeComponent();

            CheckSubscriptions(); // Перемещает просроченные подписки в архив

            FillDataGrid(); // Заполняет грид активных подписок

            FillDataGridArchive(); // Заполняет грид архивных подписок
        }

        // Перемещает просроченные подписки из активных в архив
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

        // Заполнение грида активных подписок из БД
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

        // Обработка поиска по активным подпискам
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

        // Удаление подписки из активных с подтверждением
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

        // Открытие окна редактирования выбранной подписки
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

        // Открытие окна добавления новой подписки
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = false;
            this.IsEnabled = false;

            subscriptions postmanC = new subscriptions(this);

            Grid baseContainer = mainWindow.BaseContainer;
            baseContainer?.Children.Add(postmanC);
        }

        // Включение интерфейса после закрытия модального окна
        public void EnableWindow()
        {
            MainWindow mainWindow = ((((this.Parent as ContentControl)!.Parent as Grid)!.Parent as Border)!.Parent as MainWindow)!;
            mainWindow.Menu.IsEnabled = true;
            this.IsEnabled = true;
        }

        // Заполнение грида архивных подписок из БД
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

        // Обработка поиска по архивным подпискам
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

        // Удаление записи из архива с подтверждением
        private void ArchiveDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageWindow.Show("Информационное окно", "Удалить?", MessageBoxButton.YesNo))
            {
                string id = ((DataRowView)((Button)sender).Tag).Row["IDArchive"].ToString()!;
                int rowsAffected;
                using (SqlConnection con = new SqlConnection(ConString))
                {
                    SqlCommand cmd = new SqlCommand("Delete from Archive where IDArchive=" + id, con);
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

        // Фильтрация ввода только цифр (для текстбоксов)
        private void Digit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, "[^0-9]+")) e.Handled = true;
        }

        // Генерация отчёта по подпискам, срок которых истекает в текущем месяце
        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();

            string query = @"
                    SELECT sub.FIO, p.Title, s.SubscriptionEnd
                    FROM dbo.Subscriptions s
                    JOIN dbo.Subscribers sub ON s.FKSubscriber = sub.IDSubscriber
                    JOIN dbo.Periodicals p ON s.FKPeriodical = p.IDPeriodical
                    WHERE s.SubscriptionEnd BETWEEN @Today AND @inMonth
                    ORDER BY s.SubscriptionEnd";

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                var today = DateTime.Today;
                var firstDayNextMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1);

                cmd.Parameters.AddWithValue("@Today", today);
                cmd.Parameters.AddWithValue("@inMonth", firstDayNextMonth);

                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        MessageWindow.Show("Информационное окно", "Подписок, срок которых истекает в текущем месяце, нет", MessageBoxButton.OK);
                        return;
                    }

                    sb.AppendLine("Отчёт по подпискам, срок которых истекает в текущем месяце:");
                    sb.AppendLine("-----------------------------------------------------------");

                    int count = 1;
                    while (reader.Read())
                    {
                        string fio = reader.GetString(0);
                        string title = reader.GetString(1);
                        DateTime endDate = reader.GetDateTime(2);

                        sb.AppendLine($"{count}. {fio} — подписка на \"{title}\" истекает {endDate:dd-MM-yyyy}");
                        count++;
                    }
                }
            }

            ReportTextBox.Text = sb.ToString();
        }

        // Сохранение сформированного отчёта в файл
        private void SaveReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReportTextBox.Text))
            {
                MessageWindow.Show("Информационное окно.", "Сначала сформируйте отчёт", MessageBoxButton.OK);
                return;
            }

            string monthName = DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));
            string fileName = $"Отчет по подпискам за {monthName}.txt";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == true) 
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ReportTextBox.Text, Encoding.UTF8);
                    MessageWindow.Show("Информационное окно.", "Отчёт успешно сохранён", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageWindow.Show("Информационное окно", $"Ошибка при сохранении файла:\n{ex.Message}", MessageBoxButton.OK);
                }
            }
        }

        // Сохранение отчёта по количеству подписок на издания
        private void SaveReportByPeriodicalButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReportByPeriodicalTextBox.Text))
            {
                MessageWindow.Show("Информационное окно.", "Сначала сформируйте отчёт", MessageBoxButton.OK);
                return;
            }

            string fileName = $"Отчет по количеству подписок.txt";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ReportByPeriodicalTextBox.Text, Encoding.UTF8);
                    MessageWindow.Show("Информационное окно.", "Отчёт успешно сохранён", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageWindow.Show("Информационное окно", $"Ошибка при сохранении файла:\n{ex.Message}", MessageBoxButton.OK);
                }
            }
        }

        // Генерация отчёта по количеству подписок на каждое издание
        private void GenerateReportByPeriodicalButton_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();

            string query = @"
            SELECT p.Title, COUNT(*) AS SubscriptionCount
            FROM dbo.Subscriptions s
            JOIN dbo.Periodicals p ON s.FKPeriodical = p.IDPeriodical
            GROUP BY p.Title
            ORDER BY SubscriptionCount DESC";

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        MessageWindow.Show("Информационное окно", "Нет данных о подписках.", MessageBoxButton.OK);
                        return;
                    }

                    sb.AppendLine("Отчёт по количеству подписок на издания:");
                    sb.AppendLine("----------------------------------------");

                    int count = 1;
                    while (reader.Read())
                    {
                        string title = reader.GetString(0);
                        int subscriptionCount = reader.GetInt32(1);
                        string word = GetSubscriptionWord(subscriptionCount);

                        sb.AppendLine($"{count}. \"{title}\" — {subscriptionCount} {word}");
                        count++;
                    }
                }
            }

            ReportByPeriodicalTextBox.Text = sb.ToString();
        }

        // Подбор правильного слова в зависимости от количества подписок
        private string GetSubscriptionWord(int count)
        {
            if ((count % 10 == 1) && (count % 100 != 11))
                return "подписка";
            else if ((count % 10 >= 2 && count % 10 <= 4) && !(count % 100 >= 12 && count % 100 <= 14))
                return "подписки";
            else
                return "подписок";
        }

        // Сохранение отчёта по нагрузке почтальонов
        private void SaveReportByEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReportByEmployeeTextBox.Text))
            {
                MessageWindow.Show("Информационное окно.", "Сначала сформируйте отчёт", MessageBoxButton.OK);
                return;
            }

            string fileName = $"Отчет по нагрузке почтальонов.txt";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ReportByEmployeeTextBox.Text, Encoding.UTF8);
                    MessageWindow.Show("Информационное окно.", "Отчёт успешно сохранён", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageWindow.Show("Информационное окно", $"Ошибка при сохранении файла:\n{ex.Message}", MessageBoxButton.OK);
                }
            }
        }
        // Универсальный метод для правильного склонения слов
        private string GetPluralForm(int number, string one, string few, string many)
        {
            number = Math.Abs(number) % 100;
            int n1 = number % 10;

            if (number > 10 && number < 20) return many;
            if (n1 > 1 && n1 < 5) return few;
            if (n1 == 1) return one;
            return many;
        }

        // Генерация отчёта по количеству подписчиков, закреплённых за сотрудниками
        private void GenerateReportByEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();

            string query = @"
                SELECT p.FIO AS PostmanName, COUNT(s.IDSubscriber) AS SubscriberCount
                FROM dbo.Postmen p
                LEFT JOIN dbo.Subscribers s ON p.IDPostmen = s.FKPostmen
                GROUP BY p.FIO
                ORDER BY SubscriberCount DESC;";

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        MessageWindow.Show("Информационное окно", "Данные по сотрудникам отсутствуют", MessageBoxButton.OK);
                        return;
                    }

                    sb.AppendLine("Отчёт по количеству подписчиков, закреплённых за сотрудниками:");
                    sb.AppendLine("--------------------------------------------------------------");

                    int count = 1;
                    while (reader.Read())
                    {
                        string fio = reader.GetString(0);
                        int subs = reader.GetInt32(1);
                        string plural = GetPluralForm(subs, "подписчик", "подписчика", "подписчиков");
                        sb.AppendLine($"{count}. Сотрудник {fio} обслуживает {subs} {plural}");
                        count++;
                    }
                }
            }

            ReportByEmployeeTextBox.Text = sb.ToString();
        }
    }
}
