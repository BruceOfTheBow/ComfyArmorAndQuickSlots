using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

//using static ComfyQuickSlots.ComfyQuickSlotsConfig;

namespace ComfyQuickSlots {
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ComfyQuickSlots : BaseUnityPlugin {
        public const string PluginGuid = "com.bruce.valheim.comfyquickslots";
        public const string PluginName = "ComfyQuickSlots";
        public const string PluginVersion = "0.0.1";

        private const string countsFileName = "arrowCounts";
        private static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "ComfyQuickSlots.cfg"), true);

        static ManualLogSource _logger;
        Harmony _harmony;
        public static ConfigEntry<bool> isModEnabled;

        private static bool _debug = true;

        static Assembly assem = typeof(ComfyQuickSlots).Assembly;
        public static string fpath = assem.Location;
        public static string path = fpath.Remove(fpath.Length - 13);
        public static bool GKInstalled;

        public const int quickSlotsCount = 3;

        public const int rows = 5;
        public const int columns = 8;

        public static ConfigEntry<TextAnchor> QuickSlotsAnchor;
        public static ConfigEntry<Vector2> QuickSlotsPosition;

        public static readonly List<ItemDrop.ItemData.ItemType> ArmorSlotTypes = new List<ItemDrop.ItemData.ItemType>() {
            ItemDrop.ItemData.ItemType.Helmet,
            ItemDrop.ItemData.ItemType.Chest,
            ItemDrop.ItemData.ItemType.Legs,
            ItemDrop.ItemData.ItemType.Shoulder,
            ItemDrop.ItemData.ItemType.Utility
        };
        public static Vector2i helmetSlot = new Vector2i(0, 4);
        public static Vector2i chestSlot = new Vector2i(1, 4);
        public static Vector2i legsSlot = new Vector2i(2, 4);
        public static Vector2i shoulderSlot = new Vector2i(3, 4);
        public static Vector2i utilitySlot = new Vector2i(4, 4);

        public static List<Vector2i> armorSlots = new List<Vector2i>() { helmetSlot, chestSlot, legsSlot, shoulderSlot, utilitySlot };

        public void Awake() {
            isModEnabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Enable or disable this mod.");
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
            _logger = Logger;
            QuickSlotsAnchor = Config.Bind("Quick Slots", "Quick Slots Anchor", TextAnchor.LowerLeft, "The point on the HUD to anchor the Quick Slots bar. Changing this also changes the pivot of the Quick Slots to that corner.");
            QuickSlotsPosition = Config.Bind("Quick Slots", "Quick Slots Position", new Vector2(216, 150), "The position offset from the Quick Slots Anchor at which to place the Quick Slots.");
            //BindConfig(configFile);

            if (File.Exists($@"{path}Gravekeeper.dll")) {
                Logger.LogInfo("Gravekeeper found");
                GKInstalled = true;
            } else {
                Logger.LogInfo("Gravekeeper not found");
                GKInstalled = false;
            }
        }

        public void OnDestroy() {
            _harmony?.UnpatchSelf();
        }

        public void Update() {
            var player = Player.m_localPlayer;
            if (player != null) {
                if (player.TakeInput()) {
                    if (Input.GetKeyDown(KeyCode.B)) {
                        log("Hitting B");
                        ItemDrop.ItemData item = Player.m_localPlayer.GetInventory().GetItemAt(7, 4);
                        if(item != null) {
                            log($"Using item {item.m_shared.m_name}");
                            Player.m_localPlayer.UseItem(null, item, false);
                        } else {
                            log("No item in slot");
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.V)) {
                        log("Hitting B");
                        ItemDrop.ItemData item = Player.m_localPlayer.GetInventory().GetItemAt(6, 4);
                        if (item != null) {
                            log($"Using item {item.m_shared.m_name}");
                            Player.m_localPlayer.UseItem(null, item, false);
                        } else {
                            log("No item in slot");
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Z)) {
                        log("Hitting B");
                        ItemDrop.ItemData item = Player.m_localPlayer.GetInventory().GetItemAt(5, 4);
                        if (item != null) {
                            log($"Using item {item.m_shared.m_name}");
                            Player.m_localPlayer.UseItem(null, item, false);
                        } else {
                            log("No item in slot");
                        }
                    }

                }
            }
        }

        [HarmonyPatch(typeof(Player))]
        public static class GridExtension {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Player), "Awake")]
            public static void PlayerAwakePrefix(ref Player __instance) {
                __instance.m_inventory = new Inventory("Inventory", null, columns, rows);
            }
        }

        [HarmonyPatch(typeof(InventoryGrid))]
        public static class ModifyBackground {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
            public static void Postfix(ref InventoryGrid __instance, Player player) {
                if (__instance.name == "PlayerGrid") {

                    float addedRows = rows - 4;
                    float offset = -35 * addedRows;

                    RectTransform gridBkg = ModifyBackground.GetOrCreateBackground(__instance, "ExtInvGrid");
                    gridBkg.anchoredPosition = new Vector2(0f, offset);
                    gridBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 590f);
                    gridBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300f + 75 * addedRows);

                    //Add Quick slots and equipment overlays
                    //for(int i = 36; i < rows*columns - 1; i++) {
                    Text bindingTextHead = __instance.m_elements[32].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextHead.text = "Head";
                    bindingTextHead.enabled = true;
                    bindingTextHead.fontSize = 12;
                    bindingTextHead.horizontalOverflow = HorizontalWrapMode.Overflow;
                    bindingTextHead.alignment = TextAnchor.UpperLeft;
                    Text bindingTextChest = __instance.m_elements[33].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextChest.text = "Chest";
                    bindingTextChest.enabled = true;
                    bindingTextChest.fontSize = 12;
                    bindingTextChest.alignment = TextAnchor.UpperLeft;
                    bindingTextChest.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingTextLegs = __instance.m_elements[34].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextLegs.text = "Legs";
                    bindingTextLegs.enabled = true;
                    bindingTextLegs.horizontalOverflow = HorizontalWrapMode.Overflow;
                    bindingTextLegs.fontSize = 12;
                    bindingTextLegs.alignment = TextAnchor.UpperLeft;
                    Text bindingTextCape = __instance.m_elements[35].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextCape.text = "Cape";
                    bindingTextCape.enabled = true;
                    bindingTextCape.fontSize = 12;
                    bindingTextLegs.alignment = TextAnchor.UpperLeft;
                    bindingTextCape.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingTextUtility = __instance.m_elements[36].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextUtility.text = "Util";
                    bindingTextUtility.enabled = true;
                    bindingTextUtility.fontSize = 12;
                    bindingTextUtility.alignment = TextAnchor.UpperLeft;
                    bindingTextUtility.horizontalOverflow = HorizontalWrapMode.Overflow;

                    Text bindingText1 = __instance.m_elements[37].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingText1.text = "Z";
                    bindingText1.enabled = true;
                    bindingText1.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingText2 = __instance.m_elements[38].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingText2.text = "V";
                    bindingText2.enabled = true;
                    bindingText2.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingText3 = __instance.m_elements[39].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingText3.text = "B";
                    bindingText3.enabled = true;
                    bindingText3.horizontalOverflow = HorizontalWrapMode.Overflow;
                    //}


                }
                //log(__instance.name);
                //log(__instance.m_elementPrefab.name);
                // Will bork chest background panel

                //if (__instance.name == "ContainerGrid") {
                //    var bkg = __instance.transform.parent.Find("Bkg").gameObject;
                //    Transform bkgT = bkg.transform;
                //    bkgT.position = new Vector3(33 + 295, 550 - 75 * (rows - 4), 0);


                //    //UnityEngine.Debug.Log("Attempted move");
                //}
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
            public static void Postfix(ref InventoryGrid __instance, Player player, ItemDrop.ItemData dragItem) {
                if (__instance.name == "PlayerGrid") {

                    float addedRows = rows - 4;
                    float offset = -35 * addedRows;

                    RectTransform gridBkg = ModifyBackground.GetOrCreateBackground(__instance, "ExtInvGrid");
                    gridBkg.anchoredPosition = new Vector2(0f, offset);
                    gridBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 590f);
                    gridBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300f + 75 * addedRows);

                    //Add Quick slots and equipment overlays
                    //for(int i = 36; i < rows*columns - 1; i++) {
                    Text bindingTextHead = __instance.m_elements[32].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextHead.text = "Head";
                    bindingTextHead.enabled = true;
                    bindingTextHead.fontSize = 12;
                    bindingTextHead.horizontalOverflow = HorizontalWrapMode.Overflow;
                    bindingTextHead.alignment = TextAnchor.UpperLeft;
                    Text bindingTextChest = __instance.m_elements[33].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextChest.text = "Chest";
                    bindingTextChest.enabled = true;
                    bindingTextChest.fontSize = 12;
                    bindingTextChest.alignment = TextAnchor.UpperLeft;
                    bindingTextChest.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingTextLegs = __instance.m_elements[34].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextLegs.text = "Legs";
                    bindingTextLegs.enabled = true;
                    bindingTextLegs.horizontalOverflow = HorizontalWrapMode.Overflow;
                    bindingTextLegs.fontSize = 12;
                    bindingTextLegs.alignment = TextAnchor.UpperLeft;
                    Text bindingTextCape = __instance.m_elements[35].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextCape.text = "Cape";
                    bindingTextCape.enabled = true;
                    bindingTextCape.fontSize = 12;
                    bindingTextLegs.alignment = TextAnchor.UpperLeft;
                    bindingTextCape.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingTextUtility = __instance.m_elements[36].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingTextUtility.text = "Util";
                    bindingTextUtility.enabled = true;
                    bindingTextUtility.fontSize = 12;
                    bindingTextUtility.alignment = TextAnchor.UpperLeft;
                    bindingTextUtility.horizontalOverflow = HorizontalWrapMode.Overflow;

                    Text bindingText1 = __instance.m_elements[37].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingText1.text = "Z";
                    bindingText1.enabled = true;
                    bindingText1.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingText2 = __instance.m_elements[38].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingText2.text = "V";
                    bindingText2.enabled = true;
                    bindingText2.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text bindingText3 = __instance.m_elements[39].m_go.transform.Find("binding").GetComponent<Text>();
                    bindingText3.text = "B";
                    bindingText3.enabled = true;
                    bindingText3.horizontalOverflow = HorizontalWrapMode.Overflow;
                    //}


                }
                //log(__instance.name);
                //log(__instance.m_elementPrefab.name);
                // Will bork chest background panel

                //if (__instance.name == "ContainerGrid") {
                //    var bkg = __instance.transform.parent.Find("Bkg").gameObject;
                //    Transform bkgT = bkg.transform;
                //    bkgT.position = new Vector3(33 + 295, 550 - 75 * (rows - 4), 0);


                //    //UnityEngine.Debug.Log("Attempted move");
                //}
            }

            private static RectTransform GetOrCreateBackground(InventoryGrid __instance, string name) {
                var existingBkg = __instance.transform.parent.Find(name);
                if (existingBkg == null) {
                    var bkg = __instance.transform.parent.Find("Bkg").gameObject;
                    var background = GameObject.Instantiate(bkg, bkg.transform.parent);
                    background.name = name;
                    background.transform.SetSiblingIndex(bkg.transform.GetSiblingIndex() + 1);
                    existingBkg = background.transform;
                }

                return existingBkg as RectTransform;
            }
        }

        public static void log(string message) {
            if (_debug) {
                _logger.LogInfo(message);
            }
        }

        public static bool HaveEmptyInventorySlot(Inventory inventory) {
            for (int i = 0; i < columns; i++) {
                for (int j = 0; j < rows; j++) {
                    if (inventory.GetItemAt(i, j) == null && j != 4) {
                        return true;
                    } else {
                        if(i > 4 && j == 4 && inventory.GetItemAt(i, j) == null) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static Vector2i GetEmptyInventorySlot(Inventory inventory) {
            for (int i = 0; i < columns; i++) {
                for (int j = 0; j < rows; j++) {
                    if (inventory.GetItemAt(i, j) == null && j != 4) {
                        return new Vector2i(i, j);
                    } else {
                        if (i > 4 && j == 4 && inventory.GetItemAt(i, j) == null) {
                            return new Vector2i(i, j);
                        }
                    }
                }
            }
            return new Vector2i(-1, -1);
        }
        public static bool isArmor(ItemDrop.ItemData item) {
            if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility) {
                return true;
            }
            return false;
        }

        public static bool isArmorSlot(Vector2i loc) {
            if(armorSlots.Contains(loc)) {
                return true;
            }
            return false;
        }

        public static Vector2i GetArmorSlot(ItemDrop.ItemData item) {
            return armorSlots[GetArmorTypeIndex(item)];
        }

        public static int GetArmorSlotIndex(Vector2i loc) {
            return armorSlots.IndexOf(loc);
        }

        public static int GetArmorTypeIndex(ItemDrop.ItemData item) {
            return ArmorSlotTypes.IndexOf(item.m_shared.m_itemType);
        }

        public static bool UnequipItem(Humanoid player, ItemDrop.ItemData item) {
            if (player.m_helmetItem == item) {
                player.m_helmetItem = null;
                item.m_equiped = false;
                player.SetupEquipment();
                player.TriggerEquipEffect(item);
                return true;
            }
            if (player.m_chestItem == item) {
                player.m_chestItem = null;
                item.m_equiped = false;
                player.SetupEquipment();
                player.TriggerEquipEffect(item);
                return true;
            }
            if (player.m_legItem == item) {
                player.m_legItem = null;
                item.m_equiped = false;
                player.SetupEquipment();
                player.TriggerEquipEffect(item);
                return true;
            }
            if (player.m_shoulderItem == item) {
                player.m_shoulderItem = null;
                item.m_equiped = false;
                player.SetupEquipment();
                player.TriggerEquipEffect(item);
                return true;
            }
            if (player.m_utilityItem == item) {
                player.m_utilityItem = null;
                item.m_equiped = false;
                player.SetupEquipment();
                player.TriggerEquipEffect(item);
                return true;
            }
            return false;
        }

        public static void EquipItem(Humanoid humanoid, ItemDrop.ItemData item) {
            if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet) {
                humanoid.m_helmetItem = item;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest) {
                humanoid.m_chestItem = item;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs) {
                humanoid.m_legItem = item;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder) {
                humanoid.m_shoulderItem = item;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility) {
                humanoid.m_utilityItem = item;
            }
            item.m_equiped = true;
            humanoid.SetupEquipment();
            humanoid.TriggerEquipEffect(item);
        }

        public static bool isArmorTypeEquipped(Humanoid humanoid, ItemDrop.ItemData item) {
            ItemDrop.ItemData armorItem = null;
            if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet) {
                armorItem = humanoid.m_helmetItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest) {
                armorItem = humanoid.m_chestItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs) {
                armorItem = humanoid.m_legItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder) {
                armorItem = humanoid.m_shoulderItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility) {
                armorItem = humanoid.m_utilityItem;
            }
            
            if(armorItem == null) {
                return false;
            }
            return true;
        }

        public static ItemDrop.ItemData GetArmorItemToSwap(Humanoid humanoid, ItemDrop.ItemData item) {
            if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet) {
                return humanoid.m_helmetItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest) {
                return humanoid.m_chestItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs) {
                return humanoid.m_legItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder) {
                return humanoid.m_shoulderItem;
            } else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility) {
                return humanoid.m_utilityItem;
            }
            return null;
        }

        public static bool SwapArmorItems(Humanoid humanoid, ItemDrop.ItemData itemToMove, ItemDrop.ItemData itemInArmorSlot, int armorSlotX, int armorSlotY) {
            ComfyQuickSlots.log($"Swapping {itemToMove.m_shared.m_name} in {itemToMove.m_gridPos.x},{itemToMove.m_gridPos.y} with {itemInArmorSlot} in {armorSlotX},{armorSlotY}");
            Vector2i otherSlot = itemToMove.m_gridPos;
            itemToMove.m_gridPos = new Vector2i(armorSlotX, armorSlotY);
            itemInArmorSlot.m_gridPos = otherSlot;
            humanoid.GetInventory().Changed();
            return false;
        }

        public static bool MoveArmorItemToSlot(Humanoid humanoid, ItemDrop.ItemData item, int x, int y) {
            ItemDrop.ItemData itemInArmorSlot = humanoid.GetInventory().GetItemAt(x, y);
            if(itemInArmorSlot != null) {
                return SwapArmorItems(humanoid, item, itemInArmorSlot, x, y);
            } else {
                item.m_gridPos = new Vector2i(x, y);
                if(!humanoid.GetInventory().m_inventory.Contains(item)) {
                    humanoid.GetInventory().AddItem(item);
                    return true;
                }
            }
            return false;
        }

        public static bool AddItemToSlot(Humanoid humanoid, ItemDrop.ItemData item, int x, int y) {
            humanoid.GetInventory().m_inventory.Add(item);
            item.m_gridPos = new Vector2i(x, y);
            return true;
        }

        public static bool AddItemToExistingStacks(Inventory inventory, ItemDrop.ItemData item) {
            int i = 0;
            if(item.m_shared.m_maxStackSize > 1) {
                while (i < item.m_stack) {
                    ItemDrop.ItemData itemData = inventory.FindFreeStackItem(item.m_shared.m_name, item.m_quality);
                    if (itemData != null) {
                        itemData.m_stack++;
                        i++;
                    } else {
                        item.m_stack = item.m_stack - i;
                        Vector2i vector2i = ComfyQuickSlots.GetEmptyInventorySlot(inventory);
                        if (vector2i.x >= 0) {
                            item.m_gridPos = vector2i;
                            inventory.m_inventory.Add(item);
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }
    }
}