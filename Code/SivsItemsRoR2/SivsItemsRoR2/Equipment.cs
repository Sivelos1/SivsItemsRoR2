﻿using System;
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
    public static class Heart
    {
        public static EquipmentDef equipDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        private static GameObject shieldUpPrefab;
        private static EffectDef shieldUpDef;

        private static ConfigEntry<float> cooldown;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterEffects();
            RegisterLanguageTokens();
            RegisterItemDisplayRules();
            RegisterEquipment();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Guardians Pacemaker";
            cooldown = SivsItemsPlugin.config.Bind<float>(configSection, "Cooldown", 30f, "The amount of time, in seconds, it takes for Guardians Pacemaker to cooldown.");
        }

        private static void RegisterEquipment()
        {
            equipDef = ScriptableObject.CreateInstance<EquipmentDef>();
            equipDef.name = "ShieldUpOnUse";
            equipDef.nameToken = "SHIELDUPONUSE_NAME";
            equipDef.descriptionToken = "SHIELDUPONUSE_DESCRIPTION";
            equipDef.loreToken = "SHIELDUPONUSE_LORE";
            equipDef.pickupModelPrefab = pickupPrefab;
            equipDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texHeartIcon.png");
            equipDef.cooldown = cooldown.Value;
            equipDef.appearsInMultiPlayer = true;
            equipDef.appearsInSinglePlayer = true;
            equipDef.isBoss = false;
            equipDef.isLunar = false;
            equipDef.canDrop = true;
            equipDef.enigmaCompatible = true;
            SivsItemsPlugin.allEquipDefs.Add(equipDef);
        }
        
        private static void RegisterItemDisplayRules()
        {
            Dictionary<string, ItemDisplayRuleSet> vitalIdrs = ItemDisplays.GetVitalBodiesIDRS();
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["CommandoBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["HuntressBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["Bandit2Body"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["ToolbotBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["MageBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["TreebotBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["LoaderBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["MercBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["CaptainBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["CrocoBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["EngiBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["EngiTurretBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["EquipmentDroneBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddEquipDisplayToIDRS(vitalIdrs["ScavBody"], equipDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[]
                {
                    new ItemDisplayRule
                    {
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
        }

        private static void RegisterEffects()
        {
            shieldUpDef = new EffectDef
            {
                prefab = shieldUpPrefab,
            };
            SivsItems_ContentPack.effectDefs.Add(shieldUpDef);
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("SHIELDUPONUSE_NAME", "Guardian's Pacemaker");
            LanguageAPI.Add("SHIELDUPONUSE_PICKUP", "Refresh your shields on use.");
            LanguageAPI.Add("SHIELDUPONUSE_DESCRIPTION", "Refresh the cooldown for your shields, regenerating <style=cIsHealing>100%</style> of your <style=cIsUtility>Max Shields</style>.");
            //LanguageAPI.Add("SHIELDUPONUSE_LORE", "A");
        }

        private static void Hooks()
        {

            On.RoR2.EquipmentSlot.PerformEquipmentAction += (On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef) =>
            {
                if(equipmentDef == equipDef)
                {
                    HealthComponent hc = self.gameObject.GetComponent<HealthComponent>();
                    if (hc != null)
                    {
                        hc.RechargeShieldFull();

                        EffectData effectData = new EffectData();
                        effectData.origin = self.gameObject.transform.position;
                        effectData.SetNetworkedObjectReference(self.gameObject);
                        EffectManager.SpawnEffect(shieldUpPrefab, effectData, true);
                        return true;
                    }
                    return false;
                }
                else
                {
                    return orig.Invoke(self, equipmentDef);
                }
            };
        }


        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayHeart");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupHeart");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            shieldUpPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("ShieldUpEffect");
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matHeart");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matPaceMaker");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matShieldShards");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matShieldUpLightning");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

    }
}
