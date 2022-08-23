using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ComfyQuickSlots.Patches {
    [HarmonyPatch(typeof(FejdStartup))]
    public class FejdStartupPatcher {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FejdStartup), "OnCharacterLeft")]
        public static void FejdStartupOnCharacterLeftPrefix(FejdStartup __instance) {
            ComfyQuickSlots.log("Resetting first load on character change.");
            ComfyQuickSlots.initialEquippedArmor = new List<ItemDrop.ItemData>();
            ComfyQuickSlots.firstLoad = false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FejdStartup), "OnCharacterRight")]
        public static void FejdStartupOnCharacterRightPrefix(FejdStartup __instance) {
            ComfyQuickSlots.log("Resetting first load on character change.");
            ComfyQuickSlots.initialEquippedArmor = new List<ItemDrop.ItemData>();
            ComfyQuickSlots.firstLoad = false;
        }
    }
}
