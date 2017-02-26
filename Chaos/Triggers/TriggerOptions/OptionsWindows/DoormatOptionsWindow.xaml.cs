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
    public sealed partial class DoormatOptionsWindow : ContentDialog
    {
        public DoormatOptions DO { get; set; }

        public DoormatOptionsWindow()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (messageBox.Text == "" || nameBox.Text == "")
            {
                MessageDialog dialog = new MessageDialog("You must include a name and a command.", "Error");
                await dialog.ShowAsync();
            }
            else
            {
                DO = new DoormatOptions()
                {
                    Name = nameBox.Text,
                    Message = messageBox.Text
                };
            }
        }
    }
}
