using System.Collections.Generic;
using System.Linq;
using Common;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ComfyQuickSlots {
    public static class HotKeyBarController {
        public static GameObject QuickSlotsHotkeyBar;
        public static int SelectedHotkeyBarIndex = -1;
        public static int[] hotkeysIndices = { 37, 38, 39 };
        public static string[] hotkeyTexts = { "Z", "V", "B" };

        [HarmonyPatch(typeof(HotkeyBar))]
        public static class HotkeyBarPatch {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
            public static bool HotkeyBarPrefix(HotkeyBar __instance, Player player) {
                if (__instance.name == "QuickSlotsHotkeyBar") {
                    if (player == null || player.IsDead()) {
                        foreach (var element in __instance.m_elements) {
                            Object.Destroy(element.m_go);
                        }

                        __instance.m_elements.Clear();
                    } else {

                        //if (__instance.m_elements.Count != ComfyQuickSlots.quickSlotsCount) {
                        //player.GetInventory().GetBoundItems(__instance.m_items);
                        //__instance.m_items.Sort((x, y) => x.m_gridPos.x.CompareTo(y.m_gridPos.x));
                        //player.GetInventory().GetBoundItems(__instance.m_items);
                        __instance.m_items = new List<ItemDrop.ItemData>();
                        for (int i = 5; i < ComfyQuickSlots.columns; i++) {
                            if (player.GetInventory().GetItemAt(i, 4) != null) {
                                __instance.m_items.Add(player.GetInventory().GetItemAt(i, 4));
                            }
                        }

                        foreach (var element in __instance.m_elements) {
                            Object.Destroy(element.m_go);
                        }
                        __instance.m_elements.Clear();

                        for (var index = 0; index < ComfyQuickSlots.quickSlotsCount; ++index) {
                            var elementData = new HotkeyBar.ElementData() { m_go = Object.Instantiate(__instance.m_elementPrefab, __instance.transform) };
                            elementData.m_go.transform.localPosition = new Vector3(index * __instance.m_elementSpace, 0.0f, 0.0f);
                            elementData.m_icon = elementData.m_go.transform.transform.Find("icon").GetComponent<Image>();
                            elementData.m_durability = elementData.m_go.transform.Find("durability").GetComponent<GuiBar>();
                            elementData.m_amount = elementData.m_go.transform.Find("amount").GetComponent<Text>();
                            elementData.m_equiped = elementData.m_go.transform.Find("equiped").gameObject;
                            elementData.m_queued = elementData.m_go.transform.Find("queued").gameObject;
                            elementData.m_selection = elementData.m_go.transform.Find("selected").gameObject;

                            var bindingText = elementData.m_go.transform.Find("binding").GetComponent<Text>();
                            bindingText.enabled = true;
                            bindingText.horizontalOverflow = HorizontalWrapMode.Overflow;
                            bindingText.text = hotkeyTexts[index];

                            __instance.m_elements.Add(elementData);
                        }

                        foreach (var element in __instance.m_elements) {
                            element.m_used = false;
                        }

                        var isGamepadActive = ZInput.IsGamepadActive();
                        foreach (var itemData in __instance.m_items) {
                            var element = __instance.m_elements[itemData.m_gridPos.x - 5];
                            element.m_used = true;
                            element.m_icon.gameObject.SetActive(true);
                            element.m_icon.sprite = itemData.GetIcon();
                            element.m_durability.gameObject.SetActive(itemData.m_shared.m_useDurability);
                            if (itemData.m_shared.m_useDurability) {
                                if (itemData.m_durability <= 0.0) {
                                    element.m_durability.SetValue(1f);
                                    element.m_durability.SetColor((double)Mathf.Sin(Time.time * 10f) > 0.0 ? Color.red : new Color(0.0f, 0.0f, 0.0f, 0.0f));
                                } else {
                                    element.m_durability.SetValue(itemData.GetDurabilityPercentage());
                                    element.m_durability.ResetColor();
                                }
                            }

                            element.m_equiped.SetActive(itemData.m_equiped);
                            element.m_queued.SetActive(player.IsItemQueued(itemData));
                            if (itemData.m_shared.m_maxStackSize > 1) {
                                element.m_amount.gameObject.SetActive(true);
                                element.m_amount.text = itemData.m_stack.ToString() + "/" + itemData.m_shared.m_maxStackSize.ToString();
                            } else {
                                element.m_amount.gameObject.SetActive(false);
                            }
                        }

                        for (var index = 0; index < __instance.m_elements.Count; ++index) {
                            var element = __instance.m_elements[index];
                            element.m_selection.SetActive(isGamepadActive && index == __instance.m_selected);
                            if (!element.m_used) {
                                element.m_icon.gameObject.SetActive(false);
                                element.m_durability.gameObject.SetActive(false);
                                element.m_equiped.SetActive(false);
                                element.m_queued.SetActive(false);
                                element.m_amount.gameObject.SetActive(false);
                            }
                        }
                        return false;
                    }
                }

                return true;
            }
        }
        [HarmonyPatch(typeof(Hud), "Awake")]
        public static class Hud_Awake_Patch {
            public static void Postfix(Hud __instance) {
                var hotkeyBar = __instance.GetComponentInChildren<HotkeyBar>();

                if (ComfyQuickSlots.isModEnabled.Value && hotkeyBar.transform.parent.Find("QuickSlotsHotkeyBar") == null) {
                    QuickSlotsHotkeyBar = Object.Instantiate(hotkeyBar.gameObject, __instance.m_rootObject.transform, true);
                    QuickSlotsHotkeyBar.name = "QuickSlotsHotkeyBar";
                    QuickSlotsHotkeyBar.GetComponent<HotkeyBar>().m_selected = -1;

                    var configPositionedElement = QuickSlotsHotkeyBar.AddComponent<ConfigPositionedElement>();
                    configPositionedElement.PositionConfig = ComfyQuickSlots.QuickSlotsPosition;
                    configPositionedElement.AnchorConfig = ComfyQuickSlots.QuickSlotsAnchor;
                    configPositionedElement.EnsureCorrectPosition();
                }
            }
        }
    }
}