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

namespace PerkManager
{
    [HarmonyPatch]
    public class PerkRewrites
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Functions), nameof(Functions.GetCardByRarity))]
        public static bool GetCardByRarityPrefix(ref string __result, int rarity, CardData _cardData, bool isChallenge = false)
        {
            int num1 = 78;
            int num2 = 88;
            int num3 = 98;
            int num4;
            if (!GameManager.Instance.IsObeliskChallenge())
            {
                num4 = AtOManager.Instance.GetMadnessDifficulty();
                if (num4 > 0)
                {
                    num1 = 75;
                    num2 = 85;
                    num3 = 95;

                }
            }
            else
                num4 = AtOManager.Instance.GetObeliskMadness();

            Plugin.Log.LogDebug(debugBase + "Testing Perk shards5c: Original Corruption odds" + num3);

            if (AtOManager.Instance.TeamHavePerk(perkBase + "shards5c"))
            {
                int amountModified = 2;
                num1 -= amountModified;
                num2 -= amountModified;
                num3 -= amountModified;
                Plugin.Log.LogDebug(debugBase + "Testing Perk shards5c: Modified value " + num3);
            }
            if (num4 > 0)
            {
                num1 -= num4;
                num2 -= Functions.FuncRoundToInt((float)num4 * 0.5f);
            }
            if (rarity < num1)
            {
                __result = _cardData.CardUpgraded == Enums.CardUpgraded.No || !(_cardData.UpgradedFrom != "") ? _cardData.Id.ToLower() : _cardData.UpgradedFrom.ToLower();
                return false;
            }
            if (rarity >= num1 && rarity < num2)
            {
                if (_cardData.CardUpgraded == Enums.CardUpgraded.A)
                {
                    __result = _cardData.Id.ToLower();
                    return false;

                }
                if (_cardData.CardUpgraded == Enums.CardUpgraded.No)
                {
                    __result = _cardData.UpgradesTo1.ToLower();
                    return false;

                }
                if (_cardData.CardUpgraded == Enums.CardUpgraded.B)
                {
                    __result = (_cardData.UpgradedFrom + "A").ToLower();
                    return false;
                }
            }
            else
            {
                if (rarity >= num2 && rarity < num3)
                {
                    if (_cardData.CardUpgraded == Enums.CardUpgraded.B)
                    {
                        __result = _cardData.Id.ToLower();
                        return false;
                    }
                    __result = _cardData.CardUpgraded == Enums.CardUpgraded.No ? _cardData.UpgradesTo2.ToLower() : (_cardData.UpgradedFrom + "B").ToLower();
                    return false;
                }
                if (_cardData.CardUpgraded == Enums.CardUpgraded.No)
                {
                    __result = (UnityEngine.Object)_cardData.UpgradesToRare != (UnityEngine.Object)null ? _cardData.UpgradesToRare.Id.ToLower() : _cardData.Id.ToLower();
                    return false;
                }
                CardData cardData = Globals.Instance.GetCardData(_cardData.UpgradedFrom, false);
                if ((UnityEngine.Object)cardData != (UnityEngine.Object)null && (UnityEngine.Object)cardData.UpgradesToRare != (UnityEngine.Object)null)
                {
                    __result = cardData.UpgradesToRare.Id.ToLower();
                    return false;
                }
            }
            __result = _cardData.Id.ToLower();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.BonusResists))]
        public static bool BonusResistsPrefix(
                    ref Character __instance,
                    ref int __result,
                    Enums.DamageType damageType,
                    string acId = "",
                    bool countChargesConsumedPre = false,
                    bool countChargesConsumedPost = false)
        {
            int num = 0;
            switch (damageType)
            {
                case Enums.DamageType.Slashing:
                    if (__instance.IsImmune(Enums.DamageType.Slashing))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistSlashing").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("reinforcedmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Blunt:
                    if (__instance.IsImmune(Enums.DamageType.Blunt))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistBlunt").GetValue<int>();
                    
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("reinforcedmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Piercing:
                    if (__instance.IsImmune(Enums.DamageType.Piercing))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistPiercing").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("reinforcedmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Fire:
                    if (__instance.IsImmune(Enums.DamageType.Fire))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistFire").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("elementalmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Cold:
                    if (__instance.IsImmune(Enums.DamageType.Cold))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistCold").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("elementalmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Lightning:
                    if (__instance.IsImmune(Enums.DamageType.Lightning))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistLightning").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("elementalmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Mind:
                    if (__instance.IsImmune(Enums.DamageType.Mind))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistMind").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("spiritualmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Holy:
                    if (__instance.IsImmune(Enums.DamageType.Holy))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistHoly").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("spiritualmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
                case Enums.DamageType.Shadow:
                    if (__instance.IsImmune(Enums.DamageType.Shadow))
                    {
                        num = 1000;
                        break;
                    }
                    num += Traverse.Create(__instance).Field("resistShadow").GetValue<int>();
                    if (!__instance.IsHero && AtOManager.Instance.IsChallengeTraitActive("spiritualmonsters"))
                    {
                        num += 10;
                        break;
                    }
                    break;
            }
            if (num < 1000)
            {
                if (!__instance.IsHero)
                {
                    if (MadnessManager.Instance.IsMadnessTraitActive("resistantmonsters") || AtOManager.Instance.IsChallengeTraitActive("resistantmonsters"))
                        num += 10;
                    if (AtOManager.Instance.IsChallengeTraitActive("vulnerablemonsters"))
                        num -= 15;
                }
                num = num + __instance.GetItemResistModifiers(damageType) + __instance.GetAuraResistModifiers(damageType, acId, countChargesConsumedPre, countChargesConsumedPost);
            }
            if (AtOManager.Instance.TeamHavePerk(perkBase+"resistance5d"))
                Plugin.Log.LogDebug(debugBase+"Attemting to change max resists");
            int maxResist = AtOManager.Instance.TeamHavePerk(perkBase+"resistance5d")? 97 : 95;
            __result=Mathf.Clamp(num, -95, maxResist);            
            return false;
        }
    }
}