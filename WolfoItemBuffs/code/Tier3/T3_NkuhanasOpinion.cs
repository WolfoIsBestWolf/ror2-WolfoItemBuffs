using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
 
namespace WolfoItemBuffs
{
    public class T3_NkuhanasOpinion
    {
         
        public static void Start()
        {
            IL.RoR2.HealthComponent.ServerFixedUpdate += Nk_1Proc3Damage;
            IL.RoR2.HealthComponent.Heal += Nk_Remove_LimitedStorage;


            LanguageAPI.Add("ITEM_NOVAONHEAL_DESC", "Store <style=cIsHealing>100%</style> <style=cStack>(+100% per stack)</style> of healing as <style=cIsHealing>Soul Energy</style>. After your <style=cIsHealing>Soul Energy</style> reaches <style=cIsHealing>10%</style> of your <style=cIsHealing>maximum health</style>, <style=cIsDamage>fire a skull</style> that deals <style=cIsDamage>30%</style> of your <style=cIsHealing>maximum health</style> as <style=cIsDamage>damage</style>.");

        }

        private static void Nk_Remove_LimitedStorage(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchStfld("RoR2.HealthComponent", "devilOrbHealPool")))
            {
                Debug.Log(c);
                c.Index -= 3;
                Debug.Log(c);
                c.RemoveRange(3);
                //Remove 
            }
            else
            {
                Debug.LogWarning("IL FAILED : Nk_Remove_LimitedStorage");
            }
        }

        private static void Nk_1Proc3Damage(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdfld("RoR2.HealthComponent", "devilOrbTimer")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.1f));
                c.Next.Operand = 0.05f;
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(2.5f));
                c.Next.Operand = 3f; 
                c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt("RoR2.Orbs.OrbManager", "AddOrb"));
                c.EmitDelegate<Func<DevilOrb, DevilOrb>>((devilOrb) =>
                {
                    devilOrb.procCoefficient = 1;
                    return devilOrb;
                });
            }
            else
            {
                Debug.LogWarning("IL FAILED : Nk_1Proc3Damage");
            }
        }
    }
}