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
        public string LoggedInUser { get; private set; }

        public Loginform()
        {
            InitializeComponent();
        }

        public bool TryAutoLoginAndClose()
        {
            var cached = AppDataStorage.LoadUser();
            if (cached != null && DateTime.Now <= cached.Expiry)
            {
                LoggedInUser = cached.Username;
            }

            return false;
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
                LoggedInUser = username;
                AppDataStorage.SaveUser(username, result.Expiry);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(result.Message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Clear();
                VisiblePasswordBox.Clear();
            }
        }

        private async Task<(bool IsValid, string Message, DateTime Expiry)> ValidateUserOnline(string username, string password)
        {
            try
            {
                string url = "https://raw.githubusercontent.com/mdshahnawaz123/plugin-access-control/main/users.json";
                using var client = new HttpClient();
                string json = await client.GetStringAsync(url);

                var users = JsonConvert.DeserializeObject<List<UserRecord>>(json);
                var match = users.FirstOrDefault(u =>
                    string.Equals(u.username, username, StringComparison.OrdinalIgnoreCase) &&
                    u.password == password);

                if (match == null)
                    return (false, "Invalid username or password.", DateTime.MinValue);

                if (!match.active)
                    return (false, "Account is disabled.", DateTime.MinValue);

                if (DateTime.Now > match.expires)
                    return (false, "Access has expired.", match.expires);

                return (true, "Login successful.", match.expires);
            }
            catch (Exception ex)
            {
                return (false, "Error: " + ex.Message, DateTime.MinValue);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VisiblePasswordBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Visibility = Visibility.Visible;
            VisiblePasswordBox.Focus();
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
            public string username { get; set; }
            public string password { get; set; }
            public bool active { get; set; }
            public DateTime expires { get; set; }
        }
    }
}
