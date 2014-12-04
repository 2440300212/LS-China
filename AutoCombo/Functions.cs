using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Autocombo
{
    internal class Func
    {
        public static int _Time(Obj_AI_Base target, Obj_AI_Base unit)
        {
            var result = unit.Distance(target) / unit.BasicAttack.MissileSpeed + unit.BasicAttack.SpellCastTime * 1000;
            return (int)result;
        }

    }


}
