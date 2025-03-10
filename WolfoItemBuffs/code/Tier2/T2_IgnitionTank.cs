using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
 
namespace WolfoItemBuffs
{
    public class T2_IgnitionTank
    {
        private static float igniteReplaceFrac = 0.6f;
        public static BuffDef ignitionTankFireTrail;
        public static GameObject GasTankProjectile;
        public static GameObject ignitionTank_UpgradeEffect;
        public static ProjectileImpactExplosion impactExplosion;

        public static void Start()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += IgnitionTank_LowBurn;
            On.RoR2.FireballVehicle.OnPassengerEnter += IgnitionTank_Egg;
            IL.RoR2.DamageTrail.DoDamage += IgnitonTank_BurnForTrail;

            On.RoR2.DelayBlast.Detonate += SpawnUpgrade_Effect;

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += Proc_TankProjectile;
            //On.RoR2.CharacterBody.UpdateItemAvailability += IgnitionTank_FireTrail;
            //IL.RoR2.GlobalEventManager.OnCharacterDeath += IgnitionTank_FireTrailOnKIll;

            LanguageAPI.Add("ITEM_STRENGTHENBURN_PICKUP", "Your ignite effects deal quadruple damage. Small chance to fire an explosive gas canister.");
            //LanguageAPI.Add("ITEM_STRENGTHENBURN_DESC", "Ignite effects deal <style=cIsDamage>+300%</style> <style=cStack>(+300% per stack)</style> more damage over time. Non-igniting fire items inflict <style=cIsDamage>burn</style> for <style=cIsDamage>+20%</style> TOTAL damage. <style=cIsDamage>2%</style> chance to fire a <style=cIsDamage>gas tank</style> inflicting <style=cIsDamage>150%</style> base damage as <style=cIsDamage>burn</style>.");
            LanguageAPI.Add("ITEM_STRENGTHENBURN_DESC", "Ignite effects deal <style=cIsDamage>+300%</style> <style=cStack>(+300% per stack)</style> more damage over time. Non-igniting fire items inflict <style=cIsDamage>burn</style> for <style=cIsDamage>+80%</style><style=cStack> (+60% per stack)</style> TOTAL damage. <style=cIsDamage>2%</style> chance to fire a <style=cIsDamage>gas tank</style> inflicting <style=cIsDamage>600%</style><style=cStack> (+450% per stack)</style> base damage as <style=cIsDamage>burn</style>.");




            BuffDef OnFire = LegacyResourcesAPI.Load<BuffDef>("BuffDefs/OnFire");
            BuffDef KillMoveSpeed = LegacyResourcesAPI.Load<BuffDef>("BuffDefs/KillMoveSpeed");
            ignitionTankFireTrail = ScriptableObject.CreateInstance<BuffDef>();
            ignitionTankFireTrail.iconSprite = KillMoveSpeed.iconSprite;
            ignitionTankFireTrail.buffColor = OnFire.buffColor;
            ignitionTankFireTrail.name = "ignitionTankFireTrail";
            ContentAddition.AddBuffDef(ignitionTankFireTrail);

            GameObject Fire = LegacyResourcesAPI.Load<GameObject>("Prefabs/FireTrail");
            Fire = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FireTornado");
            Fire.GetComponent<ProjectileDamage>().damageType |= DamageType.PercentIgniteOnHit;
            Fire.AddComponent<IgnitionTankBehavior>();
            Fire = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ExplodeOnDeath/WilloWispDelay.prefab").WaitForCompletion();
            Fire.GetComponent<DelayBlast>().damageType |= DamageType.PercentIgniteOnHit;
            Fire.AddComponent<IgnitionTankBehavior>();
            Fire = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ExplodeOnDeathVoid/ExplodeOnDeathVoidExplosion.prefab").WaitForCompletion();
            Fire.GetComponent<DelayBlast>().damageType |= DamageType.PercentIgniteOnHit;
            Fire.AddComponent<IgnitionTankBehavior>();
            Fire = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/BoostedSearFireballProjectile.prefab").WaitForCompletion();
            Fire.GetComponent<ProjectileDamage>().damageType |= DamageType.PercentIgniteOnHit;
            Fire.AddComponent<IgnitionTankBehavior>();
            Fire = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FireBallDash/FireballVehicle.prefab").WaitForCompletion();
            Fire.GetComponent<FireballVehicle>().blastDamageType |= DamageType.PercentIgniteOnHit;
            Fire.AddComponent<IgnitionTankBehavior>();
            //Sprint Wisp is an Orb
            //Fireballs from Lemurians?



