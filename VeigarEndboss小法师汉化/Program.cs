﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace VeigarEndboss
{
    class Program
    {
        private static readonly string champName = "Veigar";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        private static Spell Q, W, E, R;
        public static List<Spell> SpellList = new List<Spell>();

        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        private static Menu menu;
        private static Orbwalking.Orbwalker OW;
        public static Items.Item Dfg = new Items.Item(3128, 750);

        public static void Main(string[] args)
        {
            // Register load event
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            // Validate champion
            if (Player.ChampionName != champName)
                return;

            // Initialize spells
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 650);

            SpellList.AddRange(new[] { Q, W, E, R });
            
            Q.SetTargetted(0.25f, 1500);
            W.SetSkillshot(1.25f, 225, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, 1400);

            // Setup menu
            SetuptMenu();

            // Initialize classes
            BalefulStrike.Initialize(Q, OW);
            DarkMatter.Initialize(W);

            // Register additional events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Auto stack Q
            BalefulStrike.AutoFarmMinions = menu.SubMenu("misc").Item("miscStackQ").GetValue<bool>() && !menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active;
            // Auto W on stunned
            DarkMatter.AutoCastStunned = menu.SubMenu("misc").Item("miscAutoW").GetValue<bool>();

            // Combo
            if (menu.SubMenu("combo").Item("comboActive").GetValue<KeyBind>().Active)
                OnCombo();
            // Harass
            if (menu.SubMenu("harass").Item("harassActive").GetValue<KeyBind>().Active)
                OnHarass();
            // WaveClear
            if (menu.SubMenu("waveClear").Item("waveActive").GetValue<KeyBind>().Active)
                OnWaveClear();
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (W.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.W);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.R);

            if (Items.CanUseItem(3128))
                fComboDamage += Player.GetItemDamage(vTarget, Damage.DamageItems.Dfg);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }


        private static void OnCombo()
        {
            var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            if (target == null)
                return;

            var useQ = menu.SubMenu("combo").Item("comboUseQ").GetValue<bool>();
            var useW = menu.SubMenu("combo").Item("comboUseW").GetValue<bool>();
            var useE = menu.SubMenu("combo").Item("comboUseE").GetValue<bool>();
            var useR = menu.SubMenu("combo").Item("comboUseR").GetValue<bool>();

            var comboResult = GetComboDamage(target);
            var eResult = EventHorizon.GetCastPosition(target);

            if (target.Health < comboResult)
            { 
                if (IgniteSlot != SpellSlot.Unknown &&
                    Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                    Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }

                if (Dfg.IsReady() && target.Health < Player.GetItemDamage(target, Damage.DamageItems.Dfg))
                {
                    Dfg.Cast(target);
                }

                if (E.IsReady() && useE && eResult.Valid)
                    E.Cast(eResult.CastPosition);

                if (W.IsReady() && useW && W.InRange(target.ServerPosition))
                    W.Cast(target.Position);

                if (Q.IsReady() && useQ && Q.InRange(target.ServerPosition))
                    Q.CastOnUnit(target);

                if (R.IsReady() && useR && R.InRange(target.ServerPosition))
                    R.CastOnUnit(target);
            }
            else
            {
                if (Q.IsReady() && useQ && Q.InRange(target.ServerPosition))
                    Q.CastOnUnit(target);

                if (W.IsReady() && useW && W.InRange(target.ServerPosition))
                    W.Cast(target.Position);

                if (E.IsReady() && useE && eResult.Valid)
                    E.Cast(eResult.CastPosition);
            }
        }
        
        private static void OnHarass()
        {
            // Mana check
            if (Player.Mana / Player.MaxMana * 100 < menu.SubMenu("harass").Item("harassMana").GetValue<Slider>().Value)
                return;

            var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            if (target == null)
                return;

            // Q
            if (menu.SubMenu("harass").Item("harassUseQ").GetValue<bool>() && Q.IsReady() && Q.InRange(target.ServerPosition))
            {
                Q.CastOnUnit(target);
            }

            // W
            if (menu.SubMenu("harass").Item("harassUseW").GetValue<bool>() && W.IsReady())
            {
                W.Cast(target);
            }
        }

        private static void OnWaveClear()
        {
            if (menu.SubMenu("waveClear").Item("waveUseW").GetValue<bool>() && W.IsReady())
            {
                var farmLocation = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(Player.Position, W.Range).Select(minion => minion.ServerPosition.To2D()).ToList(), W.Width, W.Range);

                if (farmLocation.MinionsHit >= menu.SubMenu("waveClear").Item("waveNumW").GetValue<Slider>().Value && Player.Distance(farmLocation.Position) <= W.Range)
                    W.Cast(farmLocation.Position);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // Spell ranges
            foreach (var spell in SpellList)
            {
                var circleEntry = menu.SubMenu("drawings").Item("drawRange" + spell.Slot.ToString()).GetValue<Circle>();
                if (circleEntry.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, circleEntry.Color);
            }
        }

        private static void SetuptMenu()
        {
            // Create menu
            menu = new Menu("[灏忔硶] " + champName, "hells" + champName, true);

            // Target selector
            Menu targetSelector = new Menu("鐩爣閫夋嫨", "ts");
            SimpleTs.AddToMenu(targetSelector);
            menu.AddSubMenu(targetSelector);

            // Orbwalker
            Menu orbwalker = new Menu("璧扮爫閫夐」", "orbwalker");
            OW = new Orbwalking.Orbwalker(orbwalker);
            menu.AddSubMenu(orbwalker);

            // Combo
            Menu combo = new Menu("杩炴嫑", "combo");
            combo.AddItem(new MenuItem("comboUseQ", "浣跨敤Q").SetValue(true));
            combo.AddItem(new MenuItem("comboUseW", "浣跨敤W").SetValue(true));
            combo.AddItem(new MenuItem("comboUseE", "浣跨敤E").SetValue(true));
            combo.AddItem(new MenuItem("comboUseR", "浣跨敤R").SetValue(true));
            combo.AddItem(new MenuItem("comboUseIgnite", "浣跨敤鐐圭噧").SetValue(true));
            combo.AddItem(new MenuItem("comboActive", "蹇嵎閿").SetValue(new KeyBind(32, KeyBindType.Press)));
            menu.AddSubMenu(combo);

            // Harass
            Menu harass = new Menu("楠氭壈", "harass");
            harass.AddItem(new MenuItem("harassUseQ", "浣跨敤Q").SetValue(true));
            harass.AddItem(new MenuItem("harassUseW", "浣跨敤W").SetValue(true));
            harass.AddItem(new MenuItem("harassMana", "钃濋噺>%").SetValue(new Slider(30)));
            harass.AddItem(new MenuItem("harassActive", "蹇嵎閿").SetValue(new KeyBind('C', KeyBindType.Press)));
            menu.AddSubMenu(harass);

            // WaveClear
            Menu waveClear = new Menu("娓呯嚎", "waveClear");
            waveClear.AddItem(new MenuItem("waveUseW", "浣跨敤W").SetValue(true));
            waveClear.AddItem(new MenuItem("waveNumW", "钃濋噺>%").SetValue(new Slider(3, 1, 10)));
            waveClear.AddItem(new MenuItem("waveActive", "蹇嵎閿").SetValue(new KeyBind('V', KeyBindType.Press)));
            menu.AddSubMenu(waveClear);

            // Misc
            Menu misc = new Menu("鏉傞」", "misc");
            misc.AddItem(new MenuItem("miscStackQ", "鑷姩Q").SetValue(true));
            misc.AddItem(new MenuItem("miscAutoW", "EW杩炴嫑").SetValue(true));
            menu.AddSubMenu(misc);

            // Drawings
            Menu drawings = new Menu("鏄剧ず", "drawings");
            drawings.AddItem(new MenuItem("drawRangeQ", "Q鑼冨洿").SetValue(new Circle(true, Color.FromArgb(150, Color.IndianRed))));
            drawings.AddItem(new MenuItem("drawRangeW", "W鑼冨洿").SetValue(new Circle(true, Color.FromArgb(150, Color.MediumPurple))));
            drawings.AddItem(new MenuItem("drawRangeE", "E鑼冨洿").SetValue(new Circle(false, Color.FromArgb(150, Color.DarkRed))));
            drawings.AddItem(new MenuItem("drawRangeR", "R鑼冨洿").SetValue(new Circle(false, Color.FromArgb(150, Color.Red))));
            menu.AddSubMenu(drawings);

            // Finalize menu
            menu.AddToMainMenu();
        }
    }
}