# WindowsGSM.Foundry
WindowsGSM plugin that provides Foundry Dedicated server support!

# Requirements
WindowsGSM >= 1.21.0

# The Game

- Game
https://store.steampowered.com/app/983870/FOUNDRY/

- Dedicated server
https://steamdb.info/app/2915550/info/)](https://dedicated.foundry-game.com/

# Installation

1. Download the latest release
2. Move **WindowsGSM.Foundry.cs** folder to plugins folder
3. Click **[RELOAD PLUGINS]** button or restart WindowsGSM

# Settings

1. Open game and connect
2. First connection initialise parameters
3. manage serveur directly in game.
4. Add file app.cfg with parameters (copy and paste) at folder racine because auto create file failed in the script

//server_world_name
//Sets the server world name. This is the folder where the save files will be stored.
server_world_name=MyFancyFactory

//server_password
//Sets the server password.
server_password=only_friends

//pause_server_when_empty
//Will the server pause when nobody is connected.
pause_server_when_empty=true

//autosave_interval
//Sets the autosave frequency in seconds.
autosave_interval=300

//server_is_public
//Sets whether the server is listed on the Steam server browser.
server_is_public=true

//server_port
//Sets the network port used by the game. Default is 3724.
server_port=3724

//map_seed
//Sets the map seed used to generate the world.
map_seed=42938743982

//server_persistent_data_override_folder
//Sets the absolute folder where things like logs and save files will be stored. This is mostly used by server providers so that they can run multiple dedicated servers on a single machine.
server_persistent_data_override_folder=.\Save

//server_name
//This is the name of the server listed in the Steam server browser.
server_name=HappyPlace

//server_max_players
//This sets the max amount of players on a server.
server_max_players=32


 5. Create folder manually "Mods"
# License
This project is licensed under the MIT License  - see the [LICENSE.md](LICENSE) file for details
