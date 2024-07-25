using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

[DefOf]
public static class SF_DefOf
{
    public static AbilityDef MaxBodySize;

    static SF_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(SF_DefOf));
    }
}

