using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

namespace KurisuMorgana
{
    /*  _____                         
     * |     |___ ___ ___ ___ ___ ___ 
     * | | | | . |  _| . | .'|   | .'|
     * |_|_|_|___|_| |_  |__,|_|_|__,|
     *               |___|            
     * Revision: 105-2 30/10/2014
     * + Added option to delay black shield
     * + Black shield should work (does for me)
     * 
     * Revision: 105-1 - 10/10/2014
     * + Lag free drawings
     * + Current target is now red (was white)
     * 
     * Revision: 105 - 05/10/2014
     * + Possible Q Hitchance Fix
     * 
     * Revision: 104 - 04/10/2014
     * + Imporved Q Prediction
     * + Combo Key added
     * + BlackShield Distance checks
     * + KurisuLib Updates
     * + Can choose hitchance in menu now
     * 
     * Revision 103
     * + Menu Updates
     * 
     * Rivision 102
     * + Should no walk to a target to cast W
     * 
     * Revision: 101
     * + KurisuLib Updates (Over 1000 lines) x.x
     * 
     * Revision: 100
     * + Release
     */

    internal class KurisuMorgana
    {
        private static Menu _config;
        private static Obj_AI_Hero _target;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero _player = ObjectManager.Player;

        public KurisuMorgana()
        {
            
            Console.WriteLine("Kurisu assembly is loading......");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static readonly Spell Darkbinding = new Spell(SpellSlot.Q, 1175f);
        private static readonly Spell Tormentedsoil = new Spell(SpellSlot.W, 900f);
        private static readonly Spell Blackshield = new Spell(SpellSlot.E, 750f);
        private static readonly Spell Soulshackle = new Spell(SpellSlot.R, 600f);

        private static readonly List<Spell> MorganaSpellList = new List<Spell>();

        private static void SetSkills()
        {
            MorganaSpellList.AddRange(new[] { Darkbinding, Tormentedsoil, Blackshield, Soulshackle });
            Darkbinding.SetSkillshot(0.25f, 72f, 1400f, true, SkillshotType.SkillshotLine);
            Tormentedsoil.SetSkillshot(0.25f, 175f, 1200f, false, SkillshotType.SkillshotCircle);
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            if (_player.BaseSkinName != "Morgana") 
                return;

            Initialize();
            SetSkills();
            MorganaMenu();
        }

        private void Game_DrawingOnDraw(EventArgs args)
        {
            for (var i = 0; i < MorganaSpellList.Count; i++)
            {
                var spell = MorganaSpellList[i];
                var circle = _config.SubMenu("drawings").Item("draw" + spell.Slot.ToString()).GetValue<Circle>();
                if (circle.Active)
                    Utility.DrawCircle(_player.Position, spell.Range, circle.Color, 1, 1);
            }

            if (_target == null) return;
            Utility.DrawCircle(_target.Position, _target.BoundingRadius, Color.Red, 1, 1);
        }

        private void Game_OnGapCloser(ActiveGapcloser sender)
        {
            if (!_config.Item("useq").GetValue<bool>() || !_config.Item("qgap").GetValue<bool>()) return;
            Darkbinding.Cast(sender.Sender);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            _target = SimpleTs.GetTarget(1150, SimpleTs.DamageType.Magical);

            DarkBinding(_target);
            TormentedSoil(_target);

        }

        private static bool UsePackets()
        {
            return _config.Item("usepackets").GetValue<bool>();
        }

        private void DarkBinding(Obj_AI_Base target)
        {
            bool combo = _config.Item("combokey").GetValue<KeyBind>().Active;
            PredictionOutput prediction = Darkbinding.GetPrediction(target);

            switch (prediction.Hitchance)
            {
                case HitChance.High:
                    if (Darkbinding.IsReady() && combo && _config.Item("hitchance").GetValue<StringList>().SelectedIndex == 2)
                        Darkbinding.Cast(prediction.CastPosition, UsePackets());
                    break;
                case HitChance.Medium:
                    if (Darkbinding.IsReady() && combo && _config.Item("hitchance").GetValue<StringList>().SelectedIndex == 1)
                        Darkbinding.Cast(prediction.CastPosition, UsePackets());
                    break;
                case HitChance.Low:
                    if (Darkbinding.IsReady() && combo && _config.Item("hitchance").GetValue<StringList>().SelectedIndex == 0)
                        Darkbinding.Cast(prediction.CastPosition, UsePackets());
                    break;
            }

            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>().Where(
                        e => e.Team != _player.Team && e.IsValid && e.IsVisible && Vector2.DistanceSquared(_player.Position.To2D(),
                            e.ServerPosition.To2D()) < Darkbinding.Range * Darkbinding.Range))
            {
                var autopred = Darkbinding.GetPrediction(e);
                if (autopred.Hitchance == HitChance.Immobile && Tormentedsoil.IsReady())
                    Darkbinding.Cast(autopred.CastPosition, UsePackets());
                if (autopred.Hitchance == HitChance.Dashing && Tormentedsoil.IsReady())
                    Darkbinding.Cast(autopred.CastPosition, UsePackets());
            }
        }

