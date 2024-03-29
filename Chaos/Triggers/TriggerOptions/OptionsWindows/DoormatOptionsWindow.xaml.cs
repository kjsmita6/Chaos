﻿using System;
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

        public DoormatOptionsWindow(DoormatOptions options = null)
        {
            this.InitializeComponent();
            if(options != null)
            {
                AddOptions(options);
            }
        }

        public void AddOptions(DoormatOptions options)
        {
            if (DO == null)
            {
                DO = options;
                nameBox.Text = DO.Name;
                messageBox.Text = DO.Message;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (messageBox.Text == "" || nameBox.Text == "")
            {
                MessageDialog dialog = new MessageDialog("You must include a name and a command.", "Error");
                await dialog.ShowAsync();
                return;
            }
            else if (!messageBox.Text.Contains("#"))
            {
                MessageDialog dialog = new MessageDialog("You must include # where the user's name will be in the message.", "Error");
                await dialog.ShowAsync();
                return;
            }
            else if (messageBox.Text != "" && nameBox.Text != "" && messageBox.Text.Contains("#"))
            {
                DO = new DoormatOptions()
                {
                    Name = nameBox.Text,
                    Message = messageBox.Text
                };
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }
    }
}
