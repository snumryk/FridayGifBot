// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Rest;

namespace FridayGifBot
{
    public class EmptyBot : ActivityHandler
    {
        private static readonly AzureBlobStorage _myStorage = 
            new AzureBlobStorage
            ("DefaultEndpointsProtocol=https;AccountName=fridaygifbotfilestorage;AccountKey=tgyilHQvZOsJCv8jqTtA6+Uu4tF15tbLHssjnNNky9qa0x4kKwxcWqvl2uqKLlkA+2kao6l30db2IkU7SVS0CQ==;EndpointSuffix=core.windows.net", "linkscontainer");
        /// <summary>
        /// For the one who will read this
        /// Im sorry ^-^
        /// </summary>
        private const string GifAmmountKey = "GifAmmount";
        private const string IsGifPostedTodayKey = "IsGifPostedToday";
        private const string SpamGifCommand = "BOT_SPAM_GIF";
        private const string AddNewGifCommand = "BOT_ADD_NEW_GIF";
        private const string ResetDateCommand = "BOT_RESET_FRIDAY_SPAMING_PROTOCOL";

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types

            await turnContext.SendActivityAsync(turnContext.Activity.Properties.AsFormattedString(),
                cancellationToken: cancellationToken);

            if (turnContext.Activity.Type == ActivityTypes.Message
                && turnContext.Activity.Text.Contains(AddNewGifCommand)
                && turnContext.Activity.Text.Contains(".gif")) //&& DateTime.Now.DayOfWeek == DayOfWeek.Friday
            {
                if (CheckIfGifsWhereNotPostedToday())
                {
                    try
                    {
                        if (SaveNewGifAddress(turnContext.Activity.Text))
                        {
                            await turnContext.SendActivityAsync("GIF SUCCESSFULLY ADDED TO THE DATABASE",
                                cancellationToken: cancellationToken);
                        }
                        else
                            await turnContext.SendActivityAsync("FAILED TO PROCESS GIF URL",
                                cancellationToken: cancellationToken);
                    }
                    catch (Exception e)
                    {
                        await turnContext.SendActivityAsync("FAILED TO ADD GIF" + e.Message,
                            cancellationToken: cancellationToken);
                        await turnContext.SendActivityAsync(e.ToString(),
                            cancellationToken: cancellationToken);
                        throw;
                    }
                }
                else
                {
                    await turnContext.SendActivityAsync(
                        "Gif adding is alowed only once per week. Use BOT_RESET_FRIDAY_SPAMING_PROTOCOL command to reset",
                        cancellationToken: cancellationToken);
                }

            }
            else if (turnContext.Activity.Type == ActivityTypes.Message
                     && turnContext.Activity.Text.Contains(SpamGifCommand))
            {
                if (CheckIfGifsWhereNotPostedToday())
                {
                    await turnContext.SendActivityAsync("SPAMMING PROTOCOL INITIATED",
                        cancellationToken: cancellationToken);
                    int gifLinksAmmount;
                    try
                    {
                        var readigTask = _myStorage.ReadAsync(new[] { GifAmmountKey });
                        readigTask.Wait();
                        gifLinksAmmount = Convert.ToInt32(readigTask.Result.FirstOrDefault().Value);
                        gifLinksAmmount++;
                        for (int i = 0; i < gifLinksAmmount; i++)
                        {
                            await SpamGif(i, turnContext, CancellationToken.None);
                        }
                    }
                    catch (Exception e)
                    {
                        await turnContext.SendActivityAsync("SPAMMING PROTOCOL FAILED" + e.Message,
                            cancellationToken: cancellationToken);
                        await turnContext.SendActivityAsync( e.ToString(),
                            cancellationToken: cancellationToken);
                        throw;
                    }

                }
                else
                {
                    await turnContext.SendActivityAsync(
                        "Gif adding is alowed only once per week. Use BOT_RESET_FRIDAY_SPAMING_PROTOCOL command to reset",
                        cancellationToken: cancellationToken);
                }
            }

            if (turnContext.Activity.Type == ActivityTypes.Message
                && turnContext.Activity.Text.Contains
                    ("))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))"))
            {
                await turnContext.SendActivityAsync("HAVE A NICE  DAY", cancellationToken: cancellationToken);
            }

            if (turnContext.Activity.Type == ActivityTypes.Message
                && turnContext.Activity.Text.Contains
                    (ResetDateCommand))
            {
                ResetDay();
                await turnContext.SendActivityAsync("SPAMING PROTOCOL RESET. TRY TO POST GIF AGAIN", cancellationToken: cancellationToken);
            }
        }

        private async Task SpamGif(int index, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            string gifIndex = Convert.ToString(index);
            var fetchGifFromAzure = _myStorage.ReadAsync(new[] { gifIndex });
            fetchGifFromAzure.Wait();
            string currentGifAdress = Convert.ToString(fetchGifFromAzure.Result.FirstOrDefault().Value);
            IMessageActivity reply = Activity.CreateMessageActivity();
            reply.Attachments.Add(new Attachment());
            reply.Attachments.FirstOrDefault().ContentUrl = currentGifAdress;
            reply.Attachments.FirstOrDefault().ContentType = "image/gif";
            reply.Attachments.FirstOrDefault().Name = gifIndex;
            await turnContext.SendActivityAsync(reply, cancellationToken: cancellationToken);
        }

        private bool SaveNewGifAddress(string message)
        {
                int firstStringPosition = message.IndexOf("http");
                int secondStringPosition = message.IndexOf(".gif");
                string newGifAdressToSave = message.Substring(firstStringPosition,
                    secondStringPosition - firstStringPosition + 4);

                var readingTask = _myStorage.ReadAsync(new[] {GifAmmountKey});
                readingTask.Wait();
                int gifCurrentNumber = Convert.ToInt32(readingTask.Result.FirstOrDefault().Value);
                gifCurrentNumber++;

                string blobObjectNumber = Convert.ToString(gifCurrentNumber);

                var dictionary = new Dictionary<string, object>();
                dictionary.Add(blobObjectNumber, (object)newGifAdressToSave);
                var savingTask = _myStorage.WriteAsync(dictionary);
                savingTask.Wait();

                var tempDictionary = new Dictionary<string, object>();
                tempDictionary.Add(GifAmmountKey, (object)gifCurrentNumber);
                var updateGifAmmount = _myStorage.WriteAsync(tempDictionary);
                updateGifAmmount.Wait();

                return true;
        }

        private bool CheckIfGifsWhereNotPostedToday()
        {
            //var readingTask = _myStorage.ReadAsync(new[] {IsGifPostedTodayKey});
            //readingTask.Wait();
            //var gifPostedDate = (DateTime)readingTask.Result.FirstOrDefault().Value;
            //if ( gifPostedDate.ToShortDateString() == DateTime.Today.ToShortDateString())
            //    return false;
            //{
            //    ResetDay();
            //    return true;
            //}
            return true;
        }

        private void ResetDay()
        {
            var deletingTask = _myStorage.DeleteAsync(new[] { IsGifPostedTodayKey });
            deletingTask.Wait();
            var newDateEntry = new Dictionary<string, object>();
            newDateEntry.Add(IsGifPostedTodayKey, (object)DateTime.Today.ToShortDateString());
            var newDateEntryTask = _myStorage.WriteAsync(newDateEntry);
            newDateEntryTask.Wait();
        }
    }
}

