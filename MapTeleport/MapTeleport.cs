using System;
using System.IO;
using System.IO.Streams;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace MapTeleport
{
    [ApiVersion(2, 1)]
    public class MapTeleport : TerrariaPlugin
    {
        public override string Author => "Quinci, updated by Maxthegreat99";
        public override string Description => "Teleports you to where you pinged on the map.";
        public override string Name => "MapTeleport";
        public override Version Version => new Version(1, 0, 0, 1);
        public MapTeleport(Main game) : base(game)
        {

        }
        public override void Initialize()
        {
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            Commands.ChatCommands.Add(new Command("mapteleport.use", ToggleMapTeleport, "mapteleport", "maptp") { HelpText = "Toggles map teleportation (Default: Disabled)"});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);

            }
            base.Dispose(disposing);
        }
        private void OnGreet(GreetPlayerEventArgs args) {
            var player = TShock.Players[args.Who];
			if (player == null || !player.Active)
				return;

			player.SetData("mapteleport", false);
        }
        private void ToggleMapTeleport(CommandArgs args) {
            args.Player.SetData<bool>("mapteleport",!args.Player.GetData<bool>("mapteleport"));
            args.Player.SendSuccessMessage((args.Player.GetData<bool>("mapteleport") ? "En" : "Dis") + "abled map teleport.");
        }
        private void OnGetData(GetDataEventArgs args) {
            using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, 3, args.Length - 1))
            {
                if (args.MsgID == PacketTypes.LoadNetModule) {
                    if (data.ReadByte() == 2) {
                        TSPlayer player = TShock.Players[args.Msg.whoAmI];
                        if (player.GetData<bool>("mapteleport") && player.HasPermission("mapteleport.use")) {
                            data.Position++;
                            int X = Math.Min(Main.maxTilesX, Math.Max(0, (int)data.ReadSingle()));
                            int Y = Math.Min(Main.maxTilesY, Math.Max(0, (int)data.ReadSingle()));
                            if (player.HasPermission("mapteleport.solid")) {
                                player.Teleport(X * 16, Y * 16);
                                player.SendSuccessMessage($"Teleported to ({X}, {Y})");
                            } else {
                                if ((Main.tile[X, Y] == null || !Main.tile[X, Y].active() || (!Main.tileSolid[Main.tile[X, Y].type] && Main.tile[X, Y].liquid == 0)) && (Main.tile[X + 1, Y] == null || !Main.tile[X + 1, Y].active() || (!Main.tileSolid[Main.tile[X + 1, Y].type] && Main.tile[X + 1, Y].liquid == 0)) && (Main.tile[X + 1, Y + 1] == null || !Main.tile[X + 1, Y + 1].active() || (!Main.tileSolid[Main.tile[X + 1, Y + 1].type] && Main.tile[X + 1, Y + 1].liquid == 0)) && (Main.tile[X, Y + 1] == null || !Main.tile[X, Y + 1].active() || (!Main.tileSolid[Main.tile[X, Y + 1].type] && Main.tile[X, Y + 1].liquid == 0)) && (Main.tile[X + 1, Y + 2] == null || !Main.tile[X + 1, Y + 2].active() || (!Main.tileSolid[Main.tile[X + 1, Y + 2].type] && Main.tile[X + 1, Y + 2].liquid == 0)) && (Main.tile[X, Y + 2] == null || !Main.tile[X, Y + 2].active() || (!Main.tileSolid[Main.tile[X, Y + 2].type] && Main.tile[X, Y + 2].liquid == 0))) {
                                    player.Teleport(X * 16, Y * 16);
                                    player.SendSuccessMessage($"Teleported to ({X}, {Y})");
                                } else {
                                    player.SendErrorMessage("You do not have permission to teleport into solid tiles.");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}