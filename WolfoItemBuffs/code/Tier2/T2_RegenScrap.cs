using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
 
namespace WolfoItemBuffs
{
    public class T2_RegenScrap
    {
        public static float chance_HighTier = 50f;    
        public static void Start()
        {
            On.RoR2.PurchaseInteraction.OnInteractionBegin += RegenScrap_Rework;
            On.RoR2.Inventory.HasAtLeastXTotalItemsOfTier += RegenScrap_Affordable;
            On.RoR2.ShopTerminalBehavior.DropPickup += RegenScrap_Drops;
            //On.EntityStates.Duplicator.Duplicating.OnEnter += Duplicating_OnEnter; //This duration shit does not work.
            //On.EntityStates.Duplicator.Duplicating.DropDroplet += Duplicating_DropDroplet;
            //Has atleast X of tier

            LanguageAPI.Add("ITEM_REGENERATINGSCRAP_PICKUP", "Prioritized when using any 3D Printer. Usable once per stage.");
            //LanguageAPI.Add("ITEM_REGENERATINGSCRAP_DESC", "Prioritized when using <style=cIsUtility>any type</style> of 3D Printer. At the start of each stage, it <style=cIsUtility>regenerates</style>. <style=cStack>All additional Regenerative Scrap will be consumed for a " + Chance + "% chance per stack of extra items.</style> ");
            //LanguageAPI.Add("ITEM_REGENERATINGSCRAP_DESC", "Prioritized when using <style=cIsUtility>any type</style> of 3D Printer. At the start of each stage, it <style=cIsUtility>regenerates</style>. Has a <style=cIsUtility>" + chance_HighTier + "%</style> chance to payout on <style=cIsHealth>Mili-Tech</style> or <style=cIsDamage>Overgrown</style> 3D Printers. <style=cStack>All Regenerative Scrap will be used at once for an item each.</style> ");
            LanguageAPI.Add("ITEM_REGENERATINGSCRAP_DESC", "Prioritized when using <style=cIsUtility>any type</style> of 3D Printer. At the start of each stage, it <style=cIsUtility>regenerates</style>. Has a <style=cIsUtility>" + chance_HighTier + "%</style> chance to payout on <style=cIsHealth>higher</style> <style=cIsDamage>rarity</style> 3D Printers. <style=cStack>All Regenerative Scrap are used at once.</style>");

        }

        private static void Duplicating_DropDroplet(On.EntityStates.Duplicator.Duplicating.orig_DropDroplet orig, EntityStates.Duplicator.Duplicating self)
        {
            RegenScrapExtraItemTracker tracker = self.GetComponent<RegenScrapExtraItemTracker>();
            if (!self.hasDroppedDroplet && tracker && tracker.isJuicy && tracker.duration > 0)
            {
                EntityStates.Duplicator.Duplicating.timeBetweenStartAndDropDroplet -= tracker.duration;
                tracker.duration = 0;
            }
            orig(self);
        }

        private static void Duplicating_OnEnter(On.EntityStates.Duplicator.Duplicating.orig_OnEnter orig, EntityStates.Duplicator.Duplicating self)
        {
            RegenScrapExtraItemTracker tracker = self.GetComponent<RegenScrapExtraItemTracker>();
            if (tracker && tracker.isJuicy)
            {
                tracker.duration = 5f-5f*MathF.Pow(0.85f, tracker.bonusItems);
                EntityStates.Duplicator.Duplicating.timeBetweenStartAndDropDroplet += tracker.duration;
            }
            orig(self); 
            
        }

        private static bool RegenScrap_Affordable(On.RoR2.Inventory.orig_HasAtLeastXTotalItemsOfTier orig, Inventory self, ItemTier itemTier, int x)
        {       
            if (itemTier == ItemTier.Tier1 || itemTier == ItemTier.Tier3 || itemTier == ItemTier.Boss) 
            {
                return x <= (self.GetTotalItemCountOfTier(itemTier) + self.GetItemCount(DLC1Content.Items.RegeneratingScrap));
            }
            return orig(self,itemTier,x);
        }