            GameObject IgniteTank = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/StrengthenBurn/PickupGasTank.prefab").WaitForCompletion();
            GasTankProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotGrenadeLauncherProjectile.prefab").WaitForCompletion(), "IgnitionTankProjectile", true);
            impactExplosion = GasTankProjectile.GetComponent<ProjectileImpactExplosion>();
            //GameObject ExplosionVFX = PrefabAPI.InstantiateClone(impactExplosion.impactEffect, "IgnitionTankVFX", true);
            GameObject ExplosionVFX = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LemurianBruiser/OmniExplosionVFXLemurianBruiserFireballImpact.prefab").WaitForCompletion(), "IgnitionTankImpactVFX", true);
            GameObject GasTankGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Molotov/MolotovGhost.prefab").WaitForCompletion(), "IgnitionTankGhost", true);
            ContentAddition.AddProjectile(GasTankProjectile);
            ContentAddition.AddEffect(ExplosionVFX);

            //RoR2/Base/LemurianBruiser/OmniExplosionVFXLemurianBruiserFireballImpact.prefab
            Transform tempTrans = GasTankGhost.transform.GetChild(0);
            tempTrans.GetComponent<MeshFilter>().mesh = IgniteTank.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            tempTrans.GetComponent<MeshRenderer>().materials = IgniteTank.transform.GetChild(0).GetComponent<MeshRenderer>().materials;
            tempTrans.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            tempTrans.localEulerAngles = new Vector3(0,180,0);
            tempTrans.GetChild(0).localPosition = new Vector3(0f, 0f, 4f);
            tempTrans.GetChild(0).localScale = new Vector3(0.8f, 0.8f, 1.6f);



            ProjectileController projectileController = GasTankProjectile.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = GasTankGhost;
            projectileController.startSound = "Play_item_use_molotov_throw";

            var projectileDamage = GasTankProjectile.GetComponent<ProjectileDamage>();
            //RoboCratePodGroundImpact_000

            GasTankProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 160;
            impactExplosion.impactEffect = ExplosionVFX;
            //ExplosionVFX.GetComponent<EffectComponent>().soundName = "Play_MULT_crate_land"; //Too weird
            impactExplosion.falloffModel = BlastAttack.FalloffModel.Linear;
            impactExplosion.timerAfterImpact = false;
            impactExplosion.applyDot = true;
            impactExplosion.dotIndex = DotController.DotIndex.Burn;
            impactExplosion.blastProcCoefficient = 0;
            impactExplosion.blastDamageCoefficient = 0;
            impactExplosion.calculateTotalDamage = true;
            impactExplosion.dotDamageMultiplier = 1; //Decreasing this would mean DoT goes on for longer
            impactExplosion.totalDamageMultiplier = 1.5f; //This for some reason always uses baseDamage. 200% base damage burn upped to 800% by the thing?
            impactExplosion.blastRadius = 8f;
            //impactExplosion.explosionEffect =;


            //Crowbar sound 

            var Gravity = GasTankProjectile.AddComponent<AntiGravityForce>();
            Gravity.rb = GasTankProjectile.GetComponent<Rigidbody>();
            Gravity.antiGravityCoefficient = 1.03f;

            //Proc of 2 but infinite proc chain mask?
            //GasTankDoT
        }

        private static void SpawnUpgrade_Effect(On.RoR2.DelayBlast.orig_Detonate orig, DelayBlast self)
        {
            orig(self);
        }

