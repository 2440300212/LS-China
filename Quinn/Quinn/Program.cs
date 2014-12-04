using System;
using System.Linq;
using System.Runtime.InteropServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Quinn
{
class Program
{
    public const string Champion = "Quinn";
    public static Orbwalking.Orbwalker Orbwalker;
    public static Spell Q, E, R;
    public static Menu Config;
    private static Obj_AI_Hero Player;
    public static HpBarIndicator hpi = new HpBarIndicator();
    private static SpellSlot _igniteSlot;



    static void Main(string[] args)
    {
        CustomEvents.Game.OnGameLoad += OnGameLoad;
        Drawing.OnEndScene += OnEndScene;
    }

    private static void OnEndScene(EventArgs args)
    {
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                hpi.unit = enemy;
                hpi.drawDmg(calcDamage(enemy), Color.DarkGreen);

            }
        
    }

    private static void OnGameLoad(EventArgs args)
    {
        Player = ObjectManager.Player;

        Q = new Spell(SpellSlot.Q, 1010);
        E = new Spell(SpellSlot.E, 800);
        R = new Spell(SpellSlot.R, 550);

        Q.SetSkillshot(0.25f, 80f, 1150, true, SkillshotType.SkillshotLine);
        E.SetTargetted(0.25f, 2000f);

        Config = new Menu("Bird Brain", "Quinn", true);
        var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
        SimpleTs.AddToMenu(targetSelectorMenu);
        Config.AddSubMenu(targetSelectorMenu);
        Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
        Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
        Config.AddSubMenu(new Menu("Combo", "Combo"));
        Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q?").SetValue(true));
        Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E?").SetValue(true));
        Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R?").SetValue(true));
        Config.SubMenu("Combo").AddItem(new MenuItem("Force", "Force orbwalk target?").SetValue(false));
        Config.SubMenu("Combo").AddItem(new MenuItem("UseER", "Use ER valor mode (burst)").SetValue(true));
        Config.SubMenu("Combo").AddItem(new MenuItem("Double", "Double Harrier").SetValue(true));
        Config.AddSubMenu(new Menu("Harras", "Harras"));
        Config.SubMenu("Harras").AddItem(new MenuItem("UseQH", "Use Q?")).SetValue(true);
        Config.SubMenu("Harras").AddItem(new MenuItem("UseEH", "Use E?")).SetValue(true);
        Config.AddToMainMenu();

        _igniteSlot = Player.GetSpellSlot("SummonerDot");
        AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        Game.OnGameUpdate += game_Update;

    }

    private static void game_Update(EventArgs args)
    {
        if (Orbwalker.ActiveMode.ToString().ToLower() == "combo")
        {
            Combo();
        }
        if (Orbwalker.ActiveMode.ToString().ToLower() == "mixed")
        {
            Harras();
        }

    }

    public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
        if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range) && !IsValorMode())
            E.CastOnUnit(gapcloser.Sender);
    }

    private static void Harras()
    {
        var vTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
        if (Q.IsReady() && Config.Item("UseQH").GetValue<bool>())
        {
            Q.Cast(vTarget);
        }
        if (E.IsReady() && Config.Item("UseEH").GetValue<bool>())
        {
            E.CastOnUnit(vTarget, true);
        }
        
    }
    private static int calcDamage(Obj_AI_Hero target)
    {
        //var vTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
        // normal damage 2 Auto Attacks
        var AA = Player.CalcDamage(target, Damage.DamageType.Physical, Player.FlatPhysicalDamageMod + Player.BaseAttackDamage);
        var damage = AA;
       
        if (_igniteSlot != SpellSlot.Unknown &&
    Player.SummonerSpellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            damage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        
        if (Items.HasItem(3153) && Items.CanUseItem(3153))
            damage += Player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

        if (E.IsReady() && Config.Item("UseE").GetValue<bool>()) // edamage
        {
            damage += Player.CalcDamage(target, Damage.DamageType.Physical,
10 + (E.Level * 30) + (Player.FlatPhysicalDamageMod * 0.2));
        }

        if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>()) // qdamage
        {
            damage += Player.CalcDamage(target, Damage.DamageType.Physical,
30 + (Q.Level * 40) + (Player.FlatPhysicalDamageMod * 0.65));
        }

        if (!IsValorMode() && target.HasBuff("QuinnW") && E.IsReady())
        {
            damage += Player.CalcDamage(target, Damage.DamageType.Physical,
15 + (Player.Level * 10) + (Player.FlatPhysicalDamageMod * 0.5)) * 2; // //if target has buff and E is ready passive*2
        }

        if (!IsValorMode() && !target.HasBuff("QuinnW") && E.IsReady()) // if e ISANT ready then calculate only 1 time passive
        {
            damage += Player.CalcDamage(target, Damage.DamageType.Physical,
15 + (Player.Level * 10) + (Player.FlatPhysicalDamageMod * 0.5)); // passive
        }

        if (!IsValorMode() && target.HasBuff("QuinnW") && !E.IsReady()) // if e ISANT ready then calculate only 1 time passive
        {
            damage += Player.CalcDamage(target, Damage.DamageType.Physical,
15 + (Player.Level * 10) + (Player.FlatPhysicalDamageMod * 0.5)); // passive
        }

        if (R.IsReady() && Config.Item("UseR").GetValue<bool>()) // rdamage
        {
            if (Player.CalcDamage(target, Damage.DamageType.Physical, (75 + (R.Level * 55) + (Player.FlatPhysicalDamageMod * 0.5)) * (2 - ((target.Health - damage - AA*3) / target.MaxHealth))) > target.Health)
            {
                damage += Player.CalcDamage(target, Damage.DamageType.Physical, (75 + (R.Level * 55) + (Player.FlatPhysicalDamageMod * 0.5)) * (2 - ((target.Health - damage) / target.MaxHealth)));
            }
        }


        return (int)damage;
    }

    
    private static void Combo()
    {
        var vTarget = SimpleTs.GetTarget(E.Range + 100, SimpleTs.DamageType.Physical);
        var ultdamage = Player.CalcDamage(vTarget, Damage.DamageType.Physical, (75 + (R.Level * 55) + (Player.FlatPhysicalDamageMod * 0.5)) * (2 - (vTarget.Health / vTarget.MaxHealth)));
        var AA = Player.CalcDamage(vTarget, Damage.DamageType.Physical,
            Player.FlatPhysicalDamageMod + Player.BaseAttackDamage);
        //if calc damage + another Auto attack (3)
        if (calcDamage(vTarget) + AA*3 > vTarget.Health && Config.Item("UseR").GetValue<bool>())
        {
            if (!IsValorMode() && R.IsReady()) // if killable enters valor
            {
                if (E.IsReady() && !vTarget.HasBuff("QuinnW") || vTarget.Distance(Player) <= 375 && !vTarget.HasBuff("QuinnW"))
                {
                    R.Cast();
                }
            }
            if (R.IsReady() && IsValorMode() && Player.Distance(vTarget) <= R.Range && ultdamage > vTarget.Health)
            {
                R.Cast(); //if return to human will kill him.. 
            }


          
            // before R.cast back to human change orbwalking point back to mouse cursor!

        }
        if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>()) //Q Logic
        {
            if (IsValorMode() && Player.Distance(vTarget) <= 375)
            {
                Q.Cast();
            }
            if (!IsValorMode() && Player.Distance(vTarget) <= Q.Range)
            {
                Q.Cast(vTarget);
            }
        }
        if (Config.Item("UseE").GetValue<bool>() && E.IsReady())
        {
            var passive = Player.CalcDamage(vTarget, Damage.DamageType.Physical,
15 + (Player.Level * 10) + (Player.FlatPhysicalDamageMod * 0.5));
            
            if (IsValorMode())
            {
                if ( vTarget.Distance(Player) < R.Range &&
                    Config.Item("UseER").GetValue<bool>())
                {
                    if ((ultdamage + passive) > vTarget.Health || (ultdamage + passive) > calcDamage(vTarget))
                    {
                        R.Cast();
                        E.CastOnUnit(vTarget);
                    }
                }
                else
                {
                    E.CastOnUnit(vTarget);
                }
            }
            else // human form
            {
                if (Config.Item("Double").GetValue<bool>())
                {
                    if (!vTarget.HasBuff("QuinnW"))
                    {
                        E.CastOnUnit(vTarget, true);
                    }
                }
                else
                {
                    E.CastOnUnit(vTarget, true);
                }
            }
        }
        //if (Config.Item("Force").GetValue<bool>() && IsValorMode())
        //{
        //    Orbwalker.SetOrbwalkingPoint(vTarget.Position);
        //}

    }

    private static bool IsValorMode()
    {
        return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "QuinnRFinale";
    }
}
}
