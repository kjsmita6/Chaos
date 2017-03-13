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
using Windows.UI.Popups;

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
        /// <param name="error"></param>
        /// <returns>error string</returns>
        protected string IfError(Exception error)
        {
            return string.Format("{0}/{1}: {2}: {3}", Bot.Username, Name, error.Message, error.StackTrace);
        }

        #region trigger read-write

        /// <summary>
        /// Save current trigger to file
        /// </summary>
        public async Task SaveTrigger()
        {
            if (Options != null)
            {
                string json = JsonConvert.SerializeObject(Options, Formatting.Indented);
                StorageFile file = await Bot.triggerDir.CreateFileAsync(Name + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, json);
            }
        }

        /*
        /// <summary>
        /// Read triggers from username/triggers/
        /// </summary>
        /// <returns>A list of BaseTrigger objects</returns>
        public static async Task<List<BaseTrigger>> ReadTriggers()
        {
            List<BaseTrigger> temp = new List<BaseTrigger>();
            IReadOnlyList<StorageFile> files = await Bot.triggerDir.GetFilesAsync();
            foreach (StorageFile file in files)
            {string contents = await FileIO.ReadTextAsync(file);
                TriggerOptionsBase options = JsonConvert.DeserializeObject<TriggerOptionsBase>(contents);
                TriggerType type = options.Type;
                string name = options.Name;

                BaseTrigger trigger = (BaseTrigger)Activator.CreateInstance(System.Type.GetType("Chaos.Triggers." + type.ToString()), type, name, options);
                temp.Add(trigger);

                
                VVVVVV Comment this out later VVVVVV
                switch (type)
                {
                    case TriggerType.KickTrigger:
                        temp.Add(new KickTrigger(type, name, options));
                        break;
                    case TriggerType.DoormatTrigger:
                        temp.Add(new DoormatTrigger(type, name, options));
                        break;
                    case TriggerType.ChatReplyTrigger:
                        temp.Add(new ChatReplyTrigger(type, name, options));
                        break;
                    case TriggerType.IsUpTrigger:
                        temp.Add(new IsUpTrigger(type, name, options));
                        break;
                    case TriggerType.PlayGameTrigger:
                        temp.Add(new PlayGameTrigger(type, name, options));
                        break;
                    default:
                        break;
                }
                

            }
            return temp;
        }
        */

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
                    Log.Instance.Error("{0}/{1}: Error loading trigger {2}: OnLoad returned {3}", Bot.Username, Name, Name, ret);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(e));
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
            if (ReplyEnabled && CheckUser(chatterID) && !CheckIgnores(chatterID))
            {
                try
                {
                    bool messageSent = await RespondToChatMessage(roomID, chatterID, message);
                    if (messageSent)
                    {
                        Log.Instance.Silly("{0}/{1}: Sent RespondToChatMessage - {2} - {3} - {4}", Bot.Username, Name, chatterID, roomID, message);
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(e));
                    return false;
                }
            }
            return false;
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
            if (ReplyEnabled && CheckUser(userID) && !CheckIgnores(userID))
            {
                try
                {
                    bool messageSent = await RespondToEnteredMessage(roomID, userID);
                    if (messageSent)
                    {
                        Log.Instance.Silly("{0}/{1}: Sent RespondToEnteredMessage - {2} - {3} - {4}", Bot.Username, Name, userID, roomID);
                        DisableForTimeout();
                    }
                    return messageSent;
                }
                catch (Exception e)
                {
                    Log.Instance.Error(IfError(e));
                    return false;
                }
            }
            return false;
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
                return RespondToKick(roomID, kickedID, kickerID);
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(e));
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
                return RespondToBan(roomID, bannedID, bannerID);
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(e));
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
                return RespondToLeftMessage(roomID, userID);
            }
            catch (Exception e)
            {
                Log.Instance.Error(IfError(e));
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
        public virtual async Task<bool> RespondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual async Task<bool> RespondToEnteredMessage(ulong roomID, ulong userID)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool RespondToBan(ulong roomID, ulong bannedId, ulong bannerId)
        {
            return false;
        }
        
        // Return true if the event was eaten
        public virtual bool RespondToLeftMessage(ulong roomID, ulong userID)
        {
            return false;
        }

        // Return true if the event was eaten
        public virtual bool RespondToKick(ulong roomID, ulong kickedId, ulong kickerId)
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
            Log.Instance.Debug("{0}/{1}: Sending nondelayed message to {2}: {3}", Bot.Username, Name, toID, message);
            var channel = Bot.Client.GetChannel(toID) as ISocketMessageChannel;
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

        /// <summary>
        /// Check if a user is ignored
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if user is ignored, false otherwise</returns>
        protected bool CheckIgnores(ulong id)
        {
            if(Options.MainOptions.Ignores != null && Options.MainOptions.Ignores.Count > 0)
            {
                return CheckList(Options.MainOptions.Ignores, id);
            }
            return false;
        }

        /// <summary>
        /// Check if a user is allowed to use this trigger
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if allowed, false otherwise</returns>
        protected bool CheckUser(ulong id)
        {
            if(Options.MainOptions.Users != null && Options.MainOptions.Users.Count > 0)
            {
                return CheckList(Options.MainOptions.Users, id);
            }
            return true;
        }

        /// <summary>
        /// Returns if an item is found in a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns>True if found, false otherwise</returns>
        protected bool CheckList(List<ulong> list, ulong item)
        {
            foreach(ulong i in list)
            {
                if(i == item)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Disables a trigger for a certain duration
        /// </summary>
        protected void DisableForTimeout()
        {
            int to = Options.MainOptions.Timeout;
            if (to > 0)
            {
                ReplyEnabled = false;
                Log.Instance.Silly("{0}/{1}: Setting timeout ({2} ms)", Bot.Username, Name, to);
                Timer timer = new Timer(delegate
                {
                    Log.Instance.Silly("{0}/{1}: Timeout expired", Bot.Username, Name);
                    ReplyEnabled = true;
                }, null, to, Timeout.Infinite);
            }
        }



        #endregion
    }
}
