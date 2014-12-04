using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;


namespace ScaryPing
{
    class Program
    {
        public static Menu Config;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad +=Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("Scary Ping", "Scary", true);
            
            Config.AddItem(
                new MenuItem("Scary", "Scary Ping").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press, false)));
            Config.AddToMainMenu();

            Game.OnGameUpdate +=Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Scary").GetValue<KeyBind>().Active)
            {
                Pentagon();
            }
        }

        private static void Pentagon()
        {
            Vector2 P1, P2, P3, P4, P5 = new Vector2();
            P1.X = (float)(UnitUnderCursor().Position.X + 500 * Math.Cos(1 * 2 * Math.PI / 5));
            P1.Y = (float)(UnitUnderCursor().Position.Y + 500 * Math.Sin(1 * 2 * Math.PI / 5) - 100);
            P2.X = (float)(UnitUnderCursor().Position.X + 500 * Math.Cos(2 * 2 * Math.PI / 5));
            P2.Y = (float)(UnitUnderCursor().Position.Y + 500 * Math.Sin(2 * 2 * Math.PI / 5) - 100);
            P3.X = (float)(UnitUnderCursor().Position.X + 500 * Math.Cos(3 * 2 * Math.PI / 5));
            P3.Y = (float)(UnitUnderCursor().Position.Y + 500 * Math.Sin(3 * 2 * Math.PI / 5) - 100);
            P4.X = (float)(UnitUnderCursor().Position.X + 500 * Math.Cos(4 * 2 * Math.PI / 5));
            P4.Y = (float)(UnitUnderCursor().Position.Y + 500 * Math.Sin(4 * 2 * Math.PI / 5) - 100);
            P5.X = (float)(UnitUnderCursor().Position.X + 500 * Math.Cos(5 * 2 * Math.PI / 5));
            P5.Y = (float)(UnitUnderCursor().Position.Y + 500 * Math.Sin(5 * 2 * Math.PI / 5) - 100);
            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(P1.X, P1.Y, 0, Packet.PingType.Fallback)).Send();
            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(P2.X, P2.Y, 0, Packet.PingType.Fallback)).Send();
            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(P3.X, P3.Y, 0, Packet.PingType.Fallback)).Send();
            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(P4.X, P4.Y, 0, Packet.PingType.Fallback)).Send();
            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(P5.X, P5.Y, 0, Packet.PingType.Fallback)).Send();
            Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(UnitUnderCursor().Position.X, UnitUnderCursor().Position.Y, 0, Packet.PingType.Fallback)).Send();
        }

        private static Obj_AI_Base UnitUnderCursor()
        {
            return ObjectManager
                .Get<Obj_AI_Base>()
                .Where(x => x.IsAlly)
                .FirstOrDefault(unit => Vector2.Distance(Game.CursorPos.To2D(), unit.ServerPosition.To2D()) < 200);
        }
    }
}
