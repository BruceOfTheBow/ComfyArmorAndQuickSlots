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
    [HarmonyPatch(typeof(FejdStartup), "SetupCharacterPreview")]
    public static void FejdStartupSetupCharacterPreviewPrefix(FejdStartup __instance) {
      ComfyQuickSlots.log("Resetting first load on character change.");
      ComfyQuickSlots.initialEquippedArmor = new List<ItemDrop.ItemData>();
      ComfyQuickSlots.firstLoad = false;
    }
  }
}
