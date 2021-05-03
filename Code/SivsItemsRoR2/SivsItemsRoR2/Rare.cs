using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Skills;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2.UI.LogBook;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using R2API.Networking;
using HarmonyLib;
using SivsItems;
using SivsItemsRoR2;

namespace SivsItemsRoR2
{
    public static class ShadowCloneMirror
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        private static GameObject hitEffectPrefab;

        private static ConfigEntry<int> baseDoppelgangerCount;
        private static ConfigEntry<int> stackDoppelgangerCount;



        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Shattered Mirror";
            baseDoppelgangerCount = SivsItemsPlugin.config.Bind<int>(configSection, "Doppelganger Count", 1, "The amount of Doppelgangers Shattered Mirror gives you.");
            stackDoppelgangerCount = SivsItemsPlugin.config.Bind<int>(configSection, "Doppelganger Count per Stack", 1, "The amount of additional Doppelgangers Shattered Mirror gives per stack.");
            
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "ShadowDoppelganger";
            itemDef.nameToken = "SHADOWDOPPELGANGER_NAME";
            itemDef.descriptionToken = "SHADOWDOPPELGANGER_DESCRIPTION";
            itemDef.loreToken = "SHADOWDOPPELGANGER_LORE";
            itemDef.pickupToken = "SHADOWDOPPELGANGER_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Scrap };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Tier3;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }


        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("SHADOWDOPPELGANGER_NAME", "Shattered Mirror");
            LanguageAPI.Add("SHADOWDOPPELGANGER_PICKUP", "Gain a doppelganger who mimics your every move.");
            LanguageAPI.Add("SHADOWDOPPELGANGER_DESCRIPTION", "Gain a doppelganger who mimics your every move.");
            //LanguageAPI.Add("SHADOWDOPPELGANGER_LORE", "");
        }

        private static void Hooks()
        {
            
        }

        private static void UnpackAssetBundle()
        {
            //displayPrefab = Main.assetBundle.LoadAsset<GameObject>("DisplaySameEliteDamageBonus");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupMirror");
            //displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matShadowClone");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matMirror");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matDoppelganger");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class ShadowDoppelgangerController : MonoBehaviour
        {
            public GameObject attachedObject;

            public CharacterBody attachedBody
            {
                get
                {
                    return this.attachedObject.GetComponent<CharacterBody>();
                }
            }

            private List<GameObject> doppelgangers = new List<GameObject>();

            private float distance = 1.85f;

            public void AddDoppelganger()
            {
                GameObject doppelganger;

                GameObject body = this.attachedBody.gameObject;

                doppelganger = GameObject.Instantiate(body, body.transform.position, body.transform.rotation);

                doppelganger.transform.localPosition = Vector3.left * distance;
            }

        }

    }
}
