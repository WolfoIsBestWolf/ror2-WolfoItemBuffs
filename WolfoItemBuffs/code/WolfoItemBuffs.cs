using BepInEx;
using R2API;
using RoR2;

namespace WolfoItemBuffs
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin("com.Wolfo.WolfoItemBuffs", "WolfoItemBuffs", "1.0.8")]
    public class WolfoItemBuffs : BaseUnityPlugin
    {
        public void Awake()
        {
            WConfig.InitConfig();
            T1_White.Start();
            T2_Green.Start();
            T3_Red.Start();
            T_Boss.Start();
            T_Lunar.Start();
            T_Void.Start();
            Equipment.Start();
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