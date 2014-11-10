using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace VayneHunterRework
{
    class VayneHunterRework
    {
        public static LXOrbwalker Orbwalker = new LXOrbwalker();
        public static Orbwalking.Orbwalker COrbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static String charName = "Vayne";
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static Vector3 AfterCond = Vector3.Zero;
        public static Obj_AI_Base current; // for tower farming
        public static Obj_AI_Base last; // for tower farming

        public VayneHunterRework()
        {
            CustomEvents.Game.OnGameLoad +=Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != charName) return;
            Cleanser.CreateQSSSpellList();
            Menu = new Menu("Vayne鐚庢墜", "VHRework", true);
            var lxMenu = new Menu("璧扮爫閫夐」", "LXOrb");
            //LXOrbwalker.AddToMenu(lxMenu);
            COrbwalker = new Orbwalking.Orbwalker(lxMenu);
            Menu.AddSubMenu(lxMenu);
            var tsMenu = new Menu("鐩爣閫夋嫨", "TargetSel");
            SimpleTs.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);
            Menu.AddSubMenu(new Menu("杩炴嫑閫夐」", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQC", "浣跨敤Q")).SetValue(true);
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseEC", "浣跨敤E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseRC", "浣跨敤R").SetValue(false));
            Menu.SubMenu("Combo").AddItem(new MenuItem("QManaC", "Q钃濋噺%").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("EManaC", "E钃濋噺%").SetValue(new Slider(20, 1, 100)));

            Menu.AddSubMenu(new Menu("楠氭壈閫夐」", "Harrass"));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "浣跨敤Q")).SetValue(true);
            Menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "浣跨敤E").SetValue(true));
           // Menu.SubMenu("Harrass").AddItem(new MenuItem("3RdE", "绗笁涓E").SetValue(true));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("QManaH", "Q钃濋噺%").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Harrass").AddItem(new MenuItem("EManaH", "E钃濋噺%").SetValue(new Slider(20, 1, 100)));
            Menu.AddSubMenu(new Menu("琛ュ叺閫夐」", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLH", "浣跨敤Q灏惧垁")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQLC", "浣跨敤Q娓呯嚎")).SetValue(true);
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLH", "Q钃濋噺% LH").SetValue(new Slider(35, 1, 100)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("QManaLC", "Q钃濋噺% LC").SetValue(new Slider(35, 1, 100)));
            Menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Packets", "浣跨敤灏佸寘").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AntiGP", "鍙嶉绛")).SetValue(true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "鎵撴柇").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("SmartQ", "QE璧锋墜").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("ENext", "鑷姩E").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("PushDistance", "E璺濈").SetValue(new Slider(425, 400, 500)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("CondemnTurret", "E鍒伴槻寰″笅").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AutoE", "鑷姩E").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("NoEEnT", "No E Under濉斾笅").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("WallTumble", "鑷姩绌垮").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            Menu.SubMenu("Misc").AddItem(new MenuItem("ThreshLantern", "鑷姩鐏").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
            Menu.AddSubMenu(new Menu("鑽変笡閫夐」", "BushReveal"));
            //Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushReveal", "Bush Revealer").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.SubMenu("BushReveal").AddItem(new MenuItem("BushRevealer", "鑷姩鎺㈢溂").SetValue(true));
            Menu.AddSubMenu(new Menu("閬撳叿閫夐」", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkC", "杩炴嫑浣跨敤鐮磋触").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("BotrkH", "楠氭壈浣跨敤鐮磋触").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuC", "杩炴嫑浣跨敤骞芥ⅵ").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("YoumuuH", "楠氭壈浣跨敤骞芥ⅵ").SetValue(false));
            Menu.SubMenu("Items").AddItem(new MenuItem("OwnHPercBotrk", "鐮磋触鐢熷懡%").SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("Items").AddItem(new MenuItem("EnHPercBotrk", "鐮磋触鏁屼汉%").SetValue(new Slider(20, 1, 100)));

            Menu.AddSubMenu(new Menu("瑙ｆ帶閫夐」", "QSSMenu"));
           Menu.SubMenu("QSSMenu").AddItem(new MenuItem("UseQSS", "鑷姩瑙ｆ帶").SetValue(true));
            Menu.AddSubMenu(new Menu("瑙ｆ帶BUFF", "QSST"));
            Cleanser.CreateTypeQSSMenu();
            Menu.AddSubMenu(new Menu("瑙ｆ帶SPELLS", "QSSSpell"));
            Cleanser.CreateQSSSpellMenu();
            Menu.AddSubMenu(new Menu("鍒敤E", "NoCondemn"));
            CreateNoCondemnMenu();
            /**
            Menu.AddSubMenu(new Menu("[VH] AutoLeveler", "AutoLevel"));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALSeq", "AutoLevel Seq").SetValue(new StringList(new []{"Q,W,E Max W,Q,E","Q,E,W Max Q,W,E"},0)));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALAct", "AutoLevel Active").SetValue(false));
             * */
            Menu.AddSubMenu(new Menu("鏄剧ず閫夐」", "Draw"));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawE", "E鑼冨洿").SetValue(new Circle(true,Color.MediumPurple)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawCond", "Draw Pos. Aft. E if Stun").SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawDrake", "Draw Drake Spot").SetValue(new Circle(true, Color.WhiteSmoke)));
            Menu.SubMenu("Draw").AddItem(new MenuItem("DrawMid", "Draw Mid Spot").SetValue(new Circle(true, Color.WhiteSmoke)));
            Menu.AddToMainMenu();
            Game.PrintChat("<font color='#FF0000'>VayneHunter</font> <font color='#FFFFFF'>Rework loaded!</font>");
            Game.PrintChat("By <font color='#FF0000'>DZ</font><font color='#FFFFFF'>191</font>. Special Thanks to: Kurisuu");
            Game.PrintChat("If you like my assemblies feel free to donate me (link on the forum :) )");
           //Cleanser.cleanUselessSpells();
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E,550f);
            R = new Spell(SpellSlot.R);
            E.SetTargetted(0.25f,1600f);
            Orbwalking.AfterAttack += Orbwalker_AfterAttack;
            Game.OnGameUpdate += Game_OnGameUpdate;
           // Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
           Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
           AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
           Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += Cleanser.OnCreateObj;
            GameObject.OnDelete += Cleanser.OnDeleteObj;
        }

       
        private void Orbwalker_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (unit.IsMe && target.IsValidTarget())
            {
                 AfterAA(target);
            }
        }
        
        void AfterAA(Obj_AI_Base target)
        {
            if (!(target is Obj_AI_Hero)) return;
            var tar = (Obj_AI_Hero)target;


            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:

                    if (isMenuEnabled("UseQC")) SmartQCheck(tar);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (isMenuEnabled("UseQH")) SmartQCheck(tar);
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:

                default:
                    break;
            }

            ENextAuto(tar);
            UseItems(tar);
        }
        private void ENextAuto(Obj_AI_Hero tar)
        {
            if (!E.IsReady() || !tar.IsValid || !Menu.Item("ENext").GetValue<KeyBind>().Active) return;
                CastE(tar,true);
            Menu.Item("ENext").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle,false));
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            //Cleanser.enableCheck();
            if (Player.IsDead) return;
            Obj_AI_Hero tar;

            if (isMenuEnabled("AutoE") && CondemnCheck(Player.Position, out tar)) { CastE(tar,true);}
            if (Menu.Item("WallTumble").GetValue<KeyBind>().Active) WallTumble();
            if (Menu.Item("ThreshLantern").GetValue<KeyBind>().Active) takeLantern();
            QFarmCheck();
            //Cleanser
            Cleanser.cleanserBySpell();
            Cleanser.cleanserByBuffType();

            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Obj_AI_Hero tar2;
                    if (isMenuEnabled("UseEC") && CondemnCheck(Player.ServerPosition, out tar2)) { CastE(tar2);}
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Obj_AI_Hero tar3;
                    if (isMenuEnabled("UseEH") && CondemnCheck(Player.ServerPosition, out tar3)) { CastE(tar3); }
                    break;
                default:
                    break;
            }  
        }
        

        void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var DrawE = Menu.Item("DrawE").GetValue<Circle>();
            var DrawCond = Menu.Item("DrawCond").GetValue<Circle>();
            var DrawDrake = Menu.Item("DrawDrake").GetValue<Circle>();
            var DrawMid = Menu.Item("DrawMid").GetValue<Circle>();
            Vector2 MidWallQPos = new Vector2(6010.5869140625f, 8508.8740234375f);
            Vector2 DrakeWallQPos = new Vector2(11334.74f, 4517.47f);
            if (DrawDrake.Active && Player.Distance(DrakeWallQPos) < 1500f) Utility.DrawCircle(new Vector3(11590.95f, 4656.26f, 0f), 75f, DrawDrake.Color);
            if (DrawMid.Active && Player.Distance(MidWallQPos) < 1500f) Utility.DrawCircle(new Vector3(6623, 8649, 0f), 75f, DrawMid.Color);
            if (DrawE.Active)Utility.DrawCircle(Player.Position,E.Range,DrawE.Color);
            if (DrawCond.Active) DrawPostCondemn();
            
        }

        void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var GPSender = (Obj_AI_Hero)gapcloser.Sender;
            if (!isMenuEnabled("AntiGP") || !E.IsReady() || !GPSender.IsValidTarget()) return;
            CastE(GPSender, true);

        }

        void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var Sender = (Obj_AI_Hero)unit;
            if (!isMenuEnabled("Interrupt") || !E.IsReady() || !Sender.IsValidTarget()) return;
            CastE(Sender,true);
        }

        bool CondemnCheck(Vector3 Position, out Obj_AI_Hero target)
        {
            if (isUnderEnTurret(Player.Position) && isMenuEnabled("NoEEnT"))
            {
                target = null;
                return false;
            }
            foreach (var En in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget() && !isMenuEnabled("nC"+hero.ChampionName) && hero.Distance(Player.Position)<=E.Range))
            {
                var EPred = E.GetPrediction(En);
                int pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                for (int i = 0; i < pushDist; i += (int)En.BoundingRadius)
                {
                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Position.To2D(), -i).To3D();
                    var OrTurret = isMenuEnabled("CondemnTurret") && isUnderTurret(loc3);
                    AfterCond = loc3;
                    if (isWall(loc3) || OrTurret)
                    {
                        if(isMenuEnabled("BushRevealer"))CheckAndWard(Position,loc3,En);
                        target = En;
                        return true; 
                    }
                }
            }
            target = null;
            return false;
            
        }

        void QFarmCheck()
        {
            if (!Q.IsReady()) return;
            var PosAfterQ = Player.Position.To2D().Extend(Game.CursorPos.To2D(), 300);
            var minList =
                MinionManager.GetMinions(Player.Position, 550f).Where(min =>
                    HealthPrediction.GetHealthPrediction(min,(int)(Q.Delay + min.Distance(PosAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000)+Game.Ping <= (Q.GetDamage(min)+Player.GetAutoAttackDamage(min))
                    && HealthPrediction.GetHealthPrediction(min, (int)(Q.Delay + min.Distance(PosAfterQ) / Orbwalking.GetMyProjectileSpeed()) * 1000)+Game.Ping > 0);
            if (!minList.First().IsValidTarget()) return;
            CastQ(Vector3.Zero,minList.First());
        }
        
        void SmartQCheck(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !target.IsValidTarget()) return;
            if (!isMenuEnabled("SmartQ") || !E.IsReady())
            {
                CastQ(Game.CursorPos,target);
            }
            else
            {
                for (int I = 0; I <= 360; I += 65)
                {
                    var F1 = new Vector2(Player.Position.X + (float)(300 * Math.Cos(I * (Math.PI / 180))), Player.Position.Y + (float)(300 * Math.Sin(I * (Math.PI / 180)))).To3D();
                   // var FinalPos = Player.Position.To2D().Extend(F1, 300).To3D();
                    Obj_AI_Hero targ;
                    if (CondemnCheck(F1, out targ))
                    {
                        CastTumble(F1,target);
                        CastE(target);
                        return;
                    }
                }
                CastQ(Game.CursorPos, target);
            }
        }

        void CastQ(Vector3 Pos,Obj_AI_Base target,bool customPos=false)
        {
           if (!Q.IsReady() || !target.IsValidTarget()) return;
           
            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var ManaC = Menu.Item("QManaC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaC && isMenuEnabled("UseQC"))
                    {
                        if(isMenuEnabled("UseRC") && R.IsReady())R.CastOnUnit(Player);
                        if(!customPos){CastTumble(target);}else{CastTumble(Pos,target);}
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var ManaH = Menu.Item("QManaH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaH && isMenuEnabled("UseQH")){ if (!customPos){ CastTumble(target);} else{ CastTumble(Pos, target);}}
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    var ManaLH = Menu.Item("QManaLH").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaLH && isMenuEnabled("UseQLH")) { if (!customPos) { CastTumble(target); } else { CastTumble(Pos, target); } }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    var ManaLC = Menu.Item("QManaLC").GetValue<Slider>().Value;
                    if (getPerValue(true) >= ManaLC && isMenuEnabled("UseQLC")){ if (!customPos){CastTumble(target); }else{ CastTumble(Pos, target);}}
                    break;
                default:
                    break;
            }
        }

        void CastTumble(Obj_AI_Base target)
        {
            
            //Q.Cast(Game.CursorPos, isMenuEnabled("Packets"));
         //  return;
          
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100)Q.Cast(Game.CursorPos, isMenuEnabled("Packets"));
        }
        void CastTumble(Vector3 Pos,Obj_AI_Base target)
        {
           //Q.Cast(Pos, isMenuEnabled("Packets"));
          //  return;
            var posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(Pos.To2D(), 300).To3D();
            var distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550 * 550 && distanceAfterTumble > 100 * 100) Q.Cast(Pos, isMenuEnabled("Packets"));
        }
        void CastE(Obj_AI_Hero target, bool isForGapcloser = false)
        {
            if (!E.IsReady() || !target.IsValidTarget()) return;
            if (isForGapcloser)
            {
                E.Cast(target, isMenuEnabled("Packets"));
                AfterCond = Vector3.Zero;
                return;
            }
            switch (COrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var ManaC = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if (isMenuEnabled("UseEC") && getPerValue(true) >= ManaC)
                    {
                        E.Cast(target, isMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var ManaH = Menu.Item("EManaH").GetValue<Slider>().Value;
                    if (isMenuEnabled("UseEH") && getPerValue(true) >= ManaH)
                    {
                        E.Cast(target, isMenuEnabled("Packets"));
                        AfterCond = Vector3.Zero;
                    }
                    break;
                default:
                    break;
            }
        }
        private static void CreateNoCondemnMenu()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                Menu.SubMenu("NoCondemn").AddItem(new MenuItem("nC"+hero.ChampionName, hero.ChampionName).SetValue(false));
            }
        }
        
        #region Items & Tumble
        void UseItems(Obj_AI_Hero tar)
        {
            var ownH = getPerValue(false);
            if ((Menu.Item("BotrkC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
                ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar,false))))
            {
                UseItem(3153, tar);
            }
            if ((Menu.Item("BotrkH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && (Menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= ownH) &&
               ((Menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getPerValueTarget(tar, false))))
            {
                UseItem(3153, tar);
            }
            if (Menu.Item("YoumuuC").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3142);
            }
            if (Menu.Item("YoumuuH").GetValue<bool>() && COrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                UseItem(3142);
            }
        }
        void WallTumble()
        {
            //Credits to Chogart
            Vector2 MidWallQPos = new Vector2(6010.5869140625f, 8508.8740234375f);
            Vector2 DrakeWallQPos = new Vector2(11334.74f, 4517.47f);
            if (Player.Distance(MidWallQPos) >= Player.Distance(DrakeWallQPos))
            {

                if (Player.Position.X < 11540 || Player.Position.X > 11600 || Player.Position.Y < 4638 ||
                    Player.Position.Y > 4712)
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(11590.95f, 4656.26f)).Send();
                }
                else
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(11590.95f, 4656.26f)).Send();
                    Q.Cast(DrakeWallQPos, true);
                }
            }
            else
            {
                if (Player.Position.X < 6600 || Player.Position.X > 6660 || Player.Position.Y < 8630 || Player.Position.Y > 8680)
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(6623, 8649)).Send();
                }
                else
                {
                    Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(6623, 8649)).Send();
                    Q.Cast(MidWallQPos, true);
                }
            }
        }
        
        void takeLantern()
        {
            foreach (GameObject obj in ObjectManager.Get<GameObject>())
            {
                if (obj.Name.Contains("ThreshLantern") &&obj.Position.Distance(ObjectManager.Player.ServerPosition) <= 500 && obj.IsAlly)
                {
                    GamePacket pckt =Packet.C2S.InteractObject.Encoded(new Packet.C2S.InteractObject.Struct(ObjectManager.Player.NetworkId,obj.NetworkId));
                    pckt.Send();
                    return;
                }
            }
        }
        #endregion
        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }

        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;
            SpellDataInst sdi = GetItemSpell(slot);
            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)return slot;
            return null;
        }
        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        bool isWall(Vector3 Pos)
        {
            CollisionFlags cFlags = NavMesh.GetCollisionFlags(Pos);
            return (cFlags == CollisionFlags.Wall);
        }
        bool isUnderTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsAlly && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }
        bool isUnderEnTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsEnemy && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }
        public static bool isMenuEnabled(String val)
        {
            return Menu.Item(val).GetValue<bool>();
        }
        float getPerValue(bool mana)
        {
            if (mana) return (Player.Mana / Player.MaxMana) * 100;
            return (Player.Health / Player.MaxHealth) * 100;
        }
        float getPerValueTarget(Obj_AI_Hero target, bool mana)
        {
            if (mana) return (target.Mana / target.MaxMana) * 100;
            return (target.Health / target.MaxHealth) * 100;
        }
        bool isGrass(Vector3 Pos)
        {
            return NavMesh.IsWallOfGrass(Pos);
        }

        void CheckAndWard(Vector3 sPos, Vector3 EndPosition, Obj_AI_Hero target)
        {
            if (isGrass(EndPosition))
            {
                var WardSlot = FindBestWardItem();
                if (WardSlot == null) return;
                for (int i = 0; i < Vector3.Distance(sPos, EndPosition); i += (int)target.BoundingRadius)
                {
                    var v = sPos.To2D().Extend(EndPosition.To2D(), -i).To3D();
                    if (isGrass(v))
                    {
                        WardSlot.UseItem(v);
                    }
                }
            }
        }

        void DrawPostCondemn()
        {
            var DrawCond = Menu.Item("DrawCond").GetValue<Circle>();
            foreach (var En in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget() && !isMenuEnabled("nC" + hero.ChampionName) && hero.Distance(Player.Position) <= E.Range))
            {
                var EPred = E.GetPrediction(En);
                int pushDist = Menu.Item("PushDistance").GetValue<Slider>().Value;
                for (int i = 0; i < pushDist; i += (int)En.BoundingRadius)
                {
                    Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.Position.To2D(), -i).To3D();
                    if (isWall(loc3)) Utility.DrawCircle(loc3, 100f, DrawCond.Color);

                }
            }
        }
        
    }
    
}