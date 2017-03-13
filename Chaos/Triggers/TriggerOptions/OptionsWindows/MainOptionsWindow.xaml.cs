using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class MainOptionsWindow : ContentDialog
    {
        public MainOptions MO { get; set; }

        public MainOptionsWindow(MainOptions options = null)
        {
            this.InitializeComponent();
            if(options != null)
            {
                AddOptions(options);
            }

        }

        public void AddOptions(MainOptions options)
        {
            if(MO == null)
            {
                toBox.Text = options.Timeout.ToString();
                usersBox.Text = string.Join(",", options.Users);
                ignoreBox.Text = string.Join(",", options.Ignores);
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            List<ulong> users = new List<ulong>();
            List<ulong> ignores = new List<ulong>();

            if (usersBox.Text.Split(',').Length != 0 && usersBox.Text != "")
            {
                foreach (string user in usersBox.Text.Split(','))
                {
                    users.Add(Convert.ToUInt64(user));
                }
            }

            if (ignoreBox.Text.Split(',').Length != 0 && ignoreBox.Text != "")
            {
                foreach (string ignore in ignoreBox.Text.Split(','))
                {
                    ignores.Add(Convert.ToUInt64(ignore));
                }
            }

            MO = new MainOptions()
            {
                Ignores = ignores,
                Users = users,
                Timeout = toBox.Text == "" ? 0 : Convert.ToInt32(toBox.Text)
            };
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            return;
        }
    }
}
