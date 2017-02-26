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
        public static DiscordSocketClient client { get; set; }
        private static string token;
        public static List<BaseTrigger> triggers = new List<BaseTrigger>();
        public static string username { get; set; }
        public static StorageFolder folder = ApplicationData.Current.LocalFolder;
        public static StorageFolder userDir;
        public static StorageFolder triggerDir;

        public static async Task StartAsync(string u, string t)
        {
            client = new DiscordSocketClient();
            token = t;
            username = u;
            
            triggerDir = await userDir.CreateFolderAsync("triggers", CreationCollisionOption.OpenIfExists);

            Log.CreateInstance(username + "_log.txt", username, Log.LogLevel.Silly, Log.LogLevel.Silly);

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
            client.MessageReceived += async (message) =>
            {
                Log.Instance.Info("Message from {0} in {1}: {2}", message.Author.Username, message.Channel.Name, message.Content);
                foreach (BaseTrigger trigger in triggers)
                {
                    await trigger.OnChatMessage(message.Channel.Id, message.Author.Id, message.Content, true);
                }
            };

            client.UserJoined += async (user) =>
            {
                Log.Instance.Info("User {0} joined channel {1}", user.Username, user.Guild.Name);
                foreach (BaseTrigger trigger in triggers)
                {
                    await trigger.OnEnteredChat(user.Guild.Id, user.Id, true);
                }
            };

            await WriteData();
            await SaveTriggers();
            Log.Instance.Verbose("Starting bot...");

            await client.LoginAsync(TokenType.Bot, token);
            await client.ConnectAsync();
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
                Username = username,
                Token = token
            };

            string json = JsonConvert.SerializeObject(info, Formatting.Indented);
            StorageFile loginJSon = await userDir.CreateFileAsync("login.json", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(loginJSon, json);

            StorageFile chatbots = await folder.CreateFileAsync("chatbots.txt", CreationCollisionOption.OpenIfExists);

            string chatbotsTxt = await FileIO.ReadTextAsync(chatbots);

            if (!chatbotsTxt.Contains(username))
            {
                await FileIO.AppendTextAsync(chatbots, username + '\n');
            }
        }

        public static async Task SaveTriggers()
        {
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
                if (triggers.Count > 0)
                {
                    Log.Instance.Verbose("Saving triggers...");
                    int count = triggers.Count;
                    foreach (BaseTrigger trigger in triggers)
                    {
                        Log.Instance.Debug("Saving triggers, " + count + " left");
                        await trigger.SaveTrigger();
                        Log.Instance.Silly("Trigger {0}/{1} saved", trigger.Name, trigger.Type.ToString());
                        count--;
                    }
                    Log.Instance.Verbose("Successfully read trigger data from triggers window");
                }
            }

            foreach(BaseTrigger trigger in triggers)
            {
                trigger.OnLoad();
            }
        }
    }
}
