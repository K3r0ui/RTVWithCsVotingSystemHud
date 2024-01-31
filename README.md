# RTV system for counter strike voting [ community servers ]
##Requirements:

Counter-Strike Sharp: Version 144 or higher.
Metamod: Version 2.X or above.
Compatibility: Fully compatible with both Linux and Windows operating systems.
##Installation

##Functionality:

Download the latest release from the Releases page.
Extract the contents of the downloaded zip file to your server's addons directory.
Ensure that Counter-Strike Sharp and Metamod are installed and up to date.
Restart your CS:S server.


Initial State: RTV is disabled at the start of the map.
Activation Mechanism: The plugin monitors the mp_timelimit value from the server's settings. RTV is activated once half of this time limit has elapsed.
Voting Process: To initiate a successful RTV, 60% of connected players (excluding bots) must enter !rtv in the chat.
Vote Execution: Following a successful RTV, the plugin waits for the round's conclusion before initiating the end-match vote.

##Additional Features:

In-Game Information: Players can view the remaining time for RTV availability and the time left until the map ends by pressing the TAB key.
Vote Retraction: After entering !rtv, players have the option to retract their vote by typing !unrtv in chat.
Cooldown Period: To prevent abuse, a cooldown period is implemented after using !unrtv, deterring spam and troll behaviors.

##Planned Enhancements:

Nomination System: We aim to incorporate a nomination system into the voting process. However, this is a challenging endeavor due to the need to modify the existing, and somewhat flawed, end-map vote system. The complexity of this task is compounded by issues that Valve needs to address. 
We acknowledges the efforts of CS2Fixes, who have successfully reverse-engineered and implemented a new voting system through their Vote Manager plugin. It's possible to do the same with CSSharp but we don't have the time to go through all that as 2.

##Demo 
* ![Demo]([demo/demo.gif](https://im4.ezgif.com/tmp/ezgif-4-eaf373bf90.gif)https://im4.ezgif.com/tmp/ezgif-4-eaf373bf90.gif)
