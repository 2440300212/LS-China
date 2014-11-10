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

namespace AzirSaliceTheEmperor
{
    class Program
    {
        public const string ChampionName = "Azir";

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell QExtend;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spellbook spellBook = ObjectManager.Player.Spellbook;
        public static SpellDataInst qSpell = spellBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst eSpell = spellBook.GetSpell(SpellSlot.E);

        public static Obj_AI_Hero wTargetsss = null;

        public static SpellSlot IgniteSlot;

        public static Vector3 rVec;
        //Menu
        public static Menu menu;

        private static Obj_AI_Hero Player;
        public static Obj_AI_Base insecTarget;

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
            Q = new Spell(SpellSlot.Q, 850);
            QExtend = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 2000);
            R = new Spell(SpellSlot.R, 450);

            Q.SetSkillshot(0.1f, 100, 1700, false, SkillshotType.SkillshotLine);
            QExtend.SetSkillshot(0.1f, 100, 1700, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 700, 1400, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
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
            menu.AddSubMenu(new Menu("蹇嵎Key閫夐」", "Keys"));
            menu.SubMenu("Keys").AddItem(new MenuItem("ComboActive", "杩炴嫑").SetValue(new KeyBind(menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "楠氭壈").SetValue(new KeyBind(menu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActiveT", "鑷姩楠氭壈").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LaneClearActive", "琛ュ叺").SetValue(new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("escape", "閫冭窇").SetValue(new KeyBind(menu.Item("Flee_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("insec", "鍥炴棆韪").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("qeCombo", "Q->E杩炴嫑").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Spell Menu
            menu.AddSubMenu(new Menu("Spell", "Spell"));
            //Q Menu
            menu.SubMenu("Spell").AddSubMenu(new Menu("Q閫夐」", "QSpell"));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qOutRange", "Only When Enemy out of Range").SetValue(true));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qExtend", "Use Extended Q Range").SetValue(true));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qBehind", "Try to Q Behind target").SetValue(true));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qMulti", "Q if 2+ Soilder").SetValue(true));
            menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qHit", "Q HitChance").SetValue(new Slider(3, 1, 3)));
            //W Menu
            menu.SubMenu("Spell").AddSubMenu(new Menu("W閫夐」", "WSpell"));
            menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("wAtk", "鏀诲嚮鏁屼汉").SetValue(true));
            menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("wQ", "浣跨敤WQ").SetValue(true));
            //E Menu
            menu.SubMenu("Spell").AddSubMenu(new Menu("E閫夐」", "ESpell"));
            menu.SubMenu("Spell").SubMenu("ESpell").AddItem(new MenuItem("eGap", "GapClose if out of Q Range").SetValue(false));
            menu.SubMenu("Spell").SubMenu("ESpell").AddItem(new MenuItem("eKill", "浣跨敤E浜哄ご").SetValue(true));
            menu.SubMenu("Spell").SubMenu("ESpell").AddItem(new MenuItem("eKnock", "Always Knockup/DMG").SetValue(false));
            menu.SubMenu("Spell").SubMenu("ESpell").AddItem(new MenuItem("eHP", "鐢熷懡>%").SetValue(new Slider(70, 0, 100)));
            //R Menu
            menu.SubMenu("Spell").AddSubMenu(new Menu("R閫夐」", "RSpell"));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("rHP", "鐢熷懡<%").SetValue(new Slider(20, 0, 100)));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("rHit", "鏁屼汉鏁伴噺>=").SetValue(new Slider(3, 0, 5)));
            menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("rWall", "浣跨敤R").SetValue(true));

            //Combo menu:
            menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "浣跨敤鐐圭噧").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("igniteMode", "妯″紡").SetValue(new StringList(new[] { "Combo", "KS" }, 0)));

            //Harass menu:
            menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "浣跨敤W").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤E").SetValue(false));

            //killsteal
            menu.AddSubMenu(new Menu("浜哄ご閫夐」", "KillSteal"));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("smartKS", "浜哄ご").SetValue(true));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("eKS", "浣跨敤E").SetValue(false));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("wqKS", "浣跨敤WQ").SetValue(true));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("qeKS", "浣跨敤WQE").SetValue(true));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("rKS", "浣跨敤R").SetValue(true));

            //farm menu
            menu.AddSubMenu(new Menu("琛ュ叺", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "浣跨敤Q").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("qFarm", "浣跨敤Q钃濋噺%").SetValue(new Slider(3, 0, 5)));

            //Misc Menu:
            menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "浣跨敤E鎵撴柇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "浣跨敤E杩戣韩").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("fastEscape", "閫冭窇2").SetValue(true));
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
            menu.AddSubMenu(new Menu("鏄剧ず", "Drawings"));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QExtendRange", "Q鏈€澶鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W鑼冨洿").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("slaveDmg", "Draw Slave AA Needed").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.PrintChat(ChampionName + " Loaded! --- by xSalice");
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (soilderCount() > 0 || W.IsReady())
            {
                damage += 2 * Damage.CalcDamage(Player, enemy, Damage.DamageType.Magical, dmgAmount());
            }

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            return (float)damage;
        }

        public static double dmgAmount()
        {
            double dmg = 0;

            if (Player.Level < 12)
            {
                dmg += 50 + 5 * Player.Level;
            }
            else
            {
                dmg += 10 * Player.Level - 10;
            }
            dmg += .7 * Player.FlatMagicDamageMod;

            return dmg;
        }

        public static double getAutoDmg(Obj_AI_Hero enemy)
        {
            return Damage.CalcDamage(Player, enemy, Damage.DamageType.Magical, dmgAmount()); 
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

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, string Source)
        {
            var qTarget = SimpleTs.GetTarget(QExtend.Range, SimpleTs.DamageType.Magical);
            var soilderTarget = SimpleTs.GetTarget(1200, SimpleTs.DamageType.Magical);

            // Game.PrintChat("Spell state: " + qSpell.State);
            var IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;

            //R
            if (useR && R.IsReady() && shouldR(qTarget, Source) && Player.Distance(qTarget) < R.Range)
                R.Cast(qTarget);

            //WQ
            if (soilderCount() == 0 && useQ && useW && W.IsReady() && (Q.IsReady() || qSpell.State == SpellState.Surpressed) && menu.Item("wQ").GetValue<bool>())
            {
                castWQ(qTarget, Source);
            }

            //W
            if (useW && W.IsReady())
            {
                castW(qTarget, Source);
            }

            //Q
            if (useQ && Q.IsReady())
            {
                castQ(qTarget, Source);
                return;
            }

            //Ignite
            if (qTarget != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (IgniteMode == 0 && GetComboDamage(qTarget) > qTarget.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, qTarget);
                }
            }

            //E
            if (useE && (E.IsReady() || eSpell.State == SpellState.Surpressed))
            {
                castE(soilderTarget, Source);
            }


            //AutoAtk
            //attackTarget(soilderTarget);
        }

        public static bool wallStun(Obj_AI_Hero target)
        {
            var pred = R.GetPrediction(target);

            var PushedPos = pred.CastPosition + Vector3.Normalize(pred.CastPosition - Player.ServerPosition) * 200;

            if (IsWall(PushedPos))
                return true;

            return false;
        }

        public static bool IsWall(Vector3 position)
        {
            var cFlags = NavMesh.GetCollisionFlags(position);
            return (cFlags == CollisionFlags.Wall || cFlags == CollisionFlags.Building || cFlags == CollisionFlags.Prop);
        }

        public static void smartKS()
        {
            if (!menu.Item("smartKS").GetValue<bool>())
                return;
            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) <= 1200 && champ.IsEnemy select champ).ToList();
            nearChamps.OrderBy(x => x.Health);

            foreach (var target in nearChamps)
            {
                if (target != null && !target.IsDead && !target.HasBuffOfType(BuffType.Invulnerability) && target.IsValidTarget(1200))
                {
                    //ignite
                    if (target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                            Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Player.Distance(target.ServerPosition) <= 600)
                    {
                        var IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
                        if (IgniteMode == 1 && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                        {
                            Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                        }
                    }

                    //R
                    if ((Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 20 && Player.Distance(target) < R.Range && menu.Item("rKS").GetValue<bool>())
                    {
                        R.Cast(target);
                    }

                    if (soilderCount() < 1 && !W.IsReady())
                        return;

                    //WQ
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20 && menu.Item("wqKS").GetValue<bool>())
                    {
                        castWQ(target, "Combo");
                    }

                    //qe
                    if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20 && Player.Distance(target) < Q.Range && menu.Item("qeKS").GetValue<bool>())
                    {
                        castQE(target);
                    }

                }
            }
        }

        public static void escape()
        {
            Vector3 wVec = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * 450;

            if (menu.Item("fastEscape").GetValue<bool>())
            {
                if (W.IsReady() || soilderCount() > 0)
                {
                    if ((E.IsReady() || eSpell.State == SpellState.Surpressed))
                    {
                        W.Cast(wVec);
                        W.LastCastAttemptT = Environment.TickCount;
                    }

                    if ((QExtend.IsReady() || qSpell.State == SpellState.Surpressed) &&
                        ((Environment.TickCount - E.LastCastAttemptT < Game.Ping + 500 &&
                          Environment.TickCount - E.LastCastAttemptT > Game.Ping + 50) || E.IsReady()))
                    {
                        if (Environment.TickCount - W.LastCastAttemptT > Game.Ping + 300 || eSpell.State == SpellState.Cooldown || !W.IsReady())
                        {
                            Vector3 qVec = Player.ServerPosition +
                                           Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * 800;

                            var lastAttempt = (int)qVec.Distance(getNearestSoilderToMouse().ServerPosition) / 1000;

                            Q.Cast(qVec, packets());
                            Q.LastCastAttemptT = Environment.TickCount + lastAttempt;
                            return;
                        }
                    }

                    if ((E.IsReady() || eSpell.State == SpellState.Surpressed))
                    {
                        if (Player.Distance(Game.CursorPos) > getNearestSoilderToMouse().Distance(Game.CursorPos) && Environment.TickCount - Q.LastCastAttemptT > Game.Ping)
                        {
                            E.Cast(getNearestSoilderToMouse().ServerPosition, packets());
                            E.LastCastAttemptT = Environment.TickCount - 250;
                            //Game.PrintChat("Rawr2");
                            return;
                        }
                        if (Environment.TickCount - W.LastCastAttemptT < Game.Ping + 300 && (Q.IsReady() || qSpell.State == SpellState.Surpressed))
                        {
                            E.Cast(wVec, packets());
                            E.LastCastAttemptT = Environment.TickCount - 250;
                            //Game.PrintChat("Rawr1");
                        }
                    }
                }
            }
            else
            {
                if (E.IsReady() || eSpell.State == SpellState.Surpressed)
                {
                    if (soilderCount() > 0)
                    {
                        Vector3 qVec = Player.ServerPosition +
                                       Vector3.Normalize(Game.CursorPos - Player.ServerPosition)*800;

                        var slave = getNearestSoilderToMouse();

                        int delay = (int) Math.Ceiling(slave.Distance(Player.ServerPosition));

                        if (QExtend.IsReady() || qSpell.State == SpellState.Surpressed)
                            Q.Cast(qVec, packets());

                        Utility.DelayAction.Add(delay,
                            () => E.Cast(getNearestSoilderToMouse().ServerPosition, packets()));
                        return;
                    }
                    if (W.IsReady())
                    {
                        W.Cast(wVec);

                        if (E.IsReady() || eSpell.State == SpellState.Surpressed)
                            E.Cast(wVec, packets());

                        if (QExtend.IsReady() || qSpell.State == SpellState.Surpressed)
                        {
                            Vector3 qVec = Player.ServerPosition +
                                           Vector3.Normalize(Game.CursorPos - Player.ServerPosition)*800;

                            Utility.DelayAction.Add(300, () => Q.Cast(qVec, packets()));
                        }
                    }
                }
            }
        }

        public static Obj_AI_Base getNearestSoilderToMouse()
        {
            var soilder = (from obj in ObjectManager.Get<Obj_AI_Base>() where obj.Name == "AzirSoldier" && obj.IsAlly && Player.Distance(obj.ServerPosition) < 2000 select obj).ToList();
            soilder.OrderBy(x => Game.CursorPos.Distance(x.ServerPosition));

            if (soilder != null && soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        public static void castQE(Obj_AI_Hero target)
        {
            if (soilderCount() > 0)
            {
                if ((Q.IsReady() || qSpell.State == SpellState.Surpressed )&& E.IsReady())
                {
                    var slaves = (from obj in ObjectManager.Get<Obj_AI_Base>() where obj.Name == "AzirSoldier" && obj.IsAlly && target.Distance(obj.ServerPosition) < 2000 select obj).ToList();

                    foreach (var slave in slaves)
                    {
                        if (target != null && Player.Distance(target) < 800)
                        {
                            var qPred = GetP(slave.ServerPosition, QExtend, target, true);

                            if (Q.IsReady() && Player.Distance(target) < 800 && qPred.Hitchance >= getQHitchance())
                            {
                                var vec = target.ServerPosition - Player.ServerPosition;
                                var CastBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                                Q.Cast(CastBehind, packets());
                                E.Cast(slave.ServerPosition, packets());
                                return;
                                
                            }
                        }
                    }
                }
            }
            else if (W.IsReady())
            {
                Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 450;

                var qPred = GetP(wVec, QExtend, target, true);

                if ((Q.IsReady() || qSpell.State == SpellState.Surpressed) && (E.IsReady() || eSpell.State == SpellState.Surpressed) && Player.Distance(target) < 800 && qPred.Hitchance >= getQHitchance())
                {
                    var vec = target.ServerPosition - Player.ServerPosition;
                    var CastBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                    W.Cast(wVec);
                    QExtend.Cast(CastBehind, packets());
                    Utility.DelayAction.Add(1, () => E.Cast(getNearestSoilderToEnemy(target).ServerPosition, packets()));
                    return;
                }
            }
        }

        public static void insec()
        {
            var target = (Obj_AI_Hero) insecTarget;

            if (target == null)
                return;

            if (soilderCount() > 0)
            {
                if ((Q.IsReady() || qSpell.State == SpellState.Surpressed) && E.IsReady())
                {
                    var slaves = (from obj in ObjectManager.Get<Obj_AI_Base>() where obj.Name == "AzirSoldier" && obj.IsAlly && target.Distance(obj.ServerPosition) < 2000 select obj).ToList();

                    foreach (var slave in slaves)
                    {
                        if (target != null && Player.Distance(target) < 800)
                        {
                            var qPred = GetP(slave.ServerPosition, QExtend, target, true);
                            var vec = target.ServerPosition - Player.ServerPosition;
                            var CastBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;
                            rVec = qPred.CastPosition - Vector3.Normalize(vec) * 300;

                            if (Q.IsReady() && (E.IsReady() || eSpell.State == SpellState.Surpressed) && R.IsReady() && qPred.Hitchance >= getQHitchance())
                            {

                                Q.Cast(CastBehind, packets());
                                E.Cast(slave.ServerPosition, packets());
                                E.LastCastAttemptT = Environment.TickCount;
                            }
                        }
                    }
                }
                if (R.IsReady())
                {
                    if (Player.Distance(target) < 200 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 150)
                    {
                        //Game.PrintChat("rawr");
                        R.Cast(rVec);
                    }
                }
            }
            else if (W.IsReady())
            {
                Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 450;

                var qPred = GetP(wVec, QExtend, target, true);

                if ((Q.IsReady() || qSpell.State == SpellState.Surpressed) && (E.IsReady() || eSpell.State == SpellState.Surpressed) 
                    && R.IsReady() && Player.Distance(target) < 800 && qPred.Hitchance >= getQHitchance())
                {
                    var vec = target.ServerPosition - Player.ServerPosition;
                    var CastBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;
                    rVec = Player.Position;

                    W.Cast(wVec);
                    QExtend.Cast(CastBehind, packets());
                    E.Cast(getNearestSoilderToEnemy(target).ServerPosition, packets());
                }
                if (R.IsReady())
                {
                    if (Player.Distance(target) < 200 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 150)
                    {
                        //Game.PrintChat("rawr2");
                        R.Cast(rVec);
                    }
                }
            }
        }

        public static void castWQ(Obj_AI_Hero target, String source)
        {
            if (Player.Distance(target) < 1150 && Player.Distance(target) > 450)
            {
                if (W.IsReady() && (Q.IsReady() || qSpell.State == SpellState.Surpressed))
                {
                    Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 450;

                    var qPred = GetP(wVec, QExtend, target, true);

                    if (qPred.Hitchance >= getQHitchance())
                    {
                        W.Cast(wVec);
                        QExtend.Cast(qPred.CastPosition, packets());
                        return;
                    }
                }
            }
        }

        public static void castW(Obj_AI_Hero target, String source)
        {
            if (Player.Distance(target) < 1200)
            {
                if (Player.Distance(target) < 450)
                {
                    //Game.PrintChat("W Cast1");
                    W.Cast(target);
                    if (canAttack())
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    return;
                }
                else if (Player.Distance(target) < 600)
                {
                    Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 450;

                    //Game.PrintChat("W Cast2");
                    if (W.IsReady())
                    {
                        W.Cast(wVec);
                        if (canAttack())
                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                        return;
                    }
                }
                else if (Player.Distance(target) < 950)
                {
                    Vector3 wVec = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 450;

                    //Game.PrintChat("W Cast2");
                    if (W.IsReady() && (Q.IsReady() || qSpell.State == SpellState.Surpressed))
                    {
                        var qPred = GetP(wVec, QExtend, target, true);

                        if (qPred.Hitchance >= getQHitchance())
                        {
                            W.Cast(wVec);
                            return;
                        }
                    }
                }
            }
        }

        public static void castQ(Obj_AI_Hero target, String source)
        {
            if (soilderCount() < 1)
                return;

            var slaves = (from obj in ObjectManager.Get<Obj_AI_Base>() where obj.Name == "AzirSoldier" && obj.IsAlly && target.Distance(obj.ServerPosition) < 2000 select obj).ToList();

            foreach (var slave in slaves)
            {
                if (target != null && Player.Distance(target) < QExtend.Range && shouldQ(target, source, slave))
                {

                    var qPred = GetP(slave.ServerPosition, QExtend, target, true);

                    if (Q.IsReady() && Player.Distance(target) < 800 && qPred.Hitchance >= getQHitchance())
                    {
                        if (menu.Item("qBehind").GetValue<bool>())
                        {
                            var vec = target.ServerPosition - Player.ServerPosition;
                            var CastBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                            Q.Cast(CastBehind, packets());
                            return;
                        }
                        else
                        {
                            Q.Cast(qPred.CastPosition, packets());
                            return;
                        }
                    }
                    else if (Q.IsReady() && Player.Distance(target) > 800 && qPred.Hitchance >= getQHitchance() && menu.Item("qExtend").GetValue<bool>())
                    {
                        var qVector = Player.ServerPosition + Vector3.Normalize(qPred.CastPosition - Player.ServerPosition) * 800;

                        //Game.PrintChat("QHarass");
                        QExtend.Cast(qVector, packets());
                        return;
                    }
                }
            }
        }

        public static void castE(Obj_AI_Hero target, String source)
        {
            if (soilderCount() < 1)
                return;

            var slaves = (from obj in ObjectManager.Get<Obj_AI_Base>() where obj.Name == "AzirSoldier" && obj.IsAlly && target.Distance(obj.ServerPosition) < 2000 select obj).ToList();

            if (Player.Distance(target) > 1200 && menu.Item("eGap").GetValue<bool>())
            {
                var slavetar = getNearestSoilderToEnemy(target);
                if (slavetar != null && slavetar.Distance(target) < Player.Distance(target))
                {
                    E.Cast(slavetar, packets());
                }
            }

            foreach (var slave in slaves)
            {
                if (target != null && Player.Distance(slave) < E.Range)
                {
                    var ePred = GetP(slave.ServerPosition, E, target, true);
                    Object[] obj = VectorPointProjectionOnLineSegment(Player.ServerPosition.To2D(), slave.ServerPosition.To2D(), ePred.UnitPosition.To2D());
                    var isOnseg = (bool)obj[2];
                    var PointLine = (Vector2)obj[1];

                    if (E.IsReady() && isOnseg && PointLine.Distance(ePred.UnitPosition.To2D()) < E.Width && shouldE(target, source))
                    {
                        E.Cast(slave.ServerPosition, packets());
                        return;
                    }
                }
            }
        }

        public static bool shouldQ(Obj_AI_Hero target, String source, Obj_AI_Base slave)
        {
            if (!menu.Item("qOutRange").GetValue<bool>())
                return true;

            if (slave.Distance(target.ServerPosition) > 390)
                return true;

            if (soilderCount() > 1 && menu.Item("qMulti").GetValue<bool>())
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.Q) > target.Health + 10)
                return true;


            return false;
        }
        public static bool shouldE(Obj_AI_Hero target, String source)
        {
            if (menu.Item("eKnock").GetValue<bool>())
                return true;

            if (menu.Item("eKill").GetValue<bool>() && GetComboDamage(target) > target.Health + 15)
                return true;

            if (menu.Item("eKS").GetValue<bool>() && Player.GetSpellDamage(target, SpellSlot.E) > target.Health + 10)
                return true;

            //hp 
            var hp = menu.Item("eHP").GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;

            if (hpPercent > hp)
                return true;

            return false;
        }

        public static bool shouldR(Obj_AI_Hero target, String source)
        {
            if (Player.GetSpellDamage(target, SpellSlot.R) > target.Health + 10)
                return true;

            var hp = menu.Item("rHP").GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;
            if (hpPercent < hp)
                return true;

            var rHit = menu.Item("rHit").GetValue<Slider>().Value;
            var pred = R.GetPrediction(target);
            if (pred.AoeTargetsHitCount >= rHit)
                return true;

            if (wallStun(target) && GetComboDamage(target) > target.Health / 2 && menu.Item("rWall").GetValue<bool>())
            {
                //Game.PrintChat("Walled");
                return true;
            }

            return false;
        }

        public static void autoATK()
        {

            if (soilderCount() < 1)
                return;

            var soilderTarget = SimpleTs.GetTarget(800, SimpleTs.DamageType.Magical);

            //Game.PrintChat("YEhhhhh");

            attackTarget(soilderTarget);
            return;
        }
        public static HitChance getQHitchance()
        {
            var hitC = HitChance.High;
            var qHit = menu.Item("qHit").GetValue<Slider>().Value;

            // HitChance.Low = 3, Medium , High .... etc..
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

            return hitC;
        }

        public static Object[] VectorPointProjectionOnLineSegment(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float cx = v3.X;
            float cy = v3.Y;
            float ax = v1.X;
            float ay = v1.Y;
            float bx = v2.X;
            float by = v2.Y;
            float rL = ((cx - ax) * (bx - ax) + (cy - ay) * (by - ay)) /
                       ((float)Math.Pow(bx - ax, 2) + (float)Math.Pow(by - ay, 2));
            var pointLine = new Vector2(ax + rL * (bx - ax), ay + rL * (by - ay));
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }
            bool isOnSegment;
            if (rS.CompareTo(rL) == 0)
            {
                isOnSegment = true;
            }
            else
            {
                isOnSegment = false;
            }
            var pointSegment = new Vector2();
            if (isOnSegment)
            {
                pointSegment = pointLine;
            }
            else
            {
                pointSegment = new Vector2(ax + rS * (bx - ax), ay + rS * (by - ay));
            }
            return new object[3] { pointSegment, pointLine, isOnSegment };
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

        private static int soilderCount()
        {
            return ObjectManager.Get<Obj_AI_Base>().Count(obj => obj.Name == "AzirSoldier" && obj.IsAlly);
        }

        public static bool canAttack()
        {
            return LXOrbwalker.CanAttack();
        }

        public static void attackTarget(Obj_AI_Hero target)
        {
            if (soilderCount() < 1)
                return;

            var tar = getNearestSoilderToEnemy(target);
            if (tar != null && Player.Distance(tar) < 800)
            {
                if (target != null && tar.Distance(target) <= 390 && canAttack())
                {
                    LXOrbwalker.Orbwalk(Game.CursorPos, target);
                }
            }

        }

        public static Obj_AI_Base getNearestSoilderToEnemy(Obj_AI_Base target)
        {
            var soilder = (from obj in ObjectManager.Get<Obj_AI_Base>() where obj.Name == "AzirSoldier" && obj.IsAlly && target.Distance(obj.ServerPosition) < 2000 select obj).ToList();
            soilder.OrderBy(x => target.Distance(x.ServerPosition));

            if (soilder != null && soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        private static void Farm()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + Q.Width, MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width, MinionTypes.All);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var min = menu.Item("qFarm").GetValue<Slider>().Value;

            
            int hit = 0;

            if (useQ && (Q.IsReady() || qSpell.State == SpellState.Surpressed))
            {
                if (soilderCount() > 0)
                {
                    var slaves = (from obj in ObjectManager.Get<Obj_AI_Base>() where obj.Name == "AzirSoldier" && obj.IsAlly && Player.Distance(obj.ServerPosition) < 2000 select obj).ToList();
                    foreach (var slave in slaves)
                    {
                        foreach (var enemy in allMinionsQ)
                        {
                            hit = 0;
                            var prediction = GetP(slave.ServerPosition, Q, enemy, true);

                            if (Q.IsReady() && Player.Distance(enemy) <= Q.Range)
                            {
                                foreach (var enemy2 in allMinionsQ)
                                {
                                    if (enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady())
                                        hit++;
                                }
                                if (hit >= min)
                                {
                                    if (Q.IsReady())
                                    {
                                        Q.Cast(prediction.CastPosition, packets());
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (W.IsReady())
                {
                    var wpred = W.GetCircularFarmLocation(allMinionsW);
                    W.Cast(wpred.Position);

                    foreach (var enemy in allMinionsQ)
                    {
                        hit = 0;
                        var prediction = GetP(Player.ServerPosition, Q, enemy, true);

                        if (Q.IsReady() && Player.Distance(enemy) <= Q.Range)
                        {
                            foreach (var enemy2 in allMinionsQ)
                            {
                                if (enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady())
                                    hit++;

                            }
                            if (hit >= min)
                            {
                                if (Q.IsReady())
                                {
                                    Q.Cast(prediction.CastPosition, packets());
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            smartKS();

            if (menu.Item("escape").GetValue<KeyBind>().Active)
            {
                escape();
            }
            else if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else if (menu.Item("insec").GetValue<KeyBind>().Active)
            {
                LXOrbwalker.Orbwalk(Game.CursorPos, null);

                if(insecTarget != null)
                    insec();
            }
            else if (menu.Item("qeCombo").GetValue<KeyBind>().Active)
            {
                var soilderTarget = SimpleTs.GetTarget(900, SimpleTs.DamageType.Magical);

                LXOrbwalker.Orbwalk(Game.CursorPos, null);
                castQE(soilderTarget);
            }
            else
            {
                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("wAtk").GetValue<bool>())
                    autoATK();
            }
        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
            if(menu.Item("QExtendRange").GetValue<Circle>().Active)
                Utility.DrawCircle(Player.Position, QExtend.Range, Color.LightBlue);

            if (menu.Item("slaveDmg").GetValue<Circle>().Active)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team && enemy.IsValid && !enemy.IsDead))
                {
                    var wts = Drawing.WorldToScreen(enemy.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.White, "AA To Kill: " + Math.Ceiling((enemy.Health / getAutoDmg(enemy))));
                }
            }

        }

        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            //ty trees
            if (args.PacketData[0] != Packet.C2S.SetTarget.Header)
            {
                return;
            }

            var decoded = Packet.C2S.SetTarget.Decoded(args.PacketData);

            if (decoded.NetworkId != 0 && decoded.Unit.IsValid && !decoded.Unit.IsMe)
            {
                insecTarget = decoded.Unit;
            }
        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap").GetValue<bool>()) return;

            if (R.IsReady() && gapcloser.Sender.IsValidTarget(R.Range))
                R.Cast(gapcloser.Sender);
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) < R.Range && unit != null && R.IsReady())
            {
                R.CastOnUnit(unit);
            }
        }
    }
}