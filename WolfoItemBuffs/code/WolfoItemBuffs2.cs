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

namespace WolfoItemBuffs
{
    public class WolfoItemBuffs2
    {
        public static EliteDef EliteDefLunarEulogy = ScriptableObject.CreateInstance<EliteDef>();

        public static void Start()
        {
            Items();
            BetterRedWhipCheck();
            VoidAffix();
            //Inherit Elite Equipment
            if (WConfig.cfg_Orange_EliteInherit.Value == true)
            {
                On.RoR2.MinionOwnership.MinionGroup.AddMinion += MinionsInheritWithDrones;
            }
        }

        public static void MinionsInheritWithDrones(On.RoR2.MinionOwnership.MinionGroup.orig_AddMinion orig, NetworkInstanceId ownerId, global::RoR2.MinionOwnership minion)
        {
            orig(ownerId, minion);
            //Surely could be simplified to if master is player and elite and that's it
            if (NetworkServer.active)
            {
                if (!minion.name.StartsWith("AffixEarth") && minion.ownerMaster && minion.ownerMaster.GetComponent<PlayerCharacterMasterController>() && minion.ownerMaster.inventory.GetEquipmentIndex() != EquipmentIndex.None)
                {
                    //Debug.LogWarning(EquipmentCatalog.GetEquipmentDef(minion.ownerMaster.inventory.GetEquipmentIndex()).name);   
                    if (minion.ownerMaster.hasBody && minion.ownerMaster.GetBody().isElite)
                    {
                        Inventory inventory = minion.gameObject.GetComponent<Inventory>();
                        //inventory.SetEquipmentIndex(minion.ownerMaster.inventory.GetEquipmentIndex());
                        inventory.SetEquipment(new EquipmentState(minion.ownerMaster.inventory.GetEquipmentIndex(), Run.FixedTimeStamp.negativeInfinity, 0), 0);
                        inventory.GiveItem(RoR2Content.Items.BoostDamage, 10);
                        inventory.GiveItem(RoR2Content.Items.BoostHp, 30);
                    }
                }
            }
        }


