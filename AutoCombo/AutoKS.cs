using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Autocombo
{
    internal class AutoKS
    {
        public static Obj_AI_Hero Player;
        public static Menu Config;
        public static Spell _Q;
        public static Spell _W;
        public static Spell _E;
        public DamageSpell Allydamage;
        public DamageSpell Mydamage;
        public string sq;
        public string sw;
        public string se;

        public AutoKS()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnProcessSpellCast;
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        public void OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            sq = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.Name;
            sw = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).SData.Name;
            se = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.Name;
            SetSpells();
        }

        static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }
        // Need to add delay
        // need to aadd auto attack !

        static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            var packet = new GamePacket(args.PacketData);

            if (packet.Header == 0xFE)
            {
                if (Packet.MultiPacket.OnAttack.Decoded(args.PacketData).Type == Packet.AttackTypePacket.TargetedAA)
                {
                    var unit = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(packet.ReadInteger());
                    var target =
                        ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(
                            Packet.MultiPacket.OnAttack.Decoded(args.PacketData).TargetNetworkId);
                    if (Player.Distance(target) <= _Q.Range || Player.Distance(target) <= _W.Range || Player.Distance(target) <= _E.Range)
                    {
                        
                        if (unit.IsAlly && target.IsEnemy)
                        {
                            var damage = unit.CalcDamage(target, Damage.DamageType.Physical,
                                (unit.BaseAttackDamage + unit.FlatPhysicalDamageMod));

                            var qdamage = _Q.GetDamage(target);
                            var wdamage = _W.GetDamage(target);
                            var edamage = _E.GetDamage(target);

                            Game.PrintChat("here");
                            if (_Q.IsReady() && qdamage > wdamage && qdamage > edamage)
                            {
                                if ((qdamage + damage) > target.Health && damage < target.Health && Player.Distance(target) < _Q.Range)
                                {
                                    if ((unit.Distance(target) / unit.BasicAttack.MissileSpeed) + unit.BasicAttack.SpellCastTime < (Player.Distance(target) / _Q.Speed) + _Q.Delay)
                                    {
                                        _Q.Cast(target, true);
                                    }
                                    else if ((unit.Distance(target) / unit.BasicAttack.MissileSpeed) + unit.BasicAttack.SpellCastTime > (Player.Distance(target) / _Q.Speed) + _Q.Delay)
                                    {
                                        Game.PrintChat(Func._Time(target, unit).ToString());
                                        Utility.DelayAction.Add(Func._Time(target, unit), () => _Q.Cast(target, true));
                                    }
                                }
                            }
                            else if (_W.IsReady() && wdamage > qdamage && wdamage > edamage)
                            {
                                if ((wdamage + damage) > target.Health && damage < target.Health && Player.Distance(target) < _W.Range)
                                {
                                    if ((unit.Distance(target) / unit.BasicAttack.MissileSpeed) + unit.BasicAttack.SpellCastTime < (Player.Distance(target) / _W.Speed) + _W.Delay)
                                    {
                                        _W.Cast(target, true);
                                    }
                                    else if ((unit.Distance(target) / unit.BasicAttack.MissileSpeed) + unit.BasicAttack.SpellCastTime > (Player.Distance(target) / _W.Speed) + _W.Delay)
                                    {
                                        Utility.DelayAction.Add(Func._Time(target, unit), () => _W.Cast(target, true));
                                    }
                                }

                            }
                            else if (_E.IsReady() && edamage > qdamage && edamage > wdamage)
                            {
                                if ((edamage + damage) > target.Health && damage < target.Health && Player.Distance(target) < _E.Range)
                                {
                                    if ((unit.Distance(target) / unit.BasicAttack.MissileSpeed) + unit.BasicAttack.SpellCastTime < (Player.Distance(target) / _E.Speed) + _E.Delay)
                                    {
                                        _E.Cast(target, true);
                                    }
                                    else if ((unit.Distance(target) / unit.BasicAttack.MissileSpeed) + unit.BasicAttack.SpellCastTime > (Player.Distance(target) / _E.Speed) + _E.Delay)
                                    {
                                        Utility.DelayAction.Add(Func._Time(target, unit), () => _E.Cast(target, true));
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        private void SetSpells()
        {
            var dataQ = SpellDatabase.GetByName(sq);
            var dataW = SpellDatabase.GetByName(sw);
            var dataE = SpellDatabase.GetByName(se);
            Config = new Menu("Auto KillSecure", "AutoCombo", true);
            Config.AddToMainMenu();
            Config.AddSubMenu(new Menu("AutoKillSteal Settings", "AutoCombo"));

            if (dataQ != null)
            {
                Config.SubMenu("AutoCombo").AddItem(new MenuItem("SKSQ", "Use Q?").SetValue(true));
                _Q = new Spell(SpellSlot.Q, dataQ.Range);
                Game.PrintChat("Skillshot");
                _Q.SetSkillshot(dataQ.Delay / 1000f, dataQ.Radius, dataQ.MissileSpeed, true, SkillshotType.SkillshotLine);
            }

            if (dataW != null)
            {
                Config.SubMenu("AutoCombo").AddItem(new MenuItem("SKSW", "Use W?").SetValue(true));
                _W = new Spell(SpellSlot.W, dataW.Range);
                _W.SetSkillshot(dataW.Delay / 1000f, dataW.Radius, dataW.MissileSpeed, false, (SkillshotType)dataW.Type);


            }
            if (dataE != null)
            {
                Config.SubMenu("AutoCombo").AddItem(new MenuItem("SKSE", "Use E?").SetValue(true));
                _E = new Spell(SpellSlot.E, SpellDatabase.GetByName(se).Range);
                _E.SetSkillshot(dataE.Delay / 1000f, dataE.Radius, dataE.MissileSpeed, true, (SkillshotType)dataE.Type);
            }
        }

        private void ObjAiBaseOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly && !sender.IsMe && !sender.IsMinion)
            {

                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero != null && hero.IsValid && hero.IsVisible && !hero.IsDead && hero.IsEnemy))
                {

                    if (enemy.Distance(V2E(args.Start, args.End, enemy.Distance(sender.Position))) <=
                        (200))
                    {
                        Allydamage = sender.GetDamageSpell(enemy, args.SData.Name);

                        if ((Config.Item("SKSQ").GetValue<bool>()))
                        {
                            Mydamage = Player.GetDamageSpell(enemy, SpellSlot.Q);
                            if ((enemy.Distance(Player) <= _Q.Range) && (Allydamage.CalculatedDamage + Mydamage.CalculatedDamage) > enemy.Health &&
                                Allydamage.CalculatedDamage < enemy.Health)
                            {
                                Game.PrintChat("enemy health: " + enemy.Health + " Total damage : " + (Allydamage.CalculatedDamage + Mydamage.CalculatedDamage) + "health after : " + (enemy.Health - (Allydamage.CalculatedDamage + Mydamage.CalculatedDamage)) + " " + args.SData.Name);
                                var output = Prediction.GetPrediction(enemy, _Q.Delay, _Q.Width, _Q.Speed);
                                _Q.Cast(output.UnitPosition, true);

                            }
                        }

                        if ((Config.Item("SKSW").GetValue<bool>()))
                        {
                            Mydamage = Player.GetDamageSpell(enemy, SpellSlot.W);

                            if ((enemy.Distance(Player) <= _W.Range) && (Allydamage.CalculatedDamage + Mydamage.CalculatedDamage) > enemy.Health &&
                                Allydamage.CalculatedDamage < enemy.Health)
                            {
                                var output = Prediction.GetPrediction(enemy, _W.Delay, _W.Width, _W.Speed);
                                _W.Cast(output.UnitPosition, true);
                            }

                        }

                        if ((Config.Item("SKSE").GetValue<bool>()))
                        {
                            Mydamage = Player.GetDamageSpell(enemy, SpellSlot.E);

                            if ((enemy.Distance(Player) <= _E.Range) && (Allydamage.CalculatedDamage + Mydamage.CalculatedDamage) > enemy.Health &&
                                Allydamage.CalculatedDamage < enemy.Health)
                            {
                                var output = Prediction.GetPrediction(enemy, _E.Delay, _E.Width, _E.Speed);
                                _E.Cast(output.UnitPosition, true);
                            }

                        }
                    }
                }
            }

        }
    }

}
