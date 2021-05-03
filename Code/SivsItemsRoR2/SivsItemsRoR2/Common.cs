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
    public static class GlassShield
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        private static GameObject shieldUpEffectPrefab;

        private static GameObject shieldBreakEffectPrefab;

        public static BuffIndex activeBuffIndex { get; private set; }
        public static BuffIndex onCooldownBuffIndex { get; private set; }

        private static ConfigEntry<int> usePerStack;

        private static ConfigEntry<float> cooldown;



        public static void Init()
        {
            UnpackAssetBundle();
            RegisterEffects();
            RegisterBuffs();
            RegisterLanguageTokens();
            RegisterItem();
            Hooks();
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "GlassShield";
            itemDef.nameToken = "GLASSSHIELD_NAME";
            itemDef.descriptionToken = "GLASSSHIELD_DESCRIPTION";
            itemDef.loreToken = "GLASSSHIELD_LORE";
            itemDef.pickupToken = "GLASSSHIELD_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Utility };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Tier1;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterEffects()
        {
            /*EffectAPI.AddEffect(new EffectDef
            {
                prefab = shieldUpEffectPrefab,
                prefabEffectComponent = shieldUpEffectPrefab.GetComponent<EffectComponent>(),
                prefabVfxAttributes = shieldUpEffectPrefab.GetComponent<VFXAttributes>(),
                prefabName = shieldUpEffectPrefab.name,
                spawnSoundEventName = shieldUpEffectPrefab.GetComponent<EffectComponent>().soundName
            });

            EffectAPI.AddEffect(new EffectDef
            {
                prefab = shieldBreakEffectPrefab,
                prefabEffectComponent = shieldBreakEffectPrefab.GetComponent<EffectComponent>(),
                prefabVfxAttributes = shieldBreakEffectPrefab.GetComponent<VFXAttributes>(),
                prefabName = shieldBreakEffectPrefab.name,
                spawnSoundEventName = shieldBreakEffectPrefab.GetComponent<EffectComponent>().soundName
            });*/
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("GLASSSHIELD_NAME", "Glass Shield");
            LanguageAPI.Add("GLASSSHIELD_PICKUP", "Blocks all attacks, but breaks upon use. Recharges after "+cooldown+" seconds.");
            LanguageAPI.Add("GLASSSHIELD_DESCRIPTION", "<style=cUtility>Blocks all attacks</style>, breaking after <style=cIsDamage>1 use</style> <style=cStack>(+"+usePerStack+" use per stack)</style>. Recharges after <style=cIsUtility>"+cooldown+" seconds</style>.");
            LanguageAPI.Add("GLASSSHIELD_LORE", "Order: Glass Replica of Antique Shield\nTracking Number: 15***********\nEstimated Delivery: 04/05/2056\nShipping Method: Fragile\nShipping Address: Territory Museum, Io, Saturn System\nShipping Details:\nPrecious glass recreation of antique relic dating back to the War of 2019. Valued at 5.3 billion credits.\n\nEXTREMELY FRAGILE. Do NOT break.");
        }

        private static void RegisterBuffs()
        {
            BuffDef activeBuff = new BuffDef()
            {
                buffColor = new Color(0.85f, 1f, 0.9f),
                canStack = true,
                iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield"),
                isDebuff = false,
                name = "Glass Shield"
            };
            BuffDef inactiveBuff = new BuffDef()
            {
                buffColor = new Color(0.6375f, 0.75f, 0.675f),
                canStack = true,
                iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffPulverizeIcon"),
                isDebuff = false,
                name = "Glass Shield is on Cooldown"
            };
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.Awake += (On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                GlassShieldBehaviour gsb = self.gameObject.AddComponent<GlassShieldBehaviour>();
                gsb.attachedObject = self.gameObject;
            };
        }

        private static void UnpackAssetBundle()
        {
            //displayPrefab = Main.assetBundle.LoadAsset<GameObject>("DisplaySameEliteDamageBonus");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupGlassShield");
            //displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            shieldUpEffectPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("GlassShieldUpEffect");
            shieldUpEffectPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGlassShield");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGlassDistortion");
            mat.shader = Shader.Find("Hopoo Games/FX/Distortion");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGlassShieldEffect");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class GlassShieldBehaviour : MonoBehaviour
        {
            public GameObject attachedObject;

            public CharacterBody attachedBody
            {
                get
                {
                    return this.attachedObject.GetComponent<CharacterBody>();
                }
            }

            public Inventory attachedInventory
            {
                get
                {
                    return this.attachedBody.inventory;
                }
            }

            public int shieldCount
            {
                get
                {
                    if (this.attachedInventory)
                    {
                        return this.attachedInventory.GetItemCount(itemDef);
                    }
                    return 0;
                }
            }

            public int shieldCharges { get; private set; }

            public bool IsOnCooldown
            {
                get
                {
                    return this.attachedBody.HasBuff(onCooldownBuffIndex);
                }
            }

            public void AddShieldCharge(int count)
            {
                this.shieldCharges += count;
                AdjustBuffs();
            }

            public void SpendShieldCharge(int count)
            {
                this.shieldCharges -= count;
                AdjustBuffs();
            }

            private void AdjustBuffs()
            {
                int activeBuffStacks = this.attachedBody.GetBuffCount(activeBuffIndex);
                int inactiveBuffStacks = this.attachedBody.GetBuffCount(onCooldownBuffIndex);
                if (activeBuffStacks != shieldCharges && shieldCharges > 0)
                {
                    this.attachedBody.RemoveBuff(activeBuffIndex);
                    this.attachedBody.ApplyBuff(activeBuffIndex, shieldCharges);
                }
                if(shieldCharges <= 0 && inactiveBuffStacks <= 0)
                {
                    for (int i = 0; i < cooldown.Value; i++)
                    {
                        this.attachedBody.ApplyBuff(onCooldownBuffIndex, 1, (cooldown.Value - i));
                    }
                }
            }
        }
    }
}
