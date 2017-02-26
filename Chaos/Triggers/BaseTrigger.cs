using Chaos.Triggers.TriggerOptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Threading;
using Windows.Storage;

namespace Chaos.Triggers
{
    public class BaseTrigger
    {
        public TriggerType Type { get; set; }
        public string Name { get; set; }
        public TriggerOptionsBase Options { get; set; }

        public bool ReplyEnabled = true;

        public BaseTrigger(TriggerType type, string name, TriggerOptionsBase options)
        {
            Type = type;
            Name = name;
            Options = options;
        }

        /// <summary>
        /// If there is an error, log it easily
        /// </summary>
        /// <param name="cbn"></param>
        /// <param name="name"></param>
        /// <param name="error"></param>
        /// <returns>error string</returns>
        protected string IfError(string cbn, string name, Exception error)
        {
            return string.Format("{0}/{1}: {2}: {3}", cbn, name, error.Message, error.StackTrace);
        }

        #region trigger read-write

        /// <summary>
        /// Save current trigger to file
        /// </summary>
        public async Task SaveTrigger()
        {
            if (Options != null)
            {
                TriggerOptionsBase options = new TriggerOptionsBase
                {
                    Name = Options.Name,
                    Type = Options.Type,
                    ChatCommand = Options.ChatCommand
                };
                string json = JsonConvert.SerializeObject(options, Formatting.Indented);
                StorageFile file = await Bot.triggerDir.CreateFileAsync(Name + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, json);
            }
            else if (Options == null)
            {
                TriggerOptionsBase options = new TriggerOptionsBase();
                string json = JsonConvert.SerializeObject(options, Formatting.Indented);
                StorageFile file = await Bot.triggerDir.CreateFileAsync(Name + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, json);
            }
        }

        /// <summary>
        /// Read triggers from username/triggers/
        /// </summary>
        /// <returns>A list of BaseTrigger objects</returns>
        public static async Task<List<BaseTrigger>> ReadTriggers()
        {
            List<BaseTrigger> temp = new List<BaseTrigger>();
            IReadOnlyList<StorageFile> files = await Bot.triggerDir.GetFilesAsync();
            foreach (StorageFile file in files)
            {
                /*
                int start = file.IndexOf("triggers/") + "triggers/".Length;
                int end = file.IndexOf(".", start);
                string _file = file.Substring(start, end - start);
                */

                string contents = await FileIO.ReadTextAsync(file);
                TriggerOptionsBase options = JsonConvert.DeserializeObject<TriggerOptionsBase>(contents);
                TriggerType type = options.Type;
                string name = options.Name;

                switch (type)
                {
                    case TriggerType.KickTrigger:
                        temp.Add(new KickTrigger(type, name, options));
                        break;
                    case TriggerType.DoormatTrigger:
                        temp.Add(new DoormatTrigger(type, name, options));
                        break;
                    default:
                        break;
                }
            }
            return temp;
        }

        #endregion

        #region overriden methods
        /// <summary>
        /// Return true if trigger loads properly
        /// </summary>
        /// <returns></returns>
        public virtual bool OnLoad()
        {
            try
            {
                bool ret = onLoad();
                if (!ret)
                {
                    Log.Instance.Error("{0}/{1}: Error loading trigger {2}: OnLoad returned {3}", Bot.username, Name, Name, ret);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e));
                return false;
            }
        }
        
        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="chatterID"></param>
        /// <param name="message"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual async Task<bool> OnChatMessage(ulong roomID, ulong chatterID, string message, bool haveSentMessage)
        {
            try
            {
                bool messageSent = await respondToChatMessage(roomID, chatterID, message);
                if (messageSent)
                {
                    Log.Instance.Silly("{0}/{1}: Sent RespondToChatMessage - {2} - {3} - {4}", Bot.username, Name, chatterID, roomID, message);
                }
                return messageSent;
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e));
                return false;
            }
        }

        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="userID"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual async Task<bool> OnEnteredChat(ulong roomID, ulong userID, bool haveSentMessage)
        {
            try
            {
                bool messageSent = await respondToEnteredMessage(roomID, userID);
                if (messageSent)
                {
                    Log.Instance.Silly("{0}/{1}: Sent RespondToEnteredMessage - {2} - {3} - {4}", Bot.username, Name, userID, roomID);
                }
                return messageSent;
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e));
                return false;
            }
        }

        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="kickedID"></param>
        /// <param name="kickerID"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnKickedChat(ulong roomID, ulong kickedID, ulong kickerID, bool haveSentMessage)
        {
            try
            {
                return respondToKick(roomID, kickedID, kickerID);
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e));
                return false;
            }
        }

        /// <summary>
        /// Returns true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="bannedID"></param>
        /// <param name="bannerID"></param>
        /// <param name="haveSentMessage"></param>
        /// <returns></returns>
        public virtual bool OnBannedChat(ulong roomID, ulong bannedID, ulong bannerID, bool haveSentMessage)
        {
            try
            {
                return respondToBan(roomID, bannedID, bannerID);
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e));
                return false;
            }
        }
        
        /// <summary>
        /// Return true if a message was sent
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public virtual bool OnLeftChat(ulong roomID, ulong userID)
        {
            try
            {
                return respondToLeftMessage(roomID, userID);
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(Bot.username, Name, e));
                return false;
            }
        }
        #endregion

        #region subclass methods

        public virtual bool onLoad()
        {
            return true;
        }

        // Return true if a message was sent
        public virtual async Task<bool> respondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual async Task<bool> respondToEnteredMessage(ulong roomID, ulong userID)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool respondToBan(ulong roomID, ulong bannedId, ulong bannerId)
        {
            return false;
        }
        
        // Return true if the event was eaten
        public virtual bool respondToLeftMessage(ulong roomID, ulong userID)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool respondToKick(ulong roomID, ulong kickedId, ulong kickerId)
        {
            return false;
        }
        
        #endregion

        #region helper methods

        /// <summary>
        /// Sends a message to the specified ulong
        /// </summary>
        /// <param name="ulong"></param>
        /// <param name="message"></param>
        /// <param name="room"></param>
        protected async Task SendMessageAfterDelay(ulong toID, string message)
        {
            Log.Instance.Debug("{0}/{1}: Sending nondelayed message to {2}: {3}", Bot.username, Name, toID, message);
            var channel = Bot.client.GetChannel(toID) as ISocketMessageChannel;
            await channel?.SendMessageAsync(message);
        }

        /// <summary>
        /// Splits the message and returns an array of words
        /// </summary>
        /// <param name="message"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        protected string[] StripCommand(string message, string command)
        {
            if (message != null && command != null && message.ToLower().IndexOf(command.ToLower()) == 0)
            {
                return message.Split(' ');
            }
            return null;
        }

        #endregion
    }
}
