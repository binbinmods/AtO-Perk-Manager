using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using static Obeliskial_Essentials.Essentials;
using System;
using UnityEngine;
using static PerkManager.Plugin;
using static PerkManager.CustomFunctions;
using TMPro.Examples;
using System.Collections.Generic;

namespace PerkManager{
    [HarmonyPatch]
    public class PerkUnused
    {
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(Character),"DamageBonus")]
        // public static bool DamageBonusPrefix(ref Character __instance, ref float[] __result, Enums.DamageType DT, int energyCost = 0)
        // {
        //     string str1 = "damageBonusFlat_" + Enum.GetName(typeof (Enums.DamageType), (object) DT);
        //     string str2 = "damageBonusPercent_" + Enum.GetName(typeof (Enums.DamageType), (object) DT);
        //     List<Aura> auraList = [];
        //     float[] numArray = new float[2];
            
        //     int num3 = 0;
        //     int num4 = 0;
        //     for (int index = 0; index < auraList.Count; ++index)
        //     {
        //     if (index < auraList.Count && auraList[index] != null)
        //     {
        //         AuraCurseData acData = auraList[index].ACData;
        //         if ((UnityEngine.Object) acData != (UnityEngine.Object) null)
        //         {
        //         int auraCharges1 = auraList[index].AuraCharges;
        //         if (acData.AuraDamageType == DT || acData.AuraDamageType == Enums.DamageType.All)
        //         {
        //             if ((UnityEngine.Object) acData.AuraDamageChargesBasedOnACCharges != (UnityEngine.Object) null)
        //             auraCharges1 = __instance.GetAuraCharges(acData.AuraDamageChargesBasedOnACCharges.Id);
        //             if (acData.AuraDamageIncreasedTotal != 0)
        //             num3 += acData.AuraDamageIncreasedTotal;
        //             num3 += Functions.FuncRoundToInt(acData.AuraDamageIncreasedPerStack * (float) auraCharges1);
        //             if (acData.AuraDamageIncreasedPercent != 0)
        //             num4 += acData.AuraDamageIncreasedPercent;
        //             if ((double) acData.AuraDamageIncreasedPercentPerStack != 0.0)
        //             {
        //             float num5 = acData.AuraDamageIncreasedPercentPerStack + acData.AuraDamageIncreasedPercentPerStackPerEnergy * (float) energyCost;
        //             num4 += Functions.FuncRoundToInt(num5 * (float) auraCharges1);
        //             }
        //         }
        //         int auraCharges2 = auraList[index].AuraCharges;
        //         if (acData.AuraDamageType2 == DT || acData.AuraDamageType2 == Enums.DamageType.All)
        //         {
        //             if (acData.AuraDamageIncreasedTotal2 != 0)
        //             num3 += acData.AuraDamageIncreasedTotal2;
        //             num3 += Functions.FuncRoundToInt(acData.AuraDamageIncreasedPerStack2 * (float) auraCharges2);
        //             if (acData.AuraDamageIncreasedPercent2 != 0)
        //             num4 += acData.AuraDamageIncreasedPercent2;
        //             if ((double) acData.AuraDamageIncreasedPercentPerStack2 != 0.0)
        //             {
        //             float num6 = acData.AuraDamageIncreasedPercentPerStack2 + acData.AuraDamageIncreasedPercentPerStackPerEnergy2 * (float) energyCost;
        //             num4 += Functions.FuncRoundToInt(num6 * (float) auraCharges2);
        //             }
        //         }
        //         if (acData.AuraDamageType3 == DT || acData.AuraDamageType3 == Enums.DamageType.All)
        //         {
        //             if (acData.AuraDamageIncreasedTotal3 != 0)
        //             num3 += acData.AuraDamageIncreasedTotal3;
        //             num3 += Functions.FuncRoundToInt(acData.AuraDamageIncreasedPerStack3 * (float) auraCharges2);
        //             if (acData.AuraDamageIncreasedPercent3 != 0)
        //             num4 += acData.AuraDamageIncreasedPercent3;
        //             if ((double) acData.AuraDamageIncreasedPercentPerStack3 != 0.0)
        //             {
        //             float num7 = acData.AuraDamageIncreasedPercentPerStack3 + acData.AuraDamageIncreasedPercentPerStackPerEnergy3 * (float) energyCost;
        //             num4 += Functions.FuncRoundToInt(num7 * (float) auraCharges2);
        //             }
        //         }
        //         if (acData.AuraDamageType4 == DT || acData.AuraDamageType4 == Enums.DamageType.All)
        //         {
        //             if (acData.AuraDamageIncreasedTotal4 != 0)
        //             num3 += acData.AuraDamageIncreasedTotal4;
        //             num3 += Functions.FuncRoundToInt(acData.AuraDamageIncreasedPerStack4 * (float) auraCharges2);
        //             if (acData.AuraDamageIncreasedPercent4 != 0)
        //             num4 += acData.AuraDamageIncreasedPercent4;
        //             if ((double) acData.AuraDamageIncreasedPercentPerStack4 != 0.0)
        //             {
        //             float num8 = acData.AuraDamageIncreasedPercentPerStack4 + acData.AuraDamageIncreasedPercentPerStackPerEnergy4 * (float) energyCost;
        //             num4 += Functions.FuncRoundToInt(num8 * (float) auraCharges2);
        //             }
        //         }
        //         }
        //     }
        //     }
        //     if (TeamHasPerkForConsume(perkBase+"weak1b",true,AtOManager.Instance,__instance)){
        //         if (num4 < -70)
        //             num4 = -70;
        //     }
        //     else{
        //         if (num4 < -50)
        //             num4 = -50;
        //     }
        //     numArray[0] = (float) num3;
        //     numArray[1] = (float) num4;
        //     __result= numArray;
        //     return false;
        // }
    }
}