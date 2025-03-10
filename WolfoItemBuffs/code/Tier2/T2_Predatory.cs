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
    public class T2_Predatory
    {

        public static void Start()
        {
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
        }

        private static void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
             
            orig(self, buffDef, duration);
            if (buffDef == RoR2Content.Buffs.AttackSpeedOnCrit)
            {
                RefreshBuffs(self, buffDef.buffIndex, duration);
            }
        }

        public static void RefreshBuffs(CharacterBody self, BuffIndex buff, float duration)
        {
            int num = 0;
            for (int i = 0; i < self.timedBuffs.Count; i++)
            {
                CharacterBody.TimedBuff timedBuff = self.timedBuffs[i];
                if (timedBuff.buffIndex == buff)
                {
                    num++;
                    if (timedBuff.timer < duration)
                    {
                        timedBuff.timer = duration;
                        timedBuff.totalDuration = duration;
                    }
                }
            }
        }
    }
}