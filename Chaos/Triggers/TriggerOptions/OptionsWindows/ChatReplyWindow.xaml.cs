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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Chaos.Triggers.TriggerOptions.OptionsWindows
{
    public sealed partial class ChatReplyWindow : ContentDialog
    {
        public ChatReply CR { get; set; }

        public ChatReplyWindow()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (nameBox.Text == "" || matchesBox.Text == "" || responsesBox.Text == "")
            {
                MessageDialog dialog = new MessageDialog("You must include a name, matches, and responses. Matches and responses must be separated with commas.", "Error");
                await dialog.ShowAsync();
            }
            else
            {
                CR = new ChatReply()
                {
                    Name = nameBox.Text,
                    Matches = matchesBox.Text.Split(',').ToList(),
                    Responses = responsesBox.Text.Split(',').ToList()
                };
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }
    }
}
