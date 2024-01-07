using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json;
using System.Numerics;



namespace RtvWithCsVotingSystem;

[MinimumApiVersion(120)]
public class RtvWithCsVotingSystem : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName { get; } = "RTV Advanced System";
    public override string ModuleVersion { get; } = "1.0.0";
    public override string ModuleDescription { get; } = "Voting For change map system using CS Hud ";
    public override string ModuleAuthor { get; } = "K4mY & BQN";
    public List<CCSPlayerController> connectedPlayers = new List<CCSPlayerController>();
    private Dictionary<string, DateTime> _rtvCooldown = new Dictionary<string, DateTime>();
    private Dictionary<string, Boolean> _rtvVoted = new Dictionary<string, Boolean>();
    private List<ulong> _rtvCount = new();
    private bool _canRtv = false;
    private string displayTime;
    private string displayTime2;
    private int timeleft2;
    private int gameStart;
    private int timelimit;
    private int currentTime;
    private int timeleft;
    bool hasPrinted = false;
    bool _rtvVotePassed = true;

    public PluginConfig Config { get; set; }

    public void OnConfigParsed(PluginConfig config)
    {
        config = ConfigManager.Load<PluginConfig>(ModuleName);
        Config = config;
    }
    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        RegisterEventHandler<EventBeginNewMatch>(EventOnMatchStart);
        RegisterEventHandler<EventRoundEnd>(EventOnRoundEnd);
        RegisterEventHandler<EventCsPreRestart>(EventOnCsPreRestart);
        RegisterEventHandler<EventEndmatchMapvoteSelectingMap>(EventOnEndMatchVote);

        RegisterListener<Listeners.OnTick>(() =>
        {
            CCSGameRules gameRules = GetGameRules();

             gameStart = (int)gameRules.GameStartTime;
             timelimit = (int)ConVar.Find("mp_timelimit").GetPrimitiveValue<float>() * 60;
             currentTime = (int)Server.CurrentTime;
             timeleft = timelimit - (currentTime - gameStart);
             timeleft2 = timelimit/2 - (currentTime - gameStart);
            if (timelimit == 0)
            {
                return;
            }
            if (timeleft2 < 0)
            {
                _canRtv = true;
            }
            
        if (timeleft2 == 0 && hasPrinted == false)
            {
                Server.PrintToChatAll($"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.RTVOn"]}");
                hasPrinted = true;
            }
           
            TimeSpan time = TimeSpan.FromSeconds(timeleft);
            DateTime dateTime = DateTime.Today.Add(time);
            displayTime = dateTime.ToString("mm:ss");
            DateTime dateTime2= DateTime.Today.Add(TimeSpan.FromSeconds(timeleft2));
            displayTime2 = dateTime2.ToString("mm:ss");
            foreach (var player in connectedPlayers)
            {
                if (player.IsValid || !player.IsBot)
                {
                    var buttons = player.Buttons;

                    if (buttons != 0 && buttons.ToString().Contains("858993"))
                    {
                        if (!player.IsBot)
                        {
                            if (Config.PluginMode == 0)
                            {
                                
                                string mainMessage = timeleft <= 0 ? Localizer["RTVWithCsVotingSystem.LastRound"] : Localizer["RTVWithCsVotingSystem.Timeleft"].ToString().Replace("{timeleft}", displayTime);
                                string rtvMessage = timeleft2 <= 0 ? Localizer["RTVWithCsVotingSystem.RTVOn"]   : Localizer["RTVWithCsVotingSystem.RTV"].ToString().Replace("{timeleft}", displayTime2);
                                string combinedMessage = $"{mainMessage}\n{rtvMessage}";
                                player.PrintToCenter(combinedMessage);


                            }

                            if (Config.PluginMode == 1)
                            {
                                
                                player.PrintToCenterHtml($"<br><br> {(timeleft <= 0 ? Localizer["RTVWithCsVotingSystem.LastRound"] : $" {Localizer["RTVWithCsVotingSystem.Timeleft"].ToString().Replace("{timeleft}", displayTime)}")}", 1);
                            }
                        }
                    }
                }
            }
        });
    }

    [ConsoleCommand("css_rtv", "Launch the RTV")]
    [CommandHelper(whoCanExecute:CommandUsage.CLIENT_ONLY)]
    public void OnRtVCommand(CCSPlayerController? player, CommandInfo cmd)
    {
       if ( _rtvVotePassed == true) { 
        const int rtvCooldownSeconds = 60;
        ulong steamID = player!.SteamID;
        string steamIDString = steamID.ToString();

        if (_rtvVoted.ContainsKey(steamIDString))
        {
            if (_rtvVoted[steamIDString] == true)
            {
                cmd.ReplyToCommand($"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.rtv_voted"]}");
                return;
            }
        }
        if (_rtvCooldown.ContainsKey(steamIDString))
        {
            var remainingCooldown = (_rtvCooldown[steamIDString] - DateTime.UtcNow).TotalSeconds;
            if (remainingCooldown >= 0)
            {
                cmd.ReplyToCommand($"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.rtv_cooldown", Math.Round(remainingCooldown)]}");
                return;
            }
        } 
        if (!_canRtv)
        {
            cmd.ReplyToCommand(
                $"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.rtv_not_available"]}");
            return;
        }
        
        if (_rtvCount.Contains(player!.SteamID) ) return;
        _rtvCount.Add(player.SteamID);
        _rtvCooldown[steamIDString] = DateTime.UtcNow.AddSeconds(rtvCooldownSeconds);
        _rtvVoted[steamIDString] = true;

        
        var required2 = connectedPlayers.Count;
        
        //TODO: Add message saying player has voted to rtv
        Server.PrintToChatAll($"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.rtv", player.PlayerName, _rtvCount.Count, Math.Round(required2 * 0.7)]}");

        if (_rtvCount.Count < Math.Round(required2 * 0.7)) return;
        
        VoteUsingCsHud();
        _rtvCooldown.Clear();
        _rtvVoted.Clear();
        _rtvVotePassed = false;
        }
        else
        {
             Server.PrintToChatAll($" {Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.rtv_passed"]}");
        }
    }
    
    [ConsoleCommand("css_unrtv", "Remove the RTV")]
    [CommandHelper(whoCanExecute:CommandUsage.CLIENT_ONLY)]
    public void OnUnRtVCommand(CCSPlayerController? player, CommandInfo cmd)
    {
        var required2 = connectedPlayers.Count;
        if (_rtvCount.Count >= Math.Round(required2 * 0.7))
        {
            cmd.ReplyToCommand(
$"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.rtv_passed"]}");
            return;
        }
        if (!_canRtv)
        {
            cmd.ReplyToCommand(
                $"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.unrtv_not_available"]}");
            return;
        }
        
        if (!_rtvCount.Contains(player.SteamID)) return;
        _rtvCount.Remove(player.SteamID);
        Server.PrintToChatAll(
            $"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.unrtv", player.PlayerName, _rtvCount.Count, Math.Round(required2 * 0.7)]}");
    }
    private HookResult EventOnEndMatchVote( EventEndmatchMapvoteSelectingMap @event , GameEventInfo info)
    {
        Server.PrintToChatAll($"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.vote_starting"]}");
        Server.PrintToChatAll($"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.vote_starting"]}");
        Server.PrintToChatAll($"{Localizer["RTVWithCsVotingSystem.prefix"]} {Localizer["RTVWithCsVotingSystem.vote_starting"]}");
        return HookResult.Continue;
    }
    private HookResult EventOnMatchStart(EventBeginNewMatch @event, GameEventInfo info)
    {
        OnMapEnd();
        return HookResult.Continue;
    }

    private HookResult EventOnCsPreRestart(EventCsPreRestart @event, GameEventInfo info)
    {

        OnMapEnd();
        
        return HookResult.Continue;
    }
    private HookResult EventOnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
            if (@event.Reason == 10)
        {
            OnMapEnd();
        }
    
        return HookResult.Continue;
    }

    private void VoteUsingCsHud()
    {
        Server.ExecuteCommand("mp_timelimit 1");

    }
    private static CCSGameRules GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
    }

    private void OnMapEnd()
    {
        _rtvCount.Clear();
        _canRtv = false;
        _rtvVotePassed = true;


    }
    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_tabtimeleft_reload")]
    public void OnReloadCommand(CCSPlayerController? controller, CommandInfo info)
    {
        OnConfigParsed(Config);
        Console.Write("[Tab Timeleft] Config reloaded!");
        if (controller != null)
        {
            controller.PrintToChat($" {ChatColors.Gold}[Tab Timeleft] {ChatColors.Green}Config reloaded!");
        }
    }
    [GameEventHandler]
    public HookResult OnPlayerConnectedFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if (!player.IsBot)
        {
            connectedPlayers.Add(player);
            Console.Write($"{player.PlayerName} added in connectedPlayers");
            return HookResult.Continue;
        }
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        _rtvCount.Remove(player.SteamID);
        connectedPlayers.Remove(player);
        Console.Write($"{player.PlayerName} removed in connectedPlayers");
        return HookResult.Continue;
    }


}
