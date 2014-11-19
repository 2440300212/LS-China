using System;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace KurisuRiven
{
    internal class KurisuRiven
    {
        #region  Main

        public static Menu config;     
        private static Obj_AI_Hero enemy;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;
        private static Orbwalking.Orbwalker orbwalker;

        private static int etick;
        private static int tritick;
        private static int runiccount;
        public static int cleavecount;

        private static double ritems;
        private static double ua, uq, uw;
        private static double ra, rq, rw, rr, ri;
        private static float truerange;

        private static readonly Spell valor = new Spell(SpellSlot.E, 390f);
        private static readonly Spell wings = new Spell(SpellSlot.Q, 280f);
        private static readonly Spell kiburst = new Spell(SpellSlot.W, 260f);
        private static readonly Spell blade = new Spell(SpellSlot.R, 900f);

        private static bool blockmove;
        private static double now;
        private static double killsteal;
        private static double extraqtime;
        private static double extraetime;

        private static bool ultion, useblade, useautows;
        private static bool usecombo, useclear, usecursor;
        private static int gaptime, wslash, wsneed, bladewhen;

        private static float ee, ff;
        private static readonly int[] items = { 3144, 3153, 3142, 3112, 3131 };
        private static readonly int[] runicpassive =
        {
            20, 20, 25, 25, 25, 30, 30, 30, 35, 35, 35, 40, 40, 40, 45, 45, 45, 50, 50
        };

        private static readonly string[] jungleminions =
        {
            "AncientGolem",  "GreatWraith", "Wraith",  "LizardElder",  "Golem", "Worm",   "Dragon", 
            "GiantWolf", "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp",
            "SRU_Razorbeak", "SRU_Krug"
        };

        #endregion

        public KurisuRiven()
        {
            Console.WriteLine("KurisuRiven is loading..");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region  OnGameLoad
        private void Game_OnGameLoad(EventArgs args)
        {
            if (me.BaseSkinName != "Riven")
                return;    
 
            Initialize();
            
            config = new Menu("鏀鹃€愪箣鍒億", "kriven", true);

            // Target Selector
            Menu menuTS = new Menu("鐩爣閫夋嫨", "tselect");
            SimpleTs.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("璧扮爫閫夐」", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            Menu menuK = new Menu("蹇嵎閿畖", "demkeys");
            menuK.AddItem(new MenuItem("combokey", "杩炴嫑")).SetValue(new KeyBind(32, KeyBindType.Press));
            menuK.AddItem(new MenuItem("harasskey", "楠氭壈")).SetValue(new KeyBind(67, KeyBindType.Press));
            menuK.AddItem(new MenuItem("clearkey", "娓呯嚎")).SetValue(new KeyBind(86, KeyBindType.Press));
            menuK.AddItem(new MenuItem("jumpkey", "缈诲")).SetValue(new KeyBind(88, KeyBindType.Press));
            menuK.AddItem(new MenuItem("changemode", "鍒囨崲妯″紡")).SetValue(new KeyBind(90, KeyBindType.Press));
            config.AddSubMenu(menuK);

            // Draw settings
            Menu menuD = new Menu("鏄剧ず閫夐」", "dsettings");
            menuD.AddItem(new MenuItem("dsep1", "==== 鏄剧ず璁剧疆"));
            menuD.AddItem(new MenuItem("drawrr", "R 鑼冨洿")).SetValue(true);
            menuD.AddItem(new MenuItem("drawaa", "AA 鑼冨洿")).SetValue(true);
            menuD.AddItem(new MenuItem("drawp", "鏄剧ず 琚姩 鏁伴噺")).SetValue(true);
            menuD.AddItem(new MenuItem("drawengage", "绐佽繘 鑼冨洿")).SetValue(true);
            menuD.AddItem(new MenuItem("drawjumps", "绌垮 鎻愮ず")).SetValue(true);
            menuD.AddItem(new MenuItem("drawkill", "鍑绘潃 鎻愮ず")).SetValue(true);
            config.AddSubMenu(menuD);

            // Combo Settings
            Menu menuC = new Menu("杩炴嫑閫夐」", "csettings");
            menuC.AddItem(new MenuItem("csep1", "==== E 璁剧疆"));
            menuC.AddItem(new MenuItem("usevalor", "浣跨敤 E")).SetValue(true);
            menuC.AddItem(new MenuItem("valorhealth", "浣跨敤 E HP %")).SetValue(new Slider(40));
            menuC.AddItem(new MenuItem("waitvalor", "浣跨敤 R-E")).SetValue(true);
            menuC.AddItem(new MenuItem("csep2", "==== R 璁剧疆"));
            menuC.AddItem(new MenuItem("useblade", "浣跨敤 R")).SetValue(true);
            menuC.AddItem(new MenuItem("bladewhen", "浣跨敤 R 闅惧害: "))
                .SetValue(new StringList(new[] { "Easy", "Normal", "Hard" }, 2));
            menuC.AddItem(new MenuItem("checkover", "妫€娴媩鍑绘潃|")).SetValue(true);
            menuC.AddItem(new MenuItem("csep3", "==== Q 璁剧疆"));
            menuC.AddItem(new MenuItem("nostickyq", "浣跨敤 Non-Target Q")).SetValue(false);
            menuC.AddItem(new MenuItem("blockmove", "鍙栨秷 绉诲姩")).SetValue(false);
            menuC.AddItem(new MenuItem("blockanim", "鍙栨秷 Q 鍔ㄤ綔")).SetValue(false);
            menuC.AddItem(new MenuItem("qqdelay", "杩戣韩 Q 寤惰繜: ")).SetValue(new Slider(1200, 1, 4000));
            menuC.AddItem(new MenuItem("asep2", "==== 澶氭妧鑳絴璁剧疆"));
            menuC.AddItem(new MenuItem("ssWQ", "灏濊瘯 W->Q")).SetValue(true);
            menuC.AddItem(new MenuItem("ssEQ", "灏濊瘯 E->Q")).SetValue(true);
            menuC.AddItem(new MenuItem("ssEWS", "灏濊瘯 E->鐤鹃鏂﹟")).SetValue(true);
            menuC.AddItem(new MenuItem("ssWSQ", "灏濊瘯 鐤鹃鏂﹟->Q")).SetValue(true);

            config.AddSubMenu(menuC);

            // Extra Settings
            Menu menuO = new Menu("鍏朵粬閫夐」", "osettings");
            menuO.AddItem(new MenuItem("osep2", "==== 鍏朵粬 璁剧疆"));
            menuO.AddItem(new MenuItem("useignote", "浣跨敤 鐐圭噧")).SetValue(true);
            menuO.AddItem(new MenuItem("enableAntiG", "闃茬獊杩泑")).SetValue(true);
            menuO.AddItem(new MenuItem("useautow", "鑷姩 W")).SetValue(true);
            menuO.AddItem(new MenuItem("autow", "鑷姩|W|鏁屼汉鏁皘")).SetValue(new Slider(3, 1, 5));
            menuO.AddItem(new MenuItem("osep1", "==== 鐤鹃鏂﹟璁剧疆"));
            menuO.AddItem(new MenuItem("useautows", "浣跨敤|鐤鹃鏂﹟")).SetValue(true);
            menuO.AddItem(new MenuItem("wslash", "鐤鹃鏂﹟: "))
                .SetValue(new StringList(new[] { "Only Kill", "Max Damage" }, 1));
            menuO.AddItem(new MenuItem("autows", "鐤鹃鏂﹟HP %")).SetValue(new Slider(65, 1));
            menuO.AddItem(new MenuItem("autows2", "鐤鹃鏂﹟鏁屼汉鏁皘 >=")).SetValue(new Slider(3, 2, 5));
            menuO.AddItem(new MenuItem("osep3", "==== 鎵撴柇 璁剧疆"));
            menuO.AddItem(new MenuItem("interrupter", "浣跨敤 鎵撴柇")).SetValue(true);
            menuO.AddItem(new MenuItem("interruptQ3", "浣跨敤 Q3 鎵撴柇")).SetValue(true);
            menuO.AddItem(new MenuItem("interruptW", "浣跨敤 W 鎵撴柇")).SetValue(true);
            config.AddSubMenu(menuO);

            // Farm/Clear Settings
            Menu menuJ = new Menu("琛ュ叺閫夐」", "jsettings");
            menuJ.AddItem(new MenuItem("jsep1", "==== 鎵撻噹 璁剧疆"));
            menuJ.AddItem(new MenuItem("jungleE", "浣跨敤 E ")).SetValue(true);
            menuJ.AddItem(new MenuItem("jungleW", "浣跨敤 W ")).SetValue(true);
            menuJ.AddItem(new MenuItem("jungleQ", "浣跨敤 Q")).SetValue(true);
            menuJ.AddItem(new MenuItem("jsep2", "==== 琛ュ叺 璁剧疆"));
            menuJ.AddItem(new MenuItem("farmE", "浣跨敤 E")).SetValue(true);
            menuJ.AddItem(new MenuItem("farmW", "浣跨敤 W")).SetValue(true);
            menuJ.AddItem(new MenuItem("farmQ", "浣跨敤 Q")).SetValue(true);
            config.AddSubMenu(menuJ);

            Menu rivenD = new Menu("璋冭瘯閫夐」", "therivend");
            rivenD.AddItem(new MenuItem("dsep2", "==== 璋冭瘯 璁剧疆"));
            rivenD.AddItem(new MenuItem("debugdmg", "杩炴嫑 浼ゅ")).SetValue(false);
            rivenD.AddItem(new MenuItem("debugtrue", "鐪熷疄鑼冨洿")).SetValue(false);
            rivenD.AddItem(new MenuItem("exportjump", "杈撳嚭 浣嶇疆")).SetValue(new KeyBind(73, KeyBindType.Press));
            rivenD.AddItem(new MenuItem("asep4", "==== 涓嶄娇鐢▅"));
            rivenD.AddItem(new MenuItem("cursormode", "鍏夋爣 璋冭瘯 妯″紡")).SetValue(false);

            config.AddSubMenu(rivenD);
            config.AddToMainMenu();


            blade.SetSkillshot(0.25f, 300f, 120f, false, SkillshotType.SkillshotCone);

            var wc = new WebClient { Proxy = null };
            wc.DownloadString("http://league.square7.ch/put.php?name=Kurisu-Riven");

            var amount = wc.DownloadString("http://league.square7.ch/get.php?name=Kurisu-Riven");
            var intamount = Convert.ToInt32(amount);
            var tcolor = config.Item("wslash").GetValue<StringList>().SelectedIndex == 0;
            var hex = tcolor ? "#7CFC00" : "#FF00FF";

            Game.PrintChat("<font color='" + hex + "'>KurisuRiven r.0998</font> - Loaded");
            Game.PrintChat("<font color='" + hex + "'>KurisuRiven</font> has been used in <font color='" + hex + "'>" + intamount + "</font> games."); // Post Counter Data

           
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            // initialize walljumps
            new KurisuLib();

            // On Game Draw
            Drawing.OnDraw += Game_OnDraw;

            // On Game Update
            Game.OnGameUpdate += Game_OnGameUpdate;

            // On Game Process Packet
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;

            // On Possible Interrupter
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            //On Enemy Gapcloser
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            // On Game Process Spell Cast
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

        }

        #endregion

        #region  OnGameUpdate

        private static bool color;
        private void Game_OnGameUpdate(EventArgs args)
        {
            blockmove = false;
            color = wslash == 0;
            if (config.Item("changemode").GetValue<KeyBind>().Active)
            {
                if (wslash == 1)
                    Utility.DelayAction.Add(300,
                        () => config.Item("wslash").SetValue(new StringList(new[] {"Only Kill", "Max Damage"}, 0)));
                else if (wslash == 0)
                    Utility.DelayAction.Add(300,
                        () => config.Item("wslash").SetValue(new StringList(new[] {"Only Kill", "Max Damage"}, 1)));
            }

            CheckDamage(usecursor ? me : enemy);

            if (usecombo || killsteal + extraqtime > now)
            {
                CastCombo(usecursor ? me : enemy); 
            }

            Killsteal();
            Clear();
            AutoW();
            Requisites();
            RefreshBuffs();
            WindSlash();
        }

        #endregion

        #region  Requisites
        private void Requisites()
        {
            now = TimeSpan.FromMilliseconds(Environment.TickCount).TotalSeconds;
            enemy = SimpleTs.GetTarget(750, SimpleTs.DamageType.Physical);
            truerange = me.AttackRange + me.Distance(me.BBox.Minimum) + 1;

            ee = (ff - Game.Time > 0) ? (ff - Game.Time) : 0;
            ultion = me.HasBuff("RivenFengShuiEngine", true);

            wsneed = config.Item("autows").GetValue<Slider>().Value;
            gaptime = config.Item("qqdelay").GetValue<Slider>().Value;

            usecombo = config.Item("combokey").GetValue<KeyBind>().Active;
            useclear = config.Item("clearkey").GetValue<KeyBind>().Active;
            useautows = config.Item("useautows").GetValue<bool>();

            bladewhen = config.Item("bladewhen").GetValue<StringList>().SelectedIndex;
            wslash = config.Item("wslash").GetValue<StringList>().SelectedIndex;

            usecursor = config.Item("cursormode").GetValue<bool>();
            useblade = config.Item("useblade").GetValue<bool>();

            extraqtime = TimeSpan.FromMilliseconds(gaptime).TotalSeconds;
            extraetime = TimeSpan.FromMilliseconds(300).TotalSeconds;
        }

        #endregion

        #region  On Draw
        private void Game_OnDraw(EventArgs args)
        {

            if (config.Item("drawaa").GetValue<bool>() && !me.IsDead)
                Utility.DrawCircle(me.Position, me.AttackRange + 35   , color ? Color.LawnGreen : Color.Magenta, 1, 1);
            if (config.Item("drawrr").GetValue<bool>() && !me.IsDead)
                Utility.DrawCircle(me.Position, blade.Range, color ? Color.LawnGreen : Color.Magenta, 1, 1);
            if (config.Item("drawp").GetValue<bool>() && !me.IsDead)
            {
                var wts = Drawing.WorldToScreen(me.Position);

                if (wslash == 0)
                    Drawing.DrawText(wts[0] - 55, wts[1] + 30, Color.LawnGreen, "Mode: Only Kill");
                else
                    Drawing.DrawText(wts[0] - 55, wts[1] + 30, Color.Magenta, "Mode: Max Damage");
                if (me.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.NotLearned)
                    Drawing.DrawText(wts[0] - 35, wts[1] + 10, Color.White, "Q: Not Learned!");
                else if (ee <= 0)
                    Drawing.DrawText(wts[0] - 35, wts[1] + 10, Color.White, "Q: Ready");
                else
                    Drawing.DrawText(wts[0] - 35, wts[1] + 10, Color.White, "Q: " + ee.ToString("0.0"));

            }
            if (config.Item("debugtrue").GetValue<bool>())
            {
                if (!me.IsDead)
                {
                    Utility.DrawCircle(me.Position, truerange + 25, Color.Yellow, 1, 1);
                }
            }

            if (config.Item("drawengage").GetValue<bool>())
                if (!me.IsDead)
                    Utility.DrawCircle(me.Position, valor.Range + me.AttackRange - 35, color ? Color.LawnGreen : Color.Magenta, 1, 1);

            if (config.Item("drawkill").GetValue<bool>())
            {
                if (enemy != null && !enemy.IsDead && !me.IsDead)
                {
                    var ts = enemy;
                    var wts = Drawing.WorldToScreen(enemy.Position);
                    if ((float)(ra + rq * 2 + rw + ri + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 20, wts[1] + 40, Color.OrangeRed, "Kill!");
                    else if ((float)(ra * 2 + rq * 2 + rw + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Easy Kill!");
                    else if ((float)(ua * 3 + uq * 2 + uw + ri + rr + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Full Combo Kill!");
                    else if ((float)(ua * 3 + uq * 3 + uw + rr + ri + ritems) > ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Full Combo Hard Kill!");
                    else if ((float)(ua * 3 + uq * 3 + uw + rr + ri +ritems) < ts.Health)
                        Drawing.DrawText(wts[0] - 40, wts[1] + 40, Color.OrangeRed, "Cant Kill!");

                }
            }

            if (config.Item("debugdmg").GetValue<bool>())
            {
                if (enemy != null && !enemy.IsDead && !me.IsDead)
                {
                    var wts = Drawing.WorldToScreen(enemy.Position);
                    if (!blade.IsReady())
                        Drawing.DrawText(wts[0] - 75, wts[1] + 60, Color.Orange,
                            "Combo Damage: " + (float)(ra * 3 + rq * 3 + rw + rr + ri + ritems));
                    else
                        Drawing.DrawText(wts[0] - 75, wts[1] + 60, Color.Orange,
                            "Combo Damage: " + (float) (ua*3 + uq*3 + uw + rr + ri + ritems));
                }
            }

            if (config.Item("drawjumps").GetValue<bool>())
            {
                var jumplist = KurisuLib.jumpList;
                if (jumplist.Any())
                {
                    foreach (var j in jumplist)
                    {
                        if (me.Distance(j.pointA) <= 800 || me.Distance(j.pointB) <= 800)
                        {
                            Utility.DrawCircle(j.pointA, 100, color ? Color.LawnGreen : Color.Magenta, 1, 1);
                            Utility.DrawCircle(j.pointB, 100, color ? Color.LawnGreen : Color.Magenta, 1, 1);
                        }
                    }
                }
            }
        }

        #endregion

        #region Clear
        private void Clear()
        {
            if (!useclear) 
                return;

            var target = orbwalker.GetTarget();                            
            if (target.IsValidTarget(kiburst.Range) && jungleminions.Any(name => target.Name.StartsWith(name)))
            {
                if (kiburst.IsReady() && config.Item("jungleW").GetValue<bool>())
                    kiburst.Cast();
            }

            else if (target.IsValidTarget(wings.Range) && target.Name.StartsWith("Minion"))
            {
                if (!valor.IsReady() || !config.Item("farmE").GetValue<bool>()) return;
                if (wings.IsReady() && cleavecount >= 1)
                    valor.Cast(Game.CursorPos);
            }

            if (kiburst.IsReady() && config.Item("farmW").GetValue<bool>())
            {
                var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(kiburst.Range)).ToList();

                if (minions.Count() > 2)
                {
                    if (Items.HasItem(3077) && Items.CanUseItem(3077))
                        Items.UseItem(3077);
                    if (Items.HasItem(3074) && Items.CanUseItem(3074))
                        Items.UseItem(3074);
                    kiburst.Cast();
                }
            }

        }

        #endregion

        #region  AntiGapcloser
        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("enableAntiG").GetValue<bool>())
                return;

            if (gapcloser.Sender.Type == me.Type && gapcloser.Sender.IsValid)
                if (gapcloser.Sender.Distance(me.Position) < kiburst.Range && kiburst.IsReady())
                    kiburst.Cast();
        }
        #endregion

        #region  Interrupter
        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base sender, InterruptableSpell spell)
        {
            if (!config.Item("interuppter").GetValue<bool>())
                return;

            if (sender.Type == me.Type && sender.IsValid && sender.Distance(me.Position) < wings.Range)
                if (wings.IsReady() && cleavecount == 2 && config.Item("interruptQ3").GetValue<bool>())
                    wings.Cast(sender.Position, true);

            if (sender.Type == me.Type && sender.IsValid && sender.Distance(me.Position) < kiburst.Range)
                if (kiburst.IsReady() && config.Item("interruptW").GetValue<bool>())
                    kiburst.Cast();
        }
        #endregion

        #region  OnProcessSpellCast

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var target = enemy;

            if (!sender.IsMe)
                return;

            switch (args.SData.Name)
            {
                case "RivenTriCleave":
                    if (config.Item("blockmove").GetValue<bool>() && (usecombo || useclear))
                        blockmove = true;

                    tritick = Environment.TickCount;
                    if (cleavecount < 1)
                        ff = Game.Time + (13 + (13*me.PercentCooldownMod));
                    break;
                case "RivenMartyr":
                    Orbwalking.LastAATick = 0;
                    if (!config.Item("ssWQ").GetValue<bool>())
                        return;

                    if (wings.IsReady() && (usecombo || killsteal + extraqtime > now))
                        Utility.DelayAction.Add(Game.Ping + 75, () => wings.Cast(usecursor ? Game.CursorPos : target.Position, true));

                    if (wings.IsReady() && useclear)
                        Utility.DelayAction.Add(Game.Ping + 75, () => wings.Cast(orbwalker.GetTarget(), true));

                    break;
                case "ItemTiamatCleave":
                    Orbwalking.LastAATick = 0;
                    if (target.IsValidTarget(kiburst.Range) && (usecombo || killsteal + extraqtime > now))
                        if (kiburst.IsReady())
                            kiburst.Cast();
                    break;
                case "RivenFeint":
                    etick = Environment.TickCount;
                    Orbwalking.LastAATick = 0;

                    if (usecombo || killsteal + extraqtime > now)
                        castitems(target);

                    if (!config.Item("ssEWS").GetValue<bool>())
                        return;

                    if (!useblade || !useautows)
                        return;

                    if (blade.IsReady() && (usecombo || killsteal + extraqtime > now))
                        if (ultion && wslash == 1)
                            blade.Cast(usecursor ? Game.CursorPos : target.Position, true);
                    break;
                case "RivenFengShuiEngine":
                    Orbwalking.LastAATick = 0;

                    if (!usecombo || killsteal + extraqtime < now)
                        return;

                    castitems(target);
                    if (!target.IsValidTarget(kiburst.Range))
                        return;

                    if (kiburst.IsReady())
                        kiburst.Cast();
                    break;
                case "rivenizunablade":
                    if (!config.Item("ssWSQ").GetValue<bool>())
                        return;

                    if (wings.IsReady())
                        wings.Cast(usecursor ? Game.CursorPos : target.Position, true);
                    break;
            }
        }

        #endregion

        #region  OnProcessPacket
        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            var packet = new GamePacket(args.PacketData);
            if (packet.Header == 0xb0 && config.Item("blockanim").GetValue<bool>())
            {
                packet.Position = 0x1;
                if (packet.ReadInteger() == me.NetworkId)
                    args.Process = false;
            }
            
            else if (packet.Header == 0x61)
            {
                packet.Position = 0xc;
                if (packet.ReadInteger() != me.NetworkId)
                    return;

                if (config.Item("blockmove").GetValue<bool>())
                    args.Process = !blockmove;
            }

            else if (packet.Header == 0x65)
            {
                packet.Position = 0x10;
                int sourceId = packet.ReadInteger();

                if (sourceId != me.NetworkId)
                    return;

                packet.Position = 0x1;
                int targetId = packet.ReadInteger();
                int dmgType = packet.ReadByte();

                var trueTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(targetId);
                var trueOutput = wings.GetPrediction(trueTarget);

                var nonstickyPosition = new Vector3();
                if (trueOutput.Hitchance >= HitChance.Low)
                    nonstickyPosition = new Vector3(nonstickyPosition.X, nonstickyPosition.Y, nonstickyPosition.Z);

                var nosticky = config.Item("nostickyq").GetValue<bool>();
                if ((dmgType == 0x4 || dmgType == 0x3) && wings.IsReady())
                {
                    switch (trueTarget.Type)
                    {
                        case GameObjectType.obj_Barracks:
                        case GameObjectType.obj_AI_Turret:
                            if (!config.Item("clearkey").GetValue<KeyBind>().Active)
                                return;
                            castitems(trueTarget);
                            wings.Cast(trueTarget.Position, true);
                            break;
                        case GameObjectType.obj_AI_Hero:
                            if (config.Item("combokey").GetValue<KeyBind>().Active) 
                                castitems(trueTarget);
                            if (config.Item("combokey").GetValue<KeyBind>().Active) 
                                wings.Cast(nosticky ? nonstickyPosition : trueTarget.Position, true);
                            if (config.Item("harasskey").GetValue<KeyBind>().Active)
                                wings.Cast(nosticky ? nonstickyPosition : trueTarget.Position, true);
                            break;
                        case GameObjectType.obj_AI_Minion:
                            if (!config.Item("clearkey").GetValue<KeyBind>().Active)
                                return;
                            if (jungleminions.Any(name => trueTarget.Name.StartsWith(name)) && !trueTarget.Name.Contains("Mini") &&
                                wings.IsReady() && config.Item("jungleQ").GetValue<bool>())
                                wings.Cast(trueTarget.Position, true);
                            if (trueTarget.Name.StartsWith("Minion") && config.Item("farmQ").GetValue<bool>())
                            {
                                wings.Cast(trueTarget.Position, true);
                                orbwalker.ForceTarget(trueTarget);
                            }
                            if (valor.IsReady() && cleavecount >= 1 && config.Item("jungleE").GetValue<bool>())
                                valor.Cast(trueTarget.Position);
                            break;
                    }
                }
            }
            else if (packet.Header == 0x38 && packet.Size() == 0x9)
            {
                packet.Position = 0x1;
                int sourceId = packet.ReadInteger();
                if (sourceId != me.NetworkId)
                    return;

                if (config.Item("blockmove").GetValue<bool>())
                    blockmove = true;

                int targetId = orbwalker.GetTarget().NetworkId;
                Obj_AI_Base trueTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(targetId);
                if (me.Distance(orbwalker.GetTarget().Position) > truerange + 25)
                    return;

                Vector3 movePos;
                if (trueTarget.Type == GameObjectType.obj_Barracks || trueTarget.Type == GameObjectType.obj_AI_Turret ||
                    trueTarget.Type == GameObjectType.obj_AI_Minion)
                    movePos = trueTarget.Position + Vector3.Normalize(me.Position -
                                                                      trueTarget.Position)*(me.Distance(trueTarget.Position) + 63);
                else if (trueTarget.Type == GameObjectType.obj_AI_Hero)
                    movePos = trueTarget.Position + Vector3.Normalize(me.Position -
                                                                      trueTarget.Position)*(me.Distance(trueTarget.Position) + 58);
                else
                    movePos = Game.CursorPos;

                me.IssueOrder(GameObjectOrder.MoveTo, new Vector3(movePos.X, movePos.Y, movePos.Z));
                if (trueTarget.Type == GameObjectType.obj_AI_Minion)
                    Utility.DelayAction.Add(Game.Ping + 70, () => Orbwalking.LastAATick = 0);
                else
                    Orbwalking.LastAATick = 0;
            }

            else if (packet.Header == 0xfe && packet.Size() == 0x18)
            {
                packet.Position = 1;
                if (packet.ReadInteger() == me.NetworkId)
                {
                    Orbwalking.LastAATick = Environment.TickCount;
                    Orbwalking.LastMoveCommandT = Environment.TickCount;
                }
            }
        }

        #endregion

        #region  Combo Logic
        private void CastCombo(Obj_AI_Base target)
        {
            var healthvalor = config.Item("valorhealth").GetValue<Slider>().Value;
            if (target.IsValidTarget())
            {
                if (me.Distance(usecursor ? Game.CursorPos : target.Position) > truerange + 25 ||
                    ((me.Health / me.MaxHealth) * 100) <= healthvalor)
                {
                    if (valor.IsReady() && config.Item("usevalor").GetValue<bool>())
                        valor.Cast(usecursor ? Game.CursorPos : target.Position);
                    if (wings.IsReady() && cleavecount <= 1 && config.Item("waitvalor").GetValue<bool>())
                        CheckR(usecursor ? me : target);
                }

                if (kiburst.IsReady() && wings.IsReady() && valor.IsReady()
                    && me.Distance(usecursor ? me.Position : target.Position) < kiburst.Range + 20)
                {
                    if (cleavecount <= 1)
                        if (!usecursor) CheckR(target);
                }

                if (blade.IsReady() && valor.IsReady() && ultion)
                {
                    if (cleavecount == 2)
                        valor.Cast(usecursor ? Game.CursorPos : target.Position);
                }


                if (me.Distance(usecursor ? Game.CursorPos : target.Position) < kiburst.Range)
                {
                    if (Items.HasItem(3077) && Items.CanUseItem(3077))
                        Items.UseItem(3077);
                    if (Items.HasItem(3074) && Items.CanUseItem(3074))
                        Items.UseItem(3074);
                    if (kiburst.IsReady())
                        kiburst.Cast();
                }

                if (wings.IsReady() && !valor.IsReady() &&
                    me.Distance(usecursor ? Game.CursorPos : target.Position) > wings.Range)
                {
                    if (TimeSpan.FromMilliseconds(tritick).TotalSeconds + extraqtime < now &&
                        TimeSpan.FromMilliseconds(etick).TotalSeconds + extraetime < now)
                    {
                        wings.Cast(target.Position, true);
                    }
                }
            }
        }

        #endregion

        #region  Buff Handler

        private void RefreshBuffs()
        {
            var buffs = me.Buffs;

            foreach (var b in buffs)
            {
                if (b.Name == "rivenpassiveaaboost")
                    runiccount = b.Count;
                if (b.Name == "RivenTriCleave")
                    cleavecount = b.Count;
            }

            if (!me.HasBuff("rivenpassiveaaboost", true))
                runiccount = 0;
            if (!wings.IsReady())
                cleavecount = 0;
        }

        #endregion

        #region  Windlsash
        private static void WindSlash()
        {
            if (!ultion) 
                return;

            foreach (var e in ObjectManager.Get<Obj_AI_Hero>().Where(e => e.IsValidTarget(blade.Range)))
            {                   
                var hitcount = config.Item("autows2").GetValue<Slider>().Value;

                PredictionOutput prediction = blade.GetPrediction(usecursor ? me : e, true);
                if (blade.IsReady() && useautows)
                {
                    if (wslash == 1)
                    {
                        if (prediction.AoeTargetsHitCount >= hitcount)
                            blade.Cast(prediction.CastPosition, true);
                        else if (rr / e.MaxHealth * 100 > e.Health / e.MaxHealth * wsneed)
                        {
                            if (prediction.Hitchance >= HitChance.Medium)
                                blade.Cast(prediction.CastPosition);
                        }
                        else if (e.Health < rr + ra * 2 + rq * 1)
                        {
                            if (prediction.Hitchance >= HitChance.Medium)
                                blade.Cast(prediction.CastPosition);
                        }
                    }
                    else if (wslash == 0)
                    {
                        if (prediction.Hitchance >= HitChance.Medium && e.Health <= rr)
                            blade.Cast(prediction.CastPosition);
                    }
                }
            }
        }

        #endregion

        #region Killsteal
        private void Killsteal()
        {
            foreach (var e in ObjectManager.Get<Obj_AI_Hero>().Where(e => e.IsValidTarget(blade.Range)))
            {
                if (wings.IsReady() && e.Health < rq && me.Distance(enemy.Position) < wings.Range)
                    wings.Cast(enemy.Position, true);
                else if (wings.IsReady() && e.Health < rq + ra*2 + ri &&
                            me.Distance(enemy.Position) < wings.Range)
                {
                    enemy = e;
                    killsteal = TimeSpan.FromMilliseconds(Environment.TickCount).TotalSeconds;

                }
                else if (wings.IsReady() && e.Health < rq*2 + ri && me.Distance(enemy.Position) < wings.Range)
                {
                    enemy = e;
                    killsteal = TimeSpan.FromMilliseconds(Environment.TickCount).TotalSeconds;
                }
            }        
        }

        #endregion

        #region  Item Handler
        private static void castitems(Obj_AI_Base target)
        {
            foreach (var i in items.Where(i => Items.CanUseItem(i) && Items.HasItem(i)))
            {
                if (target.IsValidTarget(valor.Range + blade.Range))
                    Items.UseItem(i);
            }
        }
        #endregion

        #region  Damage Handler
        private static void CheckDamage(Obj_AI_Base target)
        {
            if (target == null) return;

            var ignite = me.GetSpellSlot("summonerdot");
            double aaa = me.GetAutoAttackDamage(target);

            double tmt = Items.HasItem(3077) && Items.CanUseItem(3077) ? me.GetItemDamage(target, Damage.DamageItems.Tiamat) : 0;
            double hyd = Items.HasItem(3074) && Items.CanUseItem(3074) ? me.GetItemDamage(target, Damage.DamageItems.Hydra) : 0;
            double bwc = Items.HasItem(3144) && Items.CanUseItem(3144) ? me.GetItemDamage(target, Damage.DamageItems.Bilgewater) : 0;
            double brk = Items.HasItem(3153) && Items.CanUseItem(3153) ? me.GetItemDamage(target, Damage.DamageItems.Botrk) : 0;

            rr = me.GetSpellDamage(target, SpellSlot.R);
            ra = aaa + (aaa * (runicpassive[me.Level] / 100));
            rq = wings.IsReady() ? DamageQ(target) : 0;
            rw = kiburst.IsReady() ? me.GetSpellDamage(target, SpellSlot.W) : 0;
            ri = me.SummonerSpellbook.CanUseSpell(ignite) == SpellState.Ready ? me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) : 0;

            ritems = tmt + hyd + bwc + brk;

            ua = blade.IsReady()
                ? ra +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2)
                : ua;

            uq = blade.IsReady()
                ? rq +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2 * 0.7)
                : uq;

            uw = blade.IsReady()
                ? rw +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2 * 1)
                : uw;

            rr = blade.IsReady()
                ? rr +
                  me.CalcDamage(target, Damage.DamageType.Physical,
                      me.BaseAttackDamage + me.FlatPhysicalDamageMod * 0.2)
                : rr;
        }

        public static float DamageQ(Obj_AI_Base target)
        {
            double dmg = 0;
            if (wings.IsReady())
            {
                dmg += me.CalcDamage(target, Damage.DamageType.Physical,
                    -10 + (wings.Level * 20) +
                    (0.35 + (wings.Level * 0.05)) * (me.FlatPhysicalDamageMod + me.BaseAttackDamage));
            }

            return (float)dmg;
        }

        #endregion

        #region  Ultimate Handler
        private void CheckR(Obj_AI_Base target)
        {
            if (useblade && usecombo)
            {
                switch (bladewhen)
                {
                    case 2:
                        if ((float) (ua*3 + uq*3 + uw + rr + ri + ritems) > target.Health && !ultion)
                        {
                            blade.Cast();
                            if (config.Item("useignote").GetValue<bool>() && blade.IsReady())
                                if (cleavecount <= 1)
                                    CastIgnite(target);
                        }
                        break;
                    case 1:
                        if ((float) (ra*3 + rq*3 + rw + rr + ri + ritems) > target.Health && !ultion)
                        {
                            blade.Cast();
                            if (config.Item("useignote").GetValue<bool>() && blade.IsReady())
                                if (cleavecount <= 1)
                                    CastIgnite(target);
                        }
                        break;
                    case 0:
                        if ((float) (ra*2 + rq*2 + rw + rr + ri + ritems) > target.Health && !ultion)
                        {
                            if (!config.Item("checkover").GetValue<bool>())
                                blade.Cast();
                        }
                        break;
                }
            }
        }
        
        #endregion

        #region  Ignote Handler
        private static void CastIgnite(Obj_AI_Base target)
        {
            if (target.IsValidTarget(600))
            {
                var ignote = me.GetSpellSlot("summonerdot");
                if (me.SummonerSpellbook.CanUseSpell(ignote) == SpellState.Ready)
                {
                    me.SummonerSpellbook.CastSpell(ignote, target);
                }
            }
        }
        #endregion

        #region  AutoW
        private void AutoW()
        {
            var getenemies = ObjectManager.Get<Obj_AI_Hero>().Where(en => en.IsValidTarget(kiburst.Range));
            if (getenemies.Count() >= config.Item("autow").GetValue<Slider>().Value)
            {
                if (kiburst.IsReady() && config.Item("useautow").GetValue<bool>())
                {
                    kiburst.Cast();
                }
            }

        }

        #endregion

    }
}
