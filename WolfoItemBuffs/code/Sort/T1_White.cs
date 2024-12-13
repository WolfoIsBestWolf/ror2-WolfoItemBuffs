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
    public class T1_White
    {
        public static void Start()
        {
            bool riskyMod = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskyLives.RiskyMod");
            if (WConfig.cfg_White_Warbanner.Value)
            {
                LanguageAPI.Add("ITEM_WARDONLEVEL_DESC", "On <style=cIsUtility>level up, stage start</style> or starting a <style=cIsUtility>boss fight</style>, drop a banner that strengthens all allies within <style=cIsUtility>16m</style> <style=cStack>(+8m per stack)</style>. Raise <style=cIsDamage>attack</style> and <style=cIsUtility>movement speed</style> by <style=cIsDamage>30%</style>.", "en");

                On.RoR2.CharacterBody.Start += WarbannerOnSpawn;
                if (!riskyMod)
                {
                    On.RoR2.ScriptedCombatEncounter.BeginEncounter += Warbanner_ScriptedCombat;
                }
                On.RoR2.VoidRaidGauntletExitController.OnBodyTeleport += Warbanner_AfterVoidlingTeleport;
                On.RoR2.Artifacts.DoppelgangerInvasionManager.PerformInvasion += Warbanner_OnVengence;

                ItemDef warbanner = Addressables.LoadAssetAsync<ItemDef>(key: "RoR2/Base/WardOnLevel/WardOnLevel.asset").WaitForCompletion();
                warbanner.tags = warbanner.tags.Remove(ItemTag.CannotCopy);
            }  
        }

        private static void Warbanner_AfterVoidlingTeleport(On.RoR2.VoidRaidGauntletExitController.orig_OnBodyTeleport orig, VoidRaidGauntletExitController self, CharacterBody body)
        {
            orig(self, body);
            RoR2.Items.WardOnLevelManager.OnCharacterLevelUp(body);
        }

        public static void Warbanner_OnVengence(On.RoR2.Artifacts.DoppelgangerInvasionManager.orig_PerformInvasion orig, Xoroshiro128Plus rng)
        {
            orig(rng);
            foreach (CharacterBody body in CharacterBody.instancesList)
            {
                RoR2.Items.WardOnLevelManager.OnCharacterLevelUp(body);
            }
        }

        public static void Warbanner_ScriptedCombat(On.RoR2.ScriptedCombatEncounter.orig_BeginEncounter orig, ScriptedCombatEncounter self)
        {
            orig(self);
            if (self.GetComponent<BossGroup>())
            {
                foreach (CharacterBody body in CharacterBody.instancesList)
                {
                    RoR2.Items.WardOnLevelManager.OnCharacterLevelUp(body);
                }
            }
        }

        public static void WarbannerOnSpawn(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            RoR2.Items.WardOnLevelManager.OnCharacterLevelUp(self);
        }

    }
}