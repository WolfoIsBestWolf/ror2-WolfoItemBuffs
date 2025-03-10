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
    public class T2_HuntersHarpoon
    {
        public static float HarpoonSpeed = 1.00f;
        public static void Start()
        {
            HarpoonSpeed = WConfig.cfg_Green_Harpoon_VAL.Value / 100;
            bool otherHarpoon = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Wolfo.RoRRHuntersHarpoon");

            Addressables.LoadAssetAsync<BuffDef>(key: "RoR2/DLC1/MoveSpeedOnKill/bdKillMoveSpeed.asset").WaitForCompletion().canStack = false;


            if (!otherHarpoon && WConfig.cfg_Green_Harpoon.Value)
            {
                IL.RoR2.CharacterBody.RecalculateStats += Harpoon_ChangeMoveSpeed;
                IL.RoR2.GlobalEventManager.OnCharacterDeath += Harpoon_ChangeDuration;

                Addressables.LoadAssetAsync<BuffDef>(key: "RoR2/DLC1/MoveSpeedOnKill/bdKillMoveSpeed.asset").WaitForCompletion().canStack = false;
                LanguageAPI.Add("ITEM_MOVESPEEDONKILL_DESC", "Killing an enemy increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>" + WConfig.cfg_Green_Harpoon_VAL.Value + "%</style> for <style=cIsUtility>1.25</style> <style=cStack>(+1 per stack)</style> seconds. Consecutive kills increase buff duration for up to <style=cIsUtility>20</style> <style=cStack>(+4 per stack)</style> seconds.", "en");
            }
        }

        public static void Harpoon_ChangeMoveSpeed(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdcR4(0.25f),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdsfld("RoR2.DLC1Content/Buffs", "KillMoveSpeed")))
            {
                c.Next.Operand = HarpoonSpeed;
                c.Index += 4;
                c.EmitDelegate<Func<int, int>>((num) =>
                {
                    if (num > 0)
                    {
                        return 1;
                    }
                    return 0;
                });
            }
            else
            {
                Debug.LogWarning("IL FAILED : Harpoon_ChangeMoveSpeed");
            }
        }

        private static void Harpoon_ChangeDuration(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdcR4(1f),
                    x => x.MatchLdloc(out _),
                    x => x.MatchConvR4(),
                    x => x.MatchLdcR4(0.5f)))
            {
                //Need to solve it getting cleared

                c.Index -= 2; //980 
                c.Next.OpCode = OpCodes.Ldc_I4_0; //Don't apply normal buff
                c.Index += 9; //990;   
                c.RemoveRange(3);  //Removes the 3 lines that Clear the buff, couldn't figure out how to null the buff
                //c.Next = null; //Clear null buffs 
                c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt("RoR2.CharacterBody", "get_corePosition"));

                c.EmitDelegate<Func<CharacterBody, CharacterBody>>((attackerBody) =>
                {
                    int countHarpoon = attackerBody.master.inventory.GetItemCount(DLC1Content.Items.MoveSpeedOnKill);
                    float newDuration = countHarpoon * 1+0.25f;
                    if (attackerBody.HasBuff(DLC1Content.Buffs.KillMoveSpeed))
                    {
                        float maxDuration = countHarpoon * 4 + 16;
                        attackerBody.ExtendTimedBuffIfPresent(DLC1Content.Buffs.KillMoveSpeed, newDuration, maxDuration);
                    }
                    else
                    {
                        attackerBody.AddTimedBuff(DLC1Content.Buffs.KillMoveSpeed, newDuration);
                    }
                    return attackerBody;
                });
            }
            else
            {
                Debug.LogWarning("IL FAILED : Harpoon_ChangeDuration");
            }
        }


    }
}