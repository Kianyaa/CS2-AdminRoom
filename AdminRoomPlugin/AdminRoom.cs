using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json;
using Microsoft.Extensions.Logging;


namespace AdminRoomPlugin;

public class PositionConfig
{
    public int MapWorkshopId { get; set; } = 0; // Recently not using this
    public float X { get; set; } = 0;
    public float Y { get; set; } = 0;
    public float Z { get; set; } = 0;
    public float AngleX { get; set; } = 0;
    public float AngleY { get; set; } = 0;
    public float AngleZ { get; set; } = 0;
}

public class Config : BasePluginConfig
{
    public Dictionary<string, PositionConfig> AdminRoomPlugin { get; set; } = new();
}

public class AdminRoomPlugin : BasePlugin
{
    public override string ModuleName => "AdminRoomPlugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kianya";
    public override string ModuleDescription => "Teleport or Set admin room to the current map";

    public Config Config { get; set; } = new Config();
    private const string ConfigFilePath = "../../csgo/addons/counterstrikesharp/configs/adminroom_config.json";

    // Method to handle the parsing of the config file
    public void OnConfigParsed(Config config)
    {
        Config = config;
    }
    
    public override void Load(bool hotReload)
    {
        LoadConfigFromFile(ConfigFilePath);

        if (!ValidateConfig(Config))
        {
            Logger.LogError("Invalid configuration detected. Please check the configuration file.");
        }
        else
        {
            Logger.LogInformation("AdminRoomPlugin Config loaded successfully.");
        }

    }

    [ConsoleCommand("css_adminroom", "Teleport admin to admin room can setpos")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void AdminRoomCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        // Check if the player is valid and connected
        if (player == null || player is not { IsValid: true, PlayerPawn.IsValid: true } ||
            player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return;
        }

        // Get the player's position
        var playerPawn = player.PlayerPawn.Value;

        // Get Map Name
        var mapName = Server.MapName;
        

        // Get the position of the admin room from the json file
        if (Config.AdminRoomPlugin.TryGetValue(mapName, out var positionConfig))
        {
            var adminRoomPosition = new Vector(positionConfig.X, positionConfig.Y, positionConfig.Z);
            var adminRoomAngles = new QAngle(positionConfig.AngleX, positionConfig.AngleY, positionConfig.AngleZ); // Adjust angles as needed

            // Executing the command to teleport the player to the admin room

            playerPawn?.Teleport(adminRoomPosition, adminRoomAngles);

            player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}Teleported you to admin room.");
        }
        else
        {
            player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}Admin room position not set for this map yet.");
        }

    }

    [ConsoleCommand("css_adminroomset", "Set position of admin room on map")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void AdminRoomSetCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        // Check if the player is valid and connected
        if (player == null || player is not { IsValid: true, PlayerPawn.IsValid: true } ||
            player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return;
        }

        // Get the player's position for set the admin room
        var playerPawn = player.PlayerPawn.Value;

        var playerPosition = playerPawn?.AbsOrigin ?? new Vector(0);
        var playerAngles = playerPawn?.AbsRotation ?? new QAngle(0);

        // Set this position as the admin room in the json file
        var mapName = Server.MapName;

        Config.AdminRoomPlugin[mapName] = new PositionConfig
        {
            MapWorkshopId = 0, // Recently not using this

            // Axis for the position
            X = playerPosition.X,
            Y = playerPosition.Y,
            Z = playerPosition.Z,

            // Axis for the angles FOV
            AngleX = playerAngles.X,
            AngleY = playerAngles.Y,
            AngleZ = playerAngles.Z
        };

        SaveConfigToFile(ConfigFilePath);

        player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}Set current position in {ChatColors.Red}{Server.MapName} {ChatColors.Default}as admin room.");
        player.PrintToChat($" {ChatColors.Default}Current Position: {playerPosition[0]}, {playerPosition[1]}, {playerPosition[2]}");

    }

    public void SaveConfigToFile(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(Config, options);
        File.WriteAllText(filePath, jsonString);

        Logger.LogInformation($"Updated Admin room to {Server.MapName}");
    }

    public void LoadConfigFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var jsonString = File.ReadAllText(filePath);
            Config = JsonSerializer.Deserialize<Config>(jsonString) ?? new Config();
        }
    }

    public bool ValidateConfig(Config config)
    {
        foreach (var entry in config.AdminRoomPlugin)
        {
            var positionConfig = entry.Value;

            if ((positionConfig.X == 0 && positionConfig.Y == 0 && positionConfig.Z == 0) || (positionConfig.X is string || positionConfig.Y is string || positionConfig.Z is string))
            {
                Logger.LogError($"PositionConfig for map '{entry.Key}' has invalid coordinates or invalid type.");
                return false;
            }

        }

        return true;
    }
}