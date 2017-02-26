using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Chaos.Triggers.TriggerOptions.OptionsWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatReplyWindow : Page
    {
        public ChatReply CR { get; set; }
        public int DialogResult { get; set; }

        public ChatReplyWindow()
        {
            this.InitializeComponent();
        }

        private async void doneButton_Click(object sender, RoutedEventArgs e)
        {
            if (matchesBox.Text == "" || responsesBox.Text == "" || nameBox.Text == "")
            {
                MessageDialog dialog = new MessageDialog("You must include matches, responses, and a name.", "Error");
                await dialog.ShowAsync();
            }
            else
            {
                CR = new ChatReply
                {
                    Name = nameBox.Text,
                    Matches = matchesBox.Text.Split(',').ToList(),
                    Responses = responsesBox.Text.Split(',').ToList(),
                };

                DialogResult = 1;
                Visibility = Visibility.Collapsed;
            }
        }
    }
}
