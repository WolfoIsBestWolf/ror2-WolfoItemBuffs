using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
                RecalculateStatsAPI.GetStatCoefficients += KnurlArmor;
                LanguageAPI.Add("ITEM_KNURL_PICKUP", "Boosts health, regeneration, and armor");
                LanguageAPI.Add("ITEM_KNURL_DESC", "<style=cIsHealing>Increase maximum health</style> by <style=cIsHealing>40</style> <style=cStack>(+40 per stack)</style>, <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+1.6 hp/s</style> <style=cStack>(+1.6 hp/s per stack)</style>, and <style=cIsHealing>armor</style> by <style=cIsHealing>12</style> <style=cStack>(+12 per stack)</style>.");
            }
            #endregion

            #region Defense Nucleus
            if (WConfig.cfg_Yellow_DefenseNuc.Value)
            {
                ItemDef HeadHunter = Addressables.LoadAssetAsync<ItemDef>(key: "RoR2/Base/HeadHunter/HeadHunter.asset").WaitForCompletion();
                GameObject MinorConstructOnKillMaster = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/MajorAndMinorConstruct/MinorConstructOnKillMaster.prefab").WaitForCompletion();
                GivePickupsOnStart.ItemDefInfo[] itemDefInfos =
                [
                    new GivePickupsOnStart.ItemDefInfo
                    {
                        itemDef = HeadHunter,
                        count = 200
                    }
                ];
                MinorConstructOnKillMaster.AddComponent<GivePickupsOnStart>().itemDefInfos = itemDefInfos;

                if (Main.mod_riskyMod)
                {
                    On.RoR2.Projectile.ProjectileSpawnMaster.SpawnMaster += Nucleus_MoreStatsPerStack;
                    On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += OnlyFourAlphaConstruct;
                    LanguageAPI.Add("ITEM_MINORCONSTRUCTONKILL_DESC", "Killing elite monsters spawns an <style=cIsDamage>Alpha Construct</style> with 400% <style=cStack>(+300% per stack)</style> <style=cIsHealing>health</style> and <style=cIsDamage>damage</style> that lasts <style=cIsUtility>30s</style>. Limited to <style=cIsUtility>4</style> constructs.");

                }

            }

            #endregion
        }

        private static void Nucleus_MoreStatsPerStack(On.RoR2.Projectile.ProjectileSpawnMaster.orig_SpawnMaster orig, RoR2.Projectile.ProjectileSpawnMaster self)
        {
            if (self.spawnCard.name.StartsWith("cscMino"))
            {
                var proj = self.GetComponent<RoR2.Projectile.ProjectileController>();
                if (proj.owner)
                {
                    CharacterBody body = proj.owner.GetComponent<CharacterBody>();
                    if (body)
                    {
                        int itemCount = body.inventory.GetItemCount(DLC1Content.Items.MinorConstructOnKill);
                        self.spawnCard.itemsToGrant[0].count = 30 * itemCount;
                        self.spawnCard.itemsToGrant[1].count = 30 * itemCount;
                    }
                }
            }
     

            orig(self);
        }

        private static int OnlyFourAlphaConstruct(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            if (slot == DeployableSlot.MinorConstructOnKill)
            {
                return 4;
            }
            return orig(self, slot);
        }

        private static void KnurlArmor(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                args.armorAdd += sender.inventory.GetItemCount(RoR2Content.Items.Knurl) * 12;
            }
        }
    }
}