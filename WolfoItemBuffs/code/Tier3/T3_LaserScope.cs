using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
 
namespace WolfoItemBuffs
{
    public class T3_LaserScope
    {
        public static int ScopeCrit = 20;

        public static void Start()
        {
            if (WConfig.cfg_Red_LaserScope.Value == WConfig.ConfigChoice.Rework)
            {
                RecalculateStatsAPI.GetStatCoefficients += LaserScope_Rework;
                IL.RoR2.CharacterBody.RecalculateStats += LaserScope_ReworkIL;

                //LanguageAPI.Add("ITEM_CRITDAMAGE_DESC", "Gain <style=cIsDamage>30% <style=cStack>(+50% per stack)</style> critical chance</style>. <style=cIsDamage>Critical Strikes</style> deal an additional <style=cIsDamage>1% damage</style> for every critical chance <style=cIsDamage>%</style> you have.");
                //LanguageAPI.Add("ITEM_CRITDAMAGE_DESC", "Gain <style=cIsDamage>25% <style=cStack>(+25% per stack)</style> critical chance</style>. <style=cIsDamage>Critical Strikes</style> deal an additional <style=cIsDamage>50% damage</style> plus <style=cIsDamage>1% damage</style> for every critical chance <style=cIsDamage>%</style> above 100% you have.");
                LanguageAPI.Add("ITEM_CRITDAMAGE_DESC", "Gain <style=cIsDamage>" + ScopeCrit * 2 + "% <style=cStack>(+" + ScopeCrit + "% per stack)</style> critical chance</style>. <style=cIsDamage>Critical Strikes</style> deal an additional <style=cIsDamage>2% damage</style> for every critical chance <style=cIsDamage>%</style> above 100%.");
            }
            else if (WConfig.cfg_Red_LaserScope.Value == WConfig.ConfigChoice.Buff)
            {
                RecalculateStatsAPI.GetStatCoefficients += Scope10Crit;
                LanguageAPI.Add("ITEM_CRITDAMAGE_DESC", "Gain <style=cIsDamage>15% critical chance</style>. <style=cIsDamage>Critical Strikes</style> deal an additional <style=cIsDamage>100% damage</style><style=cStack>(+100% per stack)</style>.");

            }
        }

        private static void Scope10Crit(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int critDamage = sender.inventory.GetItemCount(DLC1Content.Items.CritDamage);
                if (critDamage > 0)
                {
                    args.critAdd += 15;
                }
            }
        }
        private static void LaserScope_ReworkIL(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
            x => x.MatchCall("RoR2.CharacterBody", "set_critMultiplier")))
            {
                c.TryGotoPrev(MoveType.Before,
                x => x.MatchLdcR4(1f));
                c.Next.Operand = 0f;
            }
            else
            {
                Debug.LogWarning("IL FAILED : LaserScope_ReworkIL");
            }
        }
        private static void LaserScope_Rework(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            bool flag = sender.inventory != null;
            if (flag)
            {
                int laserScope = sender.inventory.GetItemCount(DLC1Content.Items.CritDamage);
                bool isRailer = sender.inventory.GetItemCount(DLC1Content.Items.ConvertCritChanceToCritDamage) > 0;
                if (laserScope > 0)
                {
                    args.critAdd += ScopeCrit * (laserScope + 1);
                    if (sender.crit > 100)
                    {
                        args.critDamageMultAdd += (sender.crit - 100) * 0.02f;
                    }
                    else
                    {
                        args.critDamageMultAdd += 0.001f;
                    }
                }
            }
        }


    }
}