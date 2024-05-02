using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.IO;
using System.Linq;
using System.Net;


namespace WindowsGSM.Plugins
{
    public class Foundry : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Foundry", // WindowsGSM.XXXX
            author = "werewolf2150",
            description = "WindowsGSM plugin for supporting Foundry Dedicated Server",
            version = "1.0",
            url = "https://github.com/werewolf2150/WindowsGSM.Foundry", // Github repository link (Best practice)
            color = "#34c9eb" // Color Hex
        };

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "2915550"; // Game server appId Steam

        // - Standard Constructor and properties
        public Foundry(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;


        // - Game server Fixed variables
        public override string StartPath => @".\FoundryDedicatedServer.exe"; // Game server start path
        public string FullName = "Foundry Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation

        // TODO: Undisclosed method
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()

        // - Game server default values
        public string Port = "3724"; // Default port
        //public string QueryPort = "3725"; // Default query port. This is the port specified in the Server Manager in the client UI to establish a server connection.
        //public string BeaconPort = "3726"; // Default beacon port. This port currently cannot be set freely.
        public string Defaultmap = "MyFancyFactory"; // Default map name
		public string DefaultPause = "true"; //  Will the server pause when nobody is connected.
		public string DefaultAutoSaveInterval = "300"; // Sets the autosave frequency in seconds.
		public string DefaultPublic = "true"; // Sets whether the server is listed on the Steam server browser.
		public string DefaultMapSeed = "42938743982"; //  Sets the map seed used to generate the world.
		//public string DefaultFolder = ".\Server01"; // Sets the absolute folder where things like logs and save files will be stored. This is mostly used by server providers so that they can run multiple dedicated servers on a single machine.
        public string Maxplayers = "32"; // Default maxplayers
        
        // TODO: Unsupported option

        // TODO: May not support
        public string Additional = ""; // Additional server start parameter


        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            /*var serverConfig = new
            {
                //Available Options:
				//server_world_name
				//Sets the server world name. This is the folder where the save files will be stored.
				server_world_name = $"{_serverData.Defaultmap}",
				//server_password
				//Sets the server password.
				server_password = $"{_serverData.ServerPassword}",
				//pause_server_when_empty
				//Will the server pause when nobody is connected.
				pause_server_when_empty = $"{_serverData.DefaultPause}",
				//autosave_interval
				//Sets the autosave frequency in seconds.
				autosave_interval = $"{_serverData.DefaultAutoSaveInterval}",
				//server_is_public
				//Sets whether the server is listed on the Steam server browser.
				server_is_public = $"{_serverData.DefaultPublic}",
				//server_port
				//Sets the network port used by the game. Default is 3724.
				server_port = $"{_serverData.Port}",
				//map_seed
				//Sets the map seed used to generate the world.
				map_seed = $"{_serverData.DefaultMapSeed}",
				//server_persistent_data_override_folder
				//Sets the absolute folder where things like logs and save files will be stored. This is mostly used by server providers so that they can run multiple dedicated servers on a single machine.
				//server_persistent_data_override_folder = $"{_serverData.DefaultFolder}",
				//server_name
				//This is the name of the server listed in the Steam server browser.
				server_name = $"{_serverData.ServerName}",
				//server_max_players
				//This sets the max amount of players on a server.
				server_max_players = $"{_serverData.MaxPlayers}"
            };

            // Convert the object to JSON format
            string jsonContent = JsonConvert.SerializeObject(serverConfig, Formatting.Indented);

            // Specify the file path
            string filePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, "enshrouded_server.json");

            // Write the JSON content to the file
            File.WriteAllText(filePath, jsonContent);*/
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string shipExePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }

            // Prepare start parameter
            string param = "FactoryGame -log -unattended";
            param += $" {_serverData.ServerParam}";
            //param += string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -Port={_serverData.ServerPort}"; 
            //param += string.IsNullOrWhiteSpace(_serverData.ServerQueryPort) ? string.Empty : $" -ServerQueryPort={_serverData.ServerQueryPort}";
            //param += string.IsNullOrWhiteSpace(_serverData.ServerIP) ? string.Empty : $" -Multihome={_serverData.ServerIP}";

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = shipExePath,
                    Arguments = param,
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = false,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
            }

            // Start Process
            try
            {
                p.Start();
                if (AllowsEmbedConsole)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }


// - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                Functions.ServerConsole.SendWaitToMainWindow("^c");
                p.WaitForExit(20000);
            });
        }

// fixes WinGSM bug, https://github.com/WindowsGSM/WindowsGSM/issues/57#issuecomment-983924499
        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            var (p, error) = await Installer.SteamCMD.UpdateEx(serverData.ServerID, AppId, validate, custom: custom, loginAnonymous: loginAnonymous);
            Error = error;
            await Task.Run(() => { p.WaitForExit(); });
            return p;
        }

    }
}
