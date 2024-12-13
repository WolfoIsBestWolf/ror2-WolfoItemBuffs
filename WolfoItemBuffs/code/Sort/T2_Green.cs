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

namespace WolfoItemBuffs
{
    public class T2_Green
    {
        public static float HarpoonSpeed = 1.00f;
        public static void Start()
        {
            #region Harpoon
            HarpoonSpeed = WConfig.cfg_Green_Harpoon_VAL.Value / 100;
            bool otherHarpoon = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Wolfo.AegisRemovesBarrierDecay");

            Addressables.LoadAssetAsync<BuffDef>(key: "RoR2/DLC1/MoveSpeedOnKill/bdKillMoveSpeed.asset").WaitForCompletion().canStack = false;


            if (!otherHarpoon && WConfig.cfg_Green_Harpoon.Value)
            {
                IL.RoR2.CharacterBody.RecalculateStats += Harpoon_ChangeMoveSpeed;
                IL.RoR2.GlobalEventManager.OnCharacterDeath += Harpoon_ChangeDuration;

                Addressables.LoadAssetAsync<BuffDef>(key: "RoR2/DLC1/MoveSpeedOnKill/bdKillMoveSpeed.asset").WaitForCompletion().canStack = false;
                LanguageAPI.Add("ITEM_MOVESPEEDONKILL_DESC", "Killing an enemy increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>" + WConfig.cfg_Green_Harpoon_VAL.Value + "%</style> for <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> seconds. Consecutive kills increase buff duration for up to 25 seconds.", "en");
            }
            #endregion

            #region Leech Seed
            bool otherSeed = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("OakPrime.LeechingSeedBuff"); //Flat Item buff too ig
            if (!otherSeed && WConfig.cfg_Green_LeechSeed.Value)
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += Seed2;

                LanguageAPI.Add("ITEM_SEED_DESC", "Dealing damage <style=cIsHealing>heals</style> you for <style=cIsHealing>1 <style=cStack>(+1 per stack)</style> health</style>. Damage over time effects <style=cIsHealing>heal</style> <style=cIsHealing>0.2 <style=cStack>(+0.2 per stack)</style> health</style> per tick.", "en");
            }
            #endregion

            #region Berzerkers Pauldron
            if (WConfig.cfg_Green_WarCry.Value)
            {
                IL.RoR2.CharacterBody.AddMultiKill += Longer_WarCryWindow;
                LanguageAPI.Add("ITEM_WARCRYONMULTIKILL_DESC", "<style=cIsDamage>Killing 4 enemies</style> within <style=cIsDamage>2</style> second of each other sends you into a <style=cIsDamage>frenzy</style> for <style=cIsDamage>6s</style> <style=cStack>(+4s per stack)</style>. Increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>50%</style> and <style=cIsDamage>attack speed</style> by <style=cIsDamage>100%</style>.", "en");
            }
            #endregion

            On.RoR2.HealthComponent.UpdateLastHitTime += StealthKitElixir;

            BetterRedWhipCheck();

            if (WConfig.cfg_Green_Squid.Value == true)
            {
                Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/Squid/SquidTurretBody.prefab").WaitForCompletion().GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.Mechanical;
            }
        }



        public static void BetterRedWhipCheck()
        {
            //Commando
            //Huntress
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Huntress/AimArrowSnipe.asset").WaitForCompletion().isCombatSkill = false;
            //Bandit2
            //MulT
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Toolbot/ToolbotBodyToolbotDash.asset").WaitForCompletion().isCombatSkill = false;

            ////Engi
            //Engi Turrets placing doesn't count?
            //Engi Harpoon painting doesn't count?
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Engi/EngiBodyPlaceBubbleShield.asset").WaitForCompletion().isCombatSkill = false;

            //Artificer
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Mage/MageBodyFlyUp.asset").WaitForCompletion().isCombatSkill = false;

            //Merc
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Merc/MercBodyAssaulter.asset").WaitForCompletion().isCombatSkill = false;
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Merc/MercBodyFocusedAssault.asset").WaitForCompletion().isCombatSkill = false;

            //REX
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Treebot/TreebotBodyPlantSonicBoom.asset").WaitForCompletion().isCombatSkill = false;
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Treebot/TreebotBodySonicBoom.asset").WaitForCompletion().isCombatSkill = false;

            //Loader
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Loader/FireHook.asset").WaitForCompletion().isCombatSkill = false;
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Loader/FireYankHook.asset").WaitForCompletion().isCombatSkill = false;

            //Acrid
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Croco/CrocoLeap.asset").WaitForCompletion().isCombatSkill = false;
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Croco/CrocoChainableLeap.asset").WaitForCompletion().isCombatSkill = false;

            //Captain
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/Base/Captain/PrepSupplyDrop.asset").WaitForCompletion().isCombatSkill = false;

            //Railgunner
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/DLC1/Railgunner/RailgunnerBodyScopeLight.asset").WaitForCompletion().isCombatSkill = false;
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/DLC1/Railgunner/RailgunnerBodyScopeHeavy.asset").WaitForCompletion().isCombatSkill = false;

            //Void Fiend
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/DLC1/VoidSurvivor/VoidBlinkDown.asset").WaitForCompletion().isCombatSkill = false;


            //Seeker
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/DLC2/Seeker/SeekerBodySojourn.asset").WaitForCompletion().isCombatSkill = false;

            //Chef
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/DLC2/Chef/ChefRolyPoly.asset").WaitForCompletion().isCombatSkill = false;
            Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>(key: "RoR2/DLC2/Chef/ChefRolyPolyBoosted.asset").WaitForCompletion().isCombatSkill = false;


            //False Son

        }



        public static void StealthKitElixir(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker, bool delayedDamage, bool firstHitOfDelayedDamage)
        {
            if (NetworkServer.active && self.body && damageValue > 0f)
            {
                RoR2.Items.PhasingBodyBehavior cloak = self.GetComponent<RoR2.Items.PhasingBodyBehavior>();
                if (cloak)
                {
                    cloak.FixedUpdate();
                }
            }
            orig(self, damageValue, damagePosition, damageIsSilent, attacker, delayedDamage, firstHitOfDelayedDamage);
        }


        public static void Longer_WarCryWindow(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdfld("RoR2.CharacterBody", "multiKillTimer")))
            {
                c.Next.OpCode = OpCodes.Ldc_R4;
                c.Next.Operand = 2f;
            }
            else
            {
                Debug.LogWarning("IL Failed: Longer_WarCryWindow");
            }
        }

        public static void Seed2(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(MoveType.Before,
            x => x.MatchLdsfld("RoR2.DLC1Content/Items", "FragileDamageBonus"));
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
                            healthComponent.Heal((float)itemCount * 0.2f, procChainMask, true);
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

        private static void Harpoon_ChangeDuration(ILContext il)
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
            }
            else
            {
                Debug.LogWarning("Failed to apply Hunter's Harpoon Duration hook");
            }
        }

    }
}