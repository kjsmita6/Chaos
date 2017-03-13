using Chaos.Triggers;
using Chaos.Triggers.TriggerOptions;
using Chaos.Triggers.TriggerOptions.OptionsWindows;
using Newtonsoft.Json;
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
            if (usernameBox.Text == "" && tokenBox.Text == "" && chatbotsList.SelectedIndex != -1 && chatbotsList.SelectedValue.ToString() != "")
            {
                string username = chatbotsList.SelectedValue.ToString();
                Bot.userDir = await Bot.folder.CreateFolderAsync(username, CreationCollisionOption.OpenIfExists);

                if (Bot.userDir.GetFileAsync("login.json") != null && tokenBox.Text == "")
                {
                    var data = await Bot.ReadData(username);
                    startButton.Visibility = Visibility.Collapsed;
                    pivot.SelectedIndex = 2;
                    await Bot.StartAsync(username, data.Token, data.Game);
                    Log.Instance.Silly("Successfully read login data from file.");
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
                    await Bot.StartAsync(usernameBox.Text, tokenBox.Text, gameBox.Text);
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

            switch (selected)
            {
                case "kickTrigger":
                case "banTrigger":
                case "isUpTrigger":
                case "playGameTrigger":
                    {
                        ChatCommandWindow window = new ChatCommandWindow();
                        ContentDialogResult result = await window.ShowAsync();
                        if (result == ContentDialogResult.Primary && window.CC != null)
                        {
                            MainOptionsWindow mow = new MainOptionsWindow();
                            ContentDialogResult r1 = await mow.ShowAsync();
                            if (r1 == ContentDialogResult.Primary && mow.MO != null)
                            {
                                cc = window.CC;

                                type = (TriggerType)Enum.Parse(typeof(TriggerType), char.ToUpper(selected[0]) + selected.Substring(1));
                                addedTriggersListBox.Items.Add(string.Format("{0} - {1}", cc.Name, type.ToString()));

                                tob.ChatCommand = cc;
                                tob.Name = cc.Name;
                                tob.Type = type;
                                tob.MainOptions = mow.MO;
                                BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + type.ToString()), type, cc.Name, tob);
                                Bot.triggers.Add(trigger);
                            }
                        }
                    }
                    break;
                case "chatReplyTrigger":
                    {
                        ChatReplyWindow window = new ChatReplyWindow();
                        ContentDialogResult result = await window.ShowAsync();
                        if (result == ContentDialogResult.Primary && window.CR != null)
                        {
                            MainOptionsWindow mow = new MainOptionsWindow();
                            ContentDialogResult r1 = await mow.ShowAsync();
                            if (r1 == ContentDialogResult.Primary && mow.MO != null)
                            {
                                cr = window.CR;
                                type = (TriggerType)Enum.Parse(typeof(TriggerType), char.ToUpper(selected[0]) + selected.Substring(1));

                                tob.ChatReply = cr;
                                tob.Name = cr.Name;
                                tob.Type = type;
                                tob.MainOptions = mow.MO;
                                addedTriggersListBox.Items.Add(string.Format("{0} - {1}", cr.Name, type.ToString()));
                                BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + type.ToString()), type, cr.Name, tob);
                                Bot.triggers.Add(trigger);
                            }
                        }
                    }
                    break;
                case "doormatTrigger":
                    {
                        DoormatOptionsWindow window = new DoormatOptionsWindow();
                        ContentDialogResult result = await window.ShowAsync();
                        if (result == ContentDialogResult.Primary && window.DO != null)
                        {
                            MainOptionsWindow mow = new MainOptionsWindow();
                            ContentDialogResult r1 = await mow.ShowAsync();
                            if (r1 == ContentDialogResult.Primary && mow.MO != null)
                            {
                                _do = window.DO;

                                type = (TriggerType)Enum.Parse(typeof(TriggerType), char.ToUpper(selected[0]) + selected.Substring(1));
                                addedTriggersListBox.Items.Add(string.Format("{0} - {1}", _do.Name, type.ToString()));

                                tob.DoormatOptions = _do;
                                tob.Name = _do.Name;
                                tob.Type = type;
                                tob.MainOptions = mow.MO;
                                BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + type.ToString()), type, _do.Name, tob);
                                Bot.triggers.Add(trigger);
                            }
                        }
                    }
                    break;
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

        private async void removeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedString = ((string)addedTriggersListBox.SelectedValue);
                addedTriggersListBox.Items.Remove(addedTriggersListBox.SelectedValue);
                IEnumerable<BaseTrigger> triggers = Bot.triggers.Where(x => x.Name == selectedString.Substring(0, selectedString.IndexOf(" -")));
                for (int i = 0; i < triggers.Count(); i++)
                {
                    Bot.triggers.Remove(triggers.ElementAt(i));
                }
            }
            catch (Exception err) { }
        }

        private async void chatbotsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            addedTriggersListBox.Items.Clear();
            if (e.AddedItems[0].ToString() != "" && e.AddedItems[0].ToString() != null)
            {
                Bot.userDir = await Bot.folder.GetFolderAsync(e.AddedItems[0].ToString());
                Bot.triggerDir = await Bot.userDir.GetFolderAsync("triggers");
                IReadOnlyList<StorageFile> files = await Bot.triggerDir.GetFilesAsync();
                foreach (StorageFile file in files)
                {
                    string text = await FileIO.ReadTextAsync(file);
                    TriggerOptionsBase options = JsonConvert.DeserializeObject<TriggerOptionsBase>(text);
                    addedTriggersListBox.Items.Add(options.Name + " - " + options.Type.ToString());
                    BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + options.Type.ToString()), options.Type, options.Name, options);
                    Bot.triggers.Add(trigger);
                }
            }
        }

        private async void editButton_Click(object sender, RoutedEventArgs e)
        {
            if(addedTriggersListBox.SelectedIndex == -1)
            {
                MessageDialog dialog = new MessageDialog("You must select a trigger to edit from the box to the right.", "Error");
                await dialog.ShowAsync();
            }
            else
            {
                BaseTrigger trigger = Bot.triggers[addedTriggersListBox.SelectedIndex];
                switch(trigger.Type)
                {
                    case TriggerType.ChatReplyTrigger:
                        {
                            ChatReplyWindow crw = new ChatReplyWindow(trigger.Options.ChatReply);
                            ContentDialogResult result = await crw.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                MainOptionsWindow mow = new MainOptionsWindow(trigger.Options.MainOptions);
                                ContentDialogResult result1 = await mow.ShowAsync();
                                if (result1 == ContentDialogResult.Primary)
                                {
                                    TriggerOptionsBase options = new TriggerOptionsBase()
                                    {
                                        Name = crw.CR.Name,
                                        Type = TriggerType.ChatReplyTrigger,
                                        ChatReply = crw.CR,
                                        MainOptions = mow.MO
                                    };
                                    addedTriggersListBox.Items[addedTriggersListBox.SelectedIndex] = options.Name + " - " + options.Type.ToString();
                                    StorageFile oldFile = await Bot.triggerDir.GetFileAsync(trigger.Name + ".json");
                                    await oldFile.DeleteAsync();
                                    BaseTrigger newTrigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + options.Type.ToString()), options.Type, options.Name, options);
                                    await newTrigger.SaveTrigger();
                                    Bot.triggers.Remove(trigger);
                                    trigger = null;
                                    Bot.triggers.Add(newTrigger);
                                }
                            }
                        }
                        break;
                    case TriggerType.DoormatTrigger:
                        {
                            DoormatOptionsWindow dow = new DoormatOptionsWindow(trigger.Options.DoormatOptions);
                            ContentDialogResult result = await dow.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                MainOptionsWindow mow = new MainOptionsWindow(trigger.Options.MainOptions);
                                ContentDialogResult result1 = await mow.ShowAsync();
                                if (result1 == ContentDialogResult.Primary)
                                {
                                    TriggerOptionsBase options = new TriggerOptionsBase()
                                    {
                                        Name = dow.DO.Name,
                                        Type = TriggerType.DoormatTrigger,
                                        DoormatOptions = dow.DO,
                                        MainOptions = mow.MO
                                    };
                                    addedTriggersListBox.Items[addedTriggersListBox.SelectedIndex] = options.Name + " - " + options.Type.ToString();
                                    StorageFile oldFile = await Bot.triggerDir.GetFileAsync(trigger.Name + ".json");
                                    await oldFile.DeleteAsync();
                                    BaseTrigger newTrigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + options.Type.ToString()), options.Type, options.Name, options);
                                    await newTrigger.SaveTrigger();
                                    Bot.triggers.Remove(trigger);
                                    trigger = null;
                                    Bot.triggers.Add(newTrigger);
                                }
                            }
                        }
                        break;
                    case TriggerType.IsUpTrigger:
                    case TriggerType.KickTrigger:
                    case TriggerType.PlayGameTrigger:
                        {
                            ChatCommandWindow ccw = new ChatCommandWindow(trigger.Options.ChatCommand);
                            ContentDialogResult result = await ccw.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                MainOptionsWindow mow = new MainOptionsWindow(trigger.Options.MainOptions);
                                ContentDialogResult result1 = await mow.ShowAsync();
                                if (result1 == ContentDialogResult.Primary)
                                {
                                    TriggerOptionsBase options = new TriggerOptionsBase()
                                    {
                                        Name = ccw.CC.Name,
                                        Type = trigger.Type,
                                        ChatCommand = ccw.CC,
                                        MainOptions = mow.MO
                                    };
                                    addedTriggersListBox.Items[addedTriggersListBox.SelectedIndex] = options.Name + " - " + options.Type.ToString();
                                    StorageFile oldFile = await Bot.triggerDir.GetFileAsync(trigger.Name + ".json");
                                    await oldFile.DeleteAsync();
                                    BaseTrigger newTrigger = (BaseTrigger)Activator.CreateInstance(Type.GetType("Chaos.Triggers." + options.Type.ToString()), options.Type, options.Name, options);
                                    await newTrigger.SaveTrigger();
                                    Bot.triggers.Remove(trigger);
                                    trigger = null;
                                    Bot.triggers.Add(newTrigger);
                                }
                            }
                        }
                        break;
                }
            }
        }

        /* // For deleting bots
        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.PrimaryButtonText = "Yes";
            dialog.SecondaryButtonText = "No";
            dialog.Content = "Are you sure you want to delete this bot? This action cannot be undone.";
            ContentDialogResult result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                StorageFolder folder = await Bot.folder.GetFolderAsync(chatbotsList.SelectedValue.ToString());
                await folder.DeleteAsync();
                chatbotsList.Items.Remove();
            }
        }
        */
    }
}
