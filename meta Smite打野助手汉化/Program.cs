using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Reflection;
using SharpDX;
using Color = System.Drawing.Color;

namespace meta_Smite
{
    class Program
    {
        public static Menu Config;
        public static Dictionary<string, SpellSlot> spellList = new Dictionary<string, SpellSlot>();
        public static Dictionary<string, float> rangeList = new Dictionary<string, float>();
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static Spell smite;
        public static Spell champSpell;
        public static bool hasSpell = false;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        private static void Game_OnGameStart(EventArgs args)
        {
            Game.PrintChat("姝ｅ湪鍚姩鎯╂垝鍔╂墜");
            setSmiteSlot();
            Config = new Menu("鎯╂垝鍔╂墜", "metaSmite", true);
            Config.AddItem(new MenuItem("Enabled", "鑷姩鎯╂垝").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle, true)));
            Config.AddItem(new MenuItem("EnabledH", "鎸夐敭鎯╂垝").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Press)));
            //Config.AddItem(new MenuItem("SmiteCast", "浣跨敤灏佸寘鎯╂垝")).SetValue(true);
            Config.AddToMainMenu();
            champSpell = addSupportedChampSkill();
            if (smiteSlot == SpellSlot.Unknown && champSpell.Slot == SpellSlot.Unknown)
            {
                Game.PrintChat("Smite and spell not found, disabling Meta Smite");
                return;
            }
            setupCampMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.PrintChat("鎯╂垝鍔╂墜杞藉叆鎴愬姛..");
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Enabled").GetValue<KeyBind>().Active || Config.Item("EnabledH").GetValue<KeyBind>().Active)
            {
                bool smiteReady = false;
                bool spellReady = false;
                Obj_AI_Base mob = GetNearest(ObjectManager.Player.ServerPosition);
                if (mob != null && Config.Item(mob.SkinName).GetValue<bool>())
                {
                    double smitedamage = smiteDamage();
                    double spelldamage = spellDamage(mob);

                    if (ObjectManager.Player.SummonerSpellbook.CanUseSpell(smiteSlot) == SpellState.Ready && Vector3.Distance(ObjectManager.Player.ServerPosition, mob.ServerPosition) < smite.Range)
                    {
                        smiteReady = true;
                    }

                    if (smiteReady && mob.Health < smitedamage) //Smite is ready and enemy is killable with smite
                    {
                        if (false) //Config.Item("SmiteCast").GetValue<bool>())
                        {
                            Game.PrintChat("Casting with packets, slot is: " + smite.Slot);
                            var pslot = 0;
                            if(smite.Slot == SpellSlot.Q)
                            {
                                pslot = 64; //Credits to Lizzaren and Trees
                            }
                            if(smite.Slot == SpellSlot.W)
                            {
                                pslot = 65; //Credits to Lizzaren and Trees
                            }

                            if(pslot != 0)
                            {
                                Game.PrintChat("Casting smite");
                                Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(mob.NetworkId, (SpellSlot)pslot)).Send(PacketChannel.C2S);
                            }
                        }
                        else
                        {
                            ObjectManager.Player.SummonerSpellbook.CastSpell(smiteSlot, mob);
                        }
                    }

                    if (!hasSpell) 
                    {
                        return; 
                    }

                    if (Config.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>())
                    {
                        spellReady = true;
                    }

                    if (champSpell.IsReady() && spellReady && Vector3.Distance(ObjectManager.Player.ServerPosition, mob.ServerPosition) < champSpell.Range + mob.BoundingRadius) //skill is ready 
                    {
                        if (smiteReady)
                        {
                            //Game.PrintChat("Mob health is: " + mob.Health + ", damage is: " + (smitedamage + spelldamage));
                            if (mob.Health < smitedamage + spelldamage) //Smite is ready and combined damage will kill
                            {
                                if(ObjectManager.Player.ChampionName == "Lux")
                                {
                                    champSpell.Cast(mob.ServerPosition);
                                }
                                if (ObjectManager.Player.ChampionName == "Twitch" ||
                                    ObjectManager.Player.ChampionName == "MonkeyKing" ||
                                    ObjectManager.Player.ChampionName == "Rammus" ||
                                    ObjectManager.Player.ChampionName == "Rengar" ||
                                    ObjectManager.Player.ChampionName == "Nasus" ||
                                    ObjectManager.Player.ChampionName == "LeeSin" ||
                                    ObjectManager.Player.ChampionName == "Udyr")
                                {
                                    if (ObjectManager.Player.ChampionName == "LeeSin")
                                    {
                                        if (!mob.HasBuff("BlindMonkSonicWave"))
                                        {
                                            return;
                                        }
                                    }
                                    champSpell.Cast();
                                } 
                                else
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(champSpell.Slot, mob);
                                }
                            }
                        }
                        else if (mob.Health < spelldamage) //Killable with spell
                        {
                            if (ObjectManager.Player.ChampionName == "Lux" || ObjectManager.Player.ChampionName == "Xerath")
                            {
                                champSpell.Cast(mob.ServerPosition);
                            }
                            if (ObjectManager.Player.ChampionName == "Twitch" ||
                                ObjectManager.Player.ChampionName == "MonkeyKing" ||
                                ObjectManager.Player.ChampionName == "Rammus" ||
                                ObjectManager.Player.ChampionName == "Rengar" ||
                                ObjectManager.Player.ChampionName == "Nasus" ||
                                ObjectManager.Player.ChampionName == "LeeSin" ||
                                ObjectManager.Player.ChampionName == "Udyr")
                            {
                                if (ObjectManager.Player.ChampionName == "LeeSin")
                                {
                                    if (!mob.HasBuff("BlindMonkSonicWave"))
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        champSpell.Cast();
                                    }
                                }
                                else
                                {
                                    champSpell.CastOnUnit(ObjectManager.Player);
                                }
                            }
                            else
                            {
                                ObjectManager.Player.Spellbook.CastSpell(champSpell.Slot, mob);
                            }
                        }
                    }
                }
            }
        }
        
        
        
        public static void Drawing_OnDraw(EventArgs args)
        {
            Obj_AI_Base mob1 = GetNearest(ObjectManager.Player.ServerPosition);
            if (Vector3.Distance(ObjectManager.Player.ServerPosition, mob1.ServerPosition) < 1500 && mob1.IsVisible)
            {
                bool smiteR = false;
                bool spellR = false;
                if (ObjectManager.Player.SummonerSpellbook.CanUseSpell(smiteSlot) == SpellState.Ready)
                {
                    smiteR = true;
                }
                if (Config.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>())
                {
                    spellR = true;
                }
                double smited = smiteDamage();
                double spelld = 0;
                if (champSpell.IsReady() && spellR)
                {
                    spelld = spellDamage(mob1);
                }

                Vector2 hpBarPos = mob1.HPBarPosition;
                hpBarPos.X += 45;
                hpBarPos.Y += 18;
                var smitePercent = smited/mob1.MaxHealth;
                var spellPercent = spelld/mob1.MaxHealth;
                double smiteXPos = hpBarPos.X + (63*smitePercent);
                double smiteXPosBig = hpBarPos.X - 30 + (126 * smitePercent);
                double spellXPos = hpBarPos.X + (63 * spellPercent);
                double spellXPosBig = hpBarPos.X - 30 + (126 * spellPercent);
                double spellsmiteXPos = hpBarPos.X + ((63 * spellPercent) + (63 * smitePercent));
                double spellsmiteXPosBig = hpBarPos.X - 30 + ((126 * spellPercent) + (126 * smitePercent));

                if (mob1.BaseSkinName == "LizardElder" ||
                    mob1.BaseSkinName == "AncientGolem" ||
                    mob1.BaseSkinName == "GiantWolf" ||
                    mob1.BaseSkinName == "GreatWraith" ||
                    mob1.BaseSkinName == "Wraith" ||
                    mob1.BaseSkinName == "Golem")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float)smiteXPos, hpBarPos.Y, (float)smiteXPos, (float)hpBarPos.Y + 5, 2,
                            smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float)spellsmiteXPos, hpBarPos.Y, (float)spellsmiteXPos, (float)hpBarPos.Y + 5, 2,
                            (smited + spelld) > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float)smiteXPos, hpBarPos.Y, (float)smiteXPos, hpBarPos.Y + 5, 2,
                            smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                        Drawing.DrawLine((float)spellXPos, hpBarPos.Y, (float)spellXPos, hpBarPos.Y + 5, 2,
                            spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
                else if (mob1.BaseSkinName == "Worm")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float)smiteXPosBig, hpBarPos.Y, (float)smiteXPosBig, hpBarPos.Y + 5, 2,
                            smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float)spellsmiteXPosBig, hpBarPos.Y, (float)spellsmiteXPosBig, hpBarPos.Y + 5, 2,
                            (smited + spelld) > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float)smiteXPosBig, hpBarPos.Y, (float)smiteXPosBig, hpBarPos.Y + 5, 2,
                            smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                        Drawing.DrawLine((float)spellXPosBig, hpBarPos.Y, (float)spellXPosBig, hpBarPos.Y + 5, 2,
                            spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
                else if (mob1.BaseSkinName == "Dragon")
                {
                    if (smiteR && spellR)
                    {
                        Drawing.DrawLine((float)smiteXPosBig, hpBarPos.Y - 5, (float)smiteXPosBig, hpBarPos.Y, 2,
                            smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                        Drawing.DrawLine((float)spellsmiteXPosBig, hpBarPos.Y - 5, (float)spellsmiteXPosBig, hpBarPos.Y, 2,
                            (smited + spelld) > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (smiteR)
                    {
                        Drawing.DrawLine((float)smiteXPosBig, hpBarPos.Y - 5, (float)smiteXPosBig, hpBarPos.Y, 2,
                            smited > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                    else if (spellR)
                    {
                        Drawing.DrawLine((float)spellXPosBig, hpBarPos.Y - 5, (float)spellXPosBig, hpBarPos.Y, 2,
                            spelld > mob1.Health ? Color.SpringGreen : Color.SeaShell);
                    }
                }
            }
        }

        public static void setSmiteSlot()
        {
            foreach (var spell in ObjectManager.Player.SummonerSpellbook.Spells.Where(spell => String.Equals(spell.Name, "SummonerSmite", StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                smite = new Spell(smiteSlot, 700);
                return;
            }
        }

        public static double smiteDamage()
        {
            int level = ObjectManager.Player.Level;
            int[] damage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return damage.Max();
        }

        public static double spellDamage(Obj_AI_Base mob)
        {
            double result = 0;
            Obj_AI_Hero hero = ObjectManager.Player;
            if(hero.ChampionName == "Nunu")
            {
                return (250 + (150 * hero.Spellbook.GetSpell(champSpell.Slot).Level));
            }
            if (hero.ChampionName == "Chogath")
            {
                return (1000 + (hero.FlatMagicDamageMod * 0.7));
            }
            if (hero.ChampionName == "Elise")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Lux")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot) - 100);
            }
            if (hero.ChampionName == "Volibear")
            {
                return (((35 + (45 * hero.Spellbook.GetSpell(SpellSlot.W).Level)) + ((hero.MaxHealth - (86 * hero.Level + 440)) * 0.15)) * ((mob.MaxHealth - mob.Health) / mob.MaxHealth + 1));
            }
            if (hero.ChampionName == "Warwick")
            {
                return (25 + (50 * hero.Spellbook.GetSpell(champSpell.Slot).Level));
            }
            if (hero.ChampionName == "Olaf")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Twitch")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Shaco")
            {
                return (10 + (40 * hero.Spellbook.GetSpell(champSpell.Slot).Level));
            }
            if (hero.ChampionName == "Vi")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Pantheon")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "MasterYi")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "MonkeyKing")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "KhaZix")
            {
                return (getKhazixDmg(mob));
            }
            if (hero.ChampionName == "Rammus")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Rengar")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Nasus")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Xerath")
            {
                champSpell.Range = 2000 + champSpell.Level * 1200;//Update R range
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "LeeSin")
            {
                return (getQ2Dmg(mob));
            }
            if (hero.ChampionName == "Veigar")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            if (hero.ChampionName == "Udyr")
            {
                return (getUdyrR(mob));
            }
            if (hero.ChampionName == "Fizz")
            {
                return (hero.GetSpellDamage(mob, champSpell.Slot));
            }
            return result;
        }

        public static Spell addSupportedChampSkill()
        {
            spellList.Add("Chogath", SpellSlot.R);
            spellList.Add("Nunu", SpellSlot.Q);
            spellList.Add("Elise", SpellSlot.Q);
            spellList.Add("Kayle", SpellSlot.Q);
            spellList.Add("Lux", SpellSlot.R);
            spellList.Add("Volibear", SpellSlot.W);
            spellList.Add("Warwick", SpellSlot.Q);
            spellList.Add("Olaf", SpellSlot.E);
            spellList.Add("Twitch", SpellSlot.E);
            spellList.Add("Shaco", SpellSlot.E);
            spellList.Add("Vi", SpellSlot.E);
            spellList.Add("Pantheon", SpellSlot.Q);
            spellList.Add("MasterYi", SpellSlot.Q);
            spellList.Add("MonkeyKing", SpellSlot.Q);
            spellList.Add("Khazix", SpellSlot.Q);
            spellList.Add("Rammus", SpellSlot.Q);
            spellList.Add("Rengar", SpellSlot.Q);
            spellList.Add("Nasus", SpellSlot.Q);
            spellList.Add("Xerath", SpellSlot.R);
            spellList.Add("LeeSin", SpellSlot.Q);
            spellList.Add("Veigar", SpellSlot.Q);
            spellList.Add("Udyr", SpellSlot.R);
            spellList.Add("Fizz", SpellSlot.Q);

            if(spellList.ContainsKey(ObjectManager.Player.ChampionName))
            {
                string champ = ObjectManager.Player.ChampionName;
                SpellSlot slot;
                spellList.TryGetValue(champ, out slot);
                Spell comboSpell = new Spell(slot, 0);
                comboSpell.Range = getRange(champ);
                Config.AddItem(new MenuItem("Enabled-" + champ, "Enabled-" + champ + "-" + slot)).SetValue(true);
                hasSpell = true;
                return comboSpell;
            }
            else
            {
                return new Spell(SpellSlot.Unknown, 0);
            }
        }

        public static float getRange(string champName)
        {
            rangeList.Add("Chogath", 175f);
            rangeList.Add("Nunu", 145f);
            rangeList.Add("Elise", 475f);
            rangeList.Add("Kayle", 650f);
            rangeList.Add("Lux", 3340f);
            rangeList.Add("Volibear", 400f);
            rangeList.Add("Warwick", 400f);
            rangeList.Add("Olaf", 325f);
            rangeList.Add("Twitch", 1200);
            rangeList.Add("Shaco", 625f);
            rangeList.Add("Vi", 600f);
            rangeList.Add("Pantheon", 600f);
            rangeList.Add("MasterYi", 600f);
            rangeList.Add("MonkeyKing", 100f);
            rangeList.Add("Khazix", 325f);
            rangeList.Add("Rammus", 100f);
            rangeList.Add("Rengar", ObjectManager.Player.AttackRange);
            rangeList.Add("Nasus", ObjectManager.Player.AttackRange);
            rangeList.Add("Xerath", 3200f);
            rangeList.Add("LeeSin", 1300f);
            rangeList.Add("Veigar", 650f);
            rangeList.Add("Udyr", ObjectManager.Player.AttackRange);
            rangeList.Add("Fizz", 550f);
            float res;
            rangeList.TryGetValue(champName, out res);
            return res;
        }

        public static void setupCampMenu()
        {
            Config.AddSubMenu(new Menu("Camps", "Camps"));
            if(Game.MapId == GameMapId.SummonersRift)
            {
                Config.SubMenu("Camps").AddItem(new MenuItem("Worm", "Baron Enabled").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("Dragon", "Dragon Enabled").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("AncientGolem", "Blue Enabled").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("LizardElder", "Red Enabled").SetValue(true));
            }
            if(Game.MapId == GameMapId.TwistedTreeline)
            {
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_Spiderboss", "Vilemaw Enabled").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_NGolem", "Golem Enabled").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_NWolf", "Wolf Enabled").SetValue(true));
                Config.SubMenu("Camps").AddItem(new MenuItem("TT_NWraith", "Wraith Enabled").SetValue(true));
            }
        }

        public static double getQ2Dmg(Obj_AI_Base target)
        {
            Int32[] dmgQ = { 50, 80, 110, 140, 170 };
            double damage = ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, dmgQ[champSpell.Level - 1] + 0.9 * ObjectManager.Player.FlatPhysicalDamageMod + 0.08 * (target.MaxHealth - target.Health));
            if(damage > 400)
            {
                return 400;
            }
            return damage;
        }

        public static double getUdyrR(Obj_AI_Base target)
        {
            Int32[] dmgQ = { 40, 80, 120, 160, 200 };
            double damage = ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, dmgQ[champSpell.Level - 1] + 0.45 * ObjectManager.Player.FlatMagicDamageMod);
            return damage;
        }

        public static double getKhazixDmg(Obj_AI_Base target)
        {
            List<Obj_AI_Base> allMobs = MinionManager.GetMinions(target.ServerPosition, 500f, MinionTypes.All, MinionTeam.Neutral);
            Int32[] dmgQ = {70, 95, 120, 145, 170};
            double damage = ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, (dmgQ[champSpell.Level - 1] + (1.2 * ObjectManager.Player.FlatPhysicalDamageMod)));
            if (allMobs.Count == 1)
            {
                if (ObjectManager.Player.HasBuff("khazixqevo", true))
                {
                    return (damage * 1.3) + (ObjectManager.Player.Level * 10) + (1.04 * ObjectManager.Player.FlatPhysicalDamageMod);
                }
                return damage + (damage*0.3);
            }
            return damage;
        }

        //Credits to Lizzaran
        private static readonly string[] MinionNames =
        {
            "Worm", "Dragon", "LizardElder", "AncientGolem", "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith"
        };

        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(minion => minion.IsValid && MinionNames.Any(name => minion.Name.StartsWith(name)));
            var objAiMinions = minions as Obj_AI_Minion[] ?? minions.ToArray();
            Obj_AI_Minion sMinion = objAiMinions.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion minion in objAiMinions)
            {
                double distance = Vector3.Distance(pos, minion.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    sMinion = minion;
                }
            }
            return sMinion;
        }
    }
}