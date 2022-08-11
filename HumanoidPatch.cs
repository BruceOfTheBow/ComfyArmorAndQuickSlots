using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ComfyQuickSlots {
    [HarmonyPatch(typeof(Humanoid))]
    public class HumanoidPatch {
        public static bool wasEquipped { get; set; }  = false;
        public static ItemDrop.ItemData unequippedItem;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]

        public static bool EquipItemPrefix(Humanoid __instance, bool __result, ItemDrop.ItemData item) {
            if (ComfyQuickSlots.isArmor(item)) {
                Vector2i armorSlot = ComfyQuickSlots.GetArmorSlot(item);
                if (ComfyQuickSlots.isArmorTypeEquipped(__instance, item)) {
                    if (ComfyQuickSlots.isArmorTypeEquipped(__instance, item)) {
                        ItemDrop.ItemData swapItem = ComfyQuickSlots.GetArmorItemToSwap(__instance, item);
                        ComfyQuickSlots.UnequipItem(__instance, swapItem);
                        ComfyQuickSlots.log($"Armor item already equipped. Swapping {item.m_shared.m_name} for {swapItem.m_shared.m_name}");
                        ComfyQuickSlots.EquipItem(__instance, item);
                        ComfyQuickSlots.MoveArmorItemToSlot(__instance, item, armorSlot.x, armorSlot.y);
                        __result = true;
                        return false;
                    }
                }
                ComfyQuickSlots.log($"Equipping item {item.m_shared.m_name}");
                ComfyQuickSlots.EquipItem(__instance, item);
                if(!(item.m_gridPos.x == armorSlot.x && item.m_gridPos.y == armorSlot.y)) {
                    ComfyQuickSlots.MoveArmorItemToSlot(__instance, item, armorSlot.x, armorSlot.y);
                    ComfyQuickSlots.log($"Moving armor to item slot {armorSlot.x},{armorSlot.y}");
                }
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItemPostfix(Humanoid __instance, ItemDrop.ItemData item) {
            if (__instance != null && item != null) {
                ComfyQuickSlots.log($"Item equipped {item.m_equiped}");
                __instance.GetInventory().Changed();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static bool UnequipItemPrefix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects) {
            if (item != null) {
                if (ComfyQuickSlots.isArmor(item) && item.m_equiped) {
                    if (!ComfyQuickSlots.HaveEmptyInventorySlot(__instance.GetInventory())) {
                        ComfyQuickSlots.log("No empty slots found. Will not unequip item.");
                        __instance.Message(MessageHud.MessageType.Center, "Inventory full. Item not unequipped.");
                        return false;
                    }
                    ComfyQuickSlots.log("Unequipping item.");
                    ComfyQuickSlots.UnequipItem(__instance, item);
                    Vector2i emptyLoc = ComfyQuickSlots.GetEmptyInventorySlot(__instance.GetInventory());
                    ComfyQuickSlots.MoveArmorItemToSlot(__instance, item, emptyLoc.x, emptyLoc.y);
                    return false;
                }
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static void UnequipItemPostfix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects) {
            if(__instance != null && item != null) {
                ComfyQuickSlots.log("Unequipping item complete.");
            }
        }
    }
}
