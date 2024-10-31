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
using System.Reflection;
using System.Text.RegularExpressions;

namespace PerkManager{
    [HarmonyPatch]
    public class PerkPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.BeginAdventure))]
        public static void BeginAdventurePostfix()
        {
            
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPrefix(ref Character __instance,     
                                            Enums.EventActivation theEvent,
                                            ref int auxInt,
                                            Character target = null,
                                            string auxString = "")

        {
            // __instance is the "source" character, target is the target

            // zeal1f: If this hero dies with Zeal, deal indirect Mind damage to all enemies equal to 5x their Burn/Insane stacks.
            if (theEvent == Enums.EventActivation.Killed && __instance.IsHero && __instance != null && CharacterObjectHavePerk(__instance,"zeal1f") && __instance.HasEffect("zeal"))
            { 
                Log.LogDebug(debugBase+"zeal1f");
                int n_stacks = __instance.GetAuraCharges("burn");
                int damageToDeal = Functions.FuncRoundToInt(8*n_stacks);
                __instance.IndirectDamage(Enums.DamageType.Mind, damageToDeal);
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && !__instance.IsHero && __instance != null && AtOManager.Instance.TeamHavePerk(debugBase+"weak1b")&&__instance.HasEffect("weak"))
            { 
                Log.LogDebug(debugBase+"weak1b");
                auxInt = Functions.FuncRoundToInt(0.8f*auxInt);
                
            }
            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character),nameof(Character.EndTurn))]
        public static void EndTurnPrefix(Character __instance)
        {
            if (!__instance.Alive || __instance == null)
                return;
            // zeal1e: When this hero loses Zeal, deal indirect Holy and Fire damage equal to 4x the number of stacks lost to all monsters.
            if (TeamHasPerkForConsume("zeal1e",__instance.IsHero,AtOManager.Instance,__instance))
            {
                Log.LogDebug(debugBase+"zeal1e");
                int zealCharges = __instance.GetAuraCharges("zeal");
                AuraCurseData zealData = GetAuraCurseData("zeal");
                int chargesRemoved;
                if (zealData.ConsumeAll)
                {
                    chargesRemoved = zealCharges;
                }
                else
                {
                    chargesRemoved = zealData.AuraConsumed;
                }
                int damageToDeal = 4*chargesRemoved;
                
                DealIndirectDamageToAllMonsters(Enums.DamageType.Holy,damageToDeal);
                DealIndirectDamageToAllMonsters(Enums.DamageType.Fire,damageToDeal);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "HeroLevelUp")]
        public static void HeroLevelUpPrefix(ref AtOManager __instance, int heroIndex, string traitId)
        {  
            Hero[] teamAtO = MatchManager.Instance.GetTeamHero();
            Hero hero = teamAtO[heroIndex];
            if (CharacterObjectHavePerk(hero,"health6b")){
                int AmountToIncreasePerLevel = 12;
                teamAtO[heroIndex].ModifyMaxHP(AmountToIncreasePerLevel);
            }
            if (CharacterObjectHavePerk(hero,"health6c")){
                int AmountToIncreasePerLevel = -14;
                teamAtO[heroIndex].ModifyMaxHP(AmountToIncreasePerLevel);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager),"GenerateHeroes")]
        public static void GenerateHeroesPostfix(ref MatchManager __instance)
        {
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            for (int index = 0; index < 4; ++index){
                if (teamHero[index]==null)
                    continue;

                Hero _hero =teamHero[index];

                if (CharacterObjectHavePerk(_hero,"weak1d"))
                    AddImmunityToHero("weak",ref _hero);
                if (CharacterObjectHavePerk(_hero,"disarm1a"))
                    AddImmunityToHero("disarm",ref _hero);
                if (CharacterObjectHavePerk(_hero,"silence1a"))
                    AddImmunityToHero("silence",ref _hero);
                if (CharacterObjectHavePerk(_hero,"shackle1b"))
                    AddImmunityToHero("shackle",ref _hero);

                // if (AtOManager.Instance.CharacterHavePerk(_hero.SubclassName, perkBase+"weak1d") && !_hero.AuracurseImmune.Contains("weak"))
                //     _hero.AuracurseImmune.Add("weak");
                // if (AtOManager.Instance.CharacterHavePerk(_hero.SubclassName, perkBase+"disarm1a") && !_hero.AuracurseImmune.Contains("disarm"))
                //     _hero.AuracurseImmune.Add("disarm");
                // if (AtOManager.Instance.CharacterHavePerk(_hero.SubclassName, perkBase+"silence1a") && !_hero.AuracurseImmune.Contains("silence"))
                //     _hero.AuracurseImmune.Add("silence");
                // if (AtOManager.Instance.CharacterHavePerk(_hero.SubclassName, perkBase+"shackle1b") && !_hero.AuracurseImmune.Contains("shackle"))
                //     _hero.AuracurseImmune.Add("shackle");
            }
            Traverse.Create(MatchManager.Instance).Field("teamHero").SetValue(teamHero);
            
        }

        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager),"GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget){

            bool ConsumeAppliesToHeroes = false; //flag1
            bool SetAppliesToHeroes = false; //flag2
            bool ConsumeAppliesToMonsters = false;
            bool SetAppliesToMonsters = false;
            bool AppliesGlobally = true;

            if (_characterCaster != null)
            {
                ConsumeAppliesToHeroes = _characterCaster.IsHero;
                ConsumeAppliesToMonsters = !_characterCaster.IsHero;
            }
            if (_characterTarget != null )
            {
                SetAppliesToHeroes = _characterTarget.IsHero;
                SetAppliesToMonsters = !_characterTarget.IsHero;
            }

            switch (_acId)
            {   
                case "burn":
                // scourge1e: Scourge on monsters increases burn damage by 15%/stack";

                    if (_type=="set")
                    {
                        if(TeamHasPerkForSet("scourge1e",SetAppliesToMonsters,__instance,_characterTarget)&&_characterTarget.HasEffect("scourge"))
                        {
                            int scourge_charges = _characterTarget.GetAuraCharges("scourge");
                            float multiplier = 0.15f * scourge_charges + 1;
                            __result.DamageWhenConsumedPerCharge *= multiplier;
                        }

                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("scourge1e",ConsumeAppliesToMonsters,__instance,_characterCaster)&&_characterCaster.HasEffect("scourge"))
                        {
                            int scourge_charges = _characterCaster.GetAuraCharges("scourge");
                            float multiplier = 1+0.15f * scourge_charges;
                            __result.DamageWhenConsumedPerCharge *= multiplier;
                        }
                    }
                    break;
                
                case "insane":
                // weak1c: Monsters cannot be immune to Weak, but no longer have their damage reduced by Insane.";

                    if (_type=="set")
                    {
                        if(TeamHasPerkForSet("weak1c",SetAppliesToMonsters,__instance,_characterTarget))
                        {
                            // __result.AuraDamageIncreasedPercent = 0;
                            // __result.AuraDamageType = Enums.DamageType.None;
                        }

                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("weak1c",SetAppliesToMonsters,__instance,_characterCaster))
                        {
                            // __result.AuraDamageIncreasedPercent = 0;
                            // __result.AuraDamageType = Enums.DamageType.None;
                        }

                    }
                    break;

                case "wet":
                // zeald: Zeal on heroes and enemies increases all resistances by 0.5% per Wet charge
                    if(_type=="set"){
                        if (CharacterHasPerkForSet("zeal1d",SetAppliesToHeroes,__instance,_characterTarget)&&_characterTarget.HasEffect("zeal"))
                        {
                            Log.LogDebug(debugBase+"zeal1d");
                            __result.ResistModified3 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack3 = 0.5f;
                        }
                    }
                    if(_type=="consume"){
                        if (CharacterHasPerkForConsume("zeal1d",ConsumeAppliesToHeroes,__instance,_characterCaster)&&_characterCaster.HasEffect("zeal"))
                        {
                            //Log.LogDebug(debugBase+"zeal1d");
                            __result.ResistModified3 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack3 = 0.5f;
                        }
                    }
                    break;

                case "zeal":
                // zealb: Zeal on this hero loses 3 charges per turn rather than all charges
                // zealc: Zeal on all heroes can stack, but reduces Speed by 2 per charge
                // zeald: Zeal on heroes and enemies increases all resistances by 0.5% per Wet charge
                // zeale: When this hero loses Zeal, deal indirect Holy and Fire damage equal to 4x the number of stacks lost to all monsters.
                // zealf: If this hero dies with Zeal, deal indirect Mind damage to all enemies equal to 5x their Burn/Insane stacks.
                    if(_type=="set"){
                        if (CharacterHasPerkForSet("zeal1b",SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            Log.LogDebug(debugBase+"zeal1b");
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;

                        }

                        if (TeamHasPerkForSet("zeal1c",SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            Log.LogDebug(debugBase+"zeal1c");
                            __result.GainCharges = true;
                            __result.CharacterStatModified = Enums.CharacterStat.Speed;
                            __result.CharacterStatModifiedValuePerStack = -2;

                        }

                    }
                    if(_type=="consume"){
                        if (CharacterHasPerkForConsume("zeal1b",ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            Log.LogDebug(debugBase+"zeal1b");
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;

                        }
                        if (TeamHasPerkForConsume("zeal1c",ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            Log.LogDebug(debugBase+"zeal1c");
                            __result.GainCharges = true;
                            __result.CharacterStatModified = Enums.CharacterStat.Speed;
                            __result.CharacterStatModifiedValuePerStack = -2;

                        }
                        
                    }
                    break;
            
                case "scourge":
                // scourge1a: Scourge +1.";
                // scourge1b: Scourge on heroes and monsters loses 3 charges per turn rather than all charges.";
                // scourge1c: Scourge on monsters can Stack but increases all resists by 3% per stack.";
                // scourge1d: Scourge deals damage based on Sight rather than Chill.";
                // scourge1e: Scourge on monsters increases burn damage by 15%/stack";
                    if (_type=="set")
                    {
                        if(TeamHasPerkForSet("scourge1b",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;
                        }
                        if(TeamHasPerkForSet("scourge1c",SetAppliesToMonsters,__instance,_characterTarget))
                        {
                            __result.GainCharges = true;
                            __result.ResistModified2 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack2 = 3.0f;
                        }
                        if(TeamHasPerkForSet("scourge1d",AppliesGlobally,__instance,_characterTarget))
                        {
                            // __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                            __result.ConsumedDamageChargesBasedOnACCharges= GetAuraCurseData("Sight");
                            __result.DamageWhenConsumedPerCharge = 2;
                        }
                        


                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("scourge1b",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;
                        }

                        if(TeamHasPerkForConsume("scourge1c",ConsumeAppliesToMonsters,__instance,_characterCaster))
                        {
                            __result.GainCharges = true;
                            __result.ResistModified2 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack2 = 3.0f;
                        }
                        if(TeamHasPerkForConsume("scourge1d",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.ConsumedDamageChargesBasedOnACCharges= GetAuraCurseData("burn");
                        }

                    }
                    break;

                case "weak":
                // weak1a: Weak +1.";
                // weak1b: Weak on heroes and monsters can stack but reduces healing and damage by 20% per stack.";
                // weak1c: Monsters cannot be immune to Weak, but no longer have their damage reduced by Insane.";
                // weak1d: This hero is immune to Weak.";

                    if (_type=="set")
                    {
                        // if(TeamHasPerkForSet("weak1b",AppliesGlobally,__instance,_characterTarget))
                        // {
                        //     __result.GainCharges = true;
                        //     __result.HealDonePercent = 0;
                        //     __result.HealDonePercentPerStack = -20;
                        //     __result.AuraDamageIncreasedPercent = 0;
                        //     __result.AuraDamageIncreasedPercentPerStack = -20;
                        // }
                        if(TeamHasPerkForSet("weak1c",SetAppliesToMonsters,__instance,_characterTarget))
                        {
                            __result.Preventable = false;
                            __result.AuraDamageIncreasedPercent=-25;
                        }

                        
                        
                    }
                    if (_type == "consume")
                    {
                        // if(TeamHasPerkForConsume("weak1b",AppliesGlobally,__instance,_characterCaster))
                        // {
                        //     __result.GainCharges = true;
                        //     __result.HealDonePercent = 0;
                        //     __result.HealDonePercentPerStack = -20;
                        //     __result.AuraDamageIncreasedPercent = 0;
                        //     __result.AuraDamageIncreasedPercentPerStack = -20;
                        // }
                        if(TeamHasPerkForSet("weak1c",ConsumeAppliesToMonsters,__instance,_characterCaster))
                        {
                            __result.Preventable = false;
                            __result.AuraDamageIncreasedPercent=-25;                            
                        }
                        

                    }
                    break;

                case "disarm":
                    if (_type=="set")
                    { //disarm1b - cannot be dispelled unless specified, increases resists by 10%
                        if (CharacterHasPerkForSet("disarm1b", SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            __result.Removable = false;
                            __result.ResistModified = Enums.DamageType.All;
                            __result.ResistModifiedValue = 10;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("disarm1b", ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            __result.ResistModified = Enums.DamageType.All;
                            __result.ResistModifiedValue = 10;
                        }
                    }
                    break;
                case "silence":
                    if (_type=="set")
                    { //silence1b - cannot be dispelled unless specified, increases damage by 7
                        if (CharacterHasPerkForSet("silence1b", SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            __result.Removable = false;
                            __result.AuraDamageType=Enums.DamageType.All;
                            __result.AuraDamageIncreasedTotal = 7;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("silence1b", ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            __result.AuraDamageType=Enums.DamageType.All;
                            __result.AuraDamageIncreasedTotal = 7;
                        }
                    }
                    break;


                case "shackle":
                    if (_type=="set")
                    {
                    // shackle1b: This hero is immune to Shackle.";
                    // shackle1c: Shackle cannot be prevented.";
                    // shackle1d: At start of your turn, gain Fortify equal to your twice your Shackles.";
                    // shackle1e: Shackle increases Dark charges you apply by 1 per charge of Shackle.";
                    // shackle1f: Shackles on monsters increases all damage received by 1 per base Speed.";

                        if(TeamHasPerkForSet("",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.Removable = false;
                            
                        }
                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.Removable = false;

                        }
                    }
                    break;

                case "ac":
                    if (_type=="set")
                    {
                        if(CharacterHasPerkForSet("",AppliesGlobally,__instance,_characterTarget))
                        {

                        }
                    }
                    if (_type == "consume")
                    {
                        if(CharacterHasPerkForConsume("",AppliesGlobally,__instance,_characterCaster))
                        {

                        }
                    }
                    break;
            }
            
        }

    }
}