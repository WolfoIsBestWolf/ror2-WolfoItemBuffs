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

            Addressables.LoadAssetAsync<EquipmentDef>(key: "RoR2/Base/Jetpack/Jetpack.asset").WaitForCompletion().canBeRandomlyTriggered = true;


            //Inherit Elite Equipment
            if (WConfig.cfg_Orange_EliteInherit.Value == true)
            {
                On.RoR2.MinionOwnership.MinionGroup.AddMinion += Minion_Inherit_Elite;
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