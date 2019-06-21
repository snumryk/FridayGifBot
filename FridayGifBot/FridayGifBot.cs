// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Azure;

namespace FridayGifBot
{
    public class EmptyBot : ActivityHandler
    {
        private static readonly AzureBlobStorage _myStorage = 
            new AzureBlobStorage
            ("DefaultEndpointsProtocol=https;AccountName=fridaygifbotfilestorage;AccountKey=tgyilHQvZOsJCv8jqTtA6+Uu4tF15tbLHssjnNNky9qa0x4kKwxcWqvl2uqKLlkA+2kao6l30db2IkU7SVS0CQ==;EndpointSuffix=core.windows.net", "linkscontainer");
        /// <summary>
        /// TEMP, UDOLI
        /// </summary>
        private static int _gifAmmount = 139;

        private static bool _gifPostedToday = false;
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types

            if (turnContext.Activity.Type == ActivityTypes.Message
                && turnContext.Activity.Text.Contains("BOT_INITIATE_FRIDAY_SPAMING_PROTOCOL")
                && turnContext.Activity.Text.Contains(".gif")) //&& DateTime.Now.DayOfWeek == DayOfWeek.Friday
            {
                if (CheckIfGifsWhereNotPostedToday())
                {
                    try
                    {
                        if (SaveNewGifAddress(turnContext.Activity.Text))
                        {
                            await turnContext.SendActivityAsync("SPAMMING PROTOCOL INITIATED",
                                cancellationToken: cancellationToken);
                            //Dictionary<string, object> gifLinks = _myStorage.ReadAsync();
                            //for (int i = 0; i < gifLinks.Length; i++)
                            //{
                            //    var reply = turnContext.Activity.CreateReply();
                            //    reply.Attachments.Add(new Attachment("image/gif", gifLinks[i]));
                            //    await turnContext.SendActivityAsync(reply, cancellationToken);
                            //}
                        }
                        else
                            await turnContext.SendActivityAsync("FAILED TO PROCESS GIF URL",
                                cancellationToken: cancellationToken);
                    }
                    catch (Exception e)
                    {
                        await turnContext.SendActivityAsync("SPAMMING PROTOCOL FAILED" + e.Message,
                            cancellationToken: cancellationToken);
                        await turnContext.SendActivityAsync(e.ToString(),
                            cancellationToken: cancellationToken);
                        throw;
                    }
                }
                else
                {
                    await turnContext.SendActivityAsync(
                        "Gif spamming is alowed only once per week. Use BOT_RESET_FRIDAY_SPAMING_PROTOCOL command to reset",
                        cancellationToken: cancellationToken);
                }

            }
            else if (turnContext.Activity.Type == ActivityTypes.Message
                     && turnContext.Activity.Text.Contains("BOT_INITIATE_FRIDAY_SPAMING_PROTOCOL")
                     && !turnContext.Activity.Text.Contains(".gif"))
            {
                if (turnContext.Activity.Text.Contains("ALL_CURRENT_GIFS"))
                {
                    await turnContext.SendActivityAsync("SPAMMING PROTOCOL INITIATED",
                        cancellationToken: cancellationToken);
                    string[] gifLinks;
                    try
                    {
                        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            @"All_gifs.txt");

                        gifLinks = File.ReadAllLines(path);
                    }
                    catch (Exception e)
                    {
                        await turnContext.SendActivityAsync("SPAMMING PROTOCOL FAILED" + e.Message,
                            cancellationToken: cancellationToken);
                        await turnContext.SendActivityAsync( e.ToString(),
                            cancellationToken: cancellationToken);
                        throw;
                    }
                    for (int i = 0; i < gifLinks.Length; i++)
                    {
                        var reply = turnContext.Activity.CreateReply();
                        reply.Attachments.Add(new Attachment("image/gif", gifLinks[i]));
                        await turnContext.SendActivityAsync(reply, cancellationToken);
                    }
                }
                else
                {
                    await turnContext.SendActivityAsync
                        ("Please provide valid url for the .gif", cancellationToken: cancellationToken);
                }
            }

            else if (turnContext.Activity.Type == ActivityTypes.Message
                     && turnContext.Activity.Text.Contains("BOT_INITIATE_FRIDAY_SPAMING_PROTOCOL")
                     && turnContext.Activity.Text.Contains(".gif") && DateTime.Now.DayOfWeek != DayOfWeek.Friday)
            {
                await turnContext.SendActivityAsync("WRONG DAY FOR GIFs", cancellationToken: cancellationToken);
            }

            if (turnContext.Activity.Type == ActivityTypes.Message
                && turnContext.Activity.Text.Contains
                    ("))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))"))
            {
                await turnContext.SendActivityAsync("HAVE A NICE  DAY", cancellationToken: cancellationToken);
            }

            if (turnContext.Activity.Type == ActivityTypes.Message
                && turnContext.Activity.Text.Contains
                    ("BOT_RESET_FRIDAY_SPAMING_PROTOCOL"))
            {
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    @"CheckIfGifsWhereNotPostedToday.txt"), String.Empty);
                await turnContext.SendActivityAsync("SPAMING PROTOCOL RESET. TRY TO POST GIF AGAIN", cancellationToken: cancellationToken);
            }
        }

        private bool SaveNewGifAddress(string message)
        {
            try
            {
                int firstStringPosition = message.IndexOf("http");
                int secondStringPosition = message.IndexOf(".gif");
                string newGifAdressToSave = message.Substring(firstStringPosition,
                    secondStringPosition - firstStringPosition + 4);
                ///TEMP!!!
                var temp = new Dictionary<string, object>();
                temp.Add("GifAmmount", (object)_gifAmmount);
                _myStorage.WriteAsync(temp);
                var anotherTemp = new Dictionary<string, object>();
                anotherTemp.Add("GifPostedToday", (object)_gifPostedToday);
                _myStorage.WriteAsync(anotherTemp);
                ///TEMP!!!
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CheckIfGifsWhereNotPostedToday()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"CheckIfGifsWhereNotPostedToday.txt");
            if (File.ReadAllText(path) == DateTime.Today.ToShortDateString())
                return false;
            else
            {
                File.WriteAllText(path, DateTime.Today.ToShortDateString());
                return true;
            }
        }
    }
}

