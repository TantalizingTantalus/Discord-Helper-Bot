# About
Discord helper bot is written in C# .NET 8.0 Framework and uses a variety of API's to achieve various in-chat channel tasks.

# Setup
- Create a bot in your discord developer portal by going Applications -> Bot
- After creating and naming your bot, generate a bot token and save it.
- After you get your token, open the .csproj and navigate to Discord.cs and replace the bot token with your copied
  token. You will need to also make an account for the weather app if you wish to use weather functionality as well as GPT Tooling
- After replacing the appropriate tokens you will need to invite the bot to a channel you own or have permission to invite to.
- Once the bot is invited to the channel all thats left is build for release and run the .exe

# Commands
- **/search 'term'** - returns a relavant wiki article snippet on the term, character limit for article is at 1700.
- **/wotd** - returns Merriam Webster's Word of the Day by scraping the website and searching for keywords, may fail if MW changes site formatting/layout.
- **/random** - returns a random GIF url from GIPHY.
- **/gpt 'question'** - Ask GPT a question! example: "/gpt how far is the moon from earth?"
- **/advice** - returns some advice.
- **/bbc** - returns recent BBC news articles.
- **/insultme** - hurls an insult your way!
- **/weather 'city,state'** - displays the weather for the selected city, optional input state.
- **/taj** - Tells a Joke! Helper bots can be funny too!
- **/commands** - lists all the available commands.

# Automatically have the bot run on startup on your pc
 So far I dont have a valid solution for this so the current workaround is to:
 - Create a shortcut to the DiscordHelperBot.exe in the release folder
 - Add the newly create shortcut to your startup folder

To disable the bot from running, simply search task manager for the name of the bot "DiscordHelperBot" and end the task. 

# Issues
- GPT tooling is still under development due to GPT rate limitations, may get mixed results.
- HTML decoder I hacked together may skip elements that havent been accounted for during WOTD scraping.
- Running multiple asynchronous commands back to back may cause unwanted spam (specifically /bbc and /gpt).

# API's Used
- [Adviceslip](https://api.adviceslip.com)
- [OpenWeatherMap](https://api.openweathermap.org)
- [EvilInsult](https://evilinsult.com)
- [BBC News](https://bbc-api.vercel.app)
- [GIFs!](https://giphy.com)
- [Wikipedia OpenSearch](https://en.wikipedia.org)
- [Word of the Day](https://www.merriam-webster.com)
- [Discord API](https://docs.discordnet.dev/guides/introduction/intro.html)
- [OpenAI API](https://github.com/RageAgainstThePixel/OpenAI-DotNet)
- [ICanHazDadJoke](https://icanhazdadjoke.com/api)
