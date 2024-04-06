using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordHelperBot
{
    /******************************************************************** BEGIN DISCORD CLASS DEFINITIONS **************************************************************************/

    public static class Discord
    {

        // Dictionary of ulong channel id's to send messages to
        public static Dictionary<string, ulong> ChannelLookup = new Dictionary<string, ulong>
        {
            {"DevDiscordServer", MyDiscordChannelIDHere},
            {"MainDiscordServer", MyOtherDiscordChannelIDHere}
        };

        public static Dictionary<string, ulong> UserLookup = new Dictionary<string, ulong>
        {
            {"MyUserName", MyUserID },
            {"None", 0 }
        };

        // Need unique bot token for server, get this on app dashboard
        public static string BOT_TOKEN = "My Discord Bot Token Here";
    }

    /******************************************************************** END DISCORD CLASS DEFINITIONS **************************************************************************/
}