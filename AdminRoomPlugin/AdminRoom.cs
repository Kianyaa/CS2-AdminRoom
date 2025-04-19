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
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Kianya";
    public override string ModuleDescription => "Teleport or Set admin room to the current map";

    public Config Config { get; set; } = new Config();
    private const string ConfigFilePath = "../../csgo/addons/counterstrikesharp/configs/adminroom_config.json";
    private int _countfind = 0;
    private List<CBaseEntity>? _listofButton = new List<CBaseEntity>();

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

        RegisterEventHandler<EventRoundStart>(EventOnRoundStart);

    }

    public override void Unload(bool hotReload)
    {

        DeregisterEventHandler<EventRoundStart>(EventOnRoundStart);
    }

    [ConsoleCommand("css_adminroom", " -set for set adminroom, -find for auto find adminroom, -range show all possible range index button, no arg for teleport to admin room (if set)")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void AdminRoomCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        // Check if the player is valid and connected
        if (player == null || player is not { IsValid: true, PlayerPawn.IsValid: true } ||
            player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return;
        }

        if (player.Team == CsTeam.None || player.Team == CsTeam.Spectator || player.PawnIsAlive == false)
        {
            return;
        }

        // Get the player's position
        var playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null || !playerPawn.IsValid)
        {
            return;
        }

        // Get Map Name
        var mapName = Server.MapName;

        var commandarg_0 = commandInfo.GetArg(1);
        var commandarg_1 = commandInfo.GetArg(2);

        // Check if the command argument is "-find"



        if (commandarg_0.ToLower() == "-find")
        {
            if (commandarg_1 == "")
            {

                if (_listofButton == null || _listofButton.Count == 0)
                {
                    player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Yellow}Failed to search admin room for this map");
                    return;
                }

                if (_countfind >= _listofButton!.Count)
                {
                    _countfind = 0;
                }

                var adminrooment = _listofButton?[_countfind];


                if (adminrooment == null)
                {
                    player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Yellow}Failed to search admin room for this map");

                    return;
                }

                playerPawn?.Teleport(adminrooment.AbsOrigin, adminrooment.AbsRotation);

                player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}auto finder success found the admin room");

                if (_listofButton.Count != 0 || _listofButton!.Count != null)
                {
                    player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}button index range [0 - {_listofButton!.Count - 1}] Current index: {ChatColors.Red}{_countfind}");
                    player.PrintToChat($" {ChatColors.Default}Current position: {adminrooment.AbsOrigin}, {adminrooment.AbsRotation}");

                }

                _countfind++;

                return;

            }
            
            else 
            {
                if (int.TryParse(commandarg_1, out int index) && _listofButton != null && index >= 0 && index < _listofButton.Count)
                {
                    var adminrooment = _listofButton[index];

                    playerPawn?.Teleport(adminrooment.AbsOrigin, adminrooment.AbsRotation);

                    player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}Teleported to admin room index {index}");
                    player.PrintToChat($" {ChatColors.Default}Current position: {adminrooment.AbsOrigin}, {adminrooment.AbsRotation}");

                    _countfind++;
                }
                else
                {
                    player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Yellow}Invalid input index");
                }

                return;
            }

        }

        if (commandarg_0.ToLower() == "-set")
        {

            var playerPosition = playerPawn?.AbsOrigin ?? new Vector(0);
            var playerAngles = playerPawn?.AbsRotation ?? new QAngle(0);


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

            return;
        }

        if (commandarg_0.ToLower() == "-range")
        {
            player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}index admin button possible range [0 - {_listofButton!.Count - 1}]");
        }


        if (commandarg_0.ToLower() == "")
        {
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

            return;
        }

        else
        {
            player.PrintToChat($" {ChatColors.Green}[AdminRoom] {ChatColors.Default}Invalid input arg (-find, -set)");
        }
        
    }

    [GameEventHandler(HookMode.Post)]
    public HookResult EventOnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        var entities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_button");

        if (entities == null)
        {
            return HookResult.Continue;
        }

        _listofButton!.Clear();
        _countfind = 0;

        foreach (var ent in entities)
        {
            if (ent.DesignerName != null && ent.DesignerName.ToLower().Contains("func_button"))
            {

                string fileNameWithExtension = ent.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName.Substring(ent.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName.LastIndexOf('/') + 1);
                string fileNameWithoutExtension = fileNameWithExtension.Substring(0, fileNameWithExtension.LastIndexOf('.'));


                if (fileNameWithoutExtension.ToLower().Split("_").Any(part => part is "admin" or "stage" or "level" or "level1" or "level2" or "level3" or "lvl" or "act" or "extreme" or "ex" or "ex1" or "ex2" or "ex3" or "ext" or "ext1" or "ext2" or "ext3" or "round" or "kill" or "restart" or "nuke"))
                {

                    //Server.PrintToChatAll($"[Button] fileNameWithExtension: {fileNameWithoutExtension}");

                    _listofButton?.Add(ent);
                }
            }

        }

        return HookResult.Continue;
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