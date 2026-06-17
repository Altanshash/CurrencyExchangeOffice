using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ExchangeServiceRef;

namespace CurrencyExchangeOffice.Client
{
    public partial class MainWindow : Window
    {
        private ExchangeServiceClient _client;
        private int _userId;
        private string _username = "";

        public MainWindow()
        {
            InitializeComponent();
            _client = new ExchangeServiceClient();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = await _client.LoginUserAsync(TxtUsername.Text, Hash(TxtPassword.Password));
                if (user.Success)
                {
                    _userId = user.UserId;
                    _username = user.Username;
                    ShowApp();
                }
                else TxtLoginMsg.Text = user.Message ?? "Invalid credentials";
            }
            catch (Exception ex) { TxtLoginMsg.Text = "Error: " + ex.Message; }
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtUsername.Text) || string.IsNullOrWhiteSpace(TxtEmail.Text))
                { TxtLoginMsg.Text = "Fill in all fields"; return; }

                bool ok = await _client.RegisterUserAsync(TxtUsername.Text, Hash(TxtPassword.Password), TxtEmail.Text);
                if (ok)
                {
                    TxtLoginMsg.Text = "Registered successfully! Now login.";
                    TxtLoginMsg.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else TxtLoginMsg.Text = "Username already taken.";
            }
            catch (Exception ex) { TxtLoginMsg.Text = "Error: " + ex.Message; }
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            AppPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            TxtLoginMsg.Text = "";
        }

        private void ShowApp()
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            AppPanel.Visibility = Visibility.Visible;
            TxtUser.Text = $"Logged in as:\n{_username}";
            ShowRates(null, null);
        }

        private void HideAllPanels()
        {
            PanelRates.Visibility = Visibility.Collapsed;
            PanelAccount.Visibility = Visibility.Collapsed;
            PanelBuy.Visibility = Visibility.Collapsed;
            PanelSell.Visibility = Visibility.Collapsed;
            PanelHistory.Visibility = Visibility.Collapsed;
        }

        private async void ShowRates(object s, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelRates.Visibility = Visibility.Visible;
            await RefreshRates();
        }

        private async void RefreshRates(object s, RoutedEventArgs e)
        {
            await RefreshRates();
        }

        private async System.Threading.Tasks.Task RefreshRates()
        {
            try
            {
                var rates = await _client.GetAllRatesAsync();
                GridRates.ItemsSource = rates;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private async void ShowAccount(object s, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelAccount.Visibility = Visibility.Visible;
            await LoadBalance();
        }

        private async System.Threading.Tasks.Task LoadBalance()
        {
            try
            {
                var bal = await _client.GetBalanceAsync(_userId);
                var sb = new StringBuilder();
                sb.AppendLine($"PLN: {bal.PLN:F2}");
                if (bal.Currencies != null)
                    foreach (var kv in bal.Currencies)
                        sb.AppendLine($"{kv.Key}: {kv.Value:F4}");
                TxtBalance.Text = sb.ToString();
            }
            catch (Exception ex) { TxtBalance.Text = "Error: " + ex.Message; }
        }

        private void ShowBuy(object s, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelBuy.Visibility = Visibility.Visible;
        }

        private void ShowSell(object s, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelSell.Visibility = Visibility.Visible;
        }

        private async void ShowHistory(object s, RoutedEventArgs e)
        {
            HideAllPanels();
            PanelHistory.Visibility = Visibility.Visible;
            try
            {
                var history = await _client.GetTransactionHistoryAsync(_userId);
                GridHistory.ItemsSource = history;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private async void BtnTopUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(TxtTopUp.Text, out decimal amt) || amt <= 0)
                { TxtTopUpMsg.Text = "Enter a valid amount"; return; }
                bool ok = await _client.TopUpAccountAsync(_userId, amt);
                TxtTopUpMsg.Text = ok ? $"Added {amt:F2} PLN!" : "Top-up failed";
                if (ok) await LoadBalance();
            }
            catch (Exception ex) { TxtTopUpMsg.Text = "Error: " + ex.Message; }
        }

        private async void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(TxtBuyAmount.Text, out decimal amt))
                { TxtBuyMsg.Text = "Invalid amount"; return; }
                var result = await _client.BuyCurrencyAsync(_userId, TxtBuyCurrency.Text.ToUpper(), amt);
                TxtBuyMsg.Text = result.Message;
                TxtBuyMsg.Foreground = result.Success
                    ? System.Windows.Media.Brushes.LightGreen
                    : System.Windows.Media.Brushes.Salmon;
            }
            catch (Exception ex) { TxtBuyMsg.Text = "Error: " + ex.Message; }
        }

        private async void BtnSell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(TxtSellAmount.Text, out decimal amt))
                { TxtSellMsg.Text = "Invalid amount"; return; }
                var result = await _client.SellCurrencyAsync(_userId, TxtSellCurrency.Text.ToUpper(), amt);
                TxtSellMsg.Text = result.Message;
                TxtSellMsg.Foreground = result.Success
                    ? System.Windows.Media.Brushes.LightGreen
                    : System.Windows.Media.Brushes.Salmon;
            }
            catch (Exception ex) { TxtSellMsg.Text = "Error: " + ex.Message; }
        }

        private string Hash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}