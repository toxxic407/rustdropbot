# Rust Drop Bot
Bot that farms Twitch Drops for Rust

# Requirements: 

Chrome and a Twitch Account that is eligible for Drops and logged in on Chrome

# Usage:

1. If you want to use Auto Claim follow the [Setting up Auto Claim Guide](#setting-up-auto-claim) before launching the Program
2. Download the latest Release from the [Releases Page](https://github.com/toxxic407/rustdropbot/releases) and launch the Application
3. Paste in your Chrome Installation Path (i.e. "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe")
4. If you want to adjust the Program (i.e. edit watchtime or prevent the bot from viewing streamers whose drops you already have/dont want) edit the stats.json file
5. If the Bot doesnt work try deleting the path.txt and stats.json files that are located in the same folder as the Program

# Setting up Auto Claim
1. Add the [Tampermonkey extension](https://www.tampermonkey.net/) to chrome 
2. Install the [Auto Claim Twitch drops Script](https://greasyfork.org/en/scripts/420346-auto-claim-twitch-drop) to Tampermonkey

# Priorities:
The Bot supports setting Priorities in stats.json (Higher Number = Higher Priority)
This means that the Bot always tries to watch the Streamer with the highest Priority out of all the Streamers that are online

# Important things to know about this Program:
1. I recommend changing the Stream Quality to the lowest definition and disabling low latency to prevent connection problems from not adding watchtime
2. Close all Chrome tabs before launching the Bot and do not use Chrome while the Bot is running because it may prevent the Bot from closing Chrome
3. The Auto Claim Twitch drops Script is made by bAdRocK and not by me
4. I am not responsible for any Damage to your Computer/Bans from Twitch/Apocalyptical Events that might because you used this Program
