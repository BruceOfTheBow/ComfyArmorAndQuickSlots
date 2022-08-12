using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using UnityEngine;

namespace ComfyQuickSlots {

    [HarmonyPatch(typeof(Inventory))]
    public static class InventoryPatch {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), "AddItem", typeof(GameObject), typeof(int))]
        public static bool AddItemGameObjectPrefix(Inventory __instance, ref bool __result, GameObject prefab, int amount) {
            if (__instance.m_name == "ComfyQuickSlotsInventory") {
                //ComfyQuickSlots.log($"Adding {amount} game object {prefab.GetComponent<ItemDrop>().m_itemData.Clone().m_shared.m_name}");
                ItemDrop.ItemData item = prefab.GetComponent<ItemDrop>().m_itemData;
                if (ComfyQuickSlots.HaveEmptyInventorySlot(__instance)) {
                    Vector2i emptySlot = ComfyQuickSlots.GetEmptyInventorySlot(__instance);
                    __instance.AddItem(item, item.m_stack, emptySlot.x, emptySlot.y);
                    return true;
                }
                __result = false;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), "AddItem", typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
        public static bool AddItemPositionPrefix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y) {
            if(item != null) {
                ComfyQuickSlots.log($"Attempting to add item {item.m_shared.m_name} to {x},{y}. Is equipped? {item.m_equiped}. Local Player?{Player.m_localPlayer != null}. On Menu Load? {ComfyQuickSlots.onMenuLoad}");
                Vector2i loc = new Vector2i(x, y);
                if (__instance.m_name == "ComfyQuickSlotsInventory") {
                    if (item.m_equiped && ComfyQuickSlots.isArmor(item)  && Player.m_localPlayer != null) {
                        ComfyQuickSlots.UnequipItem(Player.m_localPlayer, item);
                        Vector2i armorSlot = ComfyQuickSlots.GetArmorSlot(item);
                        if (x == armorSlot.x && y == armorSlot.y) {
                            return true;
                        }
                        if(ComfyQuickSlots.firstLoad) {
                            ComfyQuickSlots.log($"Adding {item.m_shared.m_name} to initial armor list.");
                            ComfyQuickSlots.initialEquippedArmor.Add(item);
                        }
                        return false;
                    }
                    if (Player.m_localPlayer == null) {
                        ComfyQuickSlots.log("Adding item during menu loading phase.");
                        return true;
                    }
                    if (x < 5 && y == 4) {
                        return false;
                    }

                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), "MoveItemToThis", typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
        public static bool MoveItemToThisPrefix(Inventory __instance, ref bool __result, Inventory fromInventory, ItemDrop.ItemData item, int amount, int x, int y) {
            if (__instance.m_name.Equals("ComfyQuickSlotsInventory")) {
                if (x < 5 && y == 4) {
                    return false;
                }
            }
            ComfyQuickSlots.log($"Moving {item.m_shared.m_name} from {item.m_gridPos.x},{item.m_gridPos.y} in {fromInventory.m_name} to {x},{y} {__instance.m_name}");
            return true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), "CanAddItem", typeof(ItemDrop.ItemData), typeof(int))]
        public static bool CanAdditemPrefix(Inventory __instance, ref bool __result, ItemDrop.ItemData item, int stack) {
            if (__instance.m_name == "ComfyQuickSlotsInventory") {
                ComfyQuickSlots.log("Checking if can add item to player inventory");
                __result = ComfyQuickSlots.HaveEmptyInventorySlot(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), "FindEmptySlot")]
        public static bool FindEmptySlotPrefix(Inventory __instance, bool topFirst, ref Vector2i __result) {
            if (__instance.m_name == "ComfyQuickSlotsInventory") {
                __result = ComfyQuickSlots.GetEmptyInventorySlot(__instance);
                ComfyQuickSlots.log($"Found empty slot in ${__instance.m_name} at {__result.x},{__result.y}");
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Inventory), "HaveEmptySlot")]
        public static bool HaveEmptySlotPrefix(Inventory __instance, ref bool __result) {
            ComfyQuickSlots.log("Checking have empty slots");
            if (__instance.m_name == "ComfyQuickSlotsInventory") {
                __result = ComfyQuickSlots.HaveEmptyInventorySlot(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "FindEmptySlot")]
        public static void FindEmptySlotPostfix(Inventory __instance, bool topFirst) {
            ComfyQuickSlots.log($"Finding Empty slots in ${__instance.m_name}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(int))]
        public static void RemoveItemPostfix(Inventory __instance, int index) {
            ComfyQuickSlots.log($"Removing item at {index}.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(ItemDrop.ItemData))]
        public static void RemoveItemItemPostfix(Inventory __instance, ItemDrop.ItemData item) {
            ComfyQuickSlots.log($"Removing {item.m_shared.m_name}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "RemoveItem", typeof(ItemDrop.ItemData), typeof(int))]
        public static void RemoveItemAmountPostfix(Inventory __instance, ItemDrop.ItemData item, int amount) {
            ComfyQuickSlots.log($"Removing {amount} of item at {item.m_shared.m_name}.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "AddItem", typeof(ItemDrop.ItemData))]
        public static void AddItemItemPostfix(Inventory __instance, ItemDrop.ItemData item) {
            ComfyQuickSlots.log($"Adding item {item.m_shared.m_name}.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "AddItem", typeof(GameObject), typeof(int))]
        public static void AddItemGameObjectPostfix(Inventory __instance, GameObject prefab, int amount) {
            ComfyQuickSlots.log($"Adding game object {prefab.GetComponent<ItemDrop.ItemData>().m_shared.m_name}.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Inventory), "AddItem", typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
        public static void AddItemPositionPostfix(Inventory __instance, ItemDrop.ItemData item, int amount, int x, int y) {
            ComfyQuickSlots.log($"Adding item {item.m_shared.m_name} to {x},{y}.");
        }
    }
}
