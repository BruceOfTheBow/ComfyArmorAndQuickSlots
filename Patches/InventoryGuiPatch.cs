using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace ComfyQuickSlots.Patches {
    [HarmonyPatch(typeof(InventoryGui))]
    public class InventoryGuiPatch {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
        public static bool OnSelectedItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) {
            if(ComfyQuickSlots.isArmorSlot(pos)) {
                return false;
            }

            if(Player.m_localPlayer.IsItemQueued(item)) {
              return false;
            }
            return true;
        }
    }
}
