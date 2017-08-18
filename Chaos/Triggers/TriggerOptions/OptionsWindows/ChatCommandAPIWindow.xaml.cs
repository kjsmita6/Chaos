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
    public sealed partial class ChatCommandAPIWindow : ContentDialog
    {
        public ChatCommandAPI CCAPI { get; set; }

        public ChatCommandAPIWindow(ChatCommandAPI options = null)
        {
            this.InitializeComponent();
            if(options != null)
            {
                AddOptions(options);
            }
        }

        public void AddOptions(ChatCommandAPI options)
        {
            if (CCAPI == null)
            {
                CCAPI = options;
                apiBox.Text = CCAPI.APIKey;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (apiBox.Text == "")
            {
                MessageDialog dialog = new MessageDialog("You must include an API key.", "Error");
                await dialog.ShowAsync();
            }
            else
            {
                CCAPI = new ChatCommandAPI()
                {
                    APIKey = apiBox.Text
                };
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }
    }
}
