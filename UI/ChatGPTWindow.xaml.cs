using Structure.Command;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Structure.UI
{
    public partial class ChatGPTWindow : Window
    {
        public ChatGPTWindow()
        {
            InitializeComponent();
        }

        private async void AskGPT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = InputBox.Text;
                ResponseBox.Text = "BIM Digital Design start Thinking...";
                var response = await ChatGPTHandler.CallChatGPT(input);
                ResponseBox.Text = response;
            }
            catch (Exception ex)
            {
                ResponseBox.Text = $"Error: {ex.Message}";
            }

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}