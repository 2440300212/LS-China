using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Reverse_Support
{
    internal class Reverse_Support
    {
        public static Obj_AI_Hero Player;
        public Menu Config;

    

    public Reverse_Support()
    {
        CustomEvents.Game.OnGameLoad += OnGameLoad;

    }

        private void OnGameLoad(EventArgs args)
        {
            Game.OnGameUpdate += OnGameUpdate;
            Player = ObjectManager.Player;
            Config = new Menu("Hecarim Reverse Support", "Reverse", true);
            Config.AddToMainMenu();
            Config.AddSubMenu(new Menu("Reverse Support Settings", "Reverse"));
            Config.SubMenu("Reverse").AddItem(new MenuItem("Toggle", "Activate?").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));

            Config.SubMenu("Reverse").AddItem(new MenuItem("Slider", "Minion Health").SetValue(new Slider(100, 50, 500)));
            
            Game.PrintChat("<font color='#F7A100'>Reverse Support Hecarim by XcxooxL Loaded 1.0 .</font>");


        }

        private void OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Toggle").GetValue<KeyBind>().Active) //works NOW :D!
            {
               var getm = MinionManager.GetMinions(Player.Position, 500, MinionTypes.All, MinionTeam.Ally, MinionOrderTypes.Health);
               if (getm.Count == 0) return;
                    if (getm[0].Health <= Config.Item("Slider").GetValue<Slider>().Value)// 
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, getm[0].Position);
                    }
            }
        }
    }

    }