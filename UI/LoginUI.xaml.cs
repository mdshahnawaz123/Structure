using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace Structure.UI
{
    public partial class Loginform : Window
    {
        public string LoggedInEmail { get; private set; }

        public Loginform()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = ShowPasswordCheckBox.IsChecked == true
                ? VisiblePasswordBox.Text
                : PasswordBox.Password;

            var result = await ValidateUserOnline(username, password);

            if (result.IsValid)
            {
                LoggedInEmail = username;
                Properties.Settings.Default.LastLoginDate = DateTime.Now;
                Properties.Settings.Default.Save();

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Clear();
                VisiblePasswordBox.Clear();
            }
        }

        private async Task<(bool IsValid, string Message)> ValidateUserOnline(string email, string password)
        {
            try
            {
                string url = "https://opensheet.elk.sh/2PACX-1vRR5nizN1O-NKArh3PelGmmsd6q23NSeGvH5vBW5CHAHaqcbYZsy9xRUdmjmYEi4ELm7FYsMG_m1Cn2/Sheet1";
                using var client = new HttpClient();
                var json = await client.GetStringAsync(url);

                var users = JsonConvert.DeserializeObject<List<UserRecord>>(json);

                var match = users.FirstOrDefault(u =>
                    string.Equals(u.EmailId, email, StringComparison.OrdinalIgnoreCase) &&
                    u.PassWord == password);

                if (match == null)
                    return (false, "Invalid email or password.");

                if (!DateTime.TryParse(match.Expiry, out DateTime expiryDate))
                    return (false, "Account expiry date is invalid.");

                if (DateTime.Now.Date > expiryDate.Date)
                    return (false, "Your access has expired.");

                return (true, "Login successful.");
            }
            catch (Exception ex)
            {
                return (false, "Login system error: " + ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VisiblePasswordBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Visibility = Visibility.Visible;
            VisiblePasswordBox.Focus();
            VisiblePasswordBox.SelectionStart = VisiblePasswordBox.Text.Length;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = VisiblePasswordBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            VisiblePasswordBox.Visibility = Visibility.Collapsed;
            PasswordBox.Focus();
        }

        public class UserRecord
        {
            [JsonProperty("EmailId")]
            public string EmailId { get; set; }

            [JsonProperty("PassWord")]
            public string PassWord { get; set; }

            [JsonProperty("Expiry")]
            public string Expiry { get; set; }
        }
    }
}
