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
    public class T_Boss
    {
        public static void Start()
        {
            #region Knurl Armor
            bool otherKnurlBuff = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("OakPrime.KnurlBuff");
            if (WConfig.cfg_Yellow_Knurl.Value == true && !otherKnurlBuff)
            {
                RecalculateStatsAPI.GetStatCoefficients += delegate (CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
                {
                    bool flag = sender.inventory != null;
                    if (flag)
                    {
                        args.armorAdd += sender.inventory.GetItemCount(RoR2Content.Items.Knurl) * 14;
                    }
                };
                LanguageAPI.Add("ITEM_KNURL_PICKUP", "Boosts health, regeneration, and armor");
                LanguageAPI.Add("ITEM_KNURL_DESC", "<style=cIsHealing>Increase maximum health</style> by <style=cIsHealing>40</style> <style=cStack>(+40 per stack)</style>, <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+1.6 hp/s</style> <style=cStack>(+1.6 hp/s per stack)</style>, and <style=cIsHealing>armor</style> by <style=cIsHealing>14</style> <style=cStack>(+14 per stack)</style>.");
            }
            #endregion

            #region Defense Nucleus
            if (WConfig.cfg_Yellow_DefenseNuc.Value)
            {
                ItemDef HeadHunter = Addressables.LoadAssetAsync<ItemDef>(key: "RoR2/Base/HeadHunter/HeadHunter.asset").WaitForCompletion();
                GameObject MinorConstructOnKillMaster = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/MajorAndMinorConstruct/MinorConstructOnKillMaster.prefab").WaitForCompletion();

                RoR2.GivePickupsOnStart.ItemDefInfo[] itemDefInfos = new RoR2.GivePickupsOnStart.ItemDefInfo[0];
                itemDefInfos = itemDefInfos.Add(new GivePickupsOnStart.ItemDefInfo { itemDef = RoR2Content.Items.HeadHunter, count = 200 });

                MinorConstructOnKillMaster.AddComponent<RoR2.GivePickupsOnStart>().itemDefInfos = itemDefInfos;
            }

            #endregion
        }


    }
}