using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace GragasTheDrunkCarry
{
    class Gragas
    {
        public static Obj_AI_Hero Player;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;
        public static GameObject Bomb;
        public static Vector2 Rpos;

        public Gragas()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 775);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 1050);
            Q.SetSkillshot(0.3f, 110f, 1000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.3f, 50, 1000, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.3f, 700, 1000, false, SkillshotType.SkillshotCircle);
            Config = new Menu("Gragas", "Gragas", true);
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoB", "Auto Bomb?").SetValue(true));
            Config.AddItem(
    new MenuItem("Insec", "Insec").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            Config.AddSubMenu(new Menu("Harras", "Harras"));
            Config.SubMenu("Harras").AddItem(new MenuItem("UseQH", "Use Q?").SetValue(true));
            Config.SubMenu("Harras").AddItem(new MenuItem("UseEH", "Use E?").SetValue(true));
            Config.AddSubMenu(new Menu("Draw", "Draw"));
            Config.SubMenu("Harras").AddItem(new MenuItem("DrawIN", "Draw Insec Pos?").SetValue(true));
            Config.SubMenu("Harras").AddItem(new MenuItem("DrawQ", "Draw Q range").SetValue(true));
            Config.SubMenu("Harras").AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(true));
            Config.SubMenu("Harras").AddItem(new MenuItem("Draw R", "Draw R Range").SetValue(true));
            Config.AddToMainMenu();

            Player = ObjectManager.Player;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnEndScene += Drawing_OnEndScene;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += GameObject_OnDelete;

        }
        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Gragas_Base_R_End.troy")
            {
                if (Config.Item("Insec").GetValue<KeyBind>().Active)
                {
                    InsecCombo(sender.Position.To2D());
                }
            }
            if (sender.Name == "Gragas_Base_Q_Ally.troy")
            {
                Bomb = sender;
            }
        }

        static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Gragas_Base_Q_Ally.troy")
            {
                Bomb = null;
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            var vTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (Orbwalker.ActiveMode.ToString().ToLower() == "combo")
            {
                if (Config.Item("UseQ").GetValue<bool>() && Q.IsReady())
                {
                    Qcast(vTarget);
                }
                else
                {
                    if (E.IsReady() && Player.Distance(vTarget) <= E.Range && Config.Item("UseE").GetValue<bool>())
                    {
                        E.Cast(vTarget, true);
                    }
                    else
                    {
                        if (Config.Item("UseW").GetValue<bool>() && W.IsReady())
                        {
                            W.Cast();
                        }
                        else
                        {
                            if (Config.Item("UseR").GetValue<bool>() && R.IsReady() && GetCDamage(vTarget) >= vTarget.Health)
                            {
                                R.Cast(vTarget);
                            }
                        }
                    }
                }

            }
            if (Orbwalker.ActiveMode.ToString().ToLower() == "mixed")
            {
                if (Config.Item("UseQH").GetValue<bool>() && Q.IsReady())
                {
                    Qcast(vTarget);
                }
                else
                {
                    if (Config.Item("UseEH").GetValue<bool>() && E.IsReady() && Player.Distance(vTarget) <= E.Range)
                    {
                        E.Cast(vTarget, true);
                    }
                }
            }


            if (Config.Item("AutoB").GetValue<bool>() && Bomb != null)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.Distance(Bomb.Position) <= 250))
                {
                    Qcast(hero);
                }
            }

            if (Config.Item("Insec").GetValue<KeyBind>().Active)
            {
                Insec(vTarget);
            }
        }

        private static int GetCDamage(Obj_AI_Base target)
        {
            var damage = 0;
            if (Q.IsReady())
            {
                damage += (int)Q.GetDamage(target);
            }
            if (E.IsReady())
            {
                damage += (int)E.GetDamage(target);
            }
            if (R.IsReady())
            {
                damage += (int)R.GetDamage(target);
            }
            return damage;
        }


        private static void Qcast(Obj_AI_Base target)
        {
            if (!Config.Item("UseQ").GetValue<bool>()) return;
            if (!(target.Distance(Player) <= Q.Range)) return;
            if (Bomb == null)
            {
                Q.Cast(target, true);
            }

            if (Bomb != null && target.Distance(Bomb.Position) <= 250)
            {
                Q.Cast();
            }
        }

        private static void InsecCombo(Vector2 pos)
        {
            var vTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (!(vTarget.Distance(pos) <= 700)) return;
            var newpos = pos.Extend(vTarget.Position.To2D(), 700);
            Q.Cast(newpos, true);
            E.Cast(newpos, true);
        }

        public static void Insec(Obj_AI_Hero target)
        {
            Rpos = Player.Position.To2D().Extend(target.Position.To2D(), Player.Distance(target) + 250);
            if (Rpos.Distance(Player.Position) <= R.Range)
            {
                if (Player.Distance(Rpos.Extend(target.Position.To2D(), 700 - target.Distance(Rpos))) < E.Range && !IsWall(Rpos.To3D()) && target.IsFacing(Player))
                {
                    R.Cast(Rpos);
                }
            }
        }
        private static bool IsWall(Vector3 pos)
        {
            CollisionFlags cFlags = NavMesh.GetCollisionFlags(pos);
            return (cFlags == CollisionFlags.Wall);
        }
        static void Drawing_OnEndScene(EventArgs args)
        {
            var vTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (vTarget != null && R.IsReady() && Config.Item("DrawIN").GetValue<bool>())
            {
                Utility.DrawCircle(Rpos.To3D(), 50, Color.Red);
            }
            if (Config.Item("DrawQ").GetValue<bool>())
            {
                Utility.DrawCircle(Player.Position, Q.Range, Color.DarkSlateGray);
            }
            if (Config.Item("DrawE").GetValue<bool>())
            {
                Utility.DrawCircle(Player.Position, E.Range, Color.DarkSlateGray);
            }
            if (Config.Item("DrawR").GetValue<bool>())
            {
                Utility.DrawCircle(Player.Position, R.Range, Color.DarkSlateGray);
            }
            
            
        }
    }

}
