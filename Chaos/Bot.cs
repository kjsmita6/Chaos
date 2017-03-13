using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;

using Discord;
using Discord.WebSocket;
using Chaos.Triggers;
using Windows.System.Threading;
using System.IO;
using Newtonsoft.Json;
using Windows.UI.Popups;

namespace Chaos
{
    class Bot
    {
        public static DiscordSocketClient Client { get; set; }
        private static string Token { get; set; }
        public static List<BaseTrigger> triggers = new List<BaseTrigger>();
        public static string Username { get; set; }
        public static StorageFolder folder = ApplicationData.Current.LocalFolder;
        public static StorageFolder userDir;
        public static StorageFolder triggerDir;
        public static string Game { get; set; }

        public static async Task StartAsync(string u, string t, string g = null)
        {
            Client = new DiscordSocketClient();
            Token = t;
            Username = u;
            Game = g;
            
            triggerDir = await userDir.CreateFolderAsync("triggers", CreationCollisionOption.OpenIfExists);

            Log.CreateInstance(Username + "_log.txt", Username, Log.LogLevel.Silly, Log.LogLevel.Silly);

            try
            {
                await StartBot();
            }
            catch(Exception e)
            {
                Log.Instance.Error(e.Message + ": " + e.StackTrace);
                return;
            }
        }

        private static async Task StartBot()
        {
            Client.MessageReceived += async (message) =>
            {
                if (message.Author.Id != Client.CurrentUser.Id)
                {
                    Log.Instance.Info("Message from {0} in {1}: {2}", message.Author.Username, message.Channel.Name, message.Content);
                    foreach (BaseTrigger trigger in triggers)
                    {
                        await trigger.OnChatMessage(message.Channel.Id, message.Author.Id, message.Content, true);
                    }
                }
            };

            Client.UserJoined += async (user) =>
            {
                if (user.Id != Client.CurrentUser.Id)
                {
                    Log.Instance.Info("User {0} joined channel {1}", user.Username, user.Guild.Name);
                    foreach (BaseTrigger trigger in triggers)
                    {
                        await trigger.OnEnteredChat(user.Guild.Id, user.Id, true);
                    }
                }
            };

            await WriteData();
            await SaveTriggers();
            Log.Instance.Verbose("Starting bot...");

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.ConnectAsync();
            if(Game != null)
            {
                await Client.SetGameAsync(Game);
            }
            await Task.Delay(-1);
        }

        public static async Task<UserInfo> ReadData(string username)
        {
            var file = await userDir.GetFileAsync("login.json");
            string readFile = await FileIO.ReadTextAsync(file);
            return JsonConvert.DeserializeObject<UserInfo>(readFile);
        }

        public static async Task WriteData()
        {
            UserInfo info = new UserInfo
            {
                Username = Username,
                Token = Token,
                Game = Game
            };

            string json = JsonConvert.SerializeObject(info, Formatting.Indented);
            StorageFile loginJSon = await userDir.CreateFileAsync("login.json", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(loginJSon, json);

            StorageFile chatbots = await folder.CreateFileAsync("chatbots.txt", CreationCollisionOption.OpenIfExists);

            string chatbotsTxt = await FileIO.ReadTextAsync(chatbots);

            if (!chatbotsTxt.Contains(Username))
            {
                await FileIO.AppendTextAsync(chatbots, Username + '\n');
            }
        }

        public static async Task SaveTriggers()
        {
            /*
            IReadOnlyList<StorageFile> files = await triggerDir.GetFilesAsync();
            if (files.Count > 0)
            {
                if (triggers.Count > 0)
                {
                    List<BaseTrigger> oldTriggers = await BaseTrigger.ReadTriggers();
                    List<BaseTrigger> newTriggers = triggers;

                    Log.Instance.Verbose("Saving triggers...");
                    int count = triggers.Count;
                    foreach (BaseTrigger trigger in newTriggers)
                    {
                        Log.Instance.Debug("Saving triggers, " + count + " left");
                        await trigger.SaveTrigger();
                        Log.Instance.Silly("Trigger {0}/{1} saved", trigger.Name, trigger.Type.ToString());
                        count--;
                    }
                    Log.Instance.Verbose("Successfully read trigger data from " + username + "/triggers/ and from triggers tab");

                    triggers = oldTriggers.Concat(newTriggers).ToList();
                }
                else
                {
                    Log.Instance.Verbose("Loading triggers...");
                    triggers = await BaseTrigger.ReadTriggers();
                    Log.Instance.Verbose("Successfully read trigger data from " + username + "/triggers/");
                }
            }
            else
            {
            */
            if (triggers.Count > 0)
            {
                Log.Instance.Verbose("Saving triggers...");
                int count = triggers.Count;
                IReadOnlyList<StorageFile> files = await triggerDir.GetFilesAsync();
                foreach (StorageFile f in files)
                {
                    await f.DeleteAsync();
                }
                foreach (BaseTrigger trigger in triggers)
                {
                    Log.Instance.Debug("Saving triggers, " + count + " left");
                    await trigger.SaveTrigger();
                    Log.Instance.Silly("Trigger {0}/{1} saved", trigger.Name, trigger.Type.ToString());
                    count--;
                }
                Log.Instance.Verbose("Successfully added triggers");
            }
            //}

            foreach(BaseTrigger trigger in triggers)
            {
                trigger.OnLoad();
            }
        }
    }
}
