using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace KurisuFiora
{
    internal class KurisuFiora
    {
        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _config;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static Obj_AI_Hero _target;

        private static readonly Spell Q = new Spell(SpellSlot.Q, 600f);
        private static readonly Spell W = new Spell(SpellSlot.W, float.MaxValue);
        private static readonly Spell E = new Spell(SpellSlot.E, float.MaxValue);
        private static readonly Spell R = new Spell(SpellSlot.R, 400f);
        private static readonly List<Spell> SpellList = new List<Spell>();

        private int _packetTargetHit;
        private bool _packetPlayerHit = false;

        public KurisuFiora()
        {
            Console.WriteLine("Kurisu assembly is starting...");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region Fiora: OnGameLoad
        private void Game_OnGameLoad(EventArgs args)
        {
            try
            {

                Game.PrintChat("<font color=\"#CCFFF2\">[Fiora]</font><font color=\"#99FFE6\"> - <u>the Grand Duelist v1.0</u></font> - Kurisu 漏");
                SpellList.Add(Q);
                SpellList.Add(R);
                var enemy = from hero in ObjectManager.Get<Obj_AI_Hero>()
                            where hero.IsEnemy == true
                            select hero;               
                _config = new Menu("鏃犲弻鍓戝К", "fiora", true);
                _config.AddSubMenu(new Menu("璧扮爫", "orbwalker"));
                _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("orbwalker"));

                Menu fioraSelector = new Menu("鐩爣閫夋嫨", "targetselector");
                SimpleTs.AddToMenu(fioraSelector);
                _config.AddSubMenu(fioraSelector);

                Menu fioraDraws = new Menu("鏄剧ず", "drawings");
                fioraDraws.AddItem(new MenuItem("drawQ", "Q 鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.SpringGreen)));
                fioraDraws.AddItem(new MenuItem("drawR", "R 鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.MediumTurquoise)));
                _config.AddSubMenu(fioraDraws);


                Menu fioraSpells = new Menu("杩炴嫑", "combo");
                fioraSpells.AddItem(new MenuItem("useq", "浣跨敤 Q")).SetValue(true);
                //fioraSpells.AddItem(new MenuItem("useqminion", "浣跨敤 Q 杩戣韩")).SetValue(true);
                fioraSpells.AddItem(new MenuItem("qrange", "浣跨敤 Q 鑼冨洿")).SetValue(new Slider(250, 1, (int)Q.Range));
                fioraSpells.AddItem(new MenuItem("usee", "浣跨敤 E")).SetValue(true);
                fioraSpells.AddItem(new MenuItem("smana", "杩炴嫑 钃濋噺 % ")).SetValue(new Slider(0, 0, 99));
                _config.AddSubMenu(fioraSpells);

                Menu fioraParry = new Menu("鍙嶅嚮", "fparry");
                fioraParry.AddItem(new MenuItem("usew", "浣跨敤 W")).SetValue(true);
                fioraParry.AddItem(new MenuItem("wsett", "鑷姩 W 浼ゅ!")).SetValue(new Slider(50, 1, 200));
                fioraParry.AddItem(new MenuItem("wdodge", "鑷姩 W")).SetValue(true);               
                fioraParry.AddItem(new MenuItem("", ""));
                foreach (var e in enemy)
                {
                    var qdata = e.Spellbook.GetSpell(SpellSlot.Q);
                    var wdata = e.Spellbook.GetSpell(SpellSlot.W);
                    var edata = e.Spellbook.GetSpell(SpellSlot.E);
                    if (KurisuLib.DangerousList.Any(spell => spell.Contains(qdata.SData.Name)))
                        fioraParry.AddItem(new MenuItem("ws" + e.SkinName, qdata.SData.Name)).SetValue(true);
                    if (KurisuLib.DangerousList.Any(spell => spell.Contains(wdata.SData.Name)))
                        fioraParry.AddItem(new MenuItem("ws" + e.SkinName, wdata.SData.Name)).SetValue(true);
                    if (KurisuLib.DangerousList.Any(spell => spell.Contains(edata.SData.Name)))
                        fioraParry.AddItem(new MenuItem("ws" + e.SkinName, edata.SData.Name)).SetValue(true);
                }
                _config.AddSubMenu(fioraParry);

                Menu fioraWaltz = new Menu("澶ф嫑", "fbw");
                fioraWaltz.AddItem(new MenuItem("user", "浣跨敤 R")).SetValue(true);
                fioraWaltz.AddItem(new MenuItem("rdodge", "浣跨敤 R 韬查伩鍗遍櫓")).SetValue(true);
                fioraWaltz.AddItem(new MenuItem("", ""));                                         
                foreach (var e in enemy)
                {
                    SpellDataInst rdata = e.Spellbook.GetSpell(SpellSlot.R);
                    if (KurisuLib.DangerousList.Any(spell => spell.Contains(rdata.SData.Name)))
                        fioraWaltz.AddItem(new MenuItem("ds" + e.SkinName, rdata.SData.Name)).SetValue(true);
                }
 
                _config.AddSubMenu(fioraWaltz);

                Menu fioraMisc = new Menu("鏉傞」", "fmisc");            
                fioraMisc.AddItem(new MenuItem("usetiamat", "鑷姩 閬撳叿")).SetValue(true);
                _config.AddSubMenu(fioraMisc);

                Menu fioraDebug = new Menu("璋冭瘯", "fdbg");
                //fioraDebug.AddItem(new MenuItem("aadebug", "ConsoleWrite aa tick")).SetValue(false);
                _config.AddSubMenu(fioraDebug);

                //_config.AddItem(new MenuItem("ksteal", "鎶汉澶")).SetValue(true);
                _config.AddItem(new MenuItem("isteal", "鑷姩鐐圭噧")).SetValue(true);
                _config.AddItem(new MenuItem("usepackets", "浣跨敤灏佸寘")).SetValue(true);
                _config.AddToMainMenu();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.PrintChat("Error something went wrong with fiora assembly(Menu)");
            }

            #region L# Reqs
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
            Obj_AI_Base.OnProcessSpellCast += Game_OnGameProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            #endregion
        }

        #endregion

        #region Fiora: DrawingOnDraw
        private void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var circle = _config.Item("draw" + spell.Slot.ToString()).GetValue<Circle>();
                if (circle.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, circle.Color, 5, 55);
            }

            Utility.DrawCircle(_target.Position, _target.BoundingRadius, Color.OrangeRed, 5, 55);
        }
        #endregion

        #region Fiora: OnGameSendPacket
        private void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (args.PacketData[0] == 119)
                    args.Process = false;

                if (args.PacketData[0] != 154 || _orbwalker.ActiveMode.ToString() != "Combo") return;
                var cast = Packet.C2S.Cast.Decoded(args.PacketData);
                switch (cast.Slot)
                {
                    case SpellSlot.E:
                        Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(0, 0, 3, _orbwalker.GetTarget().NetworkId)).Send();
                        Orbwalking.ResetAutoAttackTimer();
                        if ((Items.HasItem(3077) && Items.CanUseItem(3077) || (Items.HasItem(3074) && Items.CanUseItem(3074)) && _config.Item("usetiamat").GetValue<bool>()))
                        {
                            Utility.DelayAction.Add(Game.Ping + 125, delegate
                            {
                                Items.UseItem(3077);
                                Items.UseItem(3074);
                                Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(0, 0, 3, _orbwalker.GetTarget().NetworkId)).Send();
                                Orbwalking.ResetAutoAttackTimer();
                                //Console.WriteLine("aacancel: 2");
                            });
                        }
                        break;
                }
            }
            catch
            {
                Game.PrintChat("Error something went wrong with fiora assembly(OnGameSendPacket)");
            }

        }
        #endregion

        #region Fiora: OnGameProccessPacket
        private void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.Damage.Header)
            {
                var damage = Packet.S2C.Damage.Decoded(args.PacketData);
                var source = damage.SourceNetworkId;
                var target = damage.TargetNetworkId;

                _packetTargetHit = target;
                _packetPlayerHit = true;
            }
        }
        #endregion

        #region Fiora: OnGameUpdate
        private void Game_OnGameUpdate(EventArgs args)
        {                 
            if (_orbwalker.ActiveMode.ToString() == "Combo")
            {
                _target = SimpleTs.GetTarget(750, SimpleTs.DamageType.Physical);
                UseCombo(_target);
                    
            }
        }
        #endregion

        #region Fiora: OnGameProcessSpellCast
        private void Game_OnGameProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            double incDmg = 0;
            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy && args.Target.IsMe)
            {

                Obj_AI_Hero attacker = ObjectManager.Get<Obj_AI_Hero>().First(n => n.NetworkId == sender.NetworkId);
                if (attacker != null)
                {
                    var slot = attacker.GetSpellSlot(args.SData.Name);
                    if (slot == SpellSlot.Unknown)
                    {
                        incDmg = sender.GetAutoAttackDamage(Player);
                    }

                }
              
            }

            UseW(incDmg);

            if (_config.Item("rdodge").GetValue<bool>() && _config.Item("ds" + sender.SkinName).GetValue<bool>())
                if (KurisuLib.DangerousList.Any(spell => spell.Contains(args.SData.Name)) &&
                    (sender.Distance(Player) < 400f || Player.Distance(args.End) <= 250f ) && R.IsReady())
                    R.Cast(sender);

            if (_config.Item("wdodge").GetValue<bool>())
                if (KurisuLib.OnHitEffectList.Any(spell => spell.Contains(args.SData.Name)) && W.IsReady())
                    W.Cast();

        }
        #endregion

        private void UseW(double damage)
        {
            var incDamageSlider = _config.Item("wsett").GetValue<Slider>().Value;
            int incDamagePercent = (int)(Player.Health / damage * 100);
            if (_config.Item("usew").GetValue<bool>())
            {
                if (Player.Spellbook.CanUseSpell(SpellSlot.W) != SpellState.Unknown)
                {
                    if (W.IsReady() && damage > incDamageSlider)
                        Player.Spellbook.CastSpell(SpellSlot.W);
                }
            }
        }

        private void UseCombo(Obj_AI_Hero target)
        {
            UseE(target);
            UseQ(target);
            UseR(target);
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            try
            {
                
                if (enemy != null)
                {
                    var ignote = Player.GetSpellSlot("summonerdot");

                    if (Q.IsReady())
                        damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                    if (R.IsReady())
                        damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                    if (Player.SummonerSpellbook.CanUseSpell(ignote) == SpellState.Ready)
                        damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                    if (Items.HasItem(3077) && Items.CanUseItem(3077))
                        damage += Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
                    if (Items.HasItem(3074) && Items.CanUseItem(3074))
                        damage += Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
                    if (Items.HasItem(3153) && Items.CanUseItem(3153))
                        damage += Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
                    if (Items.HasItem(3144) && Items.CanUseItem(3144))
                        damage += Player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.PrintChat("Error on combodamage(Fiora)");
            }

            return (float)damage;
        }

        private static bool UsePackets()
        {
            return _config.Item("usepackets").GetValue<bool>();
        }

        private void UseE(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (_config.Item("usee").GetValue<bool>())
                {
                    int minMana = _config.Item("smana").GetValue<Slider>().Value;
                    int actualHeroManaPercent = (int)((Player.Mana / Player.MaxMana) * 100);
                    if (E.IsReady() && Player.Distance(target.Position) < Player.AttackRange)
                    {
                        if (actualHeroManaPercent > minMana && _packetTargetHit == target.NetworkId && _packetPlayerHit)
                        {
                            Player.Spellbook.CastSpell(SpellSlot.E);
                            _packetPlayerHit = false;
                        }
                    }


                }
            }
        }

        private void UseQ(Obj_AI_Hero target)
        {
            if (target != null && _config.Item("useq").GetValue<bool>())
            {
                float minRange = _config.Item("qrange").GetValue<Slider>().Value;
                if (Q.IsReady() && Player.Distance(target.ServerPosition) < 600f)
                {
                    if (Player.Distance(target.ServerPosition) >= minRange)
                        Q.Cast(target, UsePackets());
                }
            }
        }

        private void UseR(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (_config.Item("user").GetValue<bool>())
                {
                    if (R.IsReady() && Player.Distance(target.Position) < R.Range)
                    {
                        if (ComboDamage(target) > target.Health)
                        {
                            if (_config.Item("isteal").GetValue<bool>())
                            {
                                var ignote = Player.GetSpellSlot("summonerdot");
                                Player.SummonerSpellbook.CastSpell(ignote, target);
                            }

                            R.CastOnUnit(target, UsePackets());
                        }

                    }
                }
            }
        }

        private void Killsteal()
        {
            if (_config.Item("ksteal").GetValue<bool>())
            {
                List<Obj_AI_Hero> enemy = ObjectManager.Get<Obj_AI_Hero>().Where(h => h.Team != Player.Team).ToList();

                foreach (Obj_AI_Hero e in enemy)
                {
                    var qdmg = Player.GetSpellDamage(e, SpellSlot.Q);
                    var rdmg = Player.GetSpellDamage(e, SpellSlot.R);
                    if (Q.IsReady() && e.Health < qdmg)
                        Q.CastOnUnit(e, UsePackets());
                    if (R.IsReady() && e.Health < rdmg)
                        R.CastOnUnit(e, UsePackets());
                }
            }
        }

    }
}
