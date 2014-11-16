using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

namespace BlitzcrankGrabDAT
{
    class Program
    {
        public const string ChampionName = "Blitzcrank";

        public static Obj_AI_Hero SelectedTarget = null;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Menu
        public static Menu menu;

        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //check to see if correct champ
            if (Player.BaseSkinName != ChampionName) return;

            //intalize spell
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, float.MaxValue);
            E = new Spell(SpellSlot.E, 140);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.22f, 70f, 1800, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 600, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            menu = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            var orbwalkerMenu = new Menu("璧扮爫", "my_Orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);

            //Target selector
            var targetSelectorMenu = new Menu("鐩爣閫夋嫨", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //Keys
            menu.AddSubMenu(new Menu("蹇嵎閿", "Keys"));
            menu.SubMenu("Keys").AddItem(new MenuItem("ComboActive", "杩炴嫑!").SetValue(new KeyBind(menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "楠氭壈!").SetValue(new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActiveT", "楠氭壈 (鑷姩)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("panic", "Panic Key(no spell)").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            //Combo menu:
            menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            menu.SubMenu("Combo")
             .AddItem(
                 new MenuItem("tsModes", "妯″紡").SetValue(
                     new StringList(new[] { "Orbwalker/LessCast", "Low HP%", "NearMouse", "CurrentHP" }, 0)));
            menu.SubMenu("Combo").AddItem(new MenuItem("selected", "浼樺厛鐩爣").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤 Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("qHit", "Q 鍛戒腑鐜").SetValue(new Slider(3, 1, 4)));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤 W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤 E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("QE", "浣跨敤 QE").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤 R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("useRQ", "浣跨敤 QR").SetValue(true));

            //Q Menu
            menu.AddSubMenu(new Menu("Q 鑿滃崟", "qMenu"));
            menu.SubMenu("qMenu").AddItem(new MenuItem("qRange2", "Q 鏈€杩戣窛绂").SetValue(new Slider(300, 1, 950)));
            menu.SubMenu("qMenu").AddItem(new MenuItem("qRange", "Q 鏈€杩滆窛绂").SetValue(new Slider(900, 1, 950)));
            menu.SubMenu("qMenu").AddItem(new MenuItem("qSlow", "鑷姩 Q 鍑忛€").SetValue(true));
            menu.SubMenu("qMenu").AddItem(new MenuItem("qImmobile", "鑷姩 Q 绂侀敘").SetValue(true));
            menu.SubMenu("qMenu").AddItem(new MenuItem("qDashing", "鑷姩 Q 绐佽繘").SetValue(true));

            //Harass menu:
            menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤 Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("qHit2", "Q 鍛戒腑鐜").SetValue(new Slider(3, 1, 4)));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "浣跨敤 W").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤 E").SetValue(true));

            //Misc Menu:
            menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "浣跨敤 R 鎵撴柇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "浣跨敤灏佸寘").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("resetE", "浣跨敤 E AA").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("autoR", "浣跨敤 R 鏁屼汉鏁").SetValue(new Slider(3, 0, 5)));

            menu.SubMenu("Misc").AddSubMenu(new Menu("鍒鍏朵娇鐢Q", "intR"));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                menu.SubMenu("Misc")
                    .SubMenu("intR")
                    .AddItem(new MenuItem("intR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "杩炴嫑浼ゅ!").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            //Drawings menu:
            menu.AddSubMenu(new Menu("鏄剧ず", "Drawings"));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W 鑼冨洿").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            LXOrbwalker.AfterAttack += Orbwalking_AfterAttack;
            Game.PrintChat(ChampionName + " Loaded! --- by xSalice");
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            return (float)damage;
        }

        private static void Combo()
        {
            UseSpells(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private static void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), menu.Item("UseWHarass").GetValue<bool>(),
                menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        public static Obj_AI_Hero getTarget()
        {
            int tsMode = menu.Item("tsModes").GetValue<StringList>().SelectedIndex;
            var focusSelected = menu.Item("selected").GetValue<bool>();

            var range = Q.Range;

            Obj_AI_Hero getTar = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);

            SelectedTarget = (Obj_AI_Hero)Hud.SelectedUnit;

            if (focusSelected && SelectedTarget != null && SelectedTarget.IsEnemy && SelectedTarget.Type == GameObjectType.obj_AI_Hero)
            {
                if (Player.Distance(SelectedTarget) < 1200 && !SelectedTarget.IsDead && SelectedTarget.IsVisible &&
                    SelectedTarget.IsValidTarget())
                {
                    //Game.PrintChat("focusing selected target");
                    LXOrbwalker.ForcedTarget = SelectedTarget;
                    return SelectedTarget;
                }

                SelectedTarget = null;
                return getTar;
            }

            if (tsMode == 0)
            {
                Hud.SelectedUnit = getTar;
                return getTar;
            }

            foreach (
                Obj_AI_Hero target in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            x =>
                                Player.Distance(x) < range && x.IsValidTarget(range) && !x.IsDead && x.IsEnemy &&
                                x.IsVisible))
            {
                if (tsMode == 1)
                {
                    float tar1hp = target.Health / target.MaxHealth * 100;
                    float tar2hp = getTar.Health / getTar.MaxHealth * 100;
                    if (tar1hp < tar2hp)
                        getTar = target;
                }

                if (tsMode == 2)
                {
                    if (target.Distance(Game.CursorPos) < getTar.Distance(Game.CursorPos))
                        getTar = target;
                }

                if (tsMode == 3)
                {
                    if (target.Health < getTar.Health)
                        getTar = target;
                }
            }

            Hud.SelectedUnit = getTar;
            LXOrbwalker.ForcedTarget = getTar;
            return getTar;
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, string Source)
        {
            var target = getTarget();

            var hitC = HitChance.High;
            var qHit = menu.Item("qHit").GetValue<Slider>().Value;
            var harassQHit = menu.Item("qHit2").GetValue<Slider>().Value;
            var qRange = menu.Item("qRange").GetValue<Slider>().Value;

            var RQ = menu.Item("useRQ").GetValue<bool>();
            var QE = menu.Item("QE").GetValue<bool>();

            Q.Range = qRange;

            // HitChance.Low = 3, Medium , High .... etc..
            if (Source == "Combo")
            {
                switch (qHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }
            else if (Source == "Harass")
            {
                switch (harassQHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }

            if (useW && target != null && W.IsReady() && Player.Distance(target) <= 500)
            {
                W.Cast();
            }

            if (useQ && Q.IsReady() && Player.Distance(target) <= Q.Range && target != null && (Q.GetPrediction(target).Hitchance >= hitC || shouldUseQ(target)) && useQonEnemy(target))
            {
                if (QE && useE && E.IsReady())
                    E.Cast();

                Q.Cast(target, packets());
                return;
            }

            if (useE && target != null && E.IsReady() && Player.Distance(target) < 300 && !menu.Item("resetE").GetValue<bool>())
            {
                E.Cast();
            }

            if (useR && target != null && R.IsReady() && Player.Distance(target) < R.Range)
            {
                if (RQ && Q.IsReady())
                    return;

                R.Cast();
            }

        }

        public static bool shouldUseQ(Obj_AI_Hero target)
        {
            var slow = menu.Item("qSlow").GetValue<bool>();
            var immobile = menu.Item("qImmobile").GetValue<bool>();
            var dashing = menu.Item("qDashing").GetValue<bool>();

            if (Q.GetPrediction(target).Hitchance == HitChance.Dashing && dashing)
                return true;

            if (Q.GetPrediction(target).Hitchance == HitChance.Immobile && immobile)
                return true;

            if (target.HasBuffOfType(BuffType.Slow) && slow && Q.GetPrediction(target).Hitchance >= HitChance.High)
                return true;

            return false;
        }

        public static void autoQ()
        {
            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) < Q.Range && champ.IsEnemy select champ).ToList();
            nearChamps.OrderBy(x => x.Health);

            if (nearChamps.FirstOrDefault() != null)
            {
                if (shouldUseQ(nearChamps.First()) && useQonEnemy(nearChamps.First()) && Q.IsReady())
                    Q.Cast(nearChamps.FirstOrDefault().ServerPosition, menu.Item("packet").GetValue<bool>());
            }
        }

        public static PredictionOutput GetPCircle(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {

            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = 1,
                Speed = spell.Speed,
                From = pos,
                Range = float.MaxValue,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }

        public static void checkRMec()
        {
            int hit = 0;
            var minHit = menu.Item("autoR").GetValue<Slider>().Value;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                if (enemy != null && !enemy.IsDead)
                {
                    var prediction = GetPCircle(Player.ServerPosition, R, enemy, true);

                    if (R.IsReady() && enemy.Distance(Player.ServerPosition) <= R.Width && prediction.CastPosition.Distance(Player.ServerPosition) <= R.Width && prediction.Hitchance >= HitChance.High)
                    {
                        hit++;
                    }
                }
            }

            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("panic").GetValue<KeyBind>().Active)
            {
                if (W.IsReady())
                    W.Cast();
                LXOrbwalker.Orbwalk(Game.CursorPos, null);
                return;
            }

            //Q grab on immobile
            autoQ();

            //Rmec
            checkRMec();

            if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if(menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        public static bool useQonEnemy(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.SpellImmunity))
            {
                return false;
            }

            var qRangeMin = menu.Item("qRange2").GetValue<Slider>().Value;
            if (Player.Distance(target.ServerPosition) < qRangeMin)
                return false;

            if (menu.Item("intR" + target.BaseSkinName) != null)
                if (menu.Item("intR" + target.BaseSkinName).GetValue<bool>() == true)
                return false;

            return true;
        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        public static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            var useECombo = menu.Item("UseECombo").GetValue<bool>();
            var useEHarass = menu.Item("UseEHarass").GetValue<bool>();

            if (unit.IsMe && menu.Item("resetE").GetValue<bool>())
            {
                if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
                {
                    if (useECombo && E.IsReady())
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        E.Cast();
                    }
                }

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active || menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                {
                    if (useEHarass && E.IsReady())
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        E.Cast();
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    if (spell == Q) {
                        var qRange = menu.Item("qRange").GetValue<Slider>().Value;

                        Q.Range = qRange;
                    }
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }

        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) < Q.Range && unit != null && Q.IsReady())
            {
                if (Q.GetPrediction(unit).Hitchance >= HitChance.High)
                    Q.Cast(unit, packets());
            }

            if (Player.Distance(unit) < R.Range && unit != null & R.IsReady())
            {
                R.Cast();
            }
        }

    }
}