        private static void Proc_TankProjectile(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
            x => x.MatchCallvirt("RoR2.CharacterMaster", "get_inventory")))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<Inventory, DamageInfo, Inventory>>((inventory, damageInfo) =>
                {
                    int itemCount5 = inventory.GetItemCount(DLC1Content.Items.StrengthenBurn);
                    if (itemCount5 > 0)
                    {
                        CharacterMaster master = inventory.GetComponent<CharacterMaster>();
                        CharacterBody body = master.GetBody();
                        if (Util.CheckRoll(2f * damageInfo.procCoefficient, master))
                        {
                            Ray ray = new Ray(body.inputBank.aimOrigin, body.inputBank.aimDirection);
                            Quaternion rotation3 = Quaternion.LookRotation(ray.direction);
                            FireProjectileInfo fireProjectileInfo2 = new FireProjectileInfo
                            {
                                projectilePrefab = GasTankProjectile,
                                crit = false,
                                damage = 0f,
                                damageColorIndex = DamageColorIndex.Item,
                                force = 8000f,
                                owner = body.gameObject,
                                position = body.aimOrigin,
                                rotation = rotation3,
                                procChainMask = damageInfo.procChainMask,
                            };
                            ProjectileManager.instance.FireProjectile(fireProjectileInfo2);

                        }
                    }
                    return inventory;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed: IgnitionTank_NerfDamage");
            }
        }

        private static void IgnitionTank_Egg(On.RoR2.FireballVehicle.orig_OnPassengerEnter orig, FireballVehicle self, GameObject passenger)
        {
            orig(self, passenger);
            if (self.overlapAttack != null)
            {
                self.overlapAttack.damageType |= DamageType.PercentIgniteOnHit;
            }
        }

        #region Unused
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


        private static void IgnitionTank_FireTrailOnKIll(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchCallOrCallvirt("RoR2.CharacterBody", "HandleOnKillEffectsServer")))
            {
                c.Index--;
                c.EmitDelegate<Func<CharacterBody, CharacterBody>>((body) =>
                {
                    if (body.inventory)
                    {
                        int ignites = body.inventory.GetItemCount(DLC1Content.Items.StrengthenBurn);
                        if (ignites > 0)
                        {
                            //Do VFX here once we make one
                            body.AddTimedBuff(ignitionTankFireTrail, 8);
                        }
                    }
                    return body;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed:IgnitionTank_FireTrailOnKIll");
            }
        }

        private static void IgnitionTank_FireTrail(On.RoR2.CharacterBody.orig_UpdateItemAvailability orig, CharacterBody self)
        {
            orig(self);
            if (!self.itemAvailability.hasFireTrail)
            {
                self.itemAvailability.hasFireTrail = self.HasBuff(ignitionTankFireTrail);
            }
        }
        #endregion

        private static void IgnitonTank_BurnForTrail(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchCallvirt("RoR2.HealthComponent", "TakeDamage")))
            {
                c.EmitDelegate<Func<DamageInfo, DamageInfo>>((damageInfo) =>
                {
                    damageInfo.damageType = DamageType.PercentIgniteOnHit;
                    return damageInfo;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed:IgnitonTank_BurnForTrail");
            }
        }

        public static void IgnitionTank_LowBurn(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2.DLC1Content/Items", "FragileDamageBonus"));
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdfld("RoR2.DamageInfo", "procCoefficient")))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<DamageInfo, HealthComponent, DamageInfo>>((damageInfo, healthComponent) =>
                {
                    if ((damageInfo.damageType & DamageType.PercentIgniteOnHit) > 0UL)
                    {
                        if (damageInfo.attacker)
                        {
                            Inventory inventory = damageInfo.attacker.GetComponent<CharacterBody>().inventory;
                            if (inventory && inventory.GetItemCount(DLC1Content.Items.StrengthenBurn) > 0)
                            {
                                InflictDotInfo inflictDotInfo = new InflictDotInfo
                                {
                                    attackerObject = damageInfo.attacker,
                                    victimObject = healthComponent.gameObject,
                                    totalDamage = new float?(damageInfo.damage * 0.2f),
                                    damageMultiplier = 1f,
                                    dotIndex = DotController.DotIndex.Burn,
                                    maxStacksFromAttacker = null
                                };
                                StrengthenBurnUtils.CheckDotForUpgrade(inventory, ref inflictDotInfo);
                                DotController.InflictDot(ref inflictDotInfo);
                            }
                        }
                    }
                    return damageInfo;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed:IgnitionTank_BurnOnProc0");
            }
        }

        

        public static bool HasIgnitionTank(GameObject owner)
        {
            if (owner)
            {
                if (owner.GetComponent<CharacterBody>().inventory.GetItemCount(DLC1Content.Items.StrengthenBurn) > 0)
                {
                    return true;
                }
            }
            return false;
        }


        public class IgnitionTankBehavior : MonoBehaviour
        {
            public static GameObject effect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LemurianBruiser/OmniExplosionVFXLemurianBruiserFireballImpact.prefab").WaitForCompletion();
            private GameObject owner;
            public float radius;
            public void Start()
            {
                bool ignitionTank = false;
                DelayBlast delayBlast = this.GetComponent<DelayBlast>();
                if (delayBlast && delayBlast.attacker)
                {
                    owner = delayBlast.attacker;
                    radius = delayBlast.radius;
                }
                else
                {
                    var Proj = this.GetComponent<ProjectileController>();
                    if (Proj && Proj.owner)
                    {
                        owner = Proj.owner;
                        radius = 10;
                    }
                } 
                if (owner)
                {
                    if (owner.GetComponent<CharacterBody>().inventory.GetItemCount(DLC1Content.Items.StrengthenBurn) > 0)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = base.transform.position,
                            scale = radius
                        };
                        EffectManager.SpawnEffect(effect, effectData, false);
                    }
                }




               
            }
        }

    }
}