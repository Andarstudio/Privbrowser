using System.Windows.Controls;
using System.Windows.Input;

namespace PrivBrowser
{
    public partial class TerminalControl : UserControl
    {
        public TerminalControl()
        {
            InitializeComponent();
            AppendOutput("PrivBrowser Secure Terminal [Version 1.0.0]\nType 'help' for available commands.\n");
        }

        private void AppendOutput(string text)
        {
            ConsoleOutput.Text += text + "\n";
            ConsoleScroll.ScrollToBottom();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string cmd = InputBox.Text.Trim();
                InputBox.Clear();
                if (string.IsNullOrWhiteSpace(cmd)) return;

                AppendOutput($"privbrowser> {cmd}");
                ProcessCommand(cmd.ToLowerInvariant());
            }
        }

        private void ProcessCommand(string cmd)
        {
            switch (cmd)
            {
                case "help":
                    AppendOutput("Commands: about, version, help");
                    break;
                case "about":
                    AppendOutput("PrivBrowser — Built by Andar Studio\nGitHub: Andarstudio/Privbrowser");
                    break;
                case "version":
                    AppendOutput("PrivBrowser Core v1.0.0 (.NET 8 x64 / CEF)");
                    break;
                default:
                    AppendOutput($"Unknown command '{cmd}'. Type 'help' for commands.");
                    break;
            }
        }
    }
}