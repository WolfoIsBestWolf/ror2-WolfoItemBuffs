using BepInEx;
using R2API;
using RoR2;

namespace WolfoItemBuffs
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin("com.Wolfo.WolfoItemBuffs", "WolfoItemBuffs", "1.5.0")]
    public class Main : BaseUnityPlugin
    {
        public static bool mod_riskyMod = false;
        public static bool mod_flatItems = false;
        public void Awake()
        {
            WConfig.InitConfig();

            mod_riskyMod = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskyLives.RiskyMod");
            mod_flatItems = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.kking117.FlatItemBuff");

            T1_White.Start();
            T2_Green.Start();
            T3_Red.Start();
            T_Boss.Start();
            T_Lunar.Start();
            T_Void.Start();
            Equipment.Start();

            if (WConfig.cfg_Red_ICBM.Value)
            {
                T3_ICBM.Start();
            }
            if (WConfig.cfg_Red_LaserScope.Value != WConfig.ConfigChoice.Off)
            {
                T3_LaserScope.Start();
            }
            if(WConfig.cfg_Red_Nkuhana.Value)
            {
                T3_NkuhanasOpinion.Start();
            }
       

            if (WConfig.cfg_Green_Harpoon.Value)
            {
                T2_HuntersHarpoon.Start();
            }
            if (WConfig.cfg_Green_IgnitionTank.Value)
            {
                T2_IgnitionTank.Start();
            }
            if (WConfig.cfg_Green_RegenScrap.Value)
            {
                T2_RegenScrap.Start();
            }
            T2_Predatory.Start();
            T2_Lepton.Start();


            
            if (WConfig.cfg_White_Warbanner.Value)
            {
                T1_Warbanner.Start();
            }
            //T1_APRounds.Start();

            GameModeCatalog.availability.CallWhenAvailable(CallLate);
        }

        public static void CallLate()
        {
            //Is language loaded here?
            if (WConfig.cfg_Red_ICBM.Value)
            {
                DLC1Content.Items.MoreMissile.tags = new ItemTag[]
                {
                    ItemTag.Damage
                };
            }
            

            if (WConfig.cfg_Orange_Molotov.Value)
            {
                string molotov = "";
                molotov = Language.GetString("EQUIPMENT_MOLOTOV_NAME");
                molotov = molotov.Replace("6", "8");
                LanguageAPI.Add("EQUIPMENT_MOLOTOV_NAME", molotov);
                molotov = Language.GetString("EQUIPMENT_MOLOTOV_PICKUP");
                molotov = molotov.Replace("6", "8");
                LanguageAPI.Add("EQUIPMENT_MOLOTOV_DESC", molotov);
                molotov = Language.GetString("EQUIPMENT_MOLOTOV_DESC");
                molotov = molotov.Replace("6", "8");
                LanguageAPI.Add("EQUIPMENT_MOLOTOV_DESC", molotov);
            }
           

            T2_RegenScrap.RegenScrapExtraItemTracker.IsBubbetInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bubbet.bubbetsitems");

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