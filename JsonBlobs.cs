using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class JokeObject
    {
        public string joke { get; set; }
        public int status { get; set; }
    }

    public class Location
    {
        public string name { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string tz_id { get; set; }
        public long localtime_epoch { get; set; }
        public string localtime { get; set; }
    }

    public class Condition
    {
        public string text { get; set; }
        public string icon { get; set; }
        public int code { get; set; }
    }

    public class Current
    {
        public string temp_f { get; set; }
        public Condition condition { get; set; }
        public double humidity { get; set; }
        public double feelslike_f { get; set; }
    }

    public class WeatherBlob
    {
        public Location location { get; set; }
        public Current current { get; set; }
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
}
/******************************************************************** END JSON BLOBS DEFINITIONS **************************************************************************/