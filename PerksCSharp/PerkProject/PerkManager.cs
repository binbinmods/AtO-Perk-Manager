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
            // TODO
            // currency6c"] = "Increases chance for Scarabs to spawn by 10%.";
            // shards5c"] = "Increases chance for cards to be corrupted by 2%.";

            // XP Perks
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            for (int i =0; i<teamHero.Length; i++)
            {
                Hero _hero = teamHero[i];
                HandleExpPerks(_hero);
            }
        }

        public static void HandleExpPerks(Hero _hero)
        {
            int expToGrant = 0;
            int expBase = 10;
            int expAdvanced = 40;
            if (CharacterObjectHavePerk(_hero,"exp0"))
                expToGrant+=expBase;
            if (CharacterObjectHavePerk(_hero,"exp1"))
                expToGrant+=expBase;
            if (CharacterObjectHavePerk(_hero,"exp2"))
                expToGrant+=expBase;
            if (CharacterObjectHavePerk(_hero,"exp3"))
                expToGrant+=expAdvanced;
            _hero.GrantExperience(expToGrant);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "HeroLevelUp")]
        public static void HeroLevelUpPrefix(ref AtOManager __instance, int heroIndex, string traitId)
        {  
            // TODO:
            // resistance5b"] = "-4% Resistances. Gain 4% to all Resistances on level up";
            // resistance5c"] = "+12% Resistances. Lose 4% to all Resistances on level up"

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
            if (CharacterObjectHavePerk(hero,"shards5b")){
                // medsTexts[perkStem + "shards5b"] = "Gain 125 shards on level up."; - NEEDS MULTIPLAYER
                int AmountToIncreasePerLevel = 125;
                AtOManager.Instance.GivePlayer(1,AmountToIncreasePerLevel, anim: true);
                
            }
            if (CharacterObjectHavePerk(hero,"currency6b")){
                // medsTexts[perkStem + "currency6b"] = "Gain 125 gold on level up."; - NEEDS MULTIPLAYER
                int AmountToIncreasePerLevel = 125;
                AtOManager.Instance.GivePlayer(0,AmountToIncreasePerLevel, anim: true);
                
            }

            HandleExpPerks(hero);
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
 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPrefix(ref Character __instance,     
                                            Enums.EventActivation theEvent,
                                            ref int auxInt,
                                            Character target = null,
                                            string auxString = "")

        {
            // Killed is "this character was killed" --> triggers things like resurrect
            // CharacterKilled is "a character was killed" --> triggers things like Yogger's Innate

            // __instance is the "source" character, target is the target


            // TODO:
            // poison2g: When a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
            // poison2h: -1 Poison. When this hero applies poison, deal Mind damage to the target equal to 30% of their Poison charges.";
            // bleed2g: When this hero kills an enemy with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";
            // bleed2e: When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
            // //block5d: Block only functions if you are above 50% Max Health [Currently not working].";
            // reinforce1d: Reinforce increases Block charges by 2 per charge of Reinforce.";
            // reinforce1d: .";
            // resistance5d"] = "Maximum resistances for heroes and monsters are now 97%.";
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();
            

            if (theEvent == Enums.EventActivation.Killed && __instance.IsHero && __instance != null && CharacterObjectHavePerk(__instance,"zeal1f") && __instance.HasEffect("zeal"))
            { 
                // zeal1f: If this hero dies with Zeal, deal indirect Mind damage to all enemies equal to 5x their Burn/Insane stacks.

                Log.LogDebug(debugBase+"zeal1f");
                int n_stacks = __instance.GetAuraCharges("burn");
                int damageToDeal = Functions.FuncRoundToInt(8*n_stacks);
                __instance.IndirectDamage(Enums.DamageType.Mind, damageToDeal);
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && !__instance.IsHero && __instance != null && AtOManager.Instance.TeamHavePerk(debugBase+"weak1b")&&__instance.HasEffect("weak"))
            { 
                // weak1b: "Weak on monsters reduces the application of Auras and Curses by 20%.";

                Log.LogDebug(debugBase+"weak1b");
                auxInt = Functions.FuncRoundToInt(0.8f*auxInt);
                
            }
            if (theEvent == Enums.EventActivation.BeginCombat && __instance.IsHero && __instance != null && CharacterObjectHavePerk(__instance,debugBase+"block5c"))
            { 
                // block5c: At start of combat, apply 2 Block to all heroes.";
                Log.LogDebug(debugBase+"block5c");
                bool allHeroes = true;
                bool allNpcs = false;
                ApplyAuraCurseTo("block",2,allHeroes,allNpcs,false,false,ref __instance,ref teamHero,ref teamNpc,"","");
            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && __instance.IsHero &&__instance != null && CharacterObjectHavePerk(target,debugBase+"block5c"))
            { 
                // block5e: When this hero gains Block, they deal 1 Blunt to themselves and a random monster.";
                Log.LogDebug(debugBase+"block5e");
                int damageToDeal = 1;
                Enums.DamageType damageType = Enums.DamageType.Blunt;
                int modifiedDamage = __instance.DamageWithCharacterBonus(damageToDeal,damageType,Enums.CardClass.None);
                Character targetCharacter = GetRandomCharacter(teamNpc);
                targetCharacter.IndirectDamage(damageType, modifiedDamage);
                __instance.IndirectDamage(damageType, modifiedDamage);
            }            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character),nameof(Character.BeginTurn))]
        public static void BeginTurnPostfix(ref Character __instance)
        {
            //shackle1d: At start of your turn, gain Fortify equal to your twice your Shackles
            // mitigate1a: At the start of your turn, gain 1 Mitigate, but only stack to 5.";
            // mitigate1c: At the start of your turn, gain 7 Block per Mitigate charge.";
            // "health6c"] = "At the start of your turn, if you are at max HP, gain 2 Vitality.";

            if (CharacterObjectHavePerk(__instance,"shackle1d"))
            {
                int n_charges = __instance.GetAuraCharges("shackle");
                __instance.SetAuraTrait(__instance,"fortify",n_charges);
            }
            if (CharacterObjectHavePerk(__instance,"mitigate1a"))
            {
                int n_charges = 1;
                __instance.SetAuraTrait(__instance,"mitigate",n_charges);
            }
            if (CharacterObjectHavePerk(__instance,"mitigate1c"))
            {
                int n_charges = 7*__instance.GetAuraCharges("mitigate");
                __instance.SetAuraTrait(__instance,"block",n_charges);
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

                        if(TeamHasPerkForSet("shackle1c",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.Removable = false;  
                        }
                        if(CharacterHasPerkForSet("shackle1e",SetAppliesToMonsters,__instance,_characterTarget))
                        {   
                            int n_charges = _characterTarget.GetAuraCharges("shackle");
                            _characterTarget.ModifyAuraCurseQuantity("dark",n_charges);
                        }
                        if(TeamHasPerkForSet("shackle1f",SetAppliesToHeroes,__instance,_characterTarget))
                        {   
                            int baseSpeed = _characterTarget.GetSpeed()[1];
                            int n_charges = _characterTarget.GetAuraCharges("shackle");
                            __result.IncreasedDamageReceivedType = Enums.DamageType.All;  
                            __result.IncreasedDirectDamageReceivedPerStack = baseSpeed/n_charges/2;
                        }
                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("shackle1c",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.Removable = false;
                        }
                        if(CharacterHasPerkForConsume("shackle1e",ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {   
                            int n_charges = _characterCaster.GetAuraCharges("shackle");
                            _characterCaster.ModifyAuraCurseQuantity("dark",n_charges);
                        }

                        if(TeamHasPerkForConsume("shackle1f",ConsumeAppliesToMonsters,__instance,_characterCaster))
                        {
                            int baseSpeed = _characterCaster.GetSpeed()[1];
                            int n_charges = _characterCaster.GetAuraCharges("shackle");
                            __result.IncreasedDamageReceivedType = Enums.DamageType.All;  
                            __result.IncreasedDirectDamageReceivedPerStack = baseSpeed/n_charges;
                        }
                    }
                    break;

                 case "mitigate":
                    if (_type=="set")
                    {
                    // mitigate1a: At the start of your turn, gain 1 Mitigate, but only stack to 5.";
                    // mitigate1b: Mitigate on this hero does not lose charges at start of turn and stacks to 12.";
                    // mitigate1c: At the start of your turn, gain 7 Block per Mitigate charge.";
                    // mitigate1d: Mitigate reduces incoming damage by 2 per charge, but loses all charges at the start of your turn.";
                    // mitigate1e: Mitigate on heroes and monsters increases damage done by 10% per charge.";
                        if(CharacterHasPerkForSet("mitigate1a",SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            __result.MaxCharges = 5;
                        }
                        if(CharacterHasPerkForSet("mitigate1b",SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            __result.ConsumedAtTurnBegin = false;
                            __result.MaxCharges = 12;
                        }
                        if(CharacterHasPerkForSet("mitigate1d",SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            __result.IncreasedDirectDamageReceivedPerStack = -2;
                            __result.ConsumeAll=true;
                        }
                        if(TeamHasPerkForSet("mitigate1e",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.AuraDamageType=Enums.DamageType.All;
                            __result.AuraDamageIncreasedPercentPerStack = 10;
                        }
                    }
                    if (_type == "consume")
                    {
                        if(CharacterHasPerkForConsume("mitigate1a",ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            __result.MaxCharges = 5;
                        }
                        
                        if(CharacterHasPerkForConsume("mitigate1b",ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            __result.ConsumedAtTurnBegin = false;
                            __result.MaxCharges = 12;
                        }
                        if(CharacterHasPerkForConsume("mitigate1d",ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            __result.IncreasedDirectDamageReceivedPerStack = -2;
                            __result.ConsumeAll=true;
                        }
                        if(TeamHasPerkForConsume("mitigate1e",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.AuraDamageType=Enums.DamageType.All;
                            __result.AuraDamageIncreasedPercentPerStack = 10;
                        }
                    }
                    break;

                case "poison":
                    if (_type=="set")
                    {
                    // poison2d: If Restricted Power is enabled, increases Max Charges to 300.";
                    // poison2e: Poison on heroes and monsters reduces Slashing resistance by 0.25% per charge.";
                    // poison2f: Poison on monsters reduces all resistances by 5% for every 100 charges.";
                    // poison2g: When a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
                    // poison2h: -1 Poison. When this hero applies poison, deal Mind damage to the target equal to 30% of their Poison charges.";

                        if(TeamHasPerkForSet("poison2d",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if(TeamHasPerkForSet("poison2e",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.ResistModified2 = Enums.DamageType.Slashing;
                            __result.ResistModifiedPercentagePerStack = 0.25f;
                        }
                        if(TeamHasPerkForSet("poison2f",SetAppliesToMonsters,__instance,_characterTarget))
                        {
                            __result.ResistModified3 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack2 = 5f;
                            __result.ChargesAuxNeedForOne2 = 100;
                        }

                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("poison2d",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if(TeamHasPerkForConsume("poison2e",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.ResistModified3 = Enums.DamageType.Slashing;
                            __result.ResistModifiedPercentagePerStack3 = 0.25f;
                        }
                        if(TeamHasPerkForConsume("poison2f",ConsumeAppliesToMonsters,__instance,_characterCaster))
                        {
                            __result.ResistModified3 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack2 = 5f;
                            __result.ChargesAuxNeedForOne2 = 100;
                        }

                    }
                    break;

                case "bleed":
                    // bleed2d: If Restricted Power is enabled, increases Max Charges to 300.";
                    // bleed2e: When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
                    // bleed2f: Bleed on monsters reduces Piercing resist by 0.25% per charge.";
                    // bleed2g: When this hero kills an enemy with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";

                    if (_type=="set")
                    {
                        if(TeamHasPerkForSet("bleed2d",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if(TeamHasPerkForSet("bleed2f",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.ResistModified3 = Enums.DamageType.Piercing;
                            __result.ResistModifiedPercentagePerStack3 = 0.25f;
                        }
                        
                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("bleed2d",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if(TeamHasPerkForConsume("bleed2f",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.ResistModified3 = Enums.DamageType.Slashing;
                            __result.ResistModifiedPercentagePerStack3 = 0.25f;
                        }

                    }
                    break;
                
                case "thorns":
                    // thorns1d: Cannot be purged unless specified.";
                    // thorns1e: When a monster with Thorns dies, transfer their Thorns charges to a random hero.";

                    if (_type=="set")
                    {
                        if(TeamHasPerkForSet("thorns1d",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.Removable=false;
                        }
                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.Removable=false;
                        }
                    }
                    break;

                case "reinforce":
                    // reinforce1d: Reinforce increases Block charges by 2 per charge of Reinforce.";
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

                case "block":
                    // block5b: If Restricted Power is enabled, increases Max Charges to 600.";
                    // block5c: At start of combat, apply 2 Block to all heroes.";
                    // //block5d: Block only functions if you are above 50% Max Health [Currently not working].";
                    // block5e: When this hero gains Block, they deal 1 Blunt to themselves and a random monster.";
                    if (_type=="set")
                    {
                        if(TeamHasPerkForSet("block5b",AppliesGlobally,__instance,_characterTarget))
                        {
                            __result.MaxMadnessCharges = 600;
                        }
                    }
                    if (_type == "consume")
                    {
                        if(TeamHasPerkForConsume("block5b",AppliesGlobally,__instance,_characterCaster))
                        {
                            __result.MaxMadnessCharges = 600;
                        }
                    }
                    break;

                case "taunt":
                    // taunt1e: Taunt on this hero can stack and increases damage by 1 per charge.";
                    if (_type=="set")
                    {
                        if(CharacterHasPerkForSet("taunt1e",SetAppliesToHeroes,__instance,_characterTarget))
                        {
                            __result.GainCharges=true;
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPerStack = 1;
                        }
                    }
                    if (_type == "consume")
                    {
                        if(CharacterHasPerkForConsume("taunt1e",ConsumeAppliesToHeroes,__instance,_characterCaster))
                        {
                            __result.GainCharges=true;
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPerStack = 1;
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