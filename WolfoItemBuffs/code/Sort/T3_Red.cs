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
    public class T3_Red
    {
        public static float AegisDrainMult = 0.2f;

        public static void Start()
        {
            #region Aegis
            AegisDrainMult = WConfig.cfg_Red_Aegis_VAL.Value;
            bool otherAegis = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Wolfo.RoRRHuntersHarpoon");
            if (!otherAegis && WConfig.cfg_Red_Aegis.Value)
            {
                On.RoR2.CharacterBody.RecalculateStats += AegisNoDecay;
                LanguageAPI.Add("ITEM_BARRIERONOVERHEAL_PICKUP", "Healing past full grants you a barrier. Barrier no longer decays.", "en");

                if (WConfig.cfg_Red_Aegis_VAL.Value == 0)
                {
                    LanguageAPI.Add("ITEM_BARRIERONOVERHEAL_DESC", "Healing past full grants you a <style=cIsHealing>barrier</style> for <style=cIsHealing>50% <style=cStack>(+50% per stack)</style></style> of the amount you <style=cIsHealing>healed</style>. All <style=cIsHealing>barrier</style> no longer naturally decays.", "en");
                }
                else
                {
                    LanguageAPI.Add("ITEM_BARRIERONOVERHEAL_DESC", "Healing past full grants you a <style=cIsHealing>barrier</style> for <style=cIsHealing>50% <style=cStack>(+50% per stack)</style></style> of the amount you <style=cIsHealing>healed</style>. All <style=cIsHealing>barrier</style> decays <style=cIsHealing>" + ((1 - WConfig.cfg_Red_Aegis_VAL.Value) * 100) + "%</style> slower.", "en");
                }
            }
            #endregion

            #region Laser Scope
            if (WConfig.cfg_Red_LaserScope.Value == true)
            {
                RecalculateStatsAPI.GetStatCoefficients += delegate (CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
                {
                    bool flag = sender.inventory != null;
                    if (flag)
                    {
                        if (sender.inventory.GetItemCount(DLC1Content.Items.CritDamage) > 0)
                        {
                            args.critAdd += 10;
                        }
                    }
                };
                LanguageAPI.Add("ITEM_CRITDAMAGE_DESC", "Gain <style=cIsDamage>10% critical chance</style>. <style=cIsDamage>Critical Strikes</style> deal an additional <style=cIsDamage>100% damage</style><style=cStack>(+100% per stack)</style>.");
            }
            #endregion
        }


        public static void AegisNoDecay(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.inventory)
            {
                if (self.inventory.GetItemCount(RoR2Content.Items.BarrierOnOverHeal) > 0)
                {
                    self.barrierDecayRate *= AegisDrainMult;
                }
            }
        }

    }
}