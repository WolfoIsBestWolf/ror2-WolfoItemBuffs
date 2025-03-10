using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WolfoItemBuffs
{
    public class T_Void
    {
        public static void Start()
        {
            if (WConfig.cfg_Pink_Ring.Value)
            {
                IL.RoR2.GlobalEventManager.ProcessHitEnemy += BuffSingularityBand;

                LanguageAPI.Add("ITEM_ELEMENTALRINGVOID_DESC", "Hits that deal <style=cIsDamage>more than 400% damage</style> also fire a black hole that <style=cIsUtility>draws enemies within 15m into its center</style>. Lasts <style=cIsUtility>5</style> seconds before collapsing, dealing <style=cIsDamage>150%</style> <style=cStack>(+150% per stack)</style> TOTAL damage. Recharges every <style=cIsUtility>20</style> seconds. <style=cIsVoid>Corrupts all Runald's and Kjaro's Bands</style>.", "en");

                Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ElementalRingVoid/ElementalRingVoidBlackHole.prefab").WaitForCompletion().GetComponent<RoR2.Projectile.ProjectileExplosion>().falloffModel = BlastAttack.FalloffModel.None;

            }

            VoidAffix();
        }

        public static void VoidAffix()
        {
            EquipmentDef VoidAffix = Addressables.LoadAssetAsync<EquipmentDef>(key: "RoR2/DLC1/EliteVoid/EliteVoidEquipment.asset").WaitForCompletion();
            GameObject displayObject = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/DLC1/EliteVoid/DisplayAffixVoid.prefab").WaitForCompletion();
            GameObject VoidAffixDisplay = R2API.PrefabAPI.InstantiateClone(displayObject, "PickupAffixVoidW", false);

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
            /* On.RoR2.CharacterBody.OnEquipmentLost += (orig, self, equipmentDef) =>
             {
                 if (equipmentDef == DLC1Content.Equipment.EliteVoidEquipment && !self.healthComponent.alive)
                 {
                     return;
                 }
                 orig(self, equipmentDef);
             };*/
            //Tho I don't really remember this something related to Vultures
            On.RoR2.AffixVoidBehavior.OnEnable += AffixVoidBehavior_OnEnable;
            displayObject.AddComponent<VoidAffixPlayer>();
        }

        private static void AffixVoidBehavior_OnEnable(On.RoR2.AffixVoidBehavior.orig_OnEnable orig, AffixVoidBehavior self)
        {
            orig(self);
            if (self.body && self.body.teamComponent && self.body.teamComponent.teamIndex == TeamIndex.Player)
            {
                if (!self.wasVoidBody)
                {
                    self.body.bodyFlags &= ~CharacterBody.BodyFlags.Void;
                }
            }
        }

        public class VoidAffixPlayer : MonoBehaviour
        {
            public void OnEnable()
            {
                CharacterModel model = this.transform.root.GetComponent<CharacterModel>();
                TeamComponent team = this.transform.root.GetComponent<TeamComponent>();
                if (model && model.body)
                {
                    team = model.body.teamComponent;
                }
                if (team && team.teamIndex == TeamIndex.Player)
                {
                    this.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
                    this.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
                }
            }
        }

        public static void BuffSingularityBand(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchLdstr("Prefabs/Projectiles/ElementalRingVoidBlackHole")))
            {
                c.TryGotoNext(MoveType.Before,
                x => x.MatchLdcR4(1f));
                c.Next.Operand = 1.5f;
                /*GameObject a = null;
                a.GetComponent<ProjectileExplosion>().blastRadius = 5;
                a.GetComponent<RadialForce>().radius = 5;
                a.transform.localScale *= 1.1f;*/
            }
            else
            {
                Debug.LogWarning("IL Failed: Singularity Band");
            }
        }
    }
}