using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace WolfoItemBuffs
{
    public class T2_Green
    {
      
        public static void Start()
        {
  
            #region Leech Seed
            bool otherSeed = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("OakPrime.LeechingSeedBuff"); //Flat Item buff too ig
            if (!otherSeed && WConfig.cfg_Green_LeechSeed.Value)
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += Seed2;

                LanguageAPI.Add("ITEM_SEED_DESC", "Dealing damage <style=cIsHealing>heals</style> you for <style=cIsHealing>1 <style=cStack>(+1 per stack)</style> health</style>. Damage over time effects <style=cIsHealing>heal</style> <style=cIsHealing>0.25 <style=cStack>(+0.25 per stack)</style> health</style> per tick.", "en");
            }
            #endregion

            #region Berzerkers Pauldron
            if (WConfig.cfg_Green_WarCry.Value)
            {
                IL.RoR2.CharacterBody.AddMultiKill += Longer_WarCryWindow;
                LanguageAPI.Add("ITEM_WARCRYONMULTIKILL_DESC", "<style=cIsDamage>Killing 4 enemies</style> within <style=cIsDamage>2</style> second of each other sends you into a <style=cIsDamage>frenzy</style> for <style=cIsDamage>6s</style> <style=cStack>(+4s per stack)</style>. Increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>50%</style> and <style=cIsDamage>attack speed</style> by <style=cIsDamage>100%</style>.", "en");
            }
            #endregion


            BetterRedWhipCheck();

            if (WConfig.cfg_Green_Squid.Value == true)
            {
                Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/Squid/SquidTurretBody.prefab").WaitForCompletion().GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.Mechanical;
            }

            #region Stealthkit
            if (WConfig.cfg_Green_Stealthkit.Value)
            {
                ItemDef Stealthkit = Addressables.LoadAssetAsync<ItemDef>(key: "RoR2/Base/Phasing/Phasing.asset").WaitForCompletion();
                Stealthkit.tags = Stealthkit.tags.Remove(ItemTag.LowHealth);

                On.RoR2.HealthComponent.UpdateLastHitTime += StealthKitElixir;
                IL.RoR2.Items.PhasingBodyBehavior.FixedUpdate += PhasingBodyBehavior_FixedUpdate;
                LanguageAPI.Add("ITEM_PHASING_PICKUP", "Turn invisible and gain speed below half health. Recharges over time.", "en");
                LanguageAPI.Add("ITEM_PHASING_DESC", "Falling below <style=cIsHealth>50% health</style> causes you to gain <style=cIsUtility>40% movement speed</style> and <style=cIsUtility>invisibility</style> for <style=cIsUtility>5s</style>. Recharges every <style=cIsUtility>30 seconds</style> <style=cStack>(-50% per stack)</style>.", "en");

            }
            #endregion
        }

        private static void PhasingBodyBehavior_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchCallvirt("RoR2.HealthComponent", "get_isHealthLow")))
            {
                c.Remove();
                c.EmitDelegate<Func<HealthComponent, bool>>((health) =>
                {
                    return health.IsHealthBelowThreshold(0.5f);
                });
            }
            else
            {
                Debug.LogWarning("IL Failed: Longer_WarCryWindow");
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
            x => x.MatchStfld("RoR2.CharacterBody", "multiKillTimer")))
            {
                c.Prev.OpCode = OpCodes.Ldc_R4;
                c.Prev.Operand = 2f;
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


    }
}