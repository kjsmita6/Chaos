using Chaos.Triggers;
using Chaos.Triggers.TriggerOptions;
using Chaos.Triggers.TriggerOptions.OptionsWindows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Chaos
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Log.dispatcher = Dispatcher;
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (usernameBox.Text == "" && chatbotsList.SelectedIndex != -1 && chatbotsList.SelectedValue.ToString() != "")
            {
                string username = chatbotsList.SelectedValue.ToString();
                Bot.userDir = await Bot.folder.CreateFolderAsync(username, CreationCollisionOption.OpenIfExists);

                if (Bot.userDir.GetFileAsync("login.json") != null && tokenBox.Text == "")
                {
                    var data = await Bot.ReadData(username);
                    startButton.Visibility = Visibility.Collapsed;
                    pivot.SelectedIndex = 2;
                    await Bot.StartAsync(username, data.Token);
                    Log.Instance.Silly("Successfully read login data from file.");
                }
            }
            else if (usernameBox.Text != "" && tokenBox.Text == "")
            {
                string username = usernameBox.Text;
                Bot.userDir = await Bot.folder.CreateFolderAsync(username, CreationCollisionOption.OpenIfExists);
                if (Bot.userDir.GetFileAsync("login.json") != null)
                {
                    var data = await Bot.ReadData(username);
                    startButton.Visibility = Visibility.Collapsed;
                    pivot.SelectedIndex = 2;
                    await Bot.StartAsync(username, data.Token);
                    Log.Instance.Silly("Successfully read login data from file.");
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Missing token.", "Error");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                if (usernameBox.Text == "")
                {
                    MessageDialog dialog = new MessageDialog("Missing username.", "Error");
                    await dialog.ShowAsync();
                }
                if (tokenBox.Text == "")
                {
                    MessageDialog dialog = new MessageDialog("Missing token.", "Error");
                    await dialog.ShowAsync();
                }
                else
                {
                    Bot.userDir = await Bot.folder.CreateFolderAsync(usernameBox.Text, CreationCollisionOption.OpenIfExists);
                    startButton.Visibility = Visibility.Collapsed;
                    pivot.SelectedIndex = 2;
                    await Bot.StartAsync(usernameBox.Text, tokenBox.Text);
                }
            }
        }

        private async void addButton_Click(object sender, RoutedEventArgs e)
        {
            string selected = "";
            TriggerType type;
            try
            {
                selected = ((ListBoxItem)triggersListBox.SelectedValue).Name;
            }
            catch (Exception err) { return; }

            ChatCommand cc = new ChatCommand();
            ChatReply cr = new ChatReply();
            DoormatOptions _do = new DoormatOptions();

            TriggerOptionsBase tob = new TriggerOptionsBase();

            if(selected == "kickTrigger" || selected == "banTrigger")
            {
                ChatCommandWindow window = new ChatCommandWindow();
                ContentDialogResult result = await window.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    cc = window.CC;

                    type = (TriggerType)Enum.Parse(typeof(TriggerType), char.ToUpper(selected[0]) + selected.Substring(1));
                    addedTriggersListBox.Items.Add(string.Format("{0} - {1}", cc.Name, type.ToString()));

                    tob.ChatCommand = cc;
                    tob.Name = cc.Name;
                    tob.Type = type;
                    BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + type.ToString()), type, cc.Name, tob);
                    Bot.triggers.Add(trigger);
                }
            }
            else if(selected == "chatReplyTrigger")
            {
                ChatReplyWindow crw = new ChatReplyWindow();
                crw.Visibility = Visibility.Visible;
                if (crw.DialogResult == 0)
                {
                    cr = crw.CR;
                    type = (TriggerType)Enum.Parse(typeof(TriggerType), char.ToUpper(selected[0]) + selected.Substring(1));

                    tob.ChatReply = cr;
                    tob.Name = cr.Name;
                    tob.Type = type;
                    addedTriggersListBox.Items.Add(string.Format("{0} - {1}", cr.Name, type.ToString()));
                    BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + type.ToString()), type, cr.Name, tob);
                    Bot.triggers.Add(trigger);
                }
            }
            else if(selected == "doormatTrigger")
            {
                DoormatOptionsWindow window = new DoormatOptionsWindow();
                ContentDialogResult result = await window.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    _do = window.DO;

                    type = (TriggerType)Enum.Parse(typeof(TriggerType), char.ToUpper(selected[0]) + selected.Substring(1));
                    addedTriggersListBox.Items.Add(string.Format("{0} - {1}", _do.Name, type.ToString()));

                    tob.DoormatOptions = _do;
                    tob.Name = _do.Name;
                    tob.Type = type;
                    BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + type.ToString()), type, _do.Name, tob);
                    Bot.triggers.Add(trigger);
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string chatbots = "";
            Log.box = outputBox;
            try
            {
                StorageFile file = await Bot.folder.GetFileAsync("chatbots.txt");
                chatbots = await FileIO.ReadTextAsync(file);
                if (chatbots.Length > 0)
                {
                    chatbotsList.ItemsSource = chatbots.Split('\n');
                }
            }
            catch (FileNotFoundException fnfe) { }
        }
    }
}
