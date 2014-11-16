/*
___________             __   .__                    _____                                              .____                     _________.__        
\_   _____/_ __   ____ |  | _|__| ____    ____     /  _  \__  _  __ ____   __________   _____   ____   |    |    ____   ____    /   _____/|__| ____  
 |    __)|  |  \_/ ___\|  |/ /  |/    \  / ___\   /  /_\  \ \/ \/ // __ \ /  ___/  _ \ /     \_/ __ \  |    |  _/ __ \_/ __ \   \_____  \ |  |/    \ 
 |     \ |  |  /\  \___|    <|  |   |  \/ /_/  > /    |    \     /\  ___/ \___ (  <_> )  Y Y  \  ___/  |    |__\  ___/\  ___/   /        \|  |   |  \
 \___  / |____/  \___  >__|_ \__|___|  /\___  /  \____|__  /\/\_/  \___  >____  >____/|__|_|  /\___  > |_______ \___  >\___  > /_______  /|__|___|  /
     \/              \/     \/       \//_____/           \/            \/     \/            \/     \/          \/   \/     \/          \/         \/ 
*/
using System.Collections.Generic;
using System.Data.Odbc;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;
using System;
using System.Linq;

namespace FuckingAwesomeLeeSin
{
    class Program
    {
        public static string ChampName = "LeeSin";
        private static Obj_AI_Hero _player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type _player
        public static Spell Q,W, E, R;
        public static Spellbook SBook;
        public static Items.Item Dfg;
        public static Vector2 JumpPos;
        public static Vector3 mouse = Game.CursorPos;
        public static SpellSlot smiteSlot;
        public static Menu Menu;
        public static bool CastQAgain;
        public static bool CastWardAgain = true;

        private static readonly string[] epics =
        {
            "Worm", "Dragon"
        };
        private static readonly string[] buffs =
        {
            "LizardElder", "AncientGolem"
        };

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }



