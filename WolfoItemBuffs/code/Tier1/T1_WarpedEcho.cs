using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace WolfoItemBuffs
{
    public class T1_WarpedEcho
    {
        public static void Start()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += WarpedEcho_WorkWithAll;
            
        }
        public static void WarpedEcho_WorkWithAll(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2.DLC2Content/Buffs", "DelayedDamageBuff")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(0x842));
                Debug.Log(c);
                c.Next.Operand = 0x800;
                Debug.Log(c);
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcI4(0x843));
                Debug.Log(c);
                c.Next.Operand = 0x801;
                Debug.Log(c);

            }
            else
            {
                Debug.LogWarning("IL FAILED : Harpoon_ChangeMoveSpeed");
            }
        }
    }
}