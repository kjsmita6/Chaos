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
    public sealed partial class ChatCommandWindow : ContentDialog
    {
        public ChatCommand CC { get; set; }

        public ChatCommandWindow(ChatCommand options = null)
        {
            this.InitializeComponent();
            if(options != null)
            {
                AddOptions(options);
            }
        }

        public void AddOptions(ChatCommand options)
        {
            if (CC == null)
            {
                CC = options;
                nameBox.Text = CC.Name;
                commandBox.Text = CC.Command;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (commandBox.Text == "" || nameBox.Text == "" || usageBox.Text == "")
            {
                MessageDialog dialog = new MessageDialog("You must include a name, a command, and command usage.", "Error");
                await dialog.ShowAsync();
            }
            else
            {
                CC = new ChatCommand()
                {
                    Name = nameBox.Text,
                    Command = commandBox.Text,
                    Usage = usageBox.Text
                };
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }
    }
}