        public static void Items()
        {
            Addressables.LoadAssetAsync<EquipmentDef>(key: "RoR2/Base/Jetpack/Jetpack.asset").WaitForCompletion().canBeRandomlyTriggered = true;


            if (WConfig.cfg_Green_Squid.Value == true)
            {
                Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Base/Squid/SquidTurretBody.prefab").WaitForCompletion().GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.Mechanical;
            }

            /*if (WConfig.ItemsAncestralIncubator.Value == true)
            {
                LegacyResourcesAPI.Load<ItemDef>("ItemDefs/Incubator")._itemTierDef = LegacyResourcesAPI.Load<ItemTierDef>("ItemTierDefs/BossTierDef");
            };*/

            /*if (WConfig.ItemsCaptainMatrix.Value == true)
            {
                ItemDef CaptainDefenseMatrix = LegacyResourcesAPI.Load<ItemDef>("ItemDefs/CaptainDefenseMatrix");
                CaptainDefenseMatrix.tags = CaptainDefenseMatrix.tags.Remove(ItemTag.WorldUnique);
            };*/



            //Laser Scope 15
            if (WConfig.cfg_Red_LaserScope.Value == true)
            {
                RecalculateStatsAPI.GetStatCoefficients += delegate (CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
                {
                    bool flag = sender.inventory != null;
                    if (flag)
                    {
                        if (sender.inventory.GetItemCount(DLC1Content.Items.CritDamage) > 0)
                        {
                            args.critAdd += 15;
                        }
                    }
                };
                LanguageAPI.Add("ITEM_CRITDAMAGE_DESC", "Gain <style=cIsDamage>15% critical chance</style>. <style=cIsDamage>Critical Strikes</style> deal an additional <style=cIsDamage>100% damage</style><style=cStack>(+100% per stack)</style>.");
            }

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

            //Eulogy Perfected
            EliteDef EliteDefLunar = Addressables.LoadAssetAsync<EliteDef>(key: "RoR2/Base/EliteLunar/edLunar.asset").WaitForCompletion();
            EliteDefLunarEulogy.modifierToken = EliteDefLunar.modifierToken;
            EliteDefLunarEulogy.eliteEquipmentDef = EliteDefLunar.eliteEquipmentDef;
            EliteDefLunarEulogy.color = EliteDefLunar.color;
            EliteDefLunarEulogy.shaderEliteRampIndex = EliteDefLunar.shaderEliteRampIndex;
            EliteDefLunarEulogy.healthBoostCoefficient = 3.2f;
            EliteDefLunarEulogy.damageBoostCoefficient = EliteDefLunar.damageBoostCoefficient;
            EliteDefLunarEulogy.name = "edLunarEulogy";

            ContentAddition.AddEliteDef(EliteDefLunar);
            if (WConfig.cfg_Blue_Eulogy.Value == true)
            {
                //On.RoR2.CombatDirector.EliteTierDef.GetRandomAvailableEliteDef += LunarEulogyHook1; 
                //Somehow hooking into Spawn is 5 times as efficient
                On.RoR2.CombatDirector.Spawn += LunarEulogyHook;
                LanguageAPI.Add("ITEM_RANDOMLYLUNAR_PICKUP", "Items, equipment and elites have a small chance to transform into a Lunar version instead.", "en");
                LanguageAPI.Add("ITEM_RANDOMLYLUNAR_DESC", "Items and equipment have a <style=cIsUtility>5% <style=cStack>(+5% per stack)</style></style> chance to become a <style=cIsLunar>Lunar</style> item or equipment and Elite Enemies have a <style=cIsUtility>10% <style=cStack>(+5% per stack)</style></style> spawn as <style=cIsLunar>Perfected</style> instead.", "en");
            }


            //Uncapped
            HoldoutZoneController.FocusConvergenceController.cap = 999999;
            On.RoR2.HoldoutZoneController.Awake += (orig, self) =>
            {
                orig(self);
                if (self.minimumRadius == 0)
                {
                    self.minimumRadius = 10;
                }
            };


            //Beads Stacking
            LanguageAPI.Add("ITEM_LUNARTRINKET_DESC", "Once guided to the <style=cIsUtility>monolith</style>, become whole and <style=cIsDamage>fight 1 <style=cStack>(+1 per stack)</style> guardians</style>", "en");

            On.RoR2.ScriptedCombatEncounter.BeginEncounter += (orig, self) =>
            {
                if (self.name.StartsWith("ScavLunar"))
                {

                    int beadcount = Util.GetItemCountForTeam(TeamIndex.Player, RoR2Content.Items.LunarTrinket.itemIndex, false, true);
                    if (beadcount == 0) { beadcount = 1; }
                    var x = self.spawns[0].explicitSpawnPosition.localPosition.x + 8;
                    var y = self.spawns[0].explicitSpawnPosition.localPosition.y;
                    var z = self.spawns[0].explicitSpawnPosition.localPosition.z;

                    int counter = 1;

                    for (int i = 0; i < beadcount; i++)
                    {
                        self.spawns[0].explicitSpawnPosition.localPosition = new Vector3(x, y, z);
                        x = x - 8;          
                        if (counter == 4)
                        {
                            counter = 0;
                            x = x + 32;
                            z = z - 8;
                        }
                        counter++;
                        self.hasSpawnedServer = false;
                        orig(self);
                    }
                    return;
                }
                orig(self);
            };

            On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.OnEnter += (orig, self) =>
            {
                orig(self);
                int beadcount = Util.GetItemCountForTeam(TeamIndex.Player, RoR2Content.Items.LunarTrinket.itemIndex, false, true);
                if (beadcount > 1)
                {
                    if (beadcount > 10) { beadcount = 10; };
                    float mult = EntityStates.Missions.LunarScavengerEncounter.FadeOut.duration / 4 * (beadcount - 1);

                    self.SetFieldValue<float>("duration", EntityStates.Missions.LunarScavengerEncounter.FadeOut.duration + mult);
                }
            };

            On.EntityStates.ScavMonster.Death.OnPreDestroyBodyServer += (orig, self) =>
            {
                if (self.outer.name.StartsWith("ScavLunar"))
                {
                    self.shouldDropPack = true;
                }
                orig(self);
            };
            //
            //

            if(WConfig.cfg_Yellow_DefenseNuc.Value)
            {
                ItemDef HeadHunter = Addressables.LoadAssetAsync<ItemDef>(key: "RoR2/Base/HeadHunter/HeadHunter.asset").WaitForCompletion();
                GameObject MinorConstructOnKillMaster = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/MajorAndMinorConstruct/MinorConstructOnKillMaster.prefab").WaitForCompletion();

                RoR2.GivePickupsOnStart.ItemDefInfo[] itemDefInfos = new RoR2.GivePickupsOnStart.ItemDefInfo[0];
                 itemDefInfos = itemDefInfos.Add(new GivePickupsOnStart.ItemDefInfo { itemDef = RoR2Content.Items.HeadHunter, count = 200 });

                MinorConstructOnKillMaster.AddComponent<RoR2.GivePickupsOnStart>().itemDefInfos = itemDefInfos;
            }



        }
 

        public static void VoidAffix()
        {
            EquipmentDef VoidAffix = Addressables.LoadAssetAsync<EquipmentDef>(key: "RoR2/DLC1/EliteVoid/EliteVoidEquipment.asset").WaitForCompletion();
            GameObject VoidAffixDisplay = R2API.PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/EliteVoid/DisplayAffixVoid.prefab").WaitForCompletion(), "PickupAffixVoidW", false);
            VoidAffixDisplay.transform.GetChild(0).GetChild(1).SetAsFirstSibling();
            VoidAffixDisplay.transform.GetChild(1).localPosition = new Vector3(0f, 0.7f, 0f);
            VoidAffixDisplay.transform.GetChild(1).GetChild(0).localPosition = new Vector3(0, -0.5f, -0.6f);
            VoidAffixDisplay.transform.GetChild(1).GetChild(0).localScale = new Vector3(1.5f, 1.5f, 1.5f);
            VoidAffixDisplay.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
            VoidAffixDisplay.transform.GetChild(1).GetChild(3).gameObject.SetActive(false);
            VoidAffixDisplay.transform.GetChild(0).eulerAngles = new Vector3(310, 0, 0);
            VoidAffixDisplay.transform.GetChild(0).localScale = new Vector3(0.75f, 0.75f, 0.75f);

            ItemDisplay display = VoidAffixDisplay.GetComponent<ItemDisplay>();
            display.rendererInfos = display.rendererInfos.Remove(display.rendererInfos[4]);

            LanguageAPI.Add("EQUIPMENT_AFFIXVOID_NAME", "Voidborne Curiosity", "en");
            LanguageAPI.Add("EQUIPMENT_AFFIXVOID_PICKUP", "Lose your aspect of self.", "en");
            LanguageAPI.Add("EQUIPMENT_AFFIXVOID_DESC", "Increases <style=cIsHealing>maximum health</style> by <style=cIsHealing>50%</style> and decrease <style=cIsDamage>base damage</style> by <style=cIsDamage>30%</style>. <style=cIsDamage>Collapse</style> enemies on hit and <style=cIsHealing>block</style> incoming damage once every <style=cIsUtility>15 seconds</style>. ", "en");

            Texture2D UniqueAffixVoid = new Texture2D(128, 128, TextureFormat.DXT5, false);
            UniqueAffixVoid.LoadImage(Properties.Resources.UniqueAffixVoid, true);
            UniqueAffixVoid.filterMode = FilterMode.Bilinear;
            UniqueAffixVoid.wrapMode = TextureWrapMode.Clamp;
            Sprite UniqueAffixVoidS = Sprite.Create(UniqueAffixVoid, WRect.rec128, WRect.half);

            VoidAffix.pickupIconSprite = UniqueAffixVoidS;
            VoidAffix.pickupModelPrefab = VoidAffixDisplay;

            VoidAffix.dropOnDeathChance = 0.00025f;
            

            On.RoR2.CharacterMaster.RespawnExtraLifeVoid += (orig, self) =>
            {
                orig(self);
                if (self.inventory.currentEquipmentIndex != EquipmentIndex.None && EquipmentCatalog.GetEquipmentDef(self.inventory.currentEquipmentIndex).passiveBuffDef)
                {
                    CharacterMasterNotificationQueue.PushEquipmentTransformNotification(self, self.inventory.currentEquipmentIndex, DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.ContagiousVoid);
                    self.inventory.SetEquipment(new EquipmentState(DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex, Run.FixedTimeStamp.negativeInfinity, 0), 0);
                }
            };


            //Something related to Void Affix dropping but also it activating Elite Activating things
            On.RoR2.CharacterBody.OnEquipmentLost += (orig, self, equipmentDef) =>
            {
                if (equipmentDef == DLC1Content.Equipment.EliteVoidEquipment && !self.healthComponent.alive)
                {
                    return;
                }
                orig(self, equipmentDef);
            };
            //Tho I don't really remember this something related to Vultures
            On.RoR2.AffixVoidBehavior.OnEnable += (orig, self) =>
            {
                orig(self);
                if (self.body && self.body.isPlayerControlled)
                {
                    if (self.body.inventory.currentEquipmentIndex != DLC1Content.Equipment.EliteVoidEquipment.equipmentIndex)
                    {
                        if (!self.wasVoidBody)
                        {
                            self.body.bodyFlags &= ~CharacterBody.BodyFlags.Void;
                        }
                    }                
                }
            };
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

        public static bool LunarEulogyHook(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            //Debug.LogWarning("CombatDirector.orig_Spawn :" + SPAWN); SPAWN++;
            //Debug.Log(eliteDef);
            if (eliteDef)
            {
                int itemCountGlobal = GetItemCountFromPlayers(DLC1Content.Items.RandomlyLunar.itemIndex);
                if (itemCountGlobal > 0)
                {
                    itemCountGlobal++;
                    if (eliteDef.healthBoostCoefficient < 8 && self.rng.nextNormalizedFloat < 0.05f * (float)itemCountGlobal)
                    {
                        eliteDef = EliteDefLunarEulogy;
                    }
                }
            }
            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }

        public static int GetItemCountFromPlayers(ItemIndex itemIndex)
        {
            int num = 0;
            System.Collections.ObjectModel.ReadOnlyCollection<PlayerCharacterMasterController> readOnlyInstancesList = PlayerCharacterMasterController.instances;
            int i = 0;
            int count = readOnlyInstancesList.Count;
            while (i < count)
            {
                CharacterMaster characterMaster = readOnlyInstancesList[i].master;
                num += characterMaster.inventory.GetItemCount(itemIndex);
                i++;
            }
            return num;
        }

    }
}