using BepInEx;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace WolfoItemBuffs
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin("com.Wolfo.WolfoItemBuffs", "WolfoItemBuffs", "1.0.0")]
    public class WolfoItemBuffs : BaseUnityPlugin
    {


        public void Awake()
        {
            WConfig.InitConfig();

            WolfoItemBuffs2.Start();

            bool otherHarpoon = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Wolfo.AegisRemovesBarrierDecay");
            bool otherAegis = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Wolfo.RoRRHuntersHarpoon");
            bool otherSeed = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("OakPrime.LeechingSeedBuff");



            if (!otherSeed && WConfig.cfg_Green_LeechSeed.Value)
            {
                //IL.RoR2.GlobalEventManager.ProcessHitEnemy += LeechSeedAlwaysHeal;
                On.RoR2.GlobalEventManager.ProcessHitEnemy += LeechSeedBuff;
                IL.RoR2.HealthComponent.TakeDamageProcess += Seed2;

                LanguageAPI.Add("ITEM_SEED_DESC", "Dealing damage <style=cIsHealing>heals</style> you for <style=cIsHealing>1 <style=cStack>(+1 per stack)</style> health</style>. Damage over time effects <style=cIsHealing>heal</style> <style=cIsHealing>0.25 <style=cStack>(+0.25 per stack)</style> health</style> per tick.", "en");
            }
            if (!otherHarpoon && WConfig.cfg_Green_Harpoon.Value)
            {
                IL.RoR2.CharacterBody.RecalculateStats += Harpoon_ChangeMoveSpeed;
                IL.RoR2.GlobalEventManager.OnCharacterDeath += Harpoon_ChangeDuration;

                UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<BuffDef>(key: "RoR2/DLC1/MoveSpeedOnKill/bdKillMoveSpeed.asset").WaitForCompletion().canStack = false;
                LanguageAPI.Add("ITEM_MOVESPEEDONKILL_DESC", "Killing an enemy increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>125%</style> for <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> seconds. Consecutive kills increase buff duration for up to 25 seconds.", "en");
            }

            if (!otherAegis && WConfig.cfg_Red_Aegis.Value)
            {
                On.RoR2.CharacterBody.RecalculateStats += AegisNoDecay;
                LanguageAPI.Add("ITEM_BARRIERONOVERHEAL_PICKUP", "Healing past full grants you a barrier. Barrier no longer decays.", "en");
                LanguageAPI.Add("ITEM_BARRIERONOVERHEAL_DESC", "Healing past full grants you a <style=cIsHealing>barrier</style> for <style=cIsHealing>50% <style=cStack>(+50% per stack)</style></style> of the amount you <style=cIsHealing>healed</style>. All <style=cIsHealing>barrier</style> no longer naturally decays.", "en");
            }
            
            if(WConfig.cfg_Pink_Ring.Value)
            {
                IL.RoR2.GlobalEventManager.ProcessHitEnemy += BuffSingularityBand;

                LanguageAPI.Add("ITEM_ELEMENTALRINGVOID_DESC", "Hits that deal <style=cIsDamage>more than 400% damage</style> also fire a black hole that <style=cIsUtility>draws enemies within 15m into its center</style>. Lasts <style=cIsUtility>5</style> seconds before collapsing, dealing <style=cIsDamage>300%</style> <style=cStack>(+300% per stack)</style> TOTAL damage. Recharges every <style=cIsUtility>20</style> seconds. <style=cIsVoid>Corrupts all Runald's and Kjaro's Bands</style>.", "en");

            }

        }

        private void Seed2(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            //c.TryGotoNext(MoveType.Before,
            //x => x.MatchCallOrCallvirt("RoR2.CharacterBody", "get_canPerformBackstab"))
            c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2.DLC2Content/Buffs", "LowerHealthHigherDamageBuff"));
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdfld("RoR2.DamageInfo", "procCoefficient")))
            {
                c.EmitDelegate<Func<DamageInfo, DamageInfo>>((damageInfo) =>
                {
                    CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
                    int itemCount = body.inventory.GetItemCount(RoR2Content.Items.Seed);
                    if (itemCount > 0 && damageInfo.damageType.damageType.HasFlag(DamageType.DoT))
                    {
                        HealthComponent healthComponent = body.GetComponent<HealthComponent>();
                        if (healthComponent)
                        {
                            ProcChainMask procChainMask = damageInfo.procChainMask;
                            procChainMask.AddProc(ProcType.HealOnHit);
                           healthComponent.Heal((float)itemCount * 0.25f, procChainMask, true);
                        }
                    }
                    return damageInfo;
                });
            }
            else
            {
                Debug.LogWarning("IL Failed: Leeching Seed");
            }
        }

        private void BuffSingularityBand(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdstr("Prefabs/Projectiles/ElementalRingVoidBlackHole")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(1f));
                c.Next.Operand = 3f;
            }
            else
            {
                Debug.LogWarning("IL Failed: Singularity Band");
            }
        }

        private void LeechSeedBuff(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
			if (!NetworkServer.active)
			{
				return;
			}
            if (!damageInfo.rejected && damageInfo.attacker && damageInfo.procCoefficient > 0f)
            {
                if (!damageInfo.procChainMask.HasProc(ProcType.HealOnHit))
                {
                    CharacterBody component2 = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (component2 && component2.inventory)
                    {
                        int itemCount = component2.inventory.GetItemCount(RoR2Content.Items.Seed);
                        if (itemCount > 0)
                        {
                            HealthComponent component4 = component2.GetComponent<HealthComponent>();
                            if (component4)
                            {
                                ProcChainMask procChainMask = damageInfo.procChainMask;
                                procChainMask.AddProc(ProcType.HealOnHit);
                                component4.Heal((float)itemCount * damageInfo.procCoefficient, procChainMask, true);
                            }
                        }
                    }
                }
            }
        }

        private void LeechSeedAlwaysHeal(ILContext il)
        {
            throw new NotImplementedException();
        }

        private void AegisNoDecay(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.inventory)
            {
                if (self.inventory.GetItemCount(RoR2Content.Items.BarrierOnOverHeal) > 0)
                {
                    self.barrierDecayRate = 0;
                }
            }
        }

        public static void Harpoon_ChangeMoveSpeed(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdcR4(0.25f),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdsfld("RoR2.DLC1Content/Buffs", "KillMoveSpeed")))
            {
                c.Next.Operand = 1.25f;
            }
            else
            {
                Debug.LogWarning("Failed to apply Hunter's Harpoon Move Speed Increase hook");
            }
        }

        private void Harpoon_ChangeDuration(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdcR4(1f),
                    x => x.MatchLdloc(out _),
                    x => x.MatchConvR4(),
                    x => x.MatchLdcR4(0.5f)))
            {
                //Need to solve it getting cleared

                c.Index -= 2; //980 
                c.Next.OpCode = OpCodes.Ldc_I4_0; //Don't apply normal buff
                c.Index += 9; //990;   
                c.RemoveRange(3);  //Removes the 3 lines that Clear the buff, couldn't figure out how to null the buff
                //c.Next = null; //Clear null buffs 
                c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt("RoR2.CharacterBody", "get_corePosition"));

                c.EmitDelegate<Func<CharacterBody, CharacterBody>>((attackerBody) =>
                {
                    //Debug.Log(attackerBody);
                    int countHarpoon = attackerBody.master.inventory.GetItemCount(DLC1Content.Items.MoveSpeedOnKill);
                    float newDuration = countHarpoon * 1;
                    if (attackerBody.HasBuff(DLC1Content.Buffs.KillMoveSpeed))
                    {
                        for (int i = 0; i < attackerBody.timedBuffs.Count; i++)
                        {
                            if (attackerBody.timedBuffs[i].buffIndex == DLC1Content.Buffs.KillMoveSpeed.buffIndex)
                            {
                                newDuration += attackerBody.timedBuffs[i].timer;
                                break;
                            }
                        }
                    }
                    if (newDuration > 25)
                    {
                        newDuration = 25;
                    };
                    //Debug.Log(newDuration);
                    attackerBody.ClearTimedBuffs(DLC1Content.Buffs.KillMoveSpeed);
                    attackerBody.AddTimedBuff(DLC1Content.Buffs.KillMoveSpeed, newDuration);

                    return attackerBody;
                });

                //Debug.Log("Applied Hunter's Harpoon Duration hook");
            }
            else
            {
                Debug.LogWarning("Failed to apply Hunter's Harpoon Duration hook");
            }
        }

        private void NerfTwistedElites(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if(c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld("RoR2.DLC2Content/Buffs", "EliteBeadCorruption")))
            {
                
                
                c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt("RoR2.CharacterBody", "get_teamComponent"));
                c.EmitDelegate<Func<CharacterBody, CharacterBody>>((body) =>
                {
                    body.ClearTimedBuffs(DLC2Content.Buffs.EliteBeadCorruption);
                    return body;
                });

                c.TryGotoNext(MoveType.Before,
                x => x.MatchStfld("RoR2.Orbs.LightningOrb", "procCoefficient"));
                c.Prev.Operand = 0f;

                c.TryGotoNext(MoveType.Before,
                x => x.MatchStfld("RoR2.Orbs.LightningOrb", "range"));
                c.Prev.Operand = 21f;

                c.TryGotoNext(MoveType.Before,
               x => x.MatchCallOrCallvirt("RoR2.Orbs.OrbManager", "AddOrb"));

                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldloc_2);
                c.EmitDelegate<Func<LightningOrb, DamageInfo, CharacterBody, LightningOrb>>((orb, damageInfo, characterBody) =>
                {
                    orb.bouncedObjects = new List<HealthComponent>();
                    orb.teamIndex = characterBody.teamComponent.teamIndex;
                    HurtBox hurtBoxNew = orb.PickNextTarget(damageInfo.position);

                    if (hurtBoxNew)
                    {
                        orb.target = hurtBoxNew;
                    }
                    else
                    {
                        orb.target = null;
                    }        
                    return orb;
                });

            }            
            else
            {
                Debug.LogWarning("IL Failed: Nerf Twisted Elite");
            }
        }

        private void KnockbackFinDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2.DLC2Content/Buffs", "KnockUpHitEnemies")))
            {
                //If Grounded UP else Down?
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<CharacterBody, DamageInfo, CharacterBody>>((characterBody, damageInfo) =>
                {
                    float itemCount = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(DLC2Content.Items.KnockBackHitEnemies);

                    float damage2 = Util.OnHitProcDamage(damageInfo.damage, 1, 0.5f + itemCount * 0.5f);
                    DamageInfo damageInfo2 = new DamageInfo
                    {
                        damage = damage2,
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Generic,
                        attacker = damageInfo.attacker,
                        crit = damageInfo.crit,
                        force = Vector3.zero,
                        inflictor = null,
                        position = damageInfo.position,
                        procCoefficient = 1f
                    };
                    characterBody.healthComponent.TakeDamage(damageInfo2);
                    return characterBody;
                });

                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(5f));
                c.Next.Operand = 0f;

                c.TryGotoPrev(MoveType.Before,
                 x => x.MatchLdcR4(7.5f));
                c.Next.Operand = 10f;

                c.TryGotoPrev(MoveType.Before,
                x => x.MatchCallOrCallvirt("RoR2.CharacterMotor", "get_isGrounded"));
                c.Index--; 
                c.RemoveRange(3);
                c.Index += 3;
                c.RemoveRange(5);

                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdfld("RoR2.CharacterMotor", "mass"));

                c.Remove();
                c.Index++;
                c.Remove(); 
                c.EmitDelegate<Func<CharacterMotor, float, float>>((motor, mass) =>
                {
                    if(!motor.isGrounded)
                    {
                        return motor.mass * mass * -1f;
                    }
                    return motor.mass * mass;
                });

            }
            else
            {
                Debug.LogWarning("IL Failed: Knockback Fin");
            }
        }

        private void InvincibilityUnstableTransmitter(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdfld("RoR2.HealthComponent/ItemCounts", "unstableTransmitter")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(2f));
                c.Next.Operand = 0f;
            }
            else
            {
                Debug.LogWarning("IL Failed: Transmitter Inv");
            }
        }

        private void BetterUnstableTransmitter(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcR4(3.5f)))
            {
                c.Next.Operand = 35f;
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(25f));
                c.Next.Operand = 35f;
            }
            else
            {
                Debug.LogWarning("IL Failed: Transmitter");
            }
        }

        private void BetterLumShotStacking(ILContext il)
        {

            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcR4(1.25f)))
            {
                c.Next.Operand = 0f;
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.25f));
                c.Next.Operand = 2f;
            }
            else
            {
                Debug.LogWarning("IL Failed: Lum Shot");
            }

        }

        private void BetterAntlerReflect(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdfld("RoR2.HealthComponent/ItemCounts", "antlerShield")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(50));
                c.Next.Operand = 100f;

                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(10f));
                c.Next.Operand = 1f;

                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.3f));
                c.Next.Operand = 1f;
            }
            else
            {
                Debug.LogWarning("IL Failed: Antler Shield");
            }
        }
    }
}