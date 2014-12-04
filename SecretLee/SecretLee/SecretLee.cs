using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace SecretLee
{
    internal class SecretLee

{
    public static Obj_AI_Hero Player;
    public static Menu Config;
    public static Spell _Q;
    public static Spell _W;
    public DamageSpell EnemyDamage;
    public DamageSpell Mydamage;

    public SecretLee()
    {
        CustomEvents.Game.OnGameLoad += OnGameLoad;
        Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnProcessSpellCast;
    }

    private void ObjAiBaseOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
    {
        if (sender.IsEnemy && sender is Obj_AI_Hero) // && Check is damage is going to hit baron)
        {
            sender.CalcDamage(sender.GetSpellDamage(/* baron */, args.SData.Name));
        }
    }

    private void OnGameLoad(EventArgs args)
    {
        Game.PrintChat("Secret Lee Super cool by xcxooxl lOADED!!!!");
    }
}
}
