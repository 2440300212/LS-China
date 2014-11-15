using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LX_Orbwalker;

namespace ViktorTheMindBlower
{
    class Program
    {
        public const string ChampionName = "Viktor";


        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static bool chargeQ;
        public static Spell W;
        public static Spell E;
        public static Spell E2;
        public static Spell R;
        public static GameObject rObj = null;
        public static bool activeR = false;
        public static int lastR;

        public static SpellSlot IgniteSlot;

        public static Obj_AI_Hero SelectedTarget = null;

        //mana manager
        public static int[] qMana = { 45, 45 , 50 , 55 , 60 , 65 };
        public static int[] wMana = { 65, 65, 65, 65, 65, 65 };
        public static int[] eMana = { 70, 70 , 80 , 90 , 100 , 110 };
        public static int[] rMana = { 100, 100, 100, 100 };

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
            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 540);
            E2 = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 700);

            //Q.SetTargetted(0.25f, 2000);
            W.SetSkillshot(1.5f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.0f, 90, 1000, false, SkillshotType.SkillshotLine);
            E2.SetSkillshot(0.0f, 90, 1000, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(E2);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

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
            menu.AddSubMenu(new Menu("蹇嵎Keys", "Keys"));
            menu.SubMenu("Keys").AddItem(new MenuItem("ComboActive", "閫冭窇!").SetValue(new KeyBind(menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "楠氭壈!").SetValue(new KeyBind(menu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActiveT", "鑷姩楠氭壈!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LaneClearActive", "琛ュ叺!").SetValue(new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LastHitQQ", "Q灏惧垁").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            //Spell Menu
            menu.AddSubMenu(new Menu("娉曟湳", "Spell"));

            //Q Menu
            menu.SubMenu("Spell").AddSubMenu(new Menu("Q娉曟湳", "QSpell"));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("QAARange", "浣跨敤Q1").SetValue(true));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("autoAtk", "浣跨敤Q2").SetValue(true));
            //W Menu
            menu.SubMenu("Spell").AddSubMenu(new Menu("W娉曟湳", "WSpell"));
            menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("wSlow", "鑷姩W鍑忛€").SetValue(true));
            menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("wImmobile", "鑷姩W1").SetValue(true));
            menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("wDashing", "A鑷姩W2").SetValue(true));
            menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("useW_Hit", "W鏁屼汉鏁伴噺").SetValue(new Slider(2, 1, 5)));
            //R
            menu.SubMenu("Spell").AddSubMenu(new Menu("R娉曟湳", "RSpell"));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("useR_Hit", "R鏁屼汉鏁伴噺").SetValue(new Slider(2, 1, 5)));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("rAlways", "浣跨敤R").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle)));

            //Combo menu:
            menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("selected", "閲嶇偣鐩爣").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤 Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤 W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤 E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("eHit", "E 鍛戒腑鐜").SetValue(new Slider(3, 1, 4)));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤 R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "浣跨敤鐐圭噧").SetValue(true));
            menu.SubMenu("Combo")
                .AddItem(new MenuItem("igniteMode", "鐐圭噧妯″紡").SetValue(new StringList(new[] { "Combo", "KS" }, 0)));

            //Harass menu:
            menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤 Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤 E").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("eHit2", "E 鍛戒腑鐜").SetValue(new Slider(4, 1, 4)));

            //Farming menu:
            menu.AddSubMenu(new Menu("琛ュ叺", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "浣跨敤 Q").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "浣跨敤 E").SetValue(false));

            //Misc Menu:
            menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "浣跨敤R鎵撴柇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "浣跨敤灏佸寘").SetValue(true));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            //Drawings menu:
            menu.AddSubMenu(new Menu("鏄剧ず閫夐」", "Drawings"));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W 鑼冨洿").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("E2Range", "Extended鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

            menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            LXOrbwalker.AfterAttack += AfterAttack;
            Game.PrintChat(ChampionName + " 鎴愬姛杞藉叆! --- by xSalice");
        }

        public static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe && chargeQ)
            {
                chargeQ = false;
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                damage += 5 * Player.GetSpellDamage(enemy, SpellSlot.R, 1);
            }

            if (activeR)
                damage += Player.GetSpellDamage(enemy, SpellSlot.R, 1);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage;
        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, String Source)
        {
            var range = E.IsReady() ? (E.Range + E2.Range) : Q.Range;
            var focusSelected = menu.Item("selected").GetValue<bool>();
            Obj_AI_Hero target = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);
            if (SimpleTs.GetSelectedTarget() != null)
                if (focusSelected && SimpleTs.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    target = SimpleTs.GetSelectedTarget();

            var dmg = GetComboDamage(target);
            int IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
            bool hasMana = manaCheck();

            if (useW && target != null && W.IsReady() && Player.Distance(target) <= W.Range && shouldW(target))
            {
                W.Cast(target, packets());
            }

            if(menu.Item("QAARange").GetValue<bool>()){
                if (useQ && target != null && Q.IsReady() && Player.Distance(target) <= Player.AttackRange) // Q only in AA range for guaranteed AutoAttack
                {
                    Q.CastOnUnit(target, packets());
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    return;
                }
            }
            else if (useQ && target != null && Q.IsReady() && Player.Distance(target) <= Q.Range)
            {
                Q.CastOnUnit(target, packets());

                //force auto
                if (menu.Item("autoAtk").GetValue<bool>())
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
                return;
            }

            if (useR && target != null && R.IsReady() && Player.Distance(target) <= R.Range && !activeR && shouldR(target))
            {
                R.Cast(target.ServerPosition, packets());
            }

            //Ignite
            if (target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Source == "Combo" && hasMana)
            {
                if (IgniteMode == 0 && dmg > target.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (useE && E.IsReady())
            {

                if (Player.Distance(target.ServerPosition) <= 1200 && Player.Distance(target.ServerPosition) > E.Range && target != null)
                {
                    //Game.PrintChat("rawr");
                    eCalc2(Source);
                    return;
                }
                else if (target.Distance(Player.ServerPosition) <= E.Range && target != null)
                {
                    eCalc(target, Source);
                    return;
                }
            }
        }

        public static bool manaCheck()
        {
            int totalMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level];

            if (Player.Mana >= totalMana)
                return true;

            return false;
        }

        public static bool shouldW(Obj_AI_Hero target)
        {
            if (GetComboDamage(target) >= target.Health)
                return true;

            var immobile = menu.Item("wSlow").GetValue<bool>();
            var slow = menu.Item("wImmobile").GetValue<bool>();
            var dashing = menu.Item("wImmobile").GetValue<bool>();

            if (W.GetPrediction(target).Hitchance == HitChance.Immobile && immobile)
                return true;

            if (target.HasBuffOfType(BuffType.Slow) && slow)
                return true;

            if (W.GetPrediction(target).Hitchance == HitChance.Dashing && dashing)
                return true;

            if (Player.Distance(target.ServerPosition) < 300)
                return true;

            return false;
        }

        public static bool shouldR(Obj_AI_Hero target)
        {
            if (GetComboDamage(target) > target.Health)
                return true;

            if (menu.Item("rAlways").GetValue<KeyBind>().Active)
                return true;

            return false;
        }
        private static void Combo()
        {
            UseSpells(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private static void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), false,
                menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            int rTimeLeft = Environment.TickCount - lastR;
            if ((rTimeLeft <= 400))
            {
                autoR();
                lastR = Environment.TickCount - 250;
            }
          
            if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                mecR();
                mecW();
                Combo();
            }
            else
            {
                if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
                    lastHit();

                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active || menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

            }
            
        }

        public static void autoR()
        {
            if (activeR)
            {
                foreach (
                    Obj_AI_Hero target in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => rObj.Position.Distance(x.ServerPosition) < 1500 && x.IsEnemy && !x.IsDead).OrderBy(x=> x.Health))
                {
                    if (target != null)
                    {
                        if (R.IsReady())
                        {
                            R.Cast(target.ServerPosition, packets());
                            return;
                        }
                    }
                }
            }
        }

        public static PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {

            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = spell.Width,
                Speed = spell.Speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = pos,
                Aoe = aoe,
            });
        }

        private static void castE(Vector3 source, Vector3 destination)
        {
            castE(source.To2D(), destination.To2D());
        }

        private static void castE(Vector2 source, Vector2 destination)
        {
            Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, SpellSlot.E, Player.NetworkId, source.X, source.Y, destination.X, destination.Y)).Send();
        }

        public static void eCalc(Obj_AI_Hero eTarget, String Source)
        {
            var hitC = GethitChance(Source);

            //case where target is in range of E
            if (Player.Distance(eTarget.ServerPosition) <= E.Range && eTarget != null)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy != null && !enemy.IsDead && eTarget.BaseSkinName != enemy.BaseSkinName && enemy.IsEnemy)
                    {
                        if (eTarget.Distance(enemy) <= E2.Range) //check if another target is in range
                        {
                            var pred = GetP(eTarget.ServerPosition, E2, enemy, true);

                            if (pred.Hitchance >= hitC && E.IsReady())
                            {
                                castE(eTarget.ServerPosition, pred.CastPosition);
                                return;
                            }
                        }
                    }
                }

                var midPos = new Vector3((Player.ServerPosition.X + eTarget.ServerPosition.X) / 2, (Player.ServerPosition.Y + eTarget.ServerPosition.Y) / 2,
                    (Player.ServerPosition.Z + eTarget.ServerPosition.Z) / 2);

                var pred2 = GetP(midPos, E2, eTarget, true);

                if (pred2.Hitchance >= hitC && E.IsReady())
                {
                    castE(midPos, pred2.CastPosition);
                    return;
                }
            }
        }

        public static void eCalc2(String Source)
        {
            var hitC = GethitChance(Source);
            //Game.PrintChat("rawr");
            var range = E.IsReady() ? (E.Range + E2.Range) : Q.Range;
            var focusSelected = menu.Item("selected").GetValue<bool>();
            Obj_AI_Hero eTarget2 = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);
            if (SimpleTs.GetSelectedTarget() != null)
                if (focusSelected && SimpleTs.GetSelectedTarget().Distance(Player.ServerPosition) < range)
                    eTarget2 = SimpleTs.GetSelectedTarget();

            if (eTarget2 != null && Player.Distance(eTarget2) <= 1200)
            {   
                // Get initial start point at the border of cast radius
                Vector3 startPos = Player.Position + Vector3.Normalize(eTarget2.ServerPosition - Player.Position) * (E.Range - 50);
                var pred = GetP(startPos, E2, eTarget2, true);

                if (pred.Hitchance >= hitC && E.IsReady())
                {
                    castE(startPos, pred.CastPosition);
                    return;
                }
            }
        }

        public static HitChance GethitChance(string Source)
        {
            var hitC = HitChance.High;
            var qHit = menu.Item("eHit").GetValue<Slider>().Value;
            var harassQHit = menu.Item("eHit2").GetValue<Slider>().Value;

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

            return hitC;
        }

        public static void mecR()
        {
            var minHit = menu.Item("useR_Hit").GetValue<Slider>().Value;
            if (minHit == 0)
                return;
            foreach (
                Obj_AI_Base target in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < R.Range && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (target != null && target.Distance(Player.ServerPosition) <= R.Range && R.IsReady())
                {
                    var pred = R.GetPrediction(target);
                    if (pred.AoeTargetsHitCount > minHit)
                    {
                        R.Cast(pred.CastPosition);
                    }
                }
            }
        }

        public static void mecW()
        {
            var minHit = menu.Item("useW_Hit").GetValue<Slider>().Value;
            if (minHit == 0)
                return;

            foreach (
                Obj_AI_Base target in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < W.Range && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (target != null && target.Distance(Player.ServerPosition) <= R.Range && R.IsReady())
                {
                    var pred = W.GetPrediction(target);
                    if (pred.AoeTargetsHitCount > minHit)
                    {
                        W.Cast(pred.CastPosition);
                    }
                }
            }
        }

        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        Q.Cast(minion, menu.Item("packet").GetValue<bool>());
                        return;
                    }
                }
            }
        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            var MinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useE = menu.Item("UseEFarm").GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady())
            {
                Q.Cast(allMinionsQ[0], menu.Item("packet").GetValue<bool>());
            }

            if (useE && E.IsReady())
            {
                var minion = MinionsE[0];
                foreach (var enemy in MinionsE)
                {
                    if (Player.Distance(enemy) >= Player.Distance(minion))
                        minion = enemy;
                }

                castE(MinionsE[0].ServerPosition, minion.ServerPosition);
            }

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);

                var menu2 = menu.Item("E2Range").GetValue<Circle>();
                if (menu2.Active)
                    Utility.DrawCircle(Player.Position, 1200, menuItem.Color);
            }
        }

        private static void OnCreate(GameObject obj, EventArgs args)
        {
            //if (Player.Distance(obj.Position) < 400 && obj.Name != "missile")
            //Game.PrintChat("Obj: " + obj.Name);

            if (Player.Distance(obj.Position) < 3000)
            {
                if (obj != null && obj.IsValid && obj.Name.Contains("Viktor_Base_R"))
                {
                    //Game.PrintChat("woot");
                    activeR = true;
                    rObj = obj;
                }
            }

        }

        private static void OnDelete(GameObject obj, EventArgs args)
        {
            //if (Player.Distance(obj.Position) < 400 && obj.Name != "missile")
            //Game.PrintChat("Obj2: " + obj.Name);
            if (Player.Distance(obj.Position) < 3000)
            {
                if (obj != null && obj.IsValid && obj.Name.Contains("Viktor_Base_R"))
                {
                    //Game.PrintChat("woot2");
                    activeR = false;
                    rObj = null;
                }
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && R.IsReady())
            {
                R.Cast(unit.ServerPosition, menu.Item("packet").GetValue<bool>());
            }
        }
    }
}