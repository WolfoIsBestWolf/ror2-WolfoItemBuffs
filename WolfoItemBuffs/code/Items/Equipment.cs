using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace WolfoItemBuffs
{
    public class Equipment
    {
        public static void Start()
        {
            #region Vending
            GameObject VendingMachine = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/VendingMachine/VendingMachine.prefab").WaitForCompletion();
            BoxCollider collider = VendingMachine.transform.GetChild(0).gameObject.GetComponent<BoxCollider>();
            collider.isTrigger = false;
            collider.size = collider.extents;
            #endregion

            #region Ocular HUD
            /*
            RecalculateStatsAPI.GetStatCoefficients += Occular;
            LanguageAPI.Add("EQUIPMENT_CRITONUSE_PICKUP", "Gain 100% Critical Strike Chance and increased Critical Strike Damage for 8 seconds.", "en");
            LanguageAPI.Add("EQUIPMENT_CRITONUSE_DESC", "Gain <style=cIsDamage>+100% Critical Strike Chance</style> for 8 seconds. <style=cIsDamage>Critical Strikes</style> deal an additional <style=cIsDamage>2% damage</style> for every critical chance <style=cIsDamage>%</style> above 100%.", "en");
            */
            #endregion

            Addressables.LoadAssetAsync<EquipmentDef>(key: "RoR2/Base/Jetpack/Jetpack.asset").WaitForCompletion().canBeRandomlyTriggered = true;


            //Inherit Elite Equipment
            if (WConfig.cfg_Orange_EliteInherit.Value == true)
            {
                On.RoR2.MinionOwnership.MinionGroup.AddMinion += Minion_Inherit_Elite;
            }

            if (WConfig.cfg_Orange_Molotov.Value)
            {
                //Molotov 8 pack
                GameObject molotovCluster = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/Molotov/MolotovClusterProjectile.prefab").WaitForCompletion();
                molotovCluster.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>().childrenCount = 8;
                GameObject molotovSingle = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/Molotov/MolotovSingleProjectile.prefab").WaitForCompletion();
                molotovSingle.GetComponent<RoR2.Projectile.ProjectileImpactExplosion>().destroyOnEnemy = true;
            }
        }

        private static void Occular(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(RoR2Content.Buffs.FullCrit))
            {
                if (sender.crit > 100)
                {
                    args.critDamageMultAdd += (sender.crit - 100) * 0.02f;
                }
            }
        }

        public static void Minion_Inherit_Elite(On.RoR2.MinionOwnership.MinionGroup.orig_AddMinion orig, NetworkInstanceId ownerId, global::RoR2.MinionOwnership minion)
        {
            orig(ownerId, minion);
            if (NetworkServer.active)
            {
                if (minion.ownerMaster && minion.ownerMaster.playerCharacterMasterController)
                {
                    EquipmentIndex equipment = minion.ownerMaster.inventory.GetEquipmentIndex();
                    if (equipment != EquipmentIndex.None)
                    {
                        EquipmentDef def = EquipmentCatalog.GetEquipmentDef(equipment);
                        if (def.passiveBuffDef && def.passiveBuffDef.isElite)
                        {
                            if (minion.name.StartsWith("AffixEarth"))
                            {
                                return;
                            }
                            Inventory inventory = minion.gameObject.GetComponent<Inventory>();
                            inventory.SetEquipment(new EquipmentState(equipment, Run.FixedTimeStamp.negativeInfinity, 0), 0);

                        }
                    }
                }
            }
        }
    }
}