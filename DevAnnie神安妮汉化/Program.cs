﻿using DevCommom;
using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


/*
 * ##### DevAnnie Mods #####
 * 
 * SBTW Assembly
 * SoloQ and Support Mode with separated logic
 * Flash + Combo Burst Key
 * Use E Shield against AA/spells and GapCloser
 * LastHit with Q in HarassMode to StackPassive (Save Q if enemy is near)
 * Use R + 4 Pyromania to Interrupt Dangerous Spells
 * Cast R if will Stun X Enemies
 * Cast R Burst Combo
 * Use Itens (DFG)
 * Skin Hack
 * Auto Spell Level UP
 * 
*/

namespace DevAnnie
{
    class Program
    {
        public const string ChampionName = "Annie";

        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Obj_AI_Hero Player;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SkinManager skinManager;
        public static SummonerSpellManager summonerSpellManager;
        public static ItemManager itemManager;
        public static AssemblyUtil assemblyUtil;
        public static LevelUpManager levelUpManager;

        private static DateTime dtBurstComboStart = DateTime.MinValue;
        private static string msgFlashCombo = string.Empty;

        private static bool mustDebug = false;

        static void Main(string[] args)
        {
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (!Player.ChampionName.Equals(ChampionName, StringComparison.CurrentCultureIgnoreCase))
                return;

            try
            {
                InitializeSpells();

                InitializeSkinManager();

                InitializeLevelUpManager();

                InitializeMainMenu();

                InitializeAttachEvents();

                Game.PrintChat(string.Format("<font color='#fb762d'>DevAnnie Loaded v{0}</font>", Assembly.GetExecutingAssembly().GetName().Version));

                assemblyUtil = new AssemblyUtil(Assembly.GetExecutingAssembly().GetName().Name);
                assemblyUtil.onGetVersionCompleted += AssemblyUtil_onGetVersionCompleted;
                assemblyUtil.GetLastVersionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void AssemblyUtil_onGetVersionCompleted(OnGetVersionCompletedArgs args)
        {
            if (args.LastAssemblyVersion == Assembly.GetExecutingAssembly().GetName().Version.ToString())
                Game.PrintChat(string.Format("<font color='#fb762d'>DevAnnie You have the lastest version.</font>"));
            else
                Game.PrintChat(string.Format("<font color='#fb762d'>DevAnnie NEW VERSION available! Tap F8 for Update! {0}</font>", args.LastAssemblyVersion));
        }

        private static void InitializeAttachEvents()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            GameObject.OnCreate += GameObject_OnCreate;

            Config.Item("ComboDamage").ValueChanged += (object sender, OnValueChangeEventArgs e) => { Utility.HpBarDamageIndicator.Enabled = e.GetNewValue<bool>(); };
            if (Config.Item("ComboDamage").GetValue<bool>())
            {
                Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                Utility.HpBarDamageIndicator.Enabled = true;
            }
        }



        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var UseEAgainstAA = Config.Item("UseEAgainstAA").GetValue<bool>();

            if (UseEAgainstAA && E.IsReady() && sender is Obj_SpellMissile)
            {
                var missile = sender as Obj_SpellMissile;
                if (missile.SpellCaster is Obj_AI_Hero && missile.SpellCaster.IsEnemy && missile.Target.IsMe)
                    CastE();
            }
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var packetCast = Config.Item("PacketCast").GetValue<bool>();
            var BarrierGapCloser = Config.Item("BarrierGapCloser").GetValue<bool>();
            var BarrierGapCloserMinHealth = Config.Item("BarrierGapCloserMinHealth").GetValue<Slider>().Value;
            var UseEGapCloser = Config.Item("UseEGapCloser").GetValue<bool>();
            
            if (BarrierGapCloser && summonerSpellManager.IsReadyBarrier() && gapcloser.Sender.IsValidTarget(Player.AttackRange) && Player.GetHealthPerc() < BarrierGapCloserMinHealth)
            {
                if (summonerSpellManager.CastBarrier())
                    Game.PrintChat(string.Format("OnEnemyGapcloser -> BarrierGapCloser on {0} !", gapcloser.Sender.SkinName));
            }

            if (UseEGapCloser && E.IsReady())
            {
                CastE();
            }
        }

        private static void CastE()
        {
            var packetCast = Config.Item("PacketCast").GetValue<bool>();
            if (packetCast)
                Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(Player.NetworkId, SpellSlot.E)).Send();
            else
                E.Cast();
        }

