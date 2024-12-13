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
    public class T_Lunar
    {
        public static EliteDef EliteDefLunarEulogy = ScriptableObject.CreateInstance<EliteDef>();

        public static void Start()
        {
            Addressables.LoadAssetAsync<BuffDef>(key: "RoR2/DLC2/FalseSon/bdlunarruin.asset").WaitForCompletion().flags = BuffDef.Flags.NONE;

            #region Perfected Eulogy Zero

            //Eulogy Perfected
            EliteDef EliteDefLunar = Addressables.LoadAssetAsync<EliteDef>(key: "RoR2/Base/EliteLunar/edLunar.asset").WaitForCompletion();
            EliteDefLunarEulogy.modifierToken = EliteDefLunar.modifierToken;
            EliteDefLunarEulogy.eliteEquipmentDef = EliteDefLunar.eliteEquipmentDef;
            EliteDefLunarEulogy.color = EliteDefLunar.color;
            EliteDefLunarEulogy.shaderEliteRampIndex = EliteDefLunar.shaderEliteRampIndex;
            EliteDefLunarEulogy.healthBoostCoefficient = 3.2f;
            EliteDefLunarEulogy.damageBoostCoefficient = EliteDefLunar.damageBoostCoefficient;
            EliteDefLunarEulogy.name = "edLunarEulogy";

            ContentAddition.AddEliteDef(EliteDefLunarEulogy);
            if (WConfig.cfg_Blue_Eulogy.Value == true)
            {
                //On.RoR2.CombatDirector.EliteTierDef.GetRandomAvailableEliteDef += LunarEulogyHook1; 
                //Somehow hooking into Spawn is 5 times as efficient
                On.RoR2.CombatDirector.Spawn += LunarEulogyHook;
                LanguageAPI.Add("ITEM_RANDOMLYLUNAR_PICKUP", "Items, equipment and elites have a small chance to transform into a Lunar version instead.", "en");
                LanguageAPI.Add("ITEM_RANDOMLYLUNAR_DESC", "Items and equipment have a <style=cIsUtility>5% <style=cStack>(+5% per stack)</style></style> chance to become a <style=cIsLunar>Lunar</style> item or equipment and Elite Enemies have a <style=cIsUtility>10% <style=cStack>(+5% per stack)</style></style> spawn as <style=cIsLunar>Perfected</style> instead.", "en");
            }

            #endregion

            #region Focus Converg

            HoldoutZoneController.FocusConvergenceController.cap = 999999;
            On.RoR2.HoldoutZoneController.Awake += (orig, self) =>
            {
                orig(self);
                if (self.minimumRadius == 0)
                {
                    self.minimumRadius = 10;
                }
            };
            if (WConfig.cfg_Blue_Focus.Value)
            {
                On.RoR2.HoldoutZoneController.FocusConvergenceController.ApplyRate += FocusConvergenceController_ApplyRate;
            }
            #endregion

            #region Lunar Beads
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
            #endregion
        }



        private static void FocusConvergenceController_ApplyRate(On.RoR2.HoldoutZoneController.FocusConvergenceController.orig_ApplyRate orig, MonoBehaviour self, ref float rate)
        {
            if ((self as HoldoutZoneController.FocusConvergenceController).currentFocusConvergenceCount > 0)
            {
                rate *= 1f / MathF.Pow(0.7f, (self as HoldoutZoneController.FocusConvergenceController).currentFocusConvergenceCount);
            }
        }

        public static bool LunarEulogyHook(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            //Debug.LogWarning("CombatDirector.orig_Spawn :" + SPAWN); SPAWN++;
            //Debug.Log(eliteDef);
            if (eliteDef)
            {
                int itemCountGlobal = WolfoItemBuffs.GetItemCountFromPlayers(DLC1Content.Items.RandomlyLunar.itemIndex);
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



    }
}