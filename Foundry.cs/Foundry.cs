using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.IO;
using System.Text;

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
        public Foundry(ServerConfig serverData) : base(serverData)
        {
            //create a random seed
            serverData.ServerMap = new Random().Next().ToString();
            base.serverData = serverData;
        }


        // - Game server Fixed variables
        public override string StartPath => @".\FoundryDedicatedServer.exe"; // Game server start path
        public string ConfigFile = "App.cfg";
        public string FullName = "Foundry Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = false;  // Only Embedd Console
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation

        // TODO: Undisclosed method
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()

        // - Game server default values
        public string ServerName = "HappyPlace";
        public string Port = "3724"; // Default port
        public string QueryPort = "3724"; // Default port
        public string Defaultmap = "1"; // Sets the map seed used to generate the world.
        public string Maxplayers = "32"; // Default maxplayers

        public string ServerPassword = "only_friends"; //password to connect
        public string DefaultPause = "true"; //  Will the server pause when nobody is connected.
        public string DefaultAutoSaveInterval = "300"; // Sets the autosave frequency in seconds.
        public string DefaultPublic = "true"; // Sets whether the server is listed on the Steam server browser.
        // TODO: Unsupported option

        // TODO: May not support
        public string Additional = ""; // Additional server start parameter


        // - Create a default cfg for the game server after installation
        public async void CreateAppCFG()
        {
            string fileName = "app.cfg";
            // Chemin complet du fichier
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            try
            {
                // Créer le fichier
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Écrire les informations dans le fichier
                    writer.WriteLine($"//Available Options:");
                    writer.WriteLine($"//server_world_name");
                    writer.WriteLine($"//Sets the server world name. This is the folder where the save files will be stored.");
                    writer.WriteLine($"server_world_name={serverData.ServerName}");
                    writer.WriteLine($"//server_password");
                    writer.WriteLine($"//Sets the server password.");
                    writer.WriteLine($"server_password={ServerPassword}");
                    writer.WriteLine($"//pause_server_when_empty");
                    writer.WriteLine($"//Will the server pause when nobody is connected.");
                    writer.WriteLine($"pause_server_when_empty={DefaultPause}");
                    writer.WriteLine($"//autosave_interval");
                    writer.WriteLine($"//Sets the autosave frequency in seconds.");
                    writer.WriteLine($"autosave_interval={DefaultAutoSaveInterval}");
                    writer.WriteLine($"//server_is_public");
                    writer.WriteLine($"//Sets whether the server is listed on the Steam server browser.");
                    writer.WriteLine($"server_is_public={DefaultPublic}");
                    writer.WriteLine($"//server_port");
                    writer.WriteLine($"//Sets the network port used by the game. Default is 3724.");
                    writer.WriteLine($"server_port={serverData.ServerPort}");
                    writer.WriteLine($"//map_seed");
                    writer.WriteLine($"//Sets the map seed used to generate the world.");
                    writer.WriteLine($"map_seed={serverData.ServerMap}");
                    writer.WriteLine($"//server_persistent_data_override_folder");
                    writer.WriteLine($"//Sets the absolute folder where things like logs and save files will be stored. This is mostly used by server providers so that they can run multiple dedicated servers on a single machine.");
                    writer.WriteLine($"server_persistent_data_override_folder={serverData.ServerName}");
                    writer.WriteLine($"//server_name");
                    writer.WriteLine($"//This is the name of the server listed in the Steam server browser.");
                    writer.WriteLine($"server_name={serverData.ServerName}");
                    writer.WriteLine($"//server_max_players");
                    writer.WriteLine($"//This sets the max amount of players on a server.");
                    writer.WriteLine($"server_max_players={serverData.ServerMaxPlayer}");
                    writer.Close();
                }

                var serverConsole = new ServerConsole(serverData.ServerID);
                serverConsole.Add($"ConfigFile {ConfigFile} successfully created!");
            }
            catch (Exception ex)
            {
                Error = $"Error Occured : {ex.Message}";
            }
        }

        public void UpdateCFG()
        {
            // Chemin complet du fichier
            string filePath = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, ConfigFile);
            StringBuilder sb = new StringBuilder();
            try
            {
                StreamReader sr = new StreamReader(filePath);
                var line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Contains("server_world_name"))
                    {
                        sb.Append($"server_world_name={serverData.ServerName}");
                        continue;
                    }
                    if (line.Contains("server_port"))
                    {
                        sb.Append($"server_port={serverData.ServerPort}");
                        continue;
                    }
                    if (line.Contains("server_name"))
                    {
                        sb.Append($"server_name={serverData.ServerName}");
                        continue;
                    }
                    if (line.Contains("map_seed"))
                    {
                        sb.Append($"map_seed={serverData.ServerMap}");
                        continue;
                    }
                    if (line.Contains("server_max_players"))
                    {
                        sb.Append($"server_max_players={serverData.ServerMaxPlayer}");
                        continue;
                    }

                    sb.Append(line);

                    line = sr.ReadLine();
                }
                sr.Close();

                File.WriteAllText(filePath, sb.ToString());

                var serverConsole = new ServerConsole(serverData.ServerID);
                serverConsole.Add($"ConfigFile {ConfigFile} successfully updated!");
            }
            catch (Exception ex)
            {
                Error = $"Error Occured : {ex.Message}";
            }
        }


        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string shipExePath = Functions.ServerPath.GetServersServerFiles(serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }
            UpdateCFG();
            // Prepare start parameter
            string param = "FactoryGame -log -unattended";
            param += $" {serverData.ServerParam}";
            //param += string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" -Port={_serverData.ServerPort}"; 
            //param += string.IsNullOrWhiteSpace(_serverData.ServerQueryPort) ? string.Empty : $" -ServerQueryPort={_serverData.ServerQueryPort}";
            //param += string.IsNullOrWhiteSpace(_serverData.ServerIP) ? string.Empty : $" -Multihome={_serverData.ServerIP}";

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(serverData.ServerID),
                    FileName = shipExePath,
                    Arguments = param,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                },
                EnableRaisingEvents = true
            };

            var serverConsole = new ServerConsole(serverData.ServerID);
            p.OutputDataReceived += serverConsole.AddOutput;
            p.ErrorDataReceived += serverConsole.AddOutput;

            // Start Process
            try
            {
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

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
                p.WaitForExit(7000);
                p.Kill();
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
