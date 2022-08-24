using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimInventorySlots {
    [BepInPlugin(FixGuiGuid, FixGuiName, FixVersion)]
    public class FixGuiFrame : BaseUnityPlugin {
        public const string FixGuiGuid = "8F0F9F2B-E481-4BEC-B641-01228D7508F9";
        public const string FixGuiName = "MoreSlotsPatcher";
        public const string FixVersion = "1.1.0";

        GameObject inventoryScreenObjectRoot;
        Transform inventoryScreenObject;
        Transform containerScreenObject;
        float bgYOffset = 0;
        float containerYOffset = 0;
        Coroutine screenSearch;
        public static int ExtraRows { get; set; } = 0;


        void OnEnable() {
            screenSearch = StartCoroutine(screenSearchCoroutine());
        }

        void OnDisable() {
            StopCoroutine(screenSearch);
        }

        public System.Collections.IEnumerator screenSearchCoroutine() {
            while (enabled) {
                if (inventoryScreenObjectRoot == null || inventoryScreenObject == null || containerScreenObject == null) {
                    bgYOffset = 0;
                    containerYOffset = 0;
                    inventoryScreenObjectRoot = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Inventory_screen").Where(obj => obj.transform.position.x != 0).SingleOrDefault();
                    if (inventoryScreenObjectRoot != null) {
                        yield return new WaitForSecondsRealtime(1f);
                        inventoryScreenObject = inventoryScreenObjectRoot.transform.Find("root/Player/Bkg");
                        containerScreenObject = inventoryScreenObjectRoot.transform.Find("root/Container");
                    }

                    yield return new WaitForSecondsRealtime(5f);
                } else {
                    // 1st scale the background
                    inventoryScreenObject.transform.localScale = new Vector3(1, (ExtraRows * 0.25f + 1), 1);

                    // Calculate the Y offset
                    if (bgYOffset == 0) {
                        float oldOffset = inventoryScreenObject.transform.localPosition.y;
                        bgYOffset = inventoryScreenObject.transform.localPosition.y * (ExtraRows * 0.25f + 1);
                        float offsetDiff = oldOffset - bgYOffset;


                        containerYOffset = containerScreenObject.localPosition.y - offsetDiff;
                        Vector3 localBgPosition = inventoryScreenObject.transform.localPosition;
                        localBgPosition.y = bgYOffset;
                        Vector3 localContainerPosition = containerScreenObject.transform.localPosition;
                        localContainerPosition.y = containerYOffset;

                        inventoryScreenObject.transform.localPosition = localBgPosition;
                        containerScreenObject.transform.localPosition = localBgPosition;

                        // Some sane default
                        float sleepSeconds = 30f;
                        yield return new WaitForSecondsRealtime(sleepSeconds);
                    }
                }
                yield return 0;
            }
        }

    }
}