        private static void RegenScrap_Drops(On.RoR2.ShopTerminalBehavior.orig_DropPickup orig, ShopTerminalBehavior self)
        {
            RegenScrapExtraItemTracker tracker = self.GetComponent<RegenScrapExtraItemTracker>();
            if (!tracker)
            {
                orig(self);
            }
            else
            {
                if (!tracker.failedRoll)
                {
                    orig(self);
                }
                if (tracker.isJuicy)
                {
                    EffectManager.SimpleMuzzleFlash(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/RegeneratingScrap/RegeneratingScrapExplosionInPrinter.prefab").WaitForCompletion(), self.gameObject, "DropPivot", true);
                    RoR2.Audio.EntitySoundManager.EmitSoundServer(RoR2.Audio.NetworkSoundEventCatalog.FindNetworkSoundEventIndex("Play_item_proc_regenScrap_consume"), self.gameObject);
                    if (!tracker.inProgress && tracker.bonusItems > 0)
                    {
                        tracker.inProgress = true;
                        for (int i = 0; tracker.bonusItems > i; i++)
                        {
                            self.Invoke("DropPickup", (i + 1f) * 0.2f);
                        }
                        tracker.bonusItems = 0;
                    }
                }
            }         
        }

 
  
        private static void RegenScrap_Rework(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (!self.CanBeAffordedByInteractor(activator))
            {
                return;
            }
            if (self.costType == CostTypeIndex.WhiteItem || self.costType == CostTypeIndex.GreenItem || self.costType == CostTypeIndex.RedItem || self.costType == CostTypeIndex.BossItem)
            {
                if (self.name.StartsWith("Dupl") || self.name.StartsWith("LunarCa"))
                {
                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    int itemCount = characterBody.inventory.GetItemCount(DLC1Content.Items.RegeneratingScrap);
                    int prevCost = self.cost;

                    var Regen = self.gameObject.GetComponent<RegenScrapExtraItemTracker>();
                    if (Regen == null)
                    {
                        Regen = self.gameObject.AddComponent<RegenScrapExtraItemTracker>();
                    }
                    Regen.isJuicy = false;
                    Regen.inProgress = false;
                    Regen.failedRoll = false;

                    if (itemCount > 0)
                    {
                        bool highTier = false;
                        float costToScrapRatio = (float)itemCount / (float)prevCost;
                        self.cost = Math.Max(self.cost, itemCount); //All Scrap at once.
                        switch (self.costType)
                        {
                            case CostTypeIndex.WhiteItem:
                                //Somehow, this breaks with Bubbets Items
                                Regen.FixBubbet(true, null, 0);
                                DLC1Content.Items.RegeneratingScrap.tier = ItemTier.Tier1;
                                break;
                            case CostTypeIndex.RedItem:
                                DLC1Content.Items.RegeneratingScrap.tier = ItemTier.Tier3;
                                highTier = true;
                                break;
                            case CostTypeIndex.BossItem:
                                DLC1Content.Items.RegeneratingScrap.tier = ItemTier.Boss;
                                highTier = true;
                                break;
                        }
                        float chance = 100f;
                        int bonusItems = -1;
                        if (highTier)
                        {
                            chance = chance_HighTier;
                        }

                        if (costToScrapRatio == 1)
                        {
                            if (highTier)
                            {
                                if (Util.CheckRoll(chance, null))
                                {
                                    //50 chance to fail.
                                    Regen.failedRoll = true;
                                }
                            }
                            //Else, do nothing
                        }
                        else if (costToScrapRatio > 1)
                        {
                            float restRatio = costToScrapRatio % 1;
                            costToScrapRatio = Mathf.Floor(costToScrapRatio);
                            Debug.Log(costToScrapRatio + " " + restRatio * 100);
                            if (highTier)
                            {
                                for (int i = 0; i < costToScrapRatio; i++)
                                {
                                    if (Util.CheckRoll(chance, null))
                                    {
                                        bonusItems++;
                                    }
                                }
                            }
                            else
                            {
                                //100
                                bonusItems = (int)costToScrapRatio - 1;
                            }
                            if (restRatio > 0)
                            {
                                if (Util.CheckRoll(restRatio * chance, null))
                                {
                                    bonusItems++;
                                }
                            }
                            if (highTier && bonusItems == -1)
                            {
                                Regen.failedRoll = true;
                            }

                        }
                        //If ratio below 1 then uhh idk, would never need a bonus item, can't really make stuff fail for high tier tho.
                        Regen.isJuicy = true;
                        Regen.bonusItems = bonusItems;
                    }
                    orig(self, activator);
                    if (itemCount > 0)
                    {                   
                        self.cost = prevCost;
                        if (self.costType == CostTypeIndex.WhiteItem)
                        {
                            Regen.FixBubbet(false, characterBody, itemCount);
                        }
                        DLC1Content.Items.RegeneratingScrap.tier = ItemTier.Tier2;
                    }
                }
                return;
            }
            orig(self, activator);
        }

        public class RegenScrapExtraItemTracker : MonoBehaviour
        {
            public bool isJuicy;
            public bool failedRoll;
            public bool inProgress;
            public int bonusItems;

            public bool addedDuration;
            public float duration;


            public static bool IsBubbetInstalled = false;
            public void FixBubbet(bool start, CharacterBody body, int Items)
            {
                if (start)
                {
                    ItemCatalog.tier1ItemList.Add(DLC1Content.Items.RegeneratingScrap.itemIndex);
                }
                else
                {
                    ItemCatalog.tier1ItemList.Remove(DLC1Content.Items.RegeneratingScrap.itemIndex);

                    body.inventory.GiveItem(DLC1Content.Items.RegeneratingScrapConsumed, Items);
                    EntitySoundManager.EmitSoundServer(NetworkSoundEventCatalog.FindNetworkSoundEventIndex("Play_item_proc_regenScrap_consume"), body.gameObject);
                    ModelLocator component = body.modelLocator;
                    if (component)
                    {
                        Transform modelTransform = component.modelTransform;
                        if (modelTransform)
                        {
                            CharacterModel component2 = modelTransform.GetComponent<CharacterModel>();
                            if (component2)
                            {
                                List<GameObject> itemDisplayObjects = component2.GetItemDisplayObjects(DLC1Content.Items.RegeneratingScrap.itemIndex);
                                if (itemDisplayObjects.Count > 0)
                                {
                                    GameObject gameObject = itemDisplayObjects[0];
                                    GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/RegeneratingScrap/RegeneratingScrapExplosionDisplay.prefab").WaitForCompletion();
                                    EffectData effectData = new EffectData
                                    {
                                        origin = gameObject.transform.position,
                                        rotation = gameObject.transform.rotation
                                    };
                                    EffectManager.SpawnEffect(effectPrefab, effectData, true);
                                }
                            }
                        }
                    }
                    EffectManager.SimpleMuzzleFlash(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/RegeneratingScrap/RegeneratingScrapExplosionInPrinter.prefab").WaitForCompletion(), this.gameObject, "DropPivot", true);

                    CharacterMasterNotificationQueue.SendTransformNotification(body.master, DLC1Content.Items.RegeneratingScrap.itemIndex, DLC1Content.Items.RegeneratingScrapConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                 
                }
            }
        }


    }
}