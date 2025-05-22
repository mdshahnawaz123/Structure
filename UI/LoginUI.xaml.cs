using System;
using System.Windows;
using System.Windows.Controls;

namespace Structure.UI
{
    public partial class Loginform : Window
    {
        public Loginform()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = ShowPasswordCheckBox.IsChecked == true
                ? VisiblePasswordBox.Text
                : PasswordBox.Password;

            if (username == "mdshahnawaz9570@gmail.com" && password == "Mohdshah@123")
            {
                // Save today's login date
                Properties.Settings.Default.LastLoginDate = DateTime.Now.Date;
                Properties.Settings.Default.Save();

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Clear();
                VisiblePasswordBox.Clear();
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
    }
}