        static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var UseRInterrupt = Config.Item("UseRInterrupt").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();

            if (UseRInterrupt && R.IsReady() && GetPassiveStacks() >= 4 &&
                spell.DangerLevel == InterruptableDangerLevel.High && unit.IsEnemy && unit.IsValidTarget(R.Range))
            {
                R.Cast(unit.ServerPosition, packetCast);
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            try
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        BurstCombo();
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        QHarassLastHit();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        WaveClear();
                        JungleClear();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        //Freeze();
                        break;
                    default:
                        break;
                }

                FlashCombo();

                skinManager.Update();

                levelUpManager.Update();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static bool IsSoloQMode
        {
            get { return Config.Item("ModeType").GetValue<StringList>().SelectedIndex == 0; }
        }

        static bool IsSupportMode
        {
            get { return Config.Item("ModeType").GetValue<StringList>().SelectedIndex == 1; }
        }

        public static void FlashCombo()
        {
            var UseFlashCombo = Config.Item("FlashComboKey").GetValue<KeyBind>().Active;
            var FlashComboMinEnemies = Config.Item("FlashComboMinEnemies").GetValue<Slider>().Value;
            var FlashAntiSuicide = Config.Item("FlashAntiSuicide").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();

            if (!UseFlashCombo)
                return;

            int qtPassiveStacks = GetPassiveStacks();

            if (((qtPassiveStacks == 3 && E.IsReady()) || qtPassiveStacks == 4) && summonerSpellManager.IsReadyFlash() && R.IsReady())
            {
                var allEnemies = DevHelper.GetEnemyList()
                    .Where(x => Player.Distance(x) > R.Range && Player.Distance(x) < R.Range + 500);

                var enemies = DevHelper.GetEnemyList()
                    .Where(x => Player.Distance(x) > R.Range && Player.Distance(x) < R.Range + 400 && GetBurstComboDamage(x) * 0.9 > x.Health)
                    .OrderBy(x => x.Health);

                bool isSuicide = FlashAntiSuicide ? allEnemies.Count() - enemies.Count() > 2 : false;

                if (enemies.Any() && !isSuicide)
                { 
                    var enemy = enemies.First();
                    if (DevHelper.CountEnemyInPositionRange(enemy.ServerPosition, 250) >= FlashComboMinEnemies)
                    {
                        var predict = R.GetPrediction(enemy, true).CastPosition;

                        if (qtPassiveStacks == 3)
                        {
                            if (packetCast)
                                Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(Player.NetworkId, SpellSlot.E)).Send();
                            else
                                E.Cast();
                        }

                        summonerSpellManager.CastFlash(predict);

                        if (itemManager.IsReadyDFG())
                            itemManager.CastDFG(enemy);

                        if (R.IsReady())
                            R.Cast(predict, packetCast);

                        if (W.IsReady())
                            W.Cast(predict, packetCast);

                        if (E.IsReady())
                            E.Cast();

                    }
                }
            }
        }

        public static double GetBurstComboDamage(Obj_AI_Hero eTarget)
        {
            double totalComboDamage = 0;
            totalComboDamage += Player.GetSpellDamage(eTarget, SpellSlot.R);
            totalComboDamage += Player.GetSpellDamage(eTarget, SpellSlot.Q);
            totalComboDamage += Player.GetSpellDamage(eTarget, SpellSlot.W);

            if (itemManager.IsReadyDFG())
                totalComboDamage = totalComboDamage * 1.2;

            if (itemManager.IsReadyDFG())
                totalComboDamage += Player.GetItemDamage(eTarget, Damage.DamageItems.Dfg);

            if (summonerSpellManager.IsReadyIgnite())
                totalComboDamage += Player.GetSummonerSpellDamage(eTarget, Damage.SummonerSpell.Ignite);

            return totalComboDamage;
        }

        public static void BurstCombo()
        {
            var eTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (eTarget == null)
                return;

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var useIgnite = Config.Item("UseIgnite").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();
            var UseRMinEnemies = Config.Item("UseRMinEnemies").GetValue<Slider>().Value;

            double totalComboDamage = 0;
            if (R.IsReady())
                totalComboDamage += Player.GetSpellDamage(eTarget, SpellSlot.R);
            totalComboDamage += Player.GetSpellDamage(eTarget, SpellSlot.Q);
            totalComboDamage += Player.GetSpellDamage(eTarget, SpellSlot.Q);
            totalComboDamage += Player.GetSpellDamage(eTarget, SpellSlot.W);

            if (itemManager.IsReadyDFG())
                totalComboDamage = totalComboDamage * 1.2;

            if (itemManager.IsReadyDFG())
                totalComboDamage += Player.GetItemDamage(eTarget, Damage.DamageItems.Dfg);

            totalComboDamage += summonerSpellManager.IsReadyIgnite() ? Player.GetSummonerSpellDamage(eTarget, Damage.SummonerSpell.Ignite) : 0;

            double totalManaCost = 0;
            if (R.IsReady())
                totalManaCost += Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
            totalManaCost += Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;

            if (mustDebug)
            {
                Game.PrintChat("BurstCombo Damage {0}/{1} {2}", Convert.ToInt32(totalComboDamage), Convert.ToInt32(eTarget.Health), eTarget.Health < totalComboDamage ? "BustKill" : "Harras");
                Game.PrintChat("BurstCombo Mana {0}/{1} {2}", Convert.ToInt32(totalManaCost), Convert.ToInt32(eTarget.Mana), Player.Mana >= totalManaCost ? "Mana OK" : "No Mana");
            }

            // R Combo
            if (eTarget.Health < totalComboDamage && Player.Mana >= totalManaCost)
            {
                if (totalComboDamage * 0.3 < eTarget.Health) // Anti OverKill
                {
                    if (mustDebug)
                        Game.PrintChat("BurstCombo R -> " + eTarget.BaseSkinName);

                    if (itemManager.IsReadyDFG())
                        itemManager.CastDFG(eTarget);

                    if (R.IsReady() && useR)
                    {
                        var pred = R.GetPrediction(eTarget, true); 
                        R.Cast(pred.CastPosition, packetCast);
                    }

                    dtBurstComboStart = DateTime.Now;
                }
                dtBurstComboStart = DateTime.Now;
            }


            // R if Hit X Enemies
            if (R.IsReady() && useR)
            {
                if (DevHelper.CountEnemyInPositionRange(eTarget.ServerPosition, 250) >= UseRMinEnemies)
                {
                    if (itemManager.IsReadyDFG())
                        itemManager.CastDFG(eTarget);

                    var pred = R.GetPrediction(eTarget, true);
                    R.Cast(pred.CastPosition, packetCast);

                    dtBurstComboStart = DateTime.Now;
                }
            }

            // Ignite
            if (dtBurstComboStart.AddSeconds(4) > DateTime.Now && summonerSpellManager.IsReadyIgnite())
            {
                if (mustDebug)
                    Game.PrintChat("Ignite -> " + eTarget.BaseSkinName);
                summonerSpellManager.CastIgnite(eTarget); ;
            }

        }

        public static void Combo()
        {
            var eTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (eTarget == null)
                return;

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();
            

            if (eTarget.IsValidTarget(Q.Range) && Q.IsReady() && useQ)
            {
                Q.CastOnUnit(eTarget, packetCast);
            }

            if (eTarget.IsValidTarget(W.Range) && W.IsReady() && useW)
            {
                W.CastIfHitchanceEquals(eTarget, eTarget.IsMoving ? HitChance.High : HitChance.Medium, packetCast);
            }

            if (summonerSpellManager.CanKillIgnite(eTarget))
            {
                if (summonerSpellManager.CastIgnite(eTarget))
                    Game.PrintChat(string.Format("Ignite Combo KS -> {0} ", eTarget.SkinName));
            }
        }

        public static void Harass()
        {
            var eTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            if (eTarget == null)
                return;

            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();
            
            if (eTarget.IsValidTarget(Q.Range) && Q.IsReady() && useQ)
            {
                Q.CastOnUnit(eTarget, packetCast);
            }

            if (eTarget.IsValidTarget(W.Range) && W.IsReady() && useW)
            {
                W.CastIfHitchanceEquals(eTarget, eTarget.IsMoving ? HitChance.High : HitChance.Medium, packetCast);
            }

        }

        public static void QHarassLastHit()
        {
            if (!IsSupportMode)
                return;

            var UseQHarassLastHit = Config.Item("UseQHarassLastHit").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();

            if (UseQHarassLastHit && Q.IsReady() && GetPassiveStacks() < 4)
            {
                var nearestEnemy = Player.GetNearestEnemy();
                if (Player.Distance(nearestEnemy) > Q.Range)
                {
                    var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy).ToList();
                    var minionLastHit = allMinions.Where(x => x.Health > Player.GetSpellDamage(x, SpellSlot.Q)).OrderBy(x => x.Health);

                    if (minionLastHit.Any())
                    {
                        var unit = minionLastHit.First();
                        Q.CastOnUnit(unit, packetCast);
                    }
                }
            }
        }

        public static void WaveClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
            var useW = Config.Item("UseWLaneClear").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();
            var ManaLaneClear = Config.Item("ManaLaneClear").GetValue<Slider>().Value;

            if (Q.IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy).ToList();
                var minionLastHit = allMinions.Where(x => HealthPrediction.LaneClearHealthPrediction(x, (int)Q.Delay * 1000) < Player.GetSpellDamage(x, SpellSlot.Q) * 0.8f).OrderBy(x => x.Health);

                if (minionLastHit.Any())
                {
                    var unit = minionLastHit.First();
                    Q.CastOnUnit(unit, packetCast);
                }
            }

            if (W.IsReady() && useW && Player.GetManaPerc() >= ManaLaneClear)
            {
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Enemy).ToList();

                if (allMinionsW.Any())
                {
                    var farm = W.GetCircularFarmLocation(allMinionsW, W.Width * 0.8f);
                    if (farm.MinionsHit >= 3)
                    {
                        W.Cast(farm.Position, packetCast);
                    }
                }
            }

        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count == 0)
                return;

            var UseQJungleClear = Config.Item("UseQJungleClear").GetValue<bool>();
            var UseWJungleClear = Config.Item("UseWJungleClear").GetValue<bool>();
            var packetCast = Config.Item("PacketCast").GetValue<bool>();

            var mob = mobs.First();

            if (UseQJungleClear && Q.IsReady() && mob.IsValidTarget(Q.Range))
            {
                Q.CastOnUnit(mob, packetCast);
            }

            if (UseWJungleClear && W.IsReady() && mob.IsValidTarget(W.Range))
            {
                W.Cast(mob.Position, packetCast);
            }
        }

        public static int GetPassiveStacks()
        {
            var buffs = Player.Buffs.Where(buff => (buff.Name.ToLower() == "pyromania" || buff.Name.ToLower() == "pyromania_particle"));
            if (buffs.Any())
            {
                var buff = buffs.First();
                if (buff.Name.ToLower() == "pyromania_particle")
                    return 4;
                else
                    return buff.Count;
            }
            return 0;
        }

        private static void InitializeSkinManager()
        {
            skinManager = new SkinManager();
            skinManager.Add("Classic Annie");
            skinManager.Add("Goth Annie");
            skinManager.Add("Red Riding Annie");
            skinManager.Add("Annie in Wonderland");
            skinManager.Add("Prom Queen Annie");
            skinManager.Add("Frostfire Annie");
            skinManager.Add("Franken Tibbers Annie");
            skinManager.Add("Reverse Annie");
            skinManager.Add("Panda Annie");
        }

        private static void InitializeLevelUpManager()
        {
            if (mustDebug)
                Game.PrintChat("InitializeLevelUpManager Start");

            var priority1 = new int[] { 1, 2, 1, 3, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };

            levelUpManager = new LevelUpManager();
            levelUpManager.Add("Q > W > Q > E ", priority1);

            if (mustDebug)
                Game.PrintChat("InitializeLevelUpManager Finish");
        }

        private static void InitializeSpells()
        {
            summonerSpellManager = new SummonerSpellManager();
            itemManager = new ItemManager();

            Q = new Spell(SpellSlot.Q, 650);
            Q.SetTargetted(0.25f, 1400);

            W = new Spell(SpellSlot.W, 625);
            W.SetSkillshot(0.6f, (float)(50 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 600);
            R.SetSkillshot(0.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.IsMinion && IsSupportMode)
            {
                var allyADC = Player.GetNearestAlly();
                if (allyADC.Distance(args.Target) < allyADC.AttackRange * 1.2)
                    args.Process = false;
            }

            //if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            //{
            //    var useQ = Config.Item("UseQCombo").GetValue<bool>();
            //    var useW = Config.Item("UseWCombo").GetValue<bool>();
            //    var useE = Config.Item("UseQCombo").GetValue<bool>();

            //    if (Player.GetNearestEnemy().IsValidTarget(W.Range) && ((useQ && Q.IsReady()) || (useW && W.IsReady() || useE && E.IsReady())))
            //        args.Process = false;
            //}
            //else
            //    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            //    {
            //        var useQ = Config.Item("UseQHarass").GetValue<bool>();
            //        var useW = Config.Item("UseWHarass").GetValue<bool>();
            //        var useE = Config.Item("UseEHarass").GetValue<bool>();

            //        if (Player.GetNearestEnemy().IsValidTarget(W.Range) && ((useQ && Q.IsReady()) || (useW && W.IsReady() || useE && E.IsReady())))
            //            args.Process = false;
            //    }
        }

        static void Drawing_OnDraw(EventArgs args)
        {


        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && spell.IsReady())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Hero enemy)
        {
            IEnumerable<SpellSlot> spellCombo = new[] { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
            return (float)Player.GetComboDamage(enemy, spellCombo);
        }

        private static void InitializeMainMenu()
        {
            Config = new Menu("Dev榛戞殫涔嬪コ", "DevAnnie", true);

            var targetSelectorMenu = new Menu("鐩爣閫夋嫨", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("璧扮爫", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("妯″紡", "Mode"));
            Config.SubMenu("Mode").AddItem(new MenuItem("ModeType", "妯″紡绫诲瀷").SetValue(new StringList(new[] { "SoloQ", "Support" })));

            Config.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "浣跨敤 Ignite").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRMinEnemies", "浣跨敤 R 鏁屼汉鏁皘").SetValue(new Slider(2, 1, 5)));

            Config.AddSubMenu(new Menu("闂幇杩炴嫑", "FlashCombo"));
            Config.SubMenu("FlashCombo").AddItem(new MenuItem("FlashComboKey", "蹇嵎閿").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("FlashCombo").AddItem(new MenuItem("FlashComboMinEnemies", "闂幇杩炴嫑鏁屼汉鏁皘").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("FlashCombo").AddItem(new MenuItem("FlashAntiSuicide", "浣跨敤闂幇闃茶嚜鏉€").SetValue(true));

            Config.AddSubMenu(new Menu("楠氭壈", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "浣跨敤 W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤 E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarassLastHit", "浣跨敤 Q 灏惧垁").SetValue(true));

            Config.AddSubMenu(new Menu("娓呯嚎", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "浣跨敤 Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWLaneClear", "浣跨敤 W").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("ManaLaneClear", "W 钃濋噺 %").SetValue(new Slider(30, 1, 100)));

            Config.AddSubMenu(new Menu("娓呴噹", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseQJungleClear", "浣跨敤 Q").SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("UseWJungleClear", "浣跨敤 W").SetValue(true));

            Config.AddSubMenu(new Menu("鏉傞」", "Extra"));
            Config.SubMenu("Extra").AddItem(new MenuItem("PacketCast", "浣跨敤灏佸寘").SetValue(true));
            Config.SubMenu("Extra").AddItem(new MenuItem("UseEAgainstAA", "鑷姩 E").SetValue(true));
            Config.SubMenu("Extra").AddItem(new MenuItem("UseRInterrupt", "浣跨敤 R 鎵撴柇").SetValue(true));

            Config.AddSubMenu(new Menu("杩戣韩", "GapCloser"));
            Config.SubMenu("GapCloser").AddItem(new MenuItem("UseEGapCloser", "浣跨敤 E 闃茶繎韬珅").SetValue(true));
            Config.SubMenu("GapCloser").AddItem(new MenuItem("BarrierGapCloser", "杩戣韩灞忛殰").SetValue(true));
            Config.SubMenu("GapCloser").AddItem(new MenuItem("BarrierGapCloserMinHealth", "灞忛殰 鐢熷懡鍊%").SetValue(new Slider(40, 0, 100)));

            Config.AddSubMenu(new Menu("鏄剧ず", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q 鑼冨洿").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W 鑼冨洿").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E 鑼冨洿").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R 鑼冨洿").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ComboDamage", "琛€鏉℃樉绀簗").SetValue(true));

            skinManager.AddToMenu(ref Config);

            levelUpManager.AddToMenu(ref Config);

            Config.AddToMainMenu();
        }
    }
}
