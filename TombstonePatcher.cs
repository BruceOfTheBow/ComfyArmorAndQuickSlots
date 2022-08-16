//This file is courtesy of the creator of the original Valheim Inventory Slots mod :)
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using static ComfyQuickSlots.PluginConfig;

namespace ComfyQuickSlots {

    [HarmonyPatch(typeof(Player))]
    public class TombstonePatcher {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "CreateTombStone")]
        public static void CreateTombStonePrefix(Player __instance) {
            if (!ComfyQuickSlots.GKInstalled) {
                Player that = __instance;
                ComfyQuickSlots.UnequipAllArmor(__instance);
                GameObject additionalTombstone = UnityEngine.Object.Instantiate(that.m_tombstone, that.GetCenterPoint() + Vector3.up * 2.5f, that.transform.rotation);
                additionalTombstone.gameObject.transform.localScale -= new Vector3(.5f, .5f, .5f);
                Container graveContainer = additionalTombstone.GetComponent<Container>();

                that.UnequipAllItems();
                Func<ItemDrop.ItemData, bool> predicate = new Func<ItemDrop.ItemData, bool>(item => item.m_gridPos.y >= 4 && !item.m_equiped);
                foreach (var item in that.GetInventory().GetAllItems().Where(predicate).ToArray()) {
                    if (item.m_gridPos.y >= 4) {
                        graveContainer.GetInventory().AddItem(item);
                        that.GetInventory().RemoveItem(item);
                    }
                }
                __instance.GetInventory().m_height = 4;
                __instance.GetInventory().m_width = 8;

                Directory.CreateDirectory(LogFilesPath.Value);
                string filename = __instance.GetPlayerID() + __instance.GetPlayerName() + ".csv";
                InventoryLogger.LogInventoryToFile(__instance.GetInventory(), Path.Combine(LogFilesPath.Value, filename));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "CreateTombStone")]
        public static void CreateTombStonePostfix(Player __instance) {
            __instance.GetInventory().m_height = ComfyQuickSlots.rows;
            __instance.GetInventory().m_width = ComfyQuickSlots.columns;
        }
    }
}
