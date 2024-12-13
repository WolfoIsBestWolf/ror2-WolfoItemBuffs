using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using HG;
using RoR2.Projectile;

namespace WolfoItemBuffs
{
    public class SotV_Reworks
    {
         
        public static void Start()
        {
            RecalculateStatsAPI.GetStatCoefficients += LaserScope_Rework;

            On.RoR2.CostTypeDef.PayCost += CostTypeDef_PayCost;

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += IgnitionTank_Willo;
            IL.RoR2.HealthComponent.TakeDamageProcess += IgnitionTank_VoidWillo;
            //IL.RoR2.StrengthenBurnUtils.CheckDotForUpgrade += IgnitionTank_NerfDamage;

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += ICBM_StickyBomb;
        }

        private static CostTypeDef.PayCostResults CostTypeDef_PayCost(On.RoR2.CostTypeDef.orig_PayCost orig, CostTypeDef self, int cost, Interactor activator, GameObject purchasedObject, Xoroshiro128Plus rng, ItemIndex avoidedItemIndex)
        {
            throw new NotImplementedException();
        }

        private static void IgnitionTank_NerfDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcI4(3)))
            {
                c.Next.OpCode = OpCodes.Ldc_R4;
                c.Next.Operand = 2.5f;
            }
            else
            {
                Debug.LogWarning("IL Failed: IgnitionTank_NerfDamage");
            }
        }

        private static void IgnitionTank_VoidWillo(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2.HealthComponent/AssetReferences", "explodeOnDeathVoidExplosionPrefab")))
            {
                c.EmitDelegate<Func<DelayBlast, DelayBlast>>((damageInfo) =>
                {

                    return damageInfo;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed: IgnitionTank_Willo");
            }
        }

       

        private static void IgnitionTank_Willo(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdstr("Prefabs/Projectiles/FireTornado")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdstr("Prefabs/Projectiles/FireTornado"));
                c.EmitDelegate<Func<FireProjectileInfo, FireProjectileInfo>>((damageInfo) =>
                {

                    return damageInfo;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed: IgnitionTank_Kjaro");
            }


            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2.GlobalEventManager/CommonAssets", "explodeOnDeathPrefab")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdstr("Prefabs/Projectiles/FireTornado"));
                c.EmitDelegate<Func<DelayBlast, DelayBlast>>((damageInfo) =>
                {
                    
                    return damageInfo;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed: IgnitionTank_Willo");
            }
        }

        private static void ICBM_StickyBomb(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2Content.Items","StickyBomb")))
            {
                c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(1.8f));
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<float, DamageInfo, GameObject,float>>((damageCoeff, damageInfo, victim) =>
                {
                    int moreMissile = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(DLC1Content.Items.MoreMissile);
                    if (moreMissile > 0)
                    {
                        damageCoeff *= (0.5f + 0.5f * moreMissile);
                        CharacterBody characterBody = victim ? victim.GetComponent<CharacterBody>() : null;
                        bool alive = characterBody.healthComponent.alive;
                        float num11 = 5f;
                        Vector3 position = damageInfo.position;
                        Vector3 forward = characterBody.corePosition - position;
                        float magnitude = forward.magnitude;
                        float damage = Util.OnHitProcDamage(damageInfo.damage, damageInfo.attacker.GetComponent<CharacterBody>().damage, damageCoeff);
                        Quaternion rotation = (magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
                        ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num11) : -1f);
                        rotation = (magnitude != 0f) ? Util.QuaternionSafeLookRotation(forward) : UnityEngine.Random.rotationUniform;
                        ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/StickyBomb"), position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item, null, alive ? (magnitude * num11) : -1f);

                    }
                    return damageCoeff;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed: ICBM_StickyBomb");
            }
        }

        private static void LaserScope_Rework(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            bool flag = sender.inventory != null;
            if (flag)
            {
                int laserScope = sender.inventory.GetItemCount(DLC1Content.Items.CritDamage);
                if (laserScope > 0)
                {
                    args.critAdd += 40 * sender.inventory.GetItemCount(DLC1Content.Items.CritDamage);
                    if (sender.crit > 100)
                    {
                        args.critDamageMultAdd += (sender.crit - 100);
                    }
                }
            }
        }
    }
}