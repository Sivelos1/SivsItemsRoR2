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
    public static class Wheel
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        private static ProcType agonyOnHit;

        public static BuffDef stackingBuffDef;
        public static BuffDef completeBuffDef;

        public static EffectDef readyEffectDef;

        private static GameObject procEffectPrefab;

        public static ConfigEntry<float> hpPercentOnHit;

        public static ConfigEntry<int> baseMaxAgonyStacks;
        public static ConfigEntry<int> stackMaxAgonyStacks;
        public static ConfigEntry<float> damagePerAgonyStack;
        public static ConfigEntry<float> blastRadius;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterProcType();
            RegisterItemDisplayRules();
            RegisterEffects();
            RegisterBuffs();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Wheel of Agony";
            hpPercentOnHit = SivsItemsPlugin.config.Bind<float>(configSection, "HP Cost on Hit", 2.5f, "The amount of your current HP that Wheel of Agony consumes per hit.");
            baseMaxAgonyStacks = SivsItemsPlugin.config.Bind<int>(configSection, "Max Agony", 5, "The amount of Agony stacks required to proc Wheel of Agony.");
            stackMaxAgonyStacks = SivsItemsPlugin.config.Bind<int>(configSection, "Max Agony per Stack", 2, "The amount of Agony stacks the proc requirement is increased by.");
            damagePerAgonyStack = SivsItemsPlugin.config.Bind<float>(configSection, "Damage per Agony Stack", 1f, "The damage coefficient of the damage dealt by Wheel of Agony.");
            blastRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Agony Blast Radius", 5f, "The radius of the blast created by Wheel of Agony.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "Wheel";
            itemDef.nameToken = "WHEEL_NAME";
            itemDef.descriptionToken = "WHEEL_DESCRIPTION";
            itemDef.loreToken = "WHEEL_LORE";
            itemDef.pickupToken = "WHEEL_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texWheelIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Lunar;
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-0.8523F, 0F, -0.8883F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
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
localPos = new Vector3(-1.0635F, 0F, -0.9445F),
localAngles = new Vector3(0F, 90F, 0F),
localScale = new Vector3(1F, 1F, 1F)
                    }
                }
            });
        }

        private static void RegisterProcType()
        {
            agonyOnHit = (ProcType)1000;
        }
        private static void RegisterEffects()
        {
            readyEffectDef = new EffectDef();
            readyEffectDef.prefab = procEffectPrefab;
            SivsItems_ContentPack.effectDefs.Add(readyEffectDef);
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("WHEEL_NAME", "Wheel of Agony");
            LanguageAPI.Add("WHEEL_PICKUP", "<style=cDeath>Sacrifice HP on hit</style> to gain a stack of Agony. Obtaining enough stacks of Agony creates a blast for massive damage.");
            LanguageAPI.Add("WHEEL_DESCRIPTION", "On hit, spend <style=cDeath>"+(hpPercentOnHit.Value)+"% of your current HP</style> to gain a stack of <style=cIsDamage>Agony</style>. At <style=cIsUtility>"+(baseMaxAgonyStacks.Value)+"</style><style=cStack>(+"+(stackMaxAgonyStacks.Value)+" per stack)</style> stack(s) of Agony, your next attack creates a blast for <style=cIsDamage>"+(damagePerAgonyStack.Value * 100f)+"% per stack of Agony</style>. Gaining a stack of Agony <style=cDeath>cannot proc on-damage items</style>.");
            string lore = "The wheel never stops turning." + Environment.NewLine + Environment.NewLine
                + "A young boy watches his father killed before his very eyes." + Environment.NewLine + Environment.NewLine
                + "The young boy grows into a man, wishing to avenge his father by slaying the killer." + Environment.NewLine + Environment.NewLine
                + "The man seeks out his father's killer, who is dining happily with his own family." + Environment.NewLine + Environment.NewLine
                + "Enraged at this sight, the man barges in and makes his move." + Environment.NewLine + Environment.NewLine
                + "A young boy watches his father killed before his very eyes." + Environment.NewLine + Environment.NewLine
                + "And the wheel keeps on turning.";
            LanguageAPI.Add("WHEEL_LORE", lore);
        }
        private static void RegisterBuffs()
        {
            Sprite wheelIcon = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texWheelBuffIcon");

            stackingBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            stackingBuffDef.name = "Agony";
            stackingBuffDef.isDebuff = false;
            stackingBuffDef.canStack = true;
            stackingBuffDef.iconSprite = wheelIcon;
            stackingBuffDef.buffColor = new Color(0.2f, 0.1f, 0.1f);
            stackingBuffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(stackingBuffDef);

            completeBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            completeBuffDef.name = "AgonyReady";
            completeBuffDef.isDebuff = false;
            completeBuffDef.canStack = false;
            completeBuffDef.iconSprite = wheelIcon;
            completeBuffDef.buffColor = new Color(1f, 1f, 1f);
            completeBuffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(completeBuffDef);
        }
        private static void Hooks()
        {
            On.RoR2.CharacterBody.Start += (On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (!self.gameObject.GetComponent<WheelController>())
                {
                    WheelController c = self.gameObject.AddComponent<WheelController>();
                }
            };
            On.RoR2.GlobalEventManager.OnHitEnemy += (On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) =>
            {
                orig.Invoke(self, damageInfo, victim);
                if (damageInfo.attacker)
                {
                    if(!damageInfo.procChainMask.HasProc(agonyOnHit))
                    {
                        WheelController wc = damageInfo.attacker.GetComponent<WheelController>();
                        if (wc != null)
                        {
                            if (wc.wheelCount > 0)
                            {
                                if (!damageInfo.procChainMask.HasProc(agonyOnHit))
                                {
                                    if (!wc.FireAttack(victim.transform.position))
                                    {
                                        wc.DeductHP();
                                        wc.AddStackOfAgony(1);
                                    }
                                    damageInfo.procChainMask.AddProc(agonyOnHit);
                                }
                            }
                        }
                    }
                }
            };
        }


        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayWheel");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupWheel");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            procEffectPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("WheelAgonyBlast");
            procEffectPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matWheel");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matAgonyShockwave");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matAgonyTracer");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matAgonyWaves");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class WheelController : MonoBehaviour
        {
            public GameObject attachedObject
            {
                get
                {
                    return this.gameObject;
                }
            }

            public CharacterBody attachedBody
            {
                get
                {
                    return attachedObject.GetComponent<CharacterBody>();
                }
            }

            public Inventory attachedInventory
            {
                get
                {
                    return attachedBody.inventory;
                }
            }

            public int wheelCount
            {
                get
                {
                    return attachedInventory ? attachedInventory.GetItemCount(Wheel.itemDef) : 0;
                }
            }

            private int agonyStacks;

            private int maxAgonyStacks
            {
                get
                {
                    return baseMaxAgonyStacks.Value + (stackMaxAgonyStacks.Value * (wheelCount - 1));
                }
            }

            public bool agonyBlastReady;

            public void DeductHP()
            {
                Transform transform = this.attachedBody.mainHurtBox ? this.attachedBody.mainHurtBox.transform : this.attachedBody.transform;
                float healthBeforeDamage = this.attachedBody.healthComponent.combinedHealth;
                float damage = this.attachedBody.healthComponent.health * (hpPercentOnHit.Value / 100f);
                ProcChainMask pcm = new ProcChainMask();
                pcm.AddProc(ProcType.Thorns);
                pcm.AddProc(ProcType.HealOnHit);
                DamageInfo di = new DamageInfo
                {
                    attacker = this.attachedObject,
                    crit = false,
                    damageColorIndex = DamageColorIndex.DeathMark,
                    damage = damage,
                    damageType = DamageType.NonLethal,
                    force = Vector3.zero,
                    position = transform.position,
                    procCoefficient = 0f,
                    procChainMask = pcm,
                };
                di.procChainMask.AddProc(agonyOnHit);
                this.attachedBody.healthComponent.TakeDamage(di);
            }
            private void ResetBuffs()
            {

                this.attachedBody.ClearTimedBuffs(stackingBuffDef);
                this.attachedBody.ClearTimedBuffs(completeBuffDef);
                for (int i = 0; i < this.attachedBody.GetBuffCount(stackingBuffDef) + 1; i++)
                {
                    this.attachedBody.RemoveBuff(stackingBuffDef);
                }
                for (int i = 0; i < this.attachedBody.GetBuffCount(completeBuffDef); i++)
                {
                    this.attachedBody.RemoveBuff(completeBuffDef);
                }
            }
            public void AddStackOfAgony(int stacks)
            {
                int lastAgonyStacks = agonyStacks;
                ResetBuffs();
                agonyStacks += stacks;
                if (agonyStacks < 0)
                {
                    agonyStacks = 0;
                    return;
                }
                if (agonyStacks < maxAgonyStacks && lastAgonyStacks >= maxAgonyStacks)
                {
                    agonyBlastReady = false;
                }
                if (agonyStacks >= maxAgonyStacks)
                {
                    agonyStacks = maxAgonyStacks;
                    agonyBlastReady = true;
                }
                AdjustBuffs();

            }
            public void SetAgonyStacks(int stacks)
            {
                int lastAgonyStacks = agonyStacks;
                ResetBuffs();
                agonyStacks = stacks;
                if (agonyStacks < 0)
                {
                    agonyStacks = 0;
                    return;
                }

                if (agonyStacks < maxAgonyStacks && lastAgonyStacks >= maxAgonyStacks)
                {
                    agonyBlastReady = false;
                }
                if (agonyStacks >= maxAgonyStacks)
                {
                    agonyStacks = maxAgonyStacks;
                    agonyBlastReady = true;
                }
                AdjustBuffs();
            }
            private void AdjustBuffs()
            {
                ResetBuffs();
                if (agonyBlastReady)
                {
                    this.attachedBody.ApplyBuff(completeBuffDef.buffIndex);
                    return;
                }
                else
                {
                    this.attachedBody.ApplyBuff(stackingBuffDef.buffIndex, agonyStacks);
                }
            }
            public int GetAgonyStacks()
            {
                return agonyStacks;
            }
            public int GetMaxAgonyStacks()
            {
                return maxAgonyStacks;
            }
            public bool FireAttack(Vector3 position)
            {
                bool result = false;
                if(this.attachedBody != null)
                {
                    if (agonyBlastReady)
                    {
                        EffectManager.SpawnEffect(procEffectPrefab, new EffectData
                        {
                            origin = position,
                        }, true);

                        Util.PlaySound("Play_deathProjectile_exit", this.attachedObject);
                        BlastAttack attack = new BlastAttack();
                        attack.position = position;
                        attack.attacker = this.attachedObject;
                        attack.baseDamage = this.attachedBody.damage * (damagePerAgonyStack.Value * agonyStacks);
                        attack.crit = attachedBody.RollCrit();
                        attack.damageColorIndex = DamageColorIndex.DeathMark;
                        attack.damageType = DamageType.Generic;
                        attack.falloffModel = BlastAttack.FalloffModel.None;
                        attack.procCoefficient = 1f;
                        attack.procChainMask = default(ProcChainMask);
                        attack.procChainMask.AddProc(agonyOnHit);
                        attack.teamIndex = TeamComponent.GetObjectTeam(this.attachedObject);
                        attack.baseForce = 10f;
                        attack.radius = blastRadius.Value;
                        attack.Fire();
                        result = true;
                        SetAgonyStacks(0);
                    }
                }
                return result;
            }
        }

    }
    public static class Monocle
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        public static ConfigEntry<float> baseEliteDamageBonus;
        public static ConfigEntry<float> stackEliteDamageBonus;
        public static ConfigEntry<float> baseNonEliteDamagePenalty;
        public static ConfigEntry<float> stackNonEliteDamagePenalty;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Vintage Monocle";
            baseEliteDamageBonus = SivsItemsPlugin.config.Bind<float>(configSection, "Elite Damage Bonus", 1.2f, "???");
            stackEliteDamageBonus = SivsItemsPlugin.config.Bind<float>(configSection, "Elite Damage Bonus per Stack", 0.2f, "???");
            baseNonEliteDamagePenalty = SivsItemsPlugin.config.Bind<float>(configSection, "Non-Elite Damage Penalty", 0.2f, "???");
            stackNonEliteDamagePenalty = SivsItemsPlugin.config.Bind<float>(configSection, "Non-Elite Damage Penalty per Stack", 0.2f, "???");


        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "Monocle";
            itemDef.nameToken = "MONOCLE_NAME";
            itemDef.descriptionToken = "MONOCLE_DESCRIPTION";
            itemDef.loreToken = "MONOCLE_LORE";
            itemDef.pickupToken = "MONOCLE_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texGeodeIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Lunar;
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
                        childName = "Base",
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = displayPrefab,
                        localAngles = new Vector3(0f, 0f, 0f),
                        localPos = new Vector3(0f, 0f, 0f),
                        localScale = Vector3.one * 1f
                    }
                }
            });
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["HuntressBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["Bandit2Body"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["ToolbotBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["MageBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["TreebotBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["LoaderBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["MercBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CaptainBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CrocoBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EngiBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EngiTurretBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EquipmentDroneBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["ScavBody"], itemDef, new DisplayRuleGroup
            {
                rules = new ItemDisplayRule[1]
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


        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("MONOCLE_NAME", "Vintage Monocle");
            LanguageAPI.Add("MONOCLE_PICKUP", "Deal more damage to elite monsters. <style=cDeath>Deal less damage to non-elite monsters.</style>");
            LanguageAPI.Add("MONOCLE_DESCRIPTION", "On hit, deal <style=cIsDamage>+"+(baseEliteDamageBonus.Value * 100f)+"%</style><style=cStack>(+"+(stackEliteDamageBonus.Value * 100f)+"% per stack)</style> to Elite monsters. However, you also deal <style=cDeath>"+((1-baseNonEliteDamagePenalty.Value) * 100f)+"% LESS damage</style><style=cStack>(+"+((1-stackNonEliteDamagePenalty.Value)*100f)+"% per stack)</style> to non-Elite monsters.");
            string lore = "???";
            //LanguageAPI.Add("MONOCLE_LORE", lore);
        }
        private static void Hooks()
        {
            
        }


        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayMonocle");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupMonocle");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matMonocle");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
        }

    }
    public static class PiggyBank
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;


        private static ConfigEntry<float> baseCoinModifier;
        private static ConfigEntry<float> stackCoinModifier;

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
            string configSection = "Boarlit Bank";
            baseCoinModifier = SivsItemsPlugin.config.Bind<float>(configSection, "Coin Modifier", 0.25f, "The multiplier for deposited Lunar Coins. 0.25 equals a +25% bonus.");
            stackCoinModifier = SivsItemsPlugin.config.Bind<float>(configSection, "Coin Modifier per Stack", 0.25f, "The amount of coin bonus you get per stack.");
        }

        private static void RegisterItem()
        {

            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "Piggybank";
            itemDef.nameToken = "PIGGYBANK_NAME";
            itemDef.descriptionToken = "PIGGYBANK_DESCRIPTION";
            itemDef.loreToken = "PIGGYBANK_LORE";
            itemDef.pickupToken = "PIGGYBANK_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Utility, ItemTag.BrotherBlacklist, ItemTag.AIBlacklist };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Lunar;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }


        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("PIGGYBANK_NAME", "Boarlit Bank");
            LanguageAPI.Add("PIGGYBANK_PICKUP", "All future Lunar Coins picked up are deposited in the bank. After beating your run, gain all coins in the bank, with interest.");
            LanguageAPI.Add("PIGGYBANK_DESCRIPTION", "All future Lunar Coins picked up are <style=cIsUtility>deposited in the bank</style>. After <style=cIsDamage>successfully completing</style> your run, gain <style=cIsUtility>"+((1+baseCoinModifier.Value)*100)+"%</style> <style=cStack>(+"+((stackCoinModifier.Value*100))+" per stack)</style> of your bank balance, rounded down.");
            LanguageAPI.Add("RUNCOMPLETE_PIGGYBANK_REWARD_LOCAL", "Your Boarlit Bank shatters, revealing {1} Lunar Coin(s).");
            LanguageAPI.Add("RUNFAILURE_PIGGYBANK_LOSS_LOCAL", "You feel your bank balance disappear into the void.");
            LanguageAPI.Add("PIGGYBANK_LORE", "A relic from the past.\n\nCrafted with clay, shaped by two, baked twice over. Sloppy craftsmanship, but functional.\n\nI remember when we made this, together. Brother, he insisted we paint the relic. \"Gives it more character\", he said.\n\nHow silly. A relic like this needs no character or soul.\n\n...I wonder... I wonder if it still has some coin in it.");
        }

        private static void Hooks()
        {
            On.RoR2.CharacterMaster.Awake += (On.RoR2.CharacterMaster.orig_Awake orig, CharacterMaster self) =>
            {
                orig.Invoke(self);
                LunarPiggyBankWallet wallet = self.gameObject.GetComponent<LunarPiggyBankWallet>();
                if (!wallet)
                {
                    wallet = self.gameObject.AddComponent<LunarPiggyBankWallet>();
                    wallet.attachedObject = self.gameObject;
                }
            };
            On.RoR2.Run.BeginGameOver += (On.RoR2.Run.orig_BeginGameOver orig, Run self, GameEndingDef gameEndingDef) =>
            {
                orig.Invoke(self, gameEndingDef);
                bool hasBank = false;
                NetworkUser localUser = null;
                foreach (var player in NetworkUser.readOnlyInstancesList)
                {
                    if (player.isClient && player.isLocalPlayer)
                    {
                        int count = player.master.inventory.GetItemCount(itemDef);
                        if (count > 0)
                        {
                            localUser = player;
                            hasBank = true;
                        }
                    }
                }
                if (hasBank && localUser != null)
                {
                    LunarPiggyBankWallet wallet = localUser.masterObject.GetComponent<LunarPiggyBankWallet>();
                    if (gameEndingDef.isWin)
                    {
                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                        {
                            baseToken = "RUNCOMPLETE_PIGGYBANK_REWARD_LOCAL",
                            subjectAsNetworkUser = localUser,
                            paramTokens = new string[]
                            {
                                wallet.coinReward.ToString()
                            }
                        });
                        localUser.AwardLunarCoins(wallet.coinReward);
                    }
                    else
                    {
                        Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                        {
                            baseToken = "RUNFAILURE_PIGGYBANK_LOSS_LOCAL",
                            subjectAsNetworkUser = localUser,
                        });
                    }
                }
            };
        }


        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayPiggyBank");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupPiggyBank");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matPiggybank");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
        }

        public class LunarPiggyBankWallet : MonoBehaviour
        {
            public GameObject attachedObject;

            public CharacterMaster attachedMaster
            {
                get
                {
                    return attachedObject.GetComponent<CharacterMaster>();
                }
            }

            public NetworkUser attachedNetworkUser
            {
                get
                {
                    return NetworkUser.readOnlyInstancesList.ToList<NetworkUser>().Find(n => n.master == this.attachedMaster);
                }
            }

            private uint coinCountOnInitialPickup;

            [SyncVar]
            private uint coinBalance;

            private int bankCount
            {
                get
                {
                    if (this.attachedMaster)
                    {
                        if (this.attachedMaster.inventory)
                        {
                            return this.attachedMaster.inventory.GetItemCount(itemDef);
                        }
                    }
                    return 0;
                }
            }
        
            public float coinModifier
            {
                get
                {
                    return ((1 + baseCoinModifier.Value) + (stackCoinModifier.Value * (this.bankCount - 1)));
                }
            }

            public uint coinReward
            {
                get
                {
                    return (uint)Mathf.Floor((float)coinBalance * coinModifier);
                }
            }

            public void AddCoinsToBalance(uint count)
            {
                this.coinBalance = HGMath.UintSafeAdd(this.coinBalance, count);
            }
            public void SubtractCoinsFromBalance(uint count)
            {
                this.coinBalance = HGMath.UintSafeSubtract(this.coinBalance, count);
            }

            public void SetInitialCoinCount(uint count)
            {
                this.coinCountOnInitialPickup = count;
            }

            private void Awake()
            {
                this.coinBalance = 0;
            }
        }
    }
}
