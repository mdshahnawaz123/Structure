using Structure.Command;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Structure.UI
{
    public partial class ChatGPTWindow : Window
    {
        private readonly string historyFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BDD Chat", "chat_history.txt");

        public ChatGPTWindow()
        {
            InitializeComponent();
            LoadPreviousChat();
        }

        private void LoadPreviousChat()
        {
            try
            {
                if (File.Exists(historyFile))
                {
                    string history = File.ReadAllText(historyFile);
                    ResponseBox.Text = history;
                }
            }
            catch (Exception ex)
            {
                ResponseBox.Text = $"Error loading history: {ex.Message}\n";
            }
        }

        private void SaveToHistory(string userInput, string response)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(historyFile));
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string log = $"\n[{timestamp}]\nYou: {userInput}\nBDD Chat: {response}\n";
                File.AppendAllText(historyFile, log);
            }
            catch (Exception ex)
            {
                ResponseBox.Text += $"Error saving history: {ex.Message}\n";
            }
        }

        private async void AskGPT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = InputBox.Text.Trim();
                if (string.IsNullOrEmpty(input)) return;

                ResponseBox.Text += $"\nYou: {input}\n";
                InputBox.Text = "";
                InputPlaceholder.Visibility = Visibility.Visible;

                ResponseBox.Text += "BIM Digital Design is thinking...\n";

                var response = await ChatGPTHandler.CallGeminiAsync(input);

                ResponseBox.Text = ResponseBox.Text.Replace("BIM Digital Design is thinking...\n", "");
                ResponseBox.Text += $"BDD Chat: {response}\n";

                SaveToHistory(input, response);
            }
            catch (Exception ex)
            {
                ResponseBox.Text += $"Error: {ex.Message}\n";
            }
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            InputPlaceholder.Visibility = string.IsNullOrWhiteSpace(InputBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                e.Handled = true;
                AskGPT_Click(SendButton, new RoutedEventArgs());
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
