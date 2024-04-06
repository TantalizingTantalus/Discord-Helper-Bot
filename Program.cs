using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging.Abstractions;
using System.Xml.Linq;
using System.Diagnostics;
using System.Net;
using OpenAi_Assistant;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Threads;
using OpenAI.Chat;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using DiscordHelperBot;



namespace DiscordHelperBot
{

    /*
     

                                                                     ____. _________________    _______   
                                                                    |    |/   _____/\_____  \   \      \  
                                                                    |    |\_____  \  /   |   \  /   |   \ 
                                                                /\__|    |/        \/    |    \/    |    \
                                                                \________/_______  /\_______  /\____|__  /
                                                                                 \/         \/         \/ 


     */

    public class InsultMeBlob
    {
        public string insult { get; set; }
    }

    public class WeatherBlob
    {

    }

    public class BBC
    {
        public string title { get; set; }
        public string news_description { get; set; }
        public string news_link { get; set; }
    }

    public class NewsArticles
    {
        public List<BBC> latest { get; set; }
    }

    public class Slip
    {

        public int id { get; set; }
        public string advice { get; set; }
    }

    public class SlipObject
    {
        public Slip slip { get; set; }
    }


    public class WordOfTheDayBlob
    {
        public List<string> SearchKey { get; set; }
    }

    public class GiphyResponse
    {
        public GiphyData Data { get; set; }
    }

    public class GiphyData
    {
        [JsonProperty("url")]
        public string Image_Url { get; set; }
    }

    /******************************************************************** END JSON BLOBS DEFINITIONS **************************************************************************/

    /*
     

                                                                           _____         .__        
                                                                          /     \ _____  |__| ____  
                                                                         /  \ /  \\__  \ |  |/    \ 
                                                                        /    Y    \/ __ \|  |   |  \
                                                                        \____|__  (____  /__|___|  /
                                                                                \/     \/        \/ 


     */

    internal class Program
    {
        public static DiscordSocketClient Client;


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            new Program().MainAsync().GetAwaiter().GetResult();
            Application.Run(new Form1());
        }

        public async Task MainAsync()
        {
            try
            {
                Client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    MessageCacheSize = 200,
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                });

                Logger.LogInformation("Successfully linked the HandleCommand method to the MessageReceived event listener!");
                Logger.LogInformation("Listening for client response...");
                Client.Log += Log;
                Client.MessageReceived += HandleCommand;

                // Cache bot token for multiple uses later
                string CurrentBot = Discord.BOT_TOKEN;

                // Begin login
                Logger.LogInformation("Attempting to login with bot token {0}", CurrentBot);
                await Client.LoginAsync(TokenType.Bot, CurrentBot);
                await Client.StartAsync();

