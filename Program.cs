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

namespace SmellyFeetRevived
{


    /******************************************************************** BEGIN LOGGING CLASS DEFINITIONS **************************************************************************/

    public static class Logger
    {
        private static ILogger Log;
        static Logger()
        {
            using ILoggerFactory Factory = LoggerFactory.Create(builder => builder.AddConsole());

            Log = Factory.CreateLogger<Program>();
        }

        public static void LogInformation(string Message)
        {
            Log.LogInformation(Message);
            return;
        }

        public static void LogInformation(string Message, object Param1, object Param2 = null, object Param3 = null, object Param4 = null, object Param5 = null)
        {
            Message = ProcessMessage(Message, Param1, Param2, Param3, Param4, Param5);
            Log.LogInformation(Message);
            return;
        }

        public static void LogError(string Message)
        {
            Log.LogError(Message);
            return;
        }

        public static void LogError(string Message, object Param1, object Param2 = null, object Param3 = null, object Param4 = null, object Param5 = null)
        {
            Message = ProcessMessage(Message, Param1, Param2, Param3, Param4, Param5);
            Log.LogError(Message);
            return;
        }

        public static string ProcessMessage(string Message, object Param1, object Param2 = null, object Param3 = null, object Param4 = null, object Param5 = null)
        {

            List<string> Parameters = new List<string>();

            if (Param1 != null)
            {
                Parameters.Add((string)Param1);
            }

            if (Param2 != null)
            {
                Parameters.Add((string)Param2);
            }

            if (Param3 != null)
            {
                Parameters.Add((string)Param3);
            }

            if (Param4 != null)
            {
                Parameters.Add((string)Param4);
            }

            if (Param5 != null)
            {
                Parameters.Add((string)Param5);
            }

            string ConcPattern = @"(\{([^}]+)\})";
            Match ConcMatch = Regex.Match(Message, ConcPattern);

            while (ConcMatch.Success)
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    Message = Message.Replace(ConcMatch.Groups[0].Value, Parameters[i]);
                    ConcMatch = Regex.Match(Message, ConcPattern);
                }
            }

            return Message;
        }
    }

    /******************************************************************** END LOGGING CLASS DEFINITIONS **************************************************************************/

    /******************************************************************** BEGIN DISCORD CLASS DEFINITIONS **************************************************************************/

    public static class DiscordInformation
    {

        // Dictionary of ulong channel id's to send messages to
        public static Dictionary<string, ulong> ChannelLookup = new Dictionary<string, ulong>
        {
            { "YourChannelName", yourulongchannelIDhere }
        };

        public static Dictionary<string, ulong> UserLookup = new Dictionary<string, ulong>
        {
            {"YourUsername", yourusernameIDhere },
            {"None", 0 }
        };

        // Need unique bot token for server, get this on app dashboard
        public static string BOT_TOKEN = "YourBotTokenHere";
    }

    /******************************************************************** END DISCORD CLASS DEFINITIONS **************************************************************************/

    /******************************************************************** BEGIN JSON BLOBS DEFINITIONS **************************************************************************/

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

    /********************************************************************** BEGIN MAIN PROGRAM LOOP ****************************************************************************/

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
                string CurrentBot = DiscordInformation.BOT_TOKEN;

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

        /********************************************************************** BEGIN UTILITY FUNCTIONS ****************************************************************************/

        private async Task HandleCommand(SocketMessage arg)
        {
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

                    const string SearchCommandPattern = $"^({CommandName})(.*)$";
                    const string WOTDCommandPattern = @"\/(wotd)";
                    const string RandomCommandPattern = @"/(?i)random";
                    const string PoopPattern = @"poop";
                    RegexOptions options = RegexOptions.IgnoreCase;
                    Match PoopMatch = Regex.Match(CachedMsg, PoopPattern, options);
                    Match SearchMatch = Regex.Match(CachedMsg, SearchCommandPattern, options);
                    Match WordMatch = Regex.Match(CachedMsg, WOTDCommandPattern, options);
                    Match RandomMatch = Regex.Match(CachedMsg, RandomCommandPattern, options);

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
                                    // Take first 1800 characters from string to be safe
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


                            SocketChannel som = Client.GetChannel(DiscordInformation.ChannelLookup["ChannelName"]);

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
                else
                {
                    if (!(await SendDiscordMessage("No message contents found :(.....", message.Channel)))
                    {
                        Console.WriteLine("Failed to send message...");
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
            const string apiKey = "yourgiphyapikey";
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

        static async Task<bool> SendDiscordMessage(string Message, ISocketMessageChannel targetChannel, bool CleanHtml = true, string RandomGIFInfluence = null)
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
                await TargetChannel.SendMessageAsync(BufferGIF);

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

