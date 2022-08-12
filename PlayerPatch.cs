using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ComfyQuickSlots {

    [HarmonyPatch(typeof(Player))]
    public static class PlayerPatch {
        private const string playerDataKey = "ComfyQuickSlotsInventory";
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "Awake")]
        public static void PlayerAwakePrefix(ref Player __instance) {
            __instance.m_inventory = new Inventory("ComfyQuickSlotsInventory", null, ComfyQuickSlots.columns, ComfyQuickSlots.rows);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Load")]
        public static void PlayerLoadPostFix(Player __instance) {
            if (__instance.m_knownTexts.ContainsKey(playerDataKey)) {
                ComfyQuickSlots.log($"Loading comfy armor and quick slots data from {playerDataKey}");
                try {
                    ZPackage pkg = new ZPackage(__instance.m_knownTexts[playerDataKey]);
                    __instance.GetInventory().Load(pkg);
                    ComfyQuickSlots.EquipArmorInArmorSlots(__instance);
                    __instance.GetInventory().Changed();
                } catch(Exception e) {
                    ComfyQuickSlots.log($"Caught exception{e}");
                }
            } else {
                ComfyQuickSlots.log($"Initial load of ComfyQuickSlots. Checking for initial equipped armor pieces.");
                ComfyQuickSlots.firstLoad = true;
                foreach(ItemDrop.ItemData armorPiece in ComfyQuickSlots.initialEquippedArmor) {
                    ComfyQuickSlots.log($"Equipping {armorPiece.m_shared.m_name} and moving to armor slot on initial load.");
                    ComfyQuickSlots.UnequipItem(__instance, armorPiece);
                    __instance.GetInventory().AddItem(armorPiece);
                    __instance.EquipItem(armorPiece);
                    Vector2i armorSlot = ComfyQuickSlots.GetArmorSlot(armorPiece);
                    ComfyQuickSlots.MoveArmorItemToSlot(__instance, armorPiece, armorSlot.x, armorSlot.y);
                    __instance.GetInventory().Changed();
                }
                ComfyQuickSlots.firstLoad = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "Save")]
        public static bool PlayerSavePrefix(Player __instance) {
            ComfyQuickSlots.log($"Adding armor and quick slots data to save file as {playerDataKey}.");
            ZPackage pkg = new ZPackage();
            __instance.GetInventory().Save(pkg);
            if (__instance.m_knownTexts.ContainsKey(playerDataKey)) {
                __instance.m_knownTexts[playerDataKey] = pkg.GetBase64();
            } else {
                __instance.m_knownTexts.Add(playerDataKey, pkg.GetBase64());
            }
            
            return true;
        }


        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
        //public static void SpawnPlayerPostfix(Game __instance) {
        //    if (!__instance.m_firstSpawn) {

        //        foreach (ItemDrop.ItemData item in Player.m_localPlayer.GetInventory().m_inventory) {
        //            if (ComfyQuickSlots.isArmor(item)) {

        //            }
        //        }

        //    }
        //}
    }
}