                await Task.Delay(-1);
            }
            catch (Exception Ex)
            {
                Logger.LogError("Ran into issues, here is the error message:\n{0}", Ex.Message);

            }
            finally
            {
                await Client.StopAsync();
                await Client.LogoutAsync();
            }
        }

        /********************************************************************** END MAIN PROGRAM LOOP ****************************************************************************/

        /*
         

                                                                         ____ ___   __  .__.__  .__  __          
                                                                        |    |   \_/  |_|__|  | |__|/  |_ ___.__.
                                                                        |    |   /\   __\  |  | |  \   __<   |  |
                                                                        |    |  /  |  | |  |  |_|  ||  |  \___  |
                                                                        |______/   |__| |__|____/__||__|  / ____|
                                                                                                          \/     


         */

        private async Task HandleCommand(SocketMessage arg)
        {

            const string GPT_TOKEN = "My OpenAI API Token";
            const string WEATHER_TOKEN = "My Weather Token";

            try
            {
                SocketUserMessage message = arg as SocketUserMessage;

                if (message == null || message.Author.IsBot)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(message.Content))
                {
                    // Cache the message text
                    string CachedMsg = message.Content;
                    const string CommandName = @"/search";

                    // Define regex capture patterns
                    const string SearchCommandPattern = $"^({CommandName})(.*)$";
                    const string WOTDCommandPattern = @"\/(wotd)";
                    const string RandomCommandPattern = @"/(?i)random";
                    const string GPTPattern = @"\/(?i)gpt(.*)";
                    const string AdvicePattern = @"/(?i)advice";
                    const string BBCPattern = @"/(?i)bbc";
                    const string InsultMePattern = @"/(?i)insultme";
                    const string WeatherUpdatePattern = @"/(?i)weather";
                    const string ListCommandPattern = @"/(?i)commands";

                    // Match the cachedMsg to the regex patterns
                    RegexOptions options = RegexOptions.IgnoreCase;
                    Match SearchMatch = Regex.Match(CachedMsg, SearchCommandPattern, options);
                    Match WordMatch = Regex.Match(CachedMsg, WOTDCommandPattern, options);
                    Match RandomMatch = Regex.Match(CachedMsg, RandomCommandPattern, options);
                    Match GPTMatch = Regex.Match(CachedMsg, GPTPattern, options);
                    Match AdviceMatch = Regex.Match(CachedMsg, AdvicePattern, options);
                    Match BBCMatch = Regex.Match(CachedMsg, BBCPattern, options);
                    Match InsultMeMatch = Regex.Match(CachedMsg, InsultMePattern, options);
                    Match WeatherMatch = Regex.Match(CachedMsg, WeatherUpdatePattern, options);
                    Match ListCommandMatch = Regex.Match(CachedMsg, ListCommandPattern, options);

                    // Begin matches
                    if (GPTMatch.Success)
                    {

                        string UNDER_CONSTRUCTION = @"
                                             ____ ___           .___             _________                         __                        __  .__       
                                            |    |   \____    __| _/___________  \_   ___ \  ____   ____   _______/  |________ __ __   _____/  |_|__| ____   ____  
                                            |    |   /    \  / __ |/ __ \_  __ \ /    \  \/ /  _ \ /    \ /  ___/\   __\_  __ \  |  \_/ ___\   __\  |/  _ \ /    \ 
                                            |    |  /   |  \/ /_/ \  ___/|  | \/ \     \___(  <_> )   |  \\___ \  |  |  |  | \/  |  /\  \___|  | |  (  <_> )   |  \
                                            |______/|___|  /\____ |\___  >__|     \______  /\____/|___|  /____  > |__|  |__|  |____/  \___  >__| |__|\____/|___|  /
                                                         \/      \/    \/                \/            \/     \/                          \/                    \/ ";


                        Logger.LogInformation("Initializing OpenAI API");

                        try
                        {

                            //using var api = new OpenAIClient(GPT_TOKEN);
                            //var AssistantRequest = new CreateAssistantRequest("gpt-3.5-turbo-1106", "GPTDiscordBot", "The goal is to help with chat conversations", "answer every inquiry as though you were a sassy Gandalf");
                            //var Assistant = await api.AssistantsEndpoint.CreateAssistantAsync(AssistantRequest);
                            //var models = api.ModelsEndpoint.GetModelsAsync();
                            //var thread = await api.ThreadsEndpoint.CreateThreadAsync();
                            //var MessageRequest = new CreateMessageRequest(GPTMatch.Groups[1].Value);
                            //var gptMessage = await api.ThreadsEndpoint.CreateMessageAsync(thread.Id, MessageRequest);
                            //var run = await thread.CreateRunAsync(Assistant);

                            //var messageList = await api.ThreadsEndpoint.ListMessagesAsync(thread.Id);

                            //while (run.Status != RunStatus.Completed)
                            //{
                            //    run = await api.ThreadsEndpoint.RetrieveRunAsync(thread.Id, run.Id);
                            //}

                            //messageList = await api.ThreadsEndpoint.ListMessagesAsync(thread.Id);

                            //var t = 5;

                            if (!(await SendDiscordMessage($"This is what GPT has to say about it:\n \n" + $"GPT bot feature is currently under construction...\n", message.Channel, true, null, false)))
                            {
                                Console.WriteLine($"Failed to post message...");
                            }
                        }
                        catch (Exception ex)
                        {
                            //const string completionsEndpoint = "https://api.openai.com/v1/chat/completions";
                            //bool checkCompletionsOnFailure = true;
                            //string tempContainer = null;
                            //if (checkCompletionsOnFailure)
                            //{

                            //    using (HttpClient httpClient = new HttpClient())
                            //    {
                            //        HttpResponseMessage Response = await httpClient.GetAsync(completionsEndpoint);
                            //        string result = await Response.Content.ReadAsStringAsync();

                            //        dynamic jsonObject = JsonConvert.DeserializeObject(result);
                            //        tempContainer = jsonObject.error.message;

                            //    }
                            //}
                            //if (!(await SendDiscordMessage($"Failed to resolve GPT request:\n \n{ex.Message} \n" +
                            //    "\nCaptured Prompt: **" +
                            //    GPTMatch.Groups[1].Value.Trim() + "**" +
                            //    "\nFull Entered Prompt: **" + GPTMatch.Value.Trim() + "**" +
                            //    "\nModel: **" + chat.Model + "**" +
                            //    "\nAPI Response: **" + (chat.MostRecentApiResult == null ? "null" : chat.MostRecentApiResult) + "**" +
                            //    $"\n\nResponse from Completions({completionsEndpoint}):\n **" + (checkCompletionsOnFailure ? tempContainer : "Completion viewing disabled...") + "**" +
                            //    "\n\n**Request Parameters**" +
                            //    "\nUser: **" + (chat.RequestParameters.user == null ? "null" : chat.RequestParameters.user) + "**" +
                            //    "\nTemperature: **" + chat.RequestParameters.Temperature + "**"
                            //    , message.Channel)))
                            //{
                            //    Console.WriteLine($"Failed to post message...");
                            //}
                        }
                    }

                    if (ListCommandMatch.Success)
                    {
                        try
                        {
                            if (!await SendDiscordMessage($"Here is a list of all the available commands: \n\n" +
                                $"**/search 'term'** - gives a relavant wiki article on the term\n" +
                                $"**/wotd** - gives Merriam Webster's Word of the Day\n" +
                                $"**/random** - returns a random GIF from GIPHY\n" +
                                $"**/gpt 'question'** - returns a gpt enabled response to the question\n" +
                                $"**/advice** - returns some advice\n" +
                                $"**/bbc** - returns recent BBC news articles\n" +
                                $"**/insultme** - attacks you with profanity\n" +
                                $"**/weather** - displays local weather\n" +
                                $"**/commands** - lists all the available commands\n", message.Channel))
                            {
                                Logger.LogInformation("Failed to send failure message...");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!await SendDiscordMessage($"Ran into issues listing the commands:\n**Error:** \n\n{ex.Message}", message.Channel))
                            {
                                Logger.LogInformation("Failed to send failure message...");
                            }
                        }
                    }

                    if (AdviceMatch.Success)
                    {
                        try
                        {
                            using (HttpClient httpClient = new HttpClient())
                            {
                                string endPoint = $"https://api.adviceslip.com/advice";
                                HttpResponseMessage result = await httpClient.GetAsync(endPoint);
                                string finResult = await result.Content.ReadAsStringAsync();
                                SlipObject AdviceObject = JsonConvert.DeserializeObject<SlipObject>(finResult);


                                if (!await SendDiscordMessage($"Here's that advice you ordered, cooked fresh: \n\n{AdviceObject.slip.advice}", message.Channel))
                                {
                                    Logger.LogInformation("Failed to send failure message...");
                                }
                            };
                        }
                        catch (Exception ex)
                        {
                            if (!await SendDiscordMessage($"Ran into issues giving you advice:\n**Error:** \n\n{ex.Message}", message.Channel))
                            {
                                Logger.LogInformation("Failed to send failure message...");
                            }
                        }
                    }

                    if (WeatherMatch.Success)
                    {
                        try
                        {
                            using (HttpClient httpClient = new HttpClient())
                            {
                                string endPoint = $"https://api.openweathermap.org/data/3.0/onecall?lat=33.44&lon=-94.04&exclude=hourly,daily&appid={WEATHER_TOKEN}";
                                HttpResponseMessage result = await httpClient.GetAsync(endPoint);
                                string finResult = await result.Content.ReadAsStringAsync();
                                WeatherBlob WeatherObject = JsonConvert.DeserializeObject<WeatherBlob>(finResult);


                                if (!await SendDiscordMessage($"Here's that weather you wanted: \n\n{finResult}", message.Channel))
                                {
                                    Logger.LogInformation("Failed to send failure message...");
                                }
                            };
                        }
                        catch (Exception ex)
                        {
                            if (!await SendDiscordMessage($"Ran into issues giving you the weather:\n**Error:** \n\n{ex.Message}", message.Channel))
                            {
                                Logger.LogInformation("Failed to send failure message...");
                            }
                        }
                    }

                    if (InsultMeMatch.Success)
                    {
                        try
                        {
                            using (HttpClient httpClient = new HttpClient())
                            {
                                string endPoint = $"https://evilinsult.com/generate_insult.php?lang=en&type=json";
                                HttpResponseMessage result = await httpClient.GetAsync(endPoint);
                                string finResult = await result.Content.ReadAsStringAsync();
                                InsultMeBlob InsultMeObject = JsonConvert.DeserializeObject<InsultMeBlob>(finResult);

                                string decodedInsult = HttpUtility.HtmlDecode(InsultMeObject.insult);
                                if (!await SendDiscordMessage(decodedInsult, message.Channel))
                                {
                                    Logger.LogInformation("Failed to send failure message...");
                                }
                            };
                        }
                        catch (Exception ex)
                        {
                            if (!await SendDiscordMessage($"Ran into issues insulting you :(\n**Error:** \n\n{ex.Message}", message.Channel))
                            {
                                Logger.LogInformation("Failed to send failure message...");
                            }
                        }
                    }

                    if (BBCMatch.Success)
                    {
                        try
                        {
                            using (HttpClient httpClient = new HttpClient())
                            {
                                string endPoint = $"https://bbc-api.vercel.app/latest?lang=english";
                                HttpResponseMessage result = await httpClient.GetAsync(endPoint);
                                string finResult = await result.Content.ReadAsStringAsync();
                                NewsArticles ArticleObject = JsonConvert.DeserializeObject<NewsArticles>(finResult);
                                string final = string.Empty;

                                foreach (BBC Article in ArticleObject.latest)
                                {
                                    if (!final.Contains(Article.news_link))
                                    {
                                        final += $"**Title**: {Article.title}\n**Link:** {Article.news_link}\n\n\n";
                                    }

                                }


                                if (!await SendDiscordMessage($"Here's some articles from BBC: \n\n{final}", message.Channel, true, null, false))
                                {
                                    Logger.LogError("Failed to send failure message...");
                                }
                            };
                        }
                        catch (Exception ex)
                        {
                            if (!await SendDiscordMessage($"Ran into issues grabbing the articles:\n**Error:** \n\n{ex.Message}", message.Channel))
                            {
                                Logger.LogError("Failed to send failure message...");
                            }
                        }
                    }

                    if (RandomMatch.Success)
                    {
                        try
                        {
                            string RandomGIF = await GetRandomGifUrl();
                            if (!(await SendDiscordMessage($"Here's a random GIF:\n\n", message.Channel)))
                            {
                                Console.WriteLine("Failed to send message...");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Caught exception attempting to send random GIF, error message: " + ex.ToString());
                        }
                    }

                    if (SearchMatch.Success)
                    {
                        string SearchKey = SearchMatch.Groups[2].Value;
                        SearchKey = SearchKey.Replace("-", " ");
                        // process search command

                        try
                        {
                            using (HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                            {
                                // Scrape the webpage and ensure Success
                                string ScraperURL = $"https://en.wikipedia.org/w/api.php?action=opensearch&format=json&search={SearchKey}&utf8=1&exintro=1&explaintext=true";

                                HttpResponseMessage Response = await Client.GetAsync(ScraperURL);
                                Response.EnsureSuccessStatusCode();

                                // Parse scraped result
                                string JsonResult = await Response.Content.ReadAsStringAsync();


                                var WTDBlob = JsonConvert.DeserializeObject(JsonResult);

                                if (WTDBlob != null)
                                {
                                    JArray JSomethign = (JArray)WTDBlob;
                                    if (JSomethign.Any())
                                    {
                                        JToken newtext = JSomethign[1][0];
                                        SearchKey = newtext.ToString();
                                    }

                                }

                                ScraperURL = $"https://en.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&titles={SearchKey}&utf8=1&exintro=1&explaintext=true";

                                Response = await Client.GetAsync(ScraperURL);
                                Response.EnsureSuccessStatusCode();

                                // Parse scraped result
                                JsonResult = await Response.Content.ReadAsStringAsync();

                                JObject ParsedResult = JObject.Parse(JsonResult);

                                Response = await Client.GetAsync(ScraperURL);
                                Response.EnsureSuccessStatusCode();

                                // Parse scraped result
                                JsonResult = await Response.Content.ReadAsStringAsync();

                                // Create Message to send
                                string Message = $"Failed to grab {SearchKey}.";
                                Message = ParsedResult.ToString();
                                Message = ParsedResult["query"]["pages"].First.First["extract"].ToString();
                                Message = $"**{SearchKey}**:\n" + Message;

                                Message = CleanHTML(Message);

                                // Check if message is less than 1800 characters (Discord's limit is 2000)
                                if (Message.Length < 1700)
                                {
                                    if (!(await SendDiscordMessage($"Here is your search on **{SearchKey}**:\n\n" + Message, message.Channel)))
                                    {
                                        Console.WriteLine($"Failed to post message...");
                                    }

                                    Console.WriteLine($"Posted search to Discord Channel.");
                                }
                                else
                                {
                                    // Take first 1700 characters from string to be safe
                                    string ModifiedMessage = new string(Message.Take(1700).ToArray());
                                    Console.WriteLine(ModifiedMessage.Length);
                                    if (!await SendDiscordMessage($"There seems to be quite a bit on **{SearchKey}**, this is all I could pull: \n\n{ModifiedMessage}", message.Channel, false))
                                    {
                                        Console.WriteLine("Failed to send message...");
                                        throw new Exception("Failed to post.");
                                    }
                                }

                                // Await next input
                                Console.WriteLine("\n\nMake another search by typing in a keyword, otherwise enter 'quit' or 'quit program'.");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!await SendDiscordMessage($"Failed to parse your search ({SearchKey}), try again!\n\n**Error Message**: \n{ex.Message}", message.Channel))
                            {
                                Console.WriteLine("Failed to send message...");
                                throw new Exception("Failed to post.");
                            }
                        }
                    }

                    if (WordMatch.Success)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            string ScraperURL = "https://www.merriam-webster.com/word-of-the-day";


                            SocketChannel som = Client.GetChannel(Discord.ChannelLookup["GulagServer"]);

                            // Get the channel using the ID
                            SocketTextChannel channel = (SocketTextChannel)som;

                            // Prepare search pattern with scraped result
                            string WordOfTheDayPattern = @"<title>(Word of the Day):\s(\w+)";
                            string Html = await client.GetStringAsync(ScraperURL);

                            // Search for word of the day 
                            Match WOTDMatch = Regex.Match(Html, WordOfTheDayPattern, options);
                            if (WOTDMatch.Success)
                            {
                                string WordOfTheDay = WOTDMatch.Groups[2].Value;
                                string WOTDDefinitionPattern = @"<h2>What It Means<\/h2>(\s.*)";
                                Match DefinitionMatch = Regex.Match(Html, WOTDDefinitionPattern, options);

                                // Search for word of the day definition
                                if (DefinitionMatch.Success)
                                {
                                    string WordDefinition = DefinitionMatch.Groups[1].Value;
                                    WordDefinition = WordDefinition.Replace("\n", "");
                                    WordDefinition = WordDefinition.Trim();

                                    // Format word of the day message with definition
                                    string Message = $"\n**{WOTDMatch.Groups[1].Value}:**\n*{WordOfTheDay}*\n" + $"\n**Definition:**\n*{WordDefinition}*";

                                    Html = Html.Replace(DefinitionMatch.Value, " ");

                                    string SentencePattern = $"<p>\\/\\/(.*<em>{WordOfTheDay.ToLower()}<\\/em>.*)<\\/p>";

                                    Match UseMatch = Regex.Match(Html, SentencePattern, options);
                                    if (UseMatch.Success)
                                    {
                                        string WordSentence = UseMatch.Groups[1].Value;
                                        WordSentence = WordSentence.Trim();
                                        Message += $"\n\n**Use** **{WordOfTheDay.ToLower()}** in a sentence:\n*{WordSentence}*";
                                    }


                                    // Send word of the day message
                                    if (!await SendDiscordMessage(Message, message.Channel))
                                    {
                                        Console.WriteLine("Failed to post the word of the day...");
                                        throw new Exception("Failed to post.");
                                    }
                                    Console.WriteLine($"Successfully posted word of the day to discord channel.");
                                }
                            }
                            // Await next input
                            Console.WriteLine("\n\nMake another search by typing in a keyword, otherwise enter 'quit' or 'quit program'.");
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Logger.LogError("Ran into error, here is the message:\n{ErrorMessage}", Ex.Message);
            }
            return;
        }

        private Task Log(LogMessage msg)
        {
            Logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        }

        static async Task<string> GetRandomGifUrl(string Keyword = null)
        {
            const string apiKey = "My GIPHY API Token";
            using (HttpClient httpClient = new HttpClient())
            {
                string giphyApiUrl = $"https://api.giphy.com/v1/gifs/random?api_key={apiKey}";
                if (Keyword != null)
                {
                    giphyApiUrl = $"https://api.giphy.com/v1/gifs/search?api_key={apiKey}&q={Keyword}&limit=50";
                }


                HttpResponseMessage response = await httpClient.GetAsync(giphyApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // Use Newtonsoft.Json for parsing the JSON response
                    var giphyResponse = JsonConvert.DeserializeObject<GiphyResponse>(jsonResponse);

                    // Check if the image URL is available in the response
                    if (giphyResponse?.Data?.Image_Url != null)
                    {
                        return giphyResponse.Data.Image_Url;
                    }
                    else
                    {
                        Console.WriteLine("Error: Image URL not found in the Giphy API response.");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
        }

        static async Task<bool> SendDiscordMessage(string Message, ISocketMessageChannel targetChannel, bool CleanHtml = true, string RandomGIFInfluence = null, bool sendGIF = true)
        {
            try
            {
                if (CleanHtml)
                {
                    Message = CleanHTML(Message, false);
                }
                SocketTextChannel TargetChannel = (SocketTextChannel)targetChannel;
                Console.WriteLine($"Attempting to write to channel {TargetChannel.Id}...");

                string BufferGIF = await GetRandomGifUrl();
                if (RandomGIFInfluence != null)
                {
                    BufferGIF = await GetRandomGifUrl(RandomGIFInfluence);
                }

                Console.WriteLine($"Random GIF URL: {BufferGIF}");

                // Send Message to channel
                await TargetChannel.SendMessageAsync($"**Smelly Feet here with a PSA!**\n\n{Message}\n\nThanks for following (**{TargetChannel.Name}**) updates!");
                if (sendGIF)
                {
                    await TargetChannel.SendMessageAsync(BufferGIF);
                }


                // Return success
                return true;
            }
            catch
            {

                // Return failure
                return false;
            }

        }

        static string CleanHTML(string Input, bool ClearTopLine = true)
        {
            Input = Replace(Input, "i");
            Input = Replace(Input, "b");
            Input = Replace(Input, "p");
            Input = Replace(Input, "em");
            Input = Replace(Input, "a");
            if (ClearTopLine)
            {
                int IndexOf = Input.IndexOf("\n");
                if (IndexOf != -1)
                {
                    Input = Input.Substring(IndexOf + 1);
                }
            }
            return Input;
        }

        static string Replace(string FullText, string TextToReplace)
        {
            switch (TextToReplace)
            {
                case "em": // Clean and format <em> tags
                    FullText = FullText.Replace($"<{TextToReplace}>", "**");
                    FullText = FullText.Replace($"</{TextToReplace}>", "**");
                    break;
                case "a": // Clean and format <a href""> links
                    FullText = FullText.Replace($"\">", ")[");
                    FullText = FullText.Replace($"</a>", "]");
                    FullText = FullText.Replace($"<a href=\"", "(");
                    string LinkPattern = @"\(([^)]*)\)\[([^)]*)\]";
                    FullText.Replace("\t", " ");
                    Match LinkMatch = Regex.Match(FullText, LinkPattern);
                    while (LinkMatch.Success)
                    {
                        // Full-text format is: [link-url](link-text)
                        FullText = FullText.Replace($"{LinkMatch.Groups[0].Value}", $"[{LinkMatch.Groups[2].Value}]({LinkMatch.Groups[1].Value})");
                        LinkMatch = Regex.Match(FullText, LinkPattern);
                    }
                    break;
                default: // Clean any <> </> tags 
                    FullText = FullText.Replace($"<{TextToReplace}>", "");
                    FullText = FullText.Replace($"</{TextToReplace}>", "");
                    break;
            }

            return FullText;
        }

        /******************************************************************** END UTILITY FUNCTIONS **************************************************************************/
    }

}
