using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WolfoItemBuffs
{
    public class T3_ICBM
    {
        public static float ICBM_DamagePerStack = 0.75f;
        public static float ICBM_Angle = 30;

        public static void Start()
        {
            IL.RoR2.MissileUtils.FireMissile_Vector3_CharacterBody_ProcChainMask_GameObject_float_bool_GameObject_DamageColorIndex_Vector3_float_bool += ICBM_NerfDamage;
            GameModeCatalog.availability.CallWhenAvailable(ProjectileCatalogSort);
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ICBM_CopyProjectiles;
            On.EntityStates.AimThrowableBase.ModifyProjectile += ICBM_Airstrikes;

            LanguageAPI.Add("ITEM_MOREMISSILE_PICKUP", "All missile and explosive projectiles fire an additional 2 copies.");
            LanguageAPI.Add("ITEM_MOREMISSILE_DESC", "All missile and explosive projectiles fire an additional <style=cIsDamage>2 copies</style> of themselves dealing <style=cIsDamage>" + ICBM_DamagePerStack * 100 + "% <style=cStack>(+" + ICBM_DamagePerStack * 100 + "% per stack)</style> TOTAL damage</style> each.");


            //I guess +2 for more missiles not every missile x2 too?
            //Like it's just a fucking lie anyways because for Missile Launcher it's 3x not +2

            #region Specific Synergies
            //Molten Perf could spawn +2 instead because 9 is too many.
            On.EntityStates.LaserTurbine.FireMainBeamState.FireBeamServer += ICBM_LaserTurbine;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += ICBM_RailgunnerMines;
            On.RoR2.GoldOnStageStartBehaviour.FireMissile += ICBM_WarBonds_2AtEnemy;
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += ICBM_NerfPlasmaShrimp;
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += ICBM_5MagmaBalls;
            #endregion



            #region Blacklist
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Firework/FireworkProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            //Plasma Shrimp isn't even a rocket.
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiHarpoon.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            //Modded Rocket??


            //Items
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Molotov/MolotovSingleProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/FireballsOnHit/FireMeatBall.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();

            //Huntress
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRain.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            //Maeg
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIcewallPillarProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIcewallWalkerProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            //Tooblot
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterBombletsProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            //Treebot
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotMortar2.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotMortarRain.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotFlowerSeed.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            //Captain Utility
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainAirstrikeProjectile1.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainAirstrikeAltProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();

          
           
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DeathProjectile/DeathProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();

            #endregion
            #region Whitelist
            //Ideally we wouldn't overlap much with Ignition Tank or SS2 Eratic Gadget (Double Lightning)

            //Behemoth, HeadStomp also say explosives but arent projectiles
            //Probably do a bunch of enemy stuff


            //Missiles
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/MicroMissileProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLunar/LunarMissileProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/MissileVoidBigProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Bombs
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantTrackingBomb.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Scorchling/ScorchlingBombProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/HermitCrab/HermitCrabBombProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Grenades
            //Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
           

            //Other 
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/Fireball.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LemurianBruiser/LemurianBigFireball.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GreaterWisp/WispCannon.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Child/ChildTrackingSparkBall.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MajorAndMinorConstruct/MinorConstructProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/LightningStake.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBoss/ClayPotProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RoboBallBoss/RoboBallProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RoboBallBoss/SuperRoboBallProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scav/ScavEnergyCannonProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSunderWave, Energized.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemTwinShotProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MinorConstructOnKill/MinorConstructOnKillProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Items
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/StickyBomb/StickyBomb.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LaserTurbine/LaserTurbineBomb.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/GoldOnStageStart/BossMissileProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMBlacklist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/LunarSun/LunarSunProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BFG/BeamSphere.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Molotov/MolotovClusterProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            //Commando
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/FMJRamping.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            //Engi 
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiMine.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/SpiderMine.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            //Toolbot
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotGrenadeLauncherProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Mage M1
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageFireboltBasic.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageLightningboltBasic.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageLightningBombProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIceBombProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Captain
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazer.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Heretic
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarSkillReplacements/LunarNeedleProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarSkillReplacements/LunarSecondaryProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Railgunner
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMine.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerMineAlt.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Void Surv M2 (Called Plasma Missile)
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterSmallProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectileCorrupted.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Seeker third punch?
            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/SpiritPunchFinisherProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Chef/BoostedSearFireballProjectile.prefab").WaitForCompletion().AddComponent<PocketICBMWhitelist>();

            //Boss Misisle needs to do X



            //Maybe do have some sort of automatical system too for modded?

            //Mines up limit

            #endregion
        }

        private static void ICBM_Airstrikes(On.EntityStates.AimThrowableBase.orig_ModifyProjectile orig, EntityStates.AimThrowableBase self, ref FireProjectileInfo fireProjectileInfo)
        {
            orig(self, ref fireProjectileInfo);
            if (self.characterBody)
            {
                if (!self.characterBody.inventory)
                {
                    return;
                }
                int itemCount = self.characterBody.inventory.GetItemCount(DLC1Content.Items.MoreMissile);
                if (itemCount > 0)
                {
                    var newfireProjectileInfo = fireProjectileInfo;
                    newfireProjectileInfo.damage *= itemCount * ICBM_DamagePerStack;

                    Ray newRay1 = ICBM_RotatedRay(self.inputBank, ICBM_Angle);
                    Ray newRay2 = ICBM_RotatedRay(self.inputBank, -ICBM_Angle);
                    RaycastHit raycastHit = default(RaycastHit);
                    bool hit = Util.CharacterRaycast(self.gameObject, newRay1, out raycastHit, self.maxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
                    if (hit)
                    {
                        newfireProjectileInfo.position = raycastHit.point;
                        ProjectileManager.instance.FireProjectile(newfireProjectileInfo);
                    }
                    hit = Util.CharacterRaycast(self.gameObject, newRay2, out raycastHit, self.maxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
                    if (hit)
                    {
                        newfireProjectileInfo.position = raycastHit.point;
                        ProjectileManager.instance.FireProjectile(newfireProjectileInfo);
                    }
                }
            }
        }

        

        private static void ICBM_WarBonds_2AtEnemy(On.RoR2.GoldOnStageStartBehaviour.orig_FireMissile orig, GoldOnStageStartBehaviour self)
        {
            orig(self);

            int itemCount = self.body.inventory.GetItemCount(DLC1Content.Items.MoreMissile);
            if (itemCount > 0)
            {
                float damage = self.body.damage * self.normalEnemyDamageRatio * (itemCount * ICBM_DamagePerStack);
                self.InitializeMonsterTargets();

                for (int i = 0; i < 2; i++)
                {
                    CharacterBody characterBody = null;
                    if (self.currentEnemy && self.currentEnemy.healthComponent.alive)
                    {
                        characterBody = self.currentEnemy;
                    }
                    if (self.targetEnemies.Count > 0)
                    {
                        self.currentEnemy = self.targetEnemies.Dequeue();
                        characterBody = self.currentEnemy;
                    }
                    if (!characterBody)
                    {
                        characterBody = self.body;
                    }
                    Quaternion rotation = Util.QuaternionSafeLookRotation(Vector3.down);
                    Vector3 a = self.CalculateHitPosition(characterBody.gameObject);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        crit = self,
                        owner = self.gameObject,
                        position = a + GoldOnStageStartBehaviour.heightOffset,
                        projectilePrefab = GoldOnStageStartBehaviour.missilePrefab,
                        rotation = rotation,
                        damage = damage,
                        maxDistance = GoldOnStageStartBehaviour.MissileOriginHeight - 4f
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }

               








            }


            
        }

        

        private static void ICBM_5MagmaBalls(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "FireballsOnHit")))
            {
                c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(3));
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<int, DamageInfo, int>>((amount, damageInfo) =>
                {
                    int moreMissile = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(DLC1Content.Items.MoreMissile);
                    if (moreMissile > 0)
                    {
                        return amount + 2;
                    }
                    return amount;
                });
                c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(3f));
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<float, DamageInfo, float>>((amount, damageInfo) =>
                {
                    int moreMissile = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(DLC1Content.Items.MoreMissile);
                    if (moreMissile > 0)
                    {
                        return amount * DamageForMultiProjectile(moreMissile, 3);
                    }
                    return amount;
                });


            }
            else
            {
                Debug.LogWarning("IL FAILED : ICBM_NerfPlasmaShrimp");
            }
        }

        public static float DamageForMultiProjectile(float itemCount, int baseProjectileCount)
        {
            float icbmMult = itemCount * ICBM_DamagePerStack;
            float totalDamage = 2 * icbmMult + baseProjectileCount;
            return totalDamage / (baseProjectileCount+2);
        }



        private static void ICBM_NerfPlasmaShrimp(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdsfld("RoR2.DLC1Content/Items", "MissileVoid")))
            {
                c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(0.5f));

                Debug.Log(c);
                c.Index += 1;
                Debug.Log(c);
                c.RemoveRange(2);
                Debug.Log(c);
                c.TryGotoNext(MoveType.After,
                x => x.MatchCall("UnityEngine.Mathf","Max"));
                c.EmitDelegate<Func<float, float>>((damage) =>
                {
                    if (damage > 1)
                    {
                        Debug.Log(damage);
                        float itemCount = (damage - 1) * 2;
                        return DamageForMultiProjectile(itemCount, 1);
                    }
                    return damage;
                });
            }
            else
            {
                Debug.LogWarning("IL FAILED : ICBM_NerfPlasmaShrimp");
            }
        }

        private static void ICBM_LaserTurbine(On.EntityStates.LaserTurbine.FireMainBeamState.orig_FireBeamServer orig, EntityStates.LaserTurbine.FireMainBeamState self, Ray aimRay, GameObject tracerEffectPrefab, float maxDistance, bool isInitialBeam)
        {
            orig(self,aimRay, tracerEffectPrefab,maxDistance,isInitialBeam);
            if (isInitialBeam && self.ownerBody)
            {
                int itemCount = self.ownerBody.inventory.GetItemCount(DLC1Content.Items.MoreMissile);
                if (itemCount > 0)
                {
                    float damageMult = itemCount * ICBM_DamagePerStack;
                    EntityStates.LaserTurbine.FireMainBeamState.mainBeamDamageCoefficient *= damageMult;
                    EntityStates.LaserTurbine.FireMainBeamState.secondBombDamageCoefficient *= damageMult;


                    aimRay.direction = Quaternion.AngleAxis(ICBM_Angle, Vector3.up) * aimRay.direction;
                    orig(self, aimRay, tracerEffectPrefab, maxDistance, isInitialBeam);
                    aimRay.direction = Quaternion.AngleAxis(-ICBM_Angle*2, Vector3.up) * aimRay.direction;
                    orig(self, aimRay, tracerEffectPrefab, maxDistance, isInitialBeam);

                    EntityStates.LaserTurbine.FireMainBeamState.mainBeamDamageCoefficient /= damageMult;
                    EntityStates.LaserTurbine.FireMainBeamState.secondBombDamageCoefficient /= damageMult;
                }
            }
        }

        public static void ProjectileCatalogSort()
        {
            ItemDef moreMissile = DLC1Content.Items.MoreMissile;
            for (int i = 0; i < moreMissile.tags.Length; i++)
            {
                Debug.Log(moreMissile.tags[i]);
                if (moreMissile.tags[i] == ItemTag.AIBlacklist)
                {
                    moreMissile.tags[i] = ItemTag.Damage;
                } 
            }

            Debug.Log("ProjectileSingleTargetImpact");
            for (int i = 0; i < ProjectileCatalog.projectilePrefabs.Length; i++)
            {
                if (ProjectileCatalog.projectilePrefabs[i].GetComponent<ProjectileSingleTargetImpact>())
                {
                    Debug.Log(ProjectileCatalog.projectilePrefabs[i]);
                }
            }

            Debug.Log("ProjectileImpactExplosion");
            for (int i = 0; i < ProjectileCatalog.projectilePrefabs.Length; i++)
            {
                var a = ProjectileCatalog.projectilePrefabs[i].GetComponent<ProjectileImpactExplosion>();
                if (a)
                {
                    //Debug.Log(ProjectileCatalog.projectilePrefabs[i] + " child:" + a.childrenProjectilePrefab);
                    Debug.Log("radius : "+ a.blastRadius+"  "+ProjectileCatalog.projectilePrefabs[i]);
                }
            }

            //Rocket Mod
            GameObject rocket = ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("RocketSurvivorRocketProjectile"));
            if (rocket)
            {
                rocket.AddComponent<PocketICBMBlacklist>();
                ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("RocketSurvivorRocketNoBlastJumpProjectile")).AddComponent<PocketICBMBlacklist>();
                ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("RocketSurvivorRocketAltProjectile")).AddComponent<PocketICBMBlacklist>();
                ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("RocketSurvivorRocketAltNoBlastJumpProjectile")).AddComponent<PocketICBMBlacklist>();
                
                //C4 is limited to only 1 and that can't easily be changed ig
                ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("RocketSurvivorC4Projectile")).AddComponent<PocketICBMWhitelist>();
            }


            Debug.Log("");
            Debug.Log("NOT Out Filtered??");
            for (int i = 0; i < ProjectileCatalog.projectilePrefabs.Length; i++)
            {
                var a = ProjectileCatalog.projectilePrefabs[i].GetComponent<ProjectileImpactExplosion>();
                if (a)
                {
                    if (a.GetComponent<PocketICBMWhitelist>())
                    {
                        continue;
                    }
                    if (!a.enabled)
                    {
                        ProjectileCatalog.projectilePrefabs[i].AddComponent<PocketICBMBlacklist>();
                        Debug.Log(ProjectileCatalog.projectilePrefabs[i]);
                    }
                    else if ((!a.destroyOnWorld && !a.destroyOnEnemy))
                    {
                        ProjectileCatalog.projectilePrefabs[i].AddComponent<PocketICBMBlacklist>();
                        Debug.Log(ProjectileCatalog.projectilePrefabs[i]);
                    }
                    else if (a.blastRadius < 2)
                    {
                        ProjectileCatalog.projectilePrefabs[i].AddComponent<PocketICBMBlacklist>();
                        Debug.Log(ProjectileCatalog.projectilePrefabs[i]);
                    }

                }
            }

            Debug.Log("");
            Debug.Log("Filtered Added");
            for (int i = 0; i < ProjectileCatalog.projectilePrefabs.Length; i++)
            {
                var a = ProjectileCatalog.projectilePrefabs[i].GetComponent<ProjectileImpactExplosion>();
                if (a && !a.GetComponent<PocketICBMBlacklist>())
                {
                    if (a.childrenProjectilePrefab)
                    {
                        GameObject.Destroy(a.childrenProjectilePrefab.GetComponent<PocketICBMWhitelist>());
                        a.childrenProjectilePrefab.AddComponent<PocketICBMBlacklist>();
                    }
                    if ((a.destroyOnWorld || a.destroyOnEnemy) && a.blastRadius > 0)
                    {
                        ProjectileCatalog.projectilePrefabs[i].AddComponent<PocketICBMWhitelist>();
                        Debug.Log(ProjectileCatalog.projectilePrefabs[i]);
                    }
                    
                }
            }
           
           


            Debug.Log("");
            Debug.Log("Final Whitelist :");
            for (int i = 0; i < ProjectileCatalog.projectilePrefabs.Length; i++)
            {
                if (ProjectileCatalog.projectilePrefabs[i].GetComponent<PocketICBMWhitelist>())
                {
                    Debug.Log(ProjectileCatalog.projectilePrefabs[i]);

                }
            }
            Debug.Log("");
            Debug.Log("Final Blacklist :");
            for (int i = 0; i < ProjectileCatalog.projectilePrefabs.Length; i++)
            {
                if (ProjectileCatalog.projectilePrefabs[i].GetComponent<PocketICBMBlacklist>())
                {
                    Debug.Log(ProjectileCatalog.projectilePrefabs[i]);
                }
            }
        }

      

        private static int ICBM_RailgunnerMines(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            if (slot == DeployableSlot.RailgunnerBomb)
            {
                if (self.inventory.GetItemCount(DLC1Content.Items.MoreMissile) > 0)
                {
                    return 3;
                }
            }
            return orig(self,slot);
        }



        public static Vector3 ICBMRotation(Vector3 originalRotation, float rotateBy)
        {

            return Quaternion.AngleAxis(rotateBy, Vector3.up) * originalRotation;
        }
        public static Ray ICBM_RotatedRay(InputBankTest input, float rotateBy)
        {
            Vector3 vector3 = input.aimDirection;
            vector3.y -= 0.04f;
            return new Ray(input.aimOrigin, Quaternion.AngleAxis(rotateBy, Vector3.up) * vector3);
        }


         

        public static bool TryICBM(GameObject prefab)
        {
            if (prefab.GetComponent<PocketICBMWhitelist>())
            {
                return true;
            }
            if (prefab.GetComponent<PocketICBMBlacklist>())
            {
                return false;
            }
            var Proj = prefab.GetComponent<ProjectileImpactExplosion>();
            if (Proj && !Proj.childrenProjectilePrefab)
            {
                return true;
            }
            return false;
        }

        public static int DoICBM(FireProjectileInfo fireProjectileInfo)
        {
            if (fireProjectileInfo.owner == null) 
            {
                return 0;
            }
            CharacterBody body = fireProjectileInfo.owner.GetComponent<CharacterBody>();
            return body.inventory.GetItemCount(DLC1Content.Items.MoreMissile); 
        }


        private static void ICBM_NerfDamage(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(1f)))
            {
                c.Next.Operand = 0f;
                c.Index++;
                c.Next.Operand = ICBM_DamagePerStack;
                c.Index++;
                c.Next.Operand = ICBM_DamagePerStack;

                c.TryGotoNext(MoveType.Before,
                x => x.MatchStfld("RoR2.Projectile.FireProjectileInfo", "owner"));
                c.Index += 2;
                Debug.Log(c);

                var temp = c.Next;

                c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt("RoR2.Projectile.ProjectileManager", "FireProjectile"));
                c.Emit(temp.OpCode, temp.Operand); 
                c.EmitDelegate<Func<FireProjectileInfo, float, FireProjectileInfo>>((projectile, ogMissileDamage) =>
                {
                    projectile.damage = ogMissileDamage;
                    return projectile;
                });

            }
            else
            {
                Debug.LogWarning("IL FAILED : ICBM_ModifyDamage");
            }
        }

        private static void ICBM_CopyProjectiles(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, ProjectileManager self, FireProjectileInfo fireProjectileInfo)
        {
            PocketICBMWhitelist icbm;
            if (fireProjectileInfo.projectilePrefab.TryGetComponent<PocketICBMWhitelist>(out icbm))
            {
                int icbmCount = DoICBM(fireProjectileInfo);
                if (icbmCount != 0)
                {
                    var fireProjectileInfoICBM = fireProjectileInfo;
                    fireProjectileInfoICBM.damage *= ICBM_DamagePerStack * icbmCount;
                    Vector3 rot = fireProjectileInfoICBM.rotation.eulerAngles;

                    rot.x += icbm.rotateX;
                    rot.y += ICBM_Angle;

                    fireProjectileInfoICBM.rotation = Quaternion.Euler(rot);
                    orig(self, fireProjectileInfoICBM);
                    rot.x -= icbm.rotateX*2;
                    rot.y -= ICBM_Angle*2;
                    fireProjectileInfoICBM.rotation = Quaternion.Euler(rot);
                    orig(self, fireProjectileInfoICBM);
                }
                orig(self, fireProjectileInfo);
            }
            else
            {
                orig(self, fireProjectileInfo);
            }
        }


         

        public class PocketICBMBlacklist : MonoBehaviour
        {
        }
        public class PocketICBMWhitelist : MonoBehaviour
        {
            public int rotateX = 0;
            public int rotateZ = 0;
            public float velocityMult = 1.0f;
        }
    }
}