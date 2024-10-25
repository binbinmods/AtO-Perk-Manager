using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using static Obeliskial_Essentials.Essentials;
using System;


namespace PerkManager{
    [HarmonyPatch]
    public class PerkPatches
    {
        public static string debugBase = "Binbin - Testing Perks - "
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkNodeAmplify),"SetForNodes")]
        public static bool SetForNodesPrefix(PerkNodeAmplify __instance, ref bool __result, int _numNodes){
            Plugin.Log.LogDebug(debugBase + "Extending Clickbox for Perks")
            PolygonCollider2D component = __instance.GetComponent<PolygonCollider2D>();
            float scaleFactor = -0.7f*(_numNodes-1);
            if (_numNodes == 2)
            {
                __instance.bg2.gameObject.SetActive(true);
                __instance.bg3.gameObject.SetActive(false);
                __instance.bg4.gameObject.SetActive(false);
                __instance.amplifyNodes.localPosition = new Vector3(scaleFactor, __instance.amplifyNodes.localPosition.y, __instance.amplifyNodes.localPosition.z);
                component.points = __instance.bg2.GetComponent<PolygonCollider2D>().points;
            }
            else if (_numNodes == 3)
            {
                __instance.bg2.gameObject.SetActive(false);
                __instance.bg3.gameObject.SetActive(true);
                __instance.bg4.gameObject.SetActive(false);
                __instance.amplifyNodes.localPosition = new Vector3(scaleFactor, __instance.amplifyNodes.localPosition.y, __instance.amplifyNodes.localPosition.z);
                component.points = __instance.bg3.GetComponent<PolygonCollider2D>().points;
            }
            else
            {
                __instance.bg2.gameObject.SetActive(false);
                __instance.bg3.gameObject.SetActive(false);
                __instance.bg4.gameObject.SetActive(true);
                __instance.amplifyNodes.localPosition = new Vector3(scaleFactor, __instance.amplifyNodes.localPosition.y, __instance.amplifyNodes.localPosition.z);
                float localWidthStretch = _numNodes/4.0f;
                __instance.bg4.transform.localScale = new Vector3(localWidthStretch,1.0f,1.0f);
                component.points = __instance.bg4.GetComponent<PolygonCollider2D>().points;
            }
            
            return false;
            
        }

    }
}