using R2API;
using RoR2;
using UnityEngine.AddressableAssets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace WolfoItemBuffs
{
    public class T1_APRounds
    {
        public static void Start()
        {

            IL.RoR2.HealthComponent.TakeDamage += APR_DamageAgainstChampion;
        }

        private static void APR_DamageAgainstChampion(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdfld("RoR2.RoR2Content/Items", "BossDamageBonus")))
            {
                c.TryGotoPrev(MoveType.Before,
                x => x.MatchStfld("RoR2.CharacterBody", "get_isBoss"));
 
            }
            else
            {
                Debug.LogWarning("IL FAILED : APR_DamageAgainstChampion");
            }
        }
    }
}