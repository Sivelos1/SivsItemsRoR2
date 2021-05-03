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
    public static class SameEliteDamageBonus
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        private static GameObject hitEffectPrefab;
        private static EffectDef hitEffectDef;

        private static ConfigEntry<float> damageBonusMultiplier;
        private static ConfigEntry<float> baseStealChance;
        private static ConfigEntry<float> stackStealChance;
        private static ConfigEntry<float> stealDuration;



        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterEffects();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Know Thy Enemy";
            damageBonusMultiplier = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Multiplier", 1.75f, "The multiplier used when Know Thy Enemy takes effect. The damage calculation per stack is EXPONENTIAL, so take care when editing this value.");
            baseStealChance = SivsItemsPlugin.config.Bind<float>(configSection, "Proc Chance", 10f, "The chance, out of 100, that a slain elite will yield its aspect.");
            stackStealChance = SivsItemsPlugin.config.Bind<float>(configSection, "Proc Chance per Stack", 2.5f, "The chance, out of 100, added to Know Thy Enemy's proc chance when stacking it.");
            stealDuration = SivsItemsPlugin.config.Bind<float>(configSection, "Steal Duration", 5f, "The amount of time, in seconds, a stolen elite aspect will last.");
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "SameEliteDamageBonus";
            itemDef.nameToken = "SAMEELITEDAMAGEBONUS_NAME";
            itemDef.descriptionToken = "SAMEELITEDAMAGEBONUS_DESCRIPTION";
            itemDef.loreToken = "SAMEELITEDAMAGEBONUS_LORE";
            itemDef.pickupToken = "SAMEELITEDAMAGEBONUS_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texSameEliteDamageBonus.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Tier2;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
            Dictionary<string, ItemDisplayRuleSet> vitalIdrs = ItemDisplays.GetVitalBodiesIDRS();
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CommandoBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["HuntressBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["Bandit2Body"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["ToolbotBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["MageBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["TreebotBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["LoaderBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["MercBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CaptainBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CrocoBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EngiBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EngiTurretBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["ScavBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
                {
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        childName = "Base",
localPos = new Vector3(0F, 0F, -1.1529F),
localAngles = new Vector3(90F, 0F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
        }
        private static void RegisterEffects()
        {
            hitEffectDef = new EffectDef
            {
                prefab = hitEffectPrefab,
            };
            SivsItems_ContentPack.effectDefs.Add(hitEffectDef);
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("SAMEELITEDAMAGEBONUS_NAME", "Know Thy Enemy");
            LanguageAPI.Add("SAMEELITEDAMAGEBONUS_PICKUP", "Enemies who are the same elite type as you take bonus damage. Chance on kill to steal a slain elite's aspect.");
            LanguageAPI.Add("SAMEELITEDAMAGEBONUS_DESCRIPTION", "Enemies who have the same <style=cIsUtility>elite affix as you</style> take <style=cIsDamage>"+(damageBonusMultiplier.Value*100)+"% TOTAL damage</style> <style=cStack>(+"+(damageBonusMultiplier.Value*100)+"% per stack)</style>. On kill, <style=cIsDamage>"+baseStealChance.Value+"% chance</style> <style=cStack>(+"+stackStealChance.Value+"% per stack)</style> to steal a slain elite's aspect for <style=cIsUtility>"+stealDuration.Value+" second(s)</style>.");
            //LanguageAPI.Add("SAMEELITEDAMAGEBONUS_LORE", "");
        }

        private static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) => {
                orig.Invoke(self, damageReport);
                if(damageReport != null)
                {
                    if (damageReport.attacker != null)
                    {
                        if (damageReport.attackerBody != null)
                        {
                            if(damageReport.attackerBody.inventory != null)
                            {
                                if (damageReport.attackerBody.inventory.GetItemCount(itemDef) > 0)
                                {
                                    if (damageReport.victim != null)
                                    {
                                        if (damageReport.victimIsElite)
                                        {
                                            float duration = stealDuration.Value;
                                            for (int k = 0; k < BuffCatalog.eliteBuffIndices.Length; k++)
                                            {
                                                BuffIndex buffType = BuffCatalog.eliteBuffIndices[k];
                                                if (damageReport.victimBody.HasBuff(buffType))
                                                {
                                                    float chance = baseStealChance.Value + (stackStealChance.Value * (damageReport.attackerBody.inventory.GetItemCount(itemDef) - 1));
                                                    if (Util.CheckRoll(chance, damageReport.attackerMaster))
                                                    {
                                                        damageReport.attackerBody.AddTimedBuff(buffType, duration);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
            };
            On.RoR2.HealthComponent.TakeDamage += (On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) =>
            {
                if(SameEliteDamageBonusCheck(damageInfo.attacker, self.gameObject))
                {
                    if (damageInfo.attacker)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (attackerBody)
                        {
                            int count = attackerBody.inventory.GetItemCount(itemDef);
                            if(count > 0)
                            {
                                EffectManager.SimpleEffect(hitEffectPrefab, damageInfo.position, Quaternion.identity, true);
                                damageInfo.damageColorIndex = DamageColorIndex.Item;
                                damageInfo.damage *= Mathf.Pow(damageBonusMultiplier.Value, count);
                            }
                        }
                    }
                }
                orig.Invoke(self, damageInfo);
            };
        }
        public static bool SameEliteDamageBonusCheck(GameObject attacker, GameObject victim)
        {
            if(attacker && victim)
            {
                CharacterBody attackerBody = attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                if(attackerBody && victimBody)
                {
                    Inventory i = attackerBody.inventory;
                    if (i)
                    {
                        int count = i.GetItemCount(itemDef);
                        if(count > 0)
                        {
                            foreach (var eliteBuff in BuffCatalog.eliteBuffIndices)
                            {
                                if (attackerBody.HasBuff(eliteBuff) && victimBody.HasBuff(eliteBuff))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    
                }
            }
            return false;
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplaySameEliteDamageBonus");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupSameEliteDamageBonus");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            hitEffectPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("SameEliteDamageEffect");
            hitEffectPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matEliteDamageBonusInside");
            mat.shader = Shader.Find("Hopoo Games/FX/Opaque Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matEliteDamageBonus");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matOmniHitsparkSameElite");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matSameEliteImpactTrail");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matSameElitePassiveParticles");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

    }
}
