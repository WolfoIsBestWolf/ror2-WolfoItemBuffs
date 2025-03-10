using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WolfoCharacterBuffs
{
    public class WConfig
    {
        public static ConfigFile ConfigFileUNSORTED = new ConfigFile(Paths.ConfigPath + "\\Wolfo.Wolfo_Item_Buffs.cfg", true);

        public static ConfigEntry<bool> cfg_Acrid;
        public static ConfigEntry<bool> cfg_AcridBlight;
        public static ConfigEntry<bool> cfg_Huntress;
        public static ConfigEntry<bool> cfg_Mage;
        public static ConfigEntry<bool> cfg_w;
        public static ConfigEntry<bool> cfg_White_WarpedEcho;

   
 

        public static void InitConfig()
        {
            
            cfg_White_WarpedEcho = ConfigFileUNSORTED.Bind(
                "White",
                "Warped Echo Off",
                false,
                "Synergize with all on hurts. Removes BypassBlock, BypassArmor."
            );
             
            RiskConfig();
        }

        public static void RiskConfig()
        {
 
            ModSettingsManager.SetModIcon(Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/MoveSpeedOnKill/texGrappleHookIcon.png").WaitForCompletion());
            ModSettingsManager.SetModDescription("Various character buffs.");

            ModSettingsManager.AddOption(new ChoiceOption(cfg_Acrid, true));
          
        }
    }
}