using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using xSLx_Orbwalker;

namespace Twilight_s_Auto_Carry___Kalista
{
    class Program
    {
        private static Menu Config;
        private static Obj_AI_Hero Player;
        private static Spell Q = new Spell(SpellSlot.Q, 1450);
        private static Spell W = new Spell(SpellSlot.W, 5500);
        private static Spell E = new Spell(SpellSlot.E, 1200);
        private static Spell R = new Spell(SpellSlot.R, 1200);
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Load;
        }
        public static void Load(EventArgs args)
        {
            
            Game.PrintChat("=========================");
            Game.PrintChat("| Twilight Auto Carry   |");
            Game.PrintChat("=========================");
            Game.PrintChat("Loading kalista plugin!");
            Game.PrintChat("Kalista loaded!");
            Game.PrintChat("=========================");
            Player = ObjectManager.Player;
            
            Config = new Menu("TAC: Kalista", "Kalista", true);
            Config.AddItem(new MenuItem("", "Version: 1.0.5"));
            Config.AddItem(new MenuItem("", "============"));
            Config.AddItem(new MenuItem("", "Not working:"));
            Config.AddItem(new MenuItem("", "WallHop options/draw"));
            Config.AddItem(new MenuItem("", "============"));
            
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            var orbwalkerMenu = new Menu("xSLx Orbwalker", "xSLx_Orbwalker");
            xSLxOrbwalker.AddToMenu(orbwalkerMenu);
            Config.AddSubMenu(orbwalkerMenu);
            Config.AddSubMenu(new Menu("AutoCarry options", "ac"));
            Config.SubMenu("ac").AddItem(new MenuItem("UseQAC", "Use Q").SetValue(true));
            Config.SubMenu("ac").AddItem(new MenuItem("UseEAC", "Use E").SetValue(true));
            Config.SubMenu("ac").AddItem(new MenuItem("Detonate", "Use E at").SetValue(new Slider(1, 1, 40)));
            Config.SubMenu("ac").AddItem(new MenuItem("DetonateAuto", "Auto E").SetValue(true));
            Config.SubMenu("ac").AddItem(new MenuItem("QManaMinAC", "Min Q Mana %").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("ac").AddItem(new MenuItem("EManaMinAC", "Min E Mana %").SetValue(new Slider(35, 1, 100)));
            Config.AddSubMenu(new Menu("Harass options", "harass"));
            Config.SubMenu("harass").AddItem(new MenuItem("stackE", "E stacks to cast").SetValue(new Slider(1, 1, 10)));
            Config.SubMenu("harass").AddItem(new MenuItem("EManaMinHS", "Min E Mana %").SetValue(new Slider(35, 1, 100)));

            Config.AddSubMenu(new Menu("Wall Hop options", "wh"));
            Config.SubMenu("wh").AddItem(new MenuItem("drawSpot", "Draw WallHop spots").SetValue(true));
            Config.SubMenu("wh").AddItem(new MenuItem("whk", "WallHop key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("drawText", "Draw text").SetValue(true));
            Config.AddItem(new MenuItem("Packets", "Packet Casting").SetValue(true));
            Config.AddItem(new MenuItem("debug", "Debug").SetValue(true));
            Config.AddToMainMenu();
            Game.OnGameUpdate += OnGameUpdate;
        }
        private static void InitializeLevelUpManager()
        {
            var priority1 = new int[] { 
                1, // level 1
                3, // level 2
                1, // level 3
                2, // level 4
                1, // level 5
                4, // level 6
                1, // level 7
                3, // level 8
                1, // level 9
                3, // level 10
                4, // level 11
                3, // level 12
                3, // level 13
                2, // level 14
                2, // level 15
                4, // level 16
                2, // level 17
                2  // level 18
            };
        }
        public static float getPerValue(bool mana)
        {
            if (mana) return (Player.Mana / Player.MaxMana) * 100;
            return (Player.Health / Player.MaxHealth) * 100;
        }
        public static void OnGameUpdate(EventArgs args)
        {
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    Combo();
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    Harass();
                    break;
            }
        }
        public static void WallHop()
        {
            //
        }
        private static float GetRealDistance(GameObject target)
        {
            return ObjectManager.Player.Position.Distance(target.Position) + ObjectManager.Player.BoundingRadius +
            target.BoundingRadius;
        }
        public static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            var distance = GetRealDistance(target);
            if (Q.IsReady() && Player.Distance(target.Position) < Q.Range)
            {
                Q.Cast(target, false);
            }
            if (E.IsReady() && distance <= E.Range && target.Health >= getDamageToTarget(target))
            {
                E.Cast(target, packetCast());
            }
        }
        private static bool packetCast()
        {
            return Config.Item("Packets").GetValue<bool>();
        }
        public static void Harass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            //var minList = MinionManager.GetMinions(Player.Position, 550f).Where(min => min.Health < Q.GetDamage(min));
            
            var ManaE = Config.Item("EManaMinHS").GetValue<Slider>().Value;
            foreach (var buff in target.Buffs.Where(buff => buff.DisplayName.ToLower() == "kalistarend").Where(buff => buff.Count == Config.Item("stackE").GetValue<Slider>().Value))
            {
                if (E.IsReady() && getPerValue(true) >= ManaE)
                    E.Cast(target, packetCast());
            }

        }
        public static int getDamageToTarget(Obj_AI_Hero target)
        {
            foreach (var buff in target.Buffs.Where(buff => buff.DisplayName.ToLower() == "kalistarend").Where(buff => buff.Count == 6))
            {
                return (int)target.GetSpellDamage(Player, SpellSlot.E) * buff.Count;
            }
            return 0;
        }

        public static float CalculateRendDamage(Obj_AI_Hero eTarget)
        {
            if (E.IsReady())
            {
//                var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
                if (eTarget.IsValidTarget(E.Range))
                {
                    foreach (var buff in eTarget.Buffs.Where(buff => buff.DisplayName.ToLower() == "kalistarend").Where(buff => buff.Count == 6))
                    {
                        Game.PrintChat("Total stacks on target " + eTarget.ChampionName + " Count: " + buff.Count + " Total Damage: " + eTarget.GetSpellDamage(Player,SpellSlot.E)*buff.Count);
                        return (float)eTarget.GetSpellDamage(Player, SpellSlot.E) * buff.Count;
//                        E.Cast();
                    }
                }

            }
            return (float)0;
        }
    }
}