        static void Game_OnGameLoad(EventArgs args)
        {
            if (_player.BaseSkinName != ChampName) return;

            smiteSlot = _player.GetSpellSlot("SummonerSmite");

            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 430);
            R = new Spell(SpellSlot.R, 375);
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed,true,SkillshotType.SkillshotLine);
            //Base menu
            Menu = new Menu("FA鐩插儳", ChampName, true);
            //Orbwalker and menu
            Menu.AddSubMenu(new Menu("璧扮爫", "Orbwalker"));
            LXOrbwalker.AddToMenu(Menu.SubMenu("Orbwalker"));
            //Target selector and menu
            var ts = new Menu("鐩爣閫夋嫨", "Target Selector");
            SimpleTs.AddToMenu(ts);
            Menu.AddSubMenu(ts);
            //Combo menu
            Menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useQ", "浣跨敤 Q?").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useW", "浣跨敤 W?").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useE", "浣跨敤 E?").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("useR", "浣跨敤 R?").SetValue(true));

            var harassMenu = new Menu("楠氭壈", "Harass");
            harassMenu.AddItem(new MenuItem("q1H", "浣跨敤 Q1").SetValue(true));
            harassMenu.AddItem(new MenuItem("q2H", "浣跨敤 Q2").SetValue(true));
            harassMenu.AddItem(new MenuItem("wH", "璺崇溂/璺充汉").SetValue(true));
            harassMenu.AddItem(new MenuItem("eH", "浣跨敤 E1").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            //Jung/Wave Clear
            var waveclearMenu = new Menu("鎵撻噹", "wjClear");
            waveclearMenu.AddItem(new MenuItem("useQClear", "浣跨敤 Q?").SetValue(true));
            waveclearMenu.AddItem(new MenuItem("useEClear", "浣跨敤 E?").SetValue(true));
            Menu.AddSubMenu(waveclearMenu);

            //InsecMenu
            var insecMenu = new Menu("鍥炴棆韪", "Insec");
            insecMenu.AddItem(new MenuItem("InsecEnabled", "鎵撳紑").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            insecMenu.AddItem(new MenuItem("insecOrbwalk", "璧扮爫?").SetValue(true));
            insecMenu.AddItem(new MenuItem("insec2champs", "韪㈠埌闃熷弸?").SetValue(true));
            insecMenu.AddItem(new MenuItem("insec2tower", "韪㈠埌濉斾笅?").SetValue(true));
            insecMenu.AddItem(new MenuItem("insec2orig", "鍥炴棆韪").SetValue(true));
            Menu.AddSubMenu(insecMenu);

            //SaveMe Menu
            var SaveMeMenu = new Menu("鎯╂垝", "Smite Save Settings");
            SaveMeMenu.AddItem(new MenuItem("smiteSave", "鎵撳紑").SetValue(true));
            SaveMeMenu.AddItem(new MenuItem("hpPercentSM", "鎯╂垝%").SetValue(new Slider(10, 1)));
            SaveMeMenu.AddItem(new MenuItem("param1", "涓嶆儵鎴= x%")); // TBC
            SaveMeMenu.AddItem(new MenuItem("dBuffs", "Buffs").SetValue(true));// TBC
            SaveMeMenu.AddItem(new MenuItem("hpBuffs", "HP %").SetValue(new Slider(30, 1)));// TBC
            SaveMeMenu.AddItem(new MenuItem("dEpics", "鍙茶瘲").SetValue(true));// TBC
            SaveMeMenu.AddItem(new MenuItem("hpEpics", "HP %").SetValue(new Slider(10, 1)));// TBC
            Menu.AddSubMenu(SaveMeMenu);
            //Wardjump menu
            var wardjumpMenu = new Menu("璺崇溂", "Wardjump");
            wardjumpMenu.AddItem(
                new MenuItem("wjump", "蹇嵎閿畖").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            wardjumpMenu.AddItem(new MenuItem("maxRange", "鏈€澶鑼冨洿|").SetValue(false));
            wardjumpMenu.AddItem(new MenuItem("castInRange", "榧犳爣鏂瑰悜").SetValue(true));
            wardjumpMenu.AddItem(new MenuItem("m2m", "榧犳爣绉诲姩").SetValue(true));
            wardjumpMenu.AddItem(new MenuItem("j2m", "璺冲皬鍏祙").SetValue(true));
            wardjumpMenu.AddItem(new MenuItem("j2c", "璺宠嫳闆剕").SetValue(true));
            Menu.AddSubMenu(wardjumpMenu);

            //Exploits
            Menu.AddItem(new MenuItem("NFE", "浣跨敤灏佸寘?").SetValue(true));
            Menu.AddItem(new MenuItem("qSmite", "鎯╂垝+Q!").SetValue(true));
            //Make the menu visible
            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw; // Add onDraw
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            PrintMessage("Loaded!");
        }

        enum HarassStatEnum {NONE, QONE, QTWO, WJ};
        static HarassStatEnum HarassSelect;


        public static void Harass()
        {
            var target = SimpleTs.GetTarget(Q.Range + 200, SimpleTs.DamageType.Physical);
            var wj = paramBool("wH");
            var q = paramBool("q1H");
            var q2 = paramBool("q2H");
            var e = paramBool("eH");

            if (!target.HasBuff("BlindMonkQOne") && HarassSelect == HarassStatEnum.NONE) HarassSelect = HarassStatEnum.QONE;
            if (target.HasBuff("BlindMonkQOne") && HarassSelect == HarassStatEnum.NONE) HarassSelect = HarassStatEnum.QTWO;
            if (!target.HasBuff("BlindMonkQOne") && HarassSelect == HarassStatEnum.NONE) HarassSelect = HarassStatEnum.QONE;

            if (q && Q.IsReady() && Q.Instance.Name == "BlindMonkQOne" && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            
        }


        public static bool isNullInsecPos = true;
        public static Vector3 insecPos;

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.Name == "BlindMonkQOne")
            {
                CastQAgain = false;
                Utility.DelayAction.Add(2900, () =>
                {
                    CastQAgain = true;
                });
            }
            if (args.SData.Name == "BlindMonkRKick")
                InsecComboStep = InsecComboStepSelect.NONE;
            if (args.SData.Name == "blindmonkqtwo" && HarassSelect != HarassStatEnum.NONE)
                HarassSelect = HarassStatEnum.WJ;
            if (args.SData.Name == "BlindMonkWOne")
            {
                InsecComboStep = InsecComboStepSelect.PRESSR;
            }
        }

        public static Vector3 getInsecPos(Obj_AI_Hero target)
        {
            if (isNullInsecPos)
            {
                isNullInsecPos = false;
                insecPos = _player.Position;
            }
            var turrets = (from tower in ObjectManager.Get<Obj_Turret>()
                where tower.IsAlly && !tower.IsDead && target.Distance(tower.Position) < 1000 && tower.Health > 0
                select tower).ToList();
            if (GetAllyHeroes(target, 2000).Count > 0 && paramBool("insec2champs"))
            {
                Vector3 insecPosition = InterceptionPoint(GetAllyInsec(GetAllyHeroes(target, 2000)));
                return V2E(insecPosition, target.Position, target.Distance(insecPosition) + 200).To3D();

            } 
            if(turrets.Any() && paramBool("insec2tower"))
            {
                return V2E(turrets[0].Position, target.Position, target.Distance(turrets[0].Position) + 200).To3D();
            }
            if (paramBool("insec2orig"))
            {
                return V2E(insecPos, target.Position, target.Distance(insecPos) + 200).To3D();
            }
            return new Vector3();
        }

        enum InsecComboStepSelect { NONE, QGAPCLOSE, WGAPCLOSE, PRESSR };
        static InsecComboStepSelect InsecComboStep;
        static void InsecCombo(Obj_AI_Hero target)
        {
             if (_player.Distance(getInsecPos(target)) <= 100) InsecComboStep = InsecComboStepSelect.PRESSR;
             else if (InsecComboStep == InsecComboStepSelect.NONE && getInsecPos(target).Distance(_player.Position) < 600) InsecComboStep = InsecComboStepSelect.WGAPCLOSE;
             else if(InsecComboStep == InsecComboStepSelect.NONE && target.Distance(_player) < Q.Range) InsecComboStep = InsecComboStepSelect.QGAPCLOSE; 
             
           
            switch (InsecComboStep)
            {
                case InsecComboStepSelect.QGAPCLOSE:
                    if ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)) && Q.Instance.Name == "BlindMonkQOne")
                    {
                        CastQ1(target);
                    }
                    else if (target.HasBuff("BlindMonkQOne"))
                    {
                        Q.Cast();
                        InsecComboStep = InsecComboStepSelect.WGAPCLOSE;
                    }
                    break;
                case InsecComboStepSelect.WGAPCLOSE:
                    WardJump(getInsecPos(target), false, false, true);
                    break;
                case InsecComboStepSelect.PRESSR:
                    R.CastOnUnit(target, true);
                    break;
                    
            }
        }

        static Vector3 InterceptionPoint(List<Obj_AI_Hero> heroes)
        {
            Vector3 result = new Vector3();
            foreach (Obj_AI_Hero hero in heroes)
            result += hero.Position;
            result.X /= heroes.Count;
            result.Y /= heroes.Count;
            return result;
        }

        static List<Obj_AI_Hero> GetAllyInsec(List<Obj_AI_Hero> heroes)
        {
            byte alliesAround = 0;
            Obj_AI_Hero tempObject = new Obj_AI_Hero();
            foreach (Obj_AI_Hero hero in heroes)
            {
                int localTemp = GetAllyHeroes(hero, 500).Count;
                if (localTemp > alliesAround)
                {
                    tempObject = hero;
                    alliesAround = (byte)localTemp;
                }
            }
            return GetAllyHeroes(tempObject, 500);
        }

        static List<Obj_AI_Hero> GetAllyHeroes(Obj_AI_Hero position, int range)
        {
            List<Obj_AI_Hero> temp = new List<Obj_AI_Hero>();
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                if (hero.IsAlly && !hero.IsMe && hero.Distance(position) < range)
                    temp.Add(hero);
            return temp;
        }
        static bool HasBuff(Obj_AI_Base target, string buffName)
        {
            foreach (BuffInstance buff in target.Buffs)
                if (buff.Name == buffName) return true;
            return false;
        }
        static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }

        public static void SaveMe()
        {

            if ((_player.Health / _player.MaxHealth * 100) > Menu.Item("hpPercentSM").GetValue<Slider>().Value || _player.SummonerSpellbook.CanUseSpell(smiteSlot) != SpellState.Ready) return;
            var epicSafe = false;
            var buffSafe = false;
            foreach (
                var minion in
                    MinionManager.GetMinions(_player.Position, 1000f, MinionTypes.All, MinionTeam.Neutral,
                        MinionOrderTypes.None))
            {
                foreach (var minionName in epics)
                {
                    if (minion.Name.ToLower().Contains(minionName.ToLower()) && hpLowerParam(minion, "hpEpics") && paramBool("dEpics"))
                    {
                        epicSafe = true;
                        break;
                    }
                }
                foreach (var minionName in buffs)
                {
                    if (minion.Name.ToLower().Contains(minionName.ToLower()) && hpLowerParam(minion, "hpBuffs") && paramBool("dBuffs"))
                    {
                        buffSafe = true;
                        break;
                    }
                }
            }

            if(epicSafe || buffSafe) return;

            foreach (var minion in MinionManager.GetMinions(_player.Position, 700f, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth))
            {
                if (!W.IsReady() && !_player.HasBuff("BlindMonkIronWill") || smiteSlot == SpellSlot.Unknown ||
                    smiteSlot != SpellSlot.Unknown &&
                    _player.SummonerSpellbook.CanUseSpell(smiteSlot) != SpellState.Ready) break;
                if (minion.Name.ToLower().Contains("ward")) return;
                if (W.Instance.Name != "blindmonkwtwo")
                {
                    W.Cast();
                    W.Cast();
                }
                if (_player.HasBuff("BlindMonkIronWill"))
                {
                    _player.SummonerSpellbook.CastSpell(smiteSlot, minion);
                }
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if(_player.IsDead) return;

            if(SimpleTs.GetSelectedTarget() == null) InsecComboStep = InsecComboStepSelect.NONE;

            if (paramBool("smiteSave"))
            {
                SaveMe();
            }
            if (Menu.Item("InsecEnabled").GetValue<KeyBind>().Active)
            {
                if (paramBool("insecOrbwalk"))
                {
                    Orbwalk(Game.CursorPos);
                }
                
                 if(SimpleTs.GetSelectedTarget() != null) InsecCombo(SimpleTs.GetSelectedTarget());
            }
            else
            {
                isNullInsecPos = true;
            }
            if(LXOrbwalker.CurrentMode != LXOrbwalker.Mode.Combo) InsecComboStep = InsecComboStepSelect.NONE;
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    StarCombo();
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    AllClear();
                    break;
            }
            if(Menu.Item("wjump").GetValue<KeyBind>().Active)
                wardjumpToMouse();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var target = SimpleTs.GetSelectedTarget();
            if(target != null)
            {
                Drawing.DrawCircle(getInsecPos(target), 100, System.Drawing.Color.White);
            }
            
        }


        public static void wardjumpToMouse()
        {
            WardJump(Game.CursorPos, paramBool("m2m"), paramBool("maxRange"), paramBool("castInRange"), paramBool("j2m"), paramBool("j2c"));
        }
        public static void PrintMessage(string msg) // Credits to ChewyMoon, and his Brain.exe
        {
            Game.PrintChat("<font color=\"#6699ff\"><b>FALeeSin:</b></font> <font color=\"#FFFFFF\">" + msg + "</font>");
        }

        public static void Orbwalk(Vector3 pos, Obj_AI_Hero target = null)
        {
            LXOrbwalker.Orbwalk(pos, target);
        }
        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return _player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }
        private static InventorySlot FindBestWardItem()
        {
            var slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;

            var sdi = GetItemSpell(slot);

            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return slot;
            }
            return null;
        }
        
        public static bool packets()
        {
            return Menu.Item("NFE").GetValue<bool>();
        }

        public static void useItems(Obj_AI_Hero enemy)
        {
            if (Items.CanUseItem("Bilgewater Cutlass") && _player.Distance(enemy) <= 450) 
                Items.UseItem("Bilgewater Cutlass", enemy);
            if(Items.CanUseItem("Blade of the Ruined King") && _player.Distance(enemy) <= 450)
                Items.UseItem("Blade of the Ruined King", enemy);
            if(Items.CanUseItem("Tiamat") && Utility.CountEnemysInRange(350) >= 1)
                Items.UseItem("Tiamat");
            if(Items.CanUseItem("Ravenous Hydra") && Utility.CountEnemysInRange(350) >= 1)
                Items.UseItem("Ravenous Hydra");
            if(Items.CanUseItem(3143) && Utility.CountEnemysInRange(450) >= 1)
                Items.UseItem(3143);
        }
        public static void AllClear()
        {
            var passiveIsActive = _player.HasBuff("blindmonkpassive_cosmetic", true);
            var minion =
                MinionManager.GetMinions(_player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                    .OrderBy(x => x.Distance(_player))
                    .FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward")) return;
            if (Menu.Item("useQClear").GetValue<bool>() && Q.IsReady())
            {
                if (Q.Instance.Name == "BlindMonkQOne")
                {
                    if (!passiveIsActive)
                    {
                        Q.Cast(minion, true);
                    }
                    
                }else if ((minion.HasBuff("BlindMonkQOne", true) ||
                          minion.HasBuff("blindmonkqonechaos", true)) && (!passiveIsActive || Q.IsKillable(minion, 1)) ||
                          _player.Distance(minion) > 500) Q.Cast();
                
            }
            if (Menu.Item("useEClear").GetValue<bool>() && E.IsReady())
            {
                if (E.Instance.Name == "BlindMonkEOne")
                {
                    if (!passiveIsActive)
                        E.Cast();
                }
                else if (minion.HasBuff("BlindMonkEOne", true) && (!passiveIsActive || _player.Distance(minion) > 450))
                {
                    E.Cast();
                }
            }

        }
        
        private static void WardJump(Vector3 pos, bool m2m = true, bool maxRange = false, bool reqinMaxRange = false, bool minions = true, bool champions = true)
        {
            var basePos = _player.Position.To2D();
            var newPos = (pos.To2D() - _player.Position.To2D());
            if (m2m)
            {
                Orbwalk(pos);
            }
            if (!W.IsReady() || W.Instance.Name == "blindmonkwtwo" || reqinMaxRange && _player.Distance(pos) > W.Range) return;
            if (minions || champions)
            {
                if (maxRange || _player.Distance(pos) > W.Range)
                {
                    JumpPos = basePos + (newPos.Normalized() * (W.Range));
                }
                else
                {
                    JumpPos = basePos + (newPos.Normalized() * (_player.Distance(pos)));
                }
                if (champions)
                {
                    var champs =
                        (from champ in ObjectManager.Get<Obj_AI_Hero>()
                            where champ.IsAlly && champ.Distance(_player) < W.Range && champ.Distance(pos) < 100 && !champ.IsMe
                            select champ).ToList();
                    if (champs.Count > 0)
                    {
                        W.CastOnUnit(champs[0], true);
                        return;
                    }
                }
                if (minions)
                {
                    var minion2 =
                        (from minion in ObjectManager.Get<Obj_AI_Minion>()
                            where
                                minion.IsAlly && minion.Distance(_player) < W.Range && minion.Distance(pos) < 200 &&
                                !minion.Name.ToLower().Contains("ward")
                            select minion).ToList();
                    if (minion2.Count > 0)
                    {
                        W.CastOnUnit(minion2[0], true);
                        return;
                    }
                }
            }
            if (maxRange || _player.Distance(pos) > 560)
            {
                JumpPos = basePos + (newPos.Normalized()*(560));
            }
            else
            {
                JumpPos = basePos + (newPos.Normalized() * (_player.Distance(pos)));
            }
            if (Utility.IsWall(JumpPos.To3D())) return;
            var isWard = false;
            foreach (var ward in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (ward.IsAlly && ward.Name.ToLower().Contains("ward") && ward.Distance(pos) < 150)
                {
                    isWard = true;
                    W.CastOnUnit(ward, true);
                }
            }
            if (!isWard && CastWardAgain)
            {
                var ward = FindBestWardItem();
                ward.UseItem(JumpPos.To3D());
                CastWardAgain = false;
                Utility.DelayAction.Add(1000, () => CastWardAgain = true);
            }
        }

        

        public static void StarCombo()
        {
            var target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);
            if (target == null) return;
            if ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true)))
            {
                if (CastQAgain || target.HasBuffOfType(BuffType.Knockup) && !_player.IsValidTarget(300) && !R.IsReady() || !target.IsValidTarget(LXOrbwalker.GetAutoAttackRange(_player)) && !R.IsReady())
                {
                    Q.Cast();
                }
            }

            if (E.IsReady() && E.Instance.Name == "BlindMonkEOne" && target.IsValidTarget(E.Range))
                E.Cast();

            if (E.IsReady() && E.Instance.Name != "BlindMonkEOne" &&
                !target.IsValidTarget(LXOrbwalker.GetAutoAttackRange(_player)))
                E.Cast();

            if (Q.IsReady() && Q.Instance.Name == "BlindMonkQOne")
                CastQ1(target);

            if (R.IsReady() && Q.IsReady() &&
                ((target.HasBuff("BlindMonkQOne", true) || target.HasBuff("blindmonkqonechaos", true))))
                R.CastOnUnit(target, packets());
        }

        public static void CastQ1(Obj_AI_Hero target)
        {
            var Qpred = Q.GetPrediction(target);
            if (Qpred.CollisionObjects.Count == 1 && _player.SummonerSpellbook.CanUseSpell(smiteSlot) == SpellState.Ready && paramBool("qSmite"))
            {
                _player.SummonerSpellbook.CastSpell(smiteSlot, Qpred.CollisionObjects[1]);
                Utility.DelayAction.Add(70, () => Q.Cast(target, packets()));
            }
            else if(Qpred.CollisionObjects.Count == 0)
            {
                Q.Cast(target, packets());
            }
        }

        public static bool paramBool(String paramName)
        {
            return Menu.Item(paramName).GetValue<bool>();
        }

        public static bool hpLowerParam(Obj_AI_Base obj, String paramName)
        {
            return ((obj.Health / obj.MaxHealth) * 100) <= Menu.Item(paramName).GetValue<Slider>().Value;
        }
    }
}
