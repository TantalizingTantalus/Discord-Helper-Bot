using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordHelperBot
{
    /*
     

                                                                        ________  .__                              .___
                                                                        \______ \ |__| ______ ____  ___________  __| _/
                                                                         |    |  \|  |/  ___// ___\/  _ \_  __ \/ __ | 
                                                                         |    `   \  |\___ \\  \__(  <_> )  | \/ /_/ | 
                                                                        /_______  /__/____  >\___  >____/|__|  \____ | 
                                                                                \/        \/     \/                 \/ 


     */

    public static class Discord
    {

        // Dictionary of ulong channel id's to send messages to
        public static Dictionary<string, ulong> ChannelLookup = new Dictionary<string, ulong>
        {
            {"TestServer", TestServerID},
            {"MainServer", MainServerID}
        };

        public static Dictionary<string, ulong> UserLookup = new Dictionary<string, ulong>
        {
            {"User1", User1_ID },
            {"None", 0 }
        };

        // Need unique bot token for server, get this on app dashboard
        public static string BOT_TOKEN = "My Discord Bot Token";
        public const string GIPHYAPI_KEY = "My GIPHY API key";

        public static async Task<bool> SendDiscordMessage(string Message, ISocketMessageChannel targetChannel, bool CleanHtml = true, string RandomGIFInfluence = null, bool sendGIF = true)
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
                await TargetChannel.SendMessageAsync($"**(My Bot Name) here with a PSA!**\n\n{Message}\n\nThanks for following (**{TargetChannel.Name}**) updates!");
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

        public static string CleanHTML(string Input, bool ClearTopLine = true)
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

        static async Task<string> GetRandomGifUrl(string Keyword = null)
        {

            using (HttpClient httpClient = new HttpClient())
            {
                string giphyApiUrl = $"https://api.giphy.com/v1/gifs/random?api_key={GIPHYAPI_KEY}";
                if (Keyword != null)
                {
                    giphyApiUrl = $"https://api.giphy.com/v1/gifs/search?api_key={GIPHYAPI_KEY}&q={Keyword}&limit=50";
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
    }

    /******************************************************************** END DISCORD CLASS DEFINITIONS **************************************************************************/
}