        private void TormentedSoil(Obj_AI_Base target)
        {
            if (_orbwalker.ActiveMode.ToString() == "Combo" && _config.Item("usew").GetValue<bool>())
            {
                var prediction = Tormentedsoil.GetPrediction(target);
                if (prediction.Hitchance == HitChance.Medium)
                    if (Tormentedsoil.IsReady() &&
                        !_config.Item("wimmobile").GetValue<bool>())
                        Tormentedsoil.Cast(prediction.CastPosition, UsePackets());
            }

            if (!_config.Item("wimmobile").GetValue<bool>()) return;
            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>().Where(
                        e => e.Team != _player.Team && e.IsVisible && e.IsValid && Vector2.DistanceSquared(_player.Position.To2D(),
                            e.ServerPosition.To2D()) < Tormentedsoil.Range*Tormentedsoil.Range)) 
            {
                var autopred = Tormentedsoil.GetPrediction(e);
                if (autopred.Hitchance == HitChance.Immobile && Tormentedsoil.IsReady())
                    Tormentedsoil.Cast(autopred.CastPosition, UsePackets());
            }
        }

        private void Game_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var sdelay = _config.Item("delaye").GetValue<Slider>().Value;
            if (_config.Item("usee").GetValue<bool>() && _config.Item("edangerous").GetValue<bool>())
            {
                if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy)
                {
                    var targetList = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly).OrderBy(h => h.Distance(args.End));
                    foreach (var a in targetList)
                    {
                        foreach (var spell in KurisuLib.CCList)
                        {
                            if (spell.SDataName == args.SData.Name)
                            {
                                Console.WriteLine(spell.Type);
                                switch (spell.Type)
                                {
                                    case Skilltype.Circle:
                                        if (a.Distance(args.End) <= 250f && Blackshield.IsReady())
                                        {
                                            if (_config.Item(spell.SpellMenuName).GetValue<bool>() &&
                                                _config.Item("shield" + a.SkinName).GetValue<bool>())
                                                    if (sdelay > 0)
                                                        Utility.DelayAction.Add(sdelay,
                                                            () => Blackshield.Cast(a.Position, UsePackets()));
                                                    else
                                                        Blackshield.Cast(a, UsePackets());
                                        }
                                        break;
                                    case Skilltype.Line:
                                        if (a.Distance(args.End) <= 100f && Blackshield.IsReady())
                                        {
                                            if (_config.Item(spell.SpellMenuName).GetValue<bool>() && _config.Item("shield" + a.SkinName).GetValue<bool>())
                                                    if (sdelay > 0)
                                                        Utility.DelayAction.Add(sdelay,
                                                            () => Blackshield.Cast(a.Position, UsePackets()));
                                                    else
                                                        Blackshield.Cast(a, UsePackets());
                                        }
                                        break;
                                    case Skilltype.Unknown:
                                        if (Blackshield.IsReady() && (a.Distance(args.End) <= 600f || a.Distance(sender.Position) <= 600f))
                                            if (a.SkinName != _player.SkinName &&
                                                a.Distance(_player.Position) < Blackshield.Range)
                                            {
                                                if (_config.Item(spell.SpellMenuName).GetValue<bool>() &&
                                                    _config.Item("shield" + a.SkinName).GetValue<bool>())
                                                    if (sdelay > 0)
                                                        Utility.DelayAction.Add(sdelay,
                                                            () => Blackshield.Cast(a, UsePackets()));
                                                    else
                                                        Blackshield.Cast(a, UsePackets());
                                            }
                                            else
                                            {
                                                if (_config.Item(spell.SpellMenuName).GetValue<bool>() &&
                                                   _config.Item("shield" + a.SkinName).GetValue<bool>())
                                                    if (sdelay > 0)
                                                        Utility.DelayAction.Add(sdelay,
                                                            () => Blackshield.Cast(a.Position, UsePackets()));
                                                    else
                                                        Blackshield.Cast(a, UsePackets());
                                            }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MorganaMenu()
        {
            _config = new Menu("鍫曡惤澶╀娇", "morgana", true);
            var morgOrb = new Menu("璧扮爫", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(morgOrb);
            _config.AddSubMenu(morgOrb);

            var morgTS = new Menu("鐩爣閫夋嫨", "target selecter");
            SimpleTs.AddToMenu(morgTS);
            _config.AddSubMenu(morgTS);
            
            var morgDraws = new Menu("鏄剧ず", "drawings");

            morgDraws.AddItem(new MenuItem("drawQ", "Q 鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            morgDraws.AddItem(new MenuItem("drawW", "W 鑼冨洿")).SetValue(new Circle(false, Color.FromArgb(150, Color.White)));
            morgDraws.AddItem(new MenuItem("drawE", "E 鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            morgDraws.AddItem(new MenuItem("drawR", "R 鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            _config.AddSubMenu(morgDraws);

            var morgBind = new Menu("Q璁剧疆", "bind");
            morgBind.AddItem(new MenuItem("useq", "鎵撳紑!")).SetValue(true);
            morgBind.AddItem(new MenuItem("minuseq", "鏈€杩鑼冨洿!")).SetValue(new Slider(0, 0, 250));
            morgBind.AddItem(new MenuItem("hitchance", "鍛戒腑鐜")).SetValue(new StringList(new[] { "Low", "Medium", "High" }, 2));
            morgBind.AddItem(new MenuItem("", ""));
            morgBind.AddItem(new MenuItem("qdash", "鑷姩-Q 绐佽繘 鏁屼汉")).SetValue(true);
            morgBind.AddItem(new MenuItem("qimmobile", "鑷姩-Q 绂侀敘 鏁屼汉")).SetValue(true);
            morgBind.AddItem(new MenuItem("qgap", "鑷姩-Q 杩戣韩 鏁屼汉")).SetValue(true);
            _config.AddSubMenu(morgBind);

            var morgSoil = new Menu("W璁剧疆", "soil");
            morgSoil.AddItem(new MenuItem("usew", "鎵撳紑!")).SetValue(true);
            morgSoil.AddItem(new MenuItem("wimmobile", "浣跨敤 QW")).SetValue(true);
            _config.AddSubMenu(morgSoil);

            var morgShield = new Menu("E璁剧疆", "shield");
            morgShield.AddItem(new MenuItem("usee", "鎵撳紑!")).SetValue(true);
            morgShield.AddItem(new MenuItem("delaye", "寤惰繜")).SetValue(new Slider(0, 0, 300));
            morgShield.AddItem(new MenuItem("minshieldpct", "钃濋噺 %")).SetValue(new Slider(40));
            morgShield.AddItem(new MenuItem("edangerous", "鎶ょ浘鎶€鑳")).SetValue(true);
            morgShield.AddItem(new MenuItem(" ", " "));
            var supSpe = new Menu("鏀寔鎶€鑳", "suppspells");
            foreach (var e in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                foreach (var s in KurisuLib.CCList)
                {
                    if (s.HeroName == e.SkinName)
                    {
                        supSpe.AddItem(new MenuItem(s.SpellMenuName, s.Slot + " " + s.SpellMenuName)).SetValue(true);
                        //supSpe.AddItem(new MenuItem(s.SpellMenuName + "dl", s.SpellMenuName + " Danger Level"))
                        //    .SetValue(new Slider(s.DangerLevel, 0, 5));
                    }
                }
            }
            morgShield.AddSubMenu(supSpe);
            foreach (var a in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly).Select(hero => hero.SkinName))
            {
                morgShield.AddItem(new MenuItem("shield" + a, a)).SetValue(true);
            }
            _config.AddSubMenu(morgShield);
            _config.AddItem(new MenuItem("usepackets", "浣跨敤灏佸寘")).SetValue(true);
            _config.AddItem(new MenuItem("combokey", "杩炴嫑!")).SetValue(new KeyBind(32, KeyBindType.Press));
            _config.AddToMainMenu();

            Game.PrintChat("<font color=\"#F2F2F2\">[Morgana]</font><font color=\"#D9D9D9\"> - <u>the Fallen Angel </u>  </font>- by Kurisu");
        }

        private void Initialize()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += Game_OnGapCloser;
            Drawing.OnDraw += Game_DrawingOnDraw;
        }

    }
}
