using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;


namespace TeemoAntiGap
{
    class Program
    {
        public static Obj_AI_Hero Player;
        public static Spell _R;
        public static Spell _W;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            _R = new Spell(SpellSlot.R, 200);
            _W = new Spell(SpellSlot.W);
            Player = ObjectManager.Player;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Game.PrintChat("Loaded Teemo Antigap");
        }
        public static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            Game.PrintChat("1"); //debugging
            if (Player.Distance(gapcloser.End) <= 200)
            {
                
                if (Player.IsFacing(gapcloser.Sender))
                {
                    Game.PrintChat("2"); //debugging
                    _W.Cast(true);
                    _R.Cast(gapcloser.End, true);
                }
                else
                {
                    Game.PrintChat("3"); //debugging
                    _R.Cast(Player.Position + (Player.Direction * 150), true);
                }
            }
            if (Player.Distance(gapcloser.End) <= 600)
            {
                Game.PrintChat("4"); //debugging
                _R.Cast(Player.Position + (Player.Direction * 150), true);
            }
        }
    }
}
