using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;

using UnityEngine;

namespace ComfyQuickSlots {
    public class PluginConfig {
        public static ConfigEntry<bool> IsModEnabled = default!;

        // QuickSlots
        public static ConfigEntry<KeyCode> QuickSlot1 = default!;
        public static ConfigEntry<KeyCode> QuickSlot2 = default!;
        public static ConfigEntry<KeyCode> QuickSlot3 = default!;

        // Quick Slots Hotkeybar Positioning
        public static ConfigEntry<TextAnchor> QuickSlotsAnchor;
        public static ConfigEntry<Vector2> QuickSlotsPosition;

        // Logging
        public static ConfigEntry<string> LogFilesPath = default!;


        public static void BindConfig(ConfigFile config) {
            IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

            // QuickSlots
            QuickSlot1 = config.Bind("QuickSlot1", "quickSlot1Use", KeyCode.Z, "Hot key for item use in quick slot 1");
            QuickSlot2 = config.Bind("QuickSlot2", "quickSlot2Use", KeyCode.V, "Hot key for item use in quick slot 2");
            QuickSlot3 = config.Bind("QuickSlot3", "quickSlot3Use", KeyCode.B, "Hot key for item use in quick slot 3");

            // Quick Slots Hotkeybar Positioning
            QuickSlotsAnchor = config.Bind("QuickSlotsAnchor", "quickSlotsAnchor", TextAnchor.LowerLeft, "The point on the HUD to anchor the Quick Slots bar. Changing this also changes the pivot of the Quick Slots to that corner.");
            QuickSlotsPosition = config.Bind("QuickSlotsOffset", "quickSlotsOffset", new Vector2(216, 150), "The position offset from the Quick Slots Anchor at which to place the Quick Slots.");

            // Logging
            LogFilesPath = config.Bind("Logging", "logFilesPath", "ItemsOnDeath/", "Path to where logging of items on death are saved.");
        }
    }
}
