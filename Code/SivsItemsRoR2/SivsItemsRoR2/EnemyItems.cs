using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Skills;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2.UI.LogBook;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using R2API.Networking;
using HarmonyLib;
using SivsItems;
using SivsItemsRoR2;

namespace SivsItemsRoR2
{
    public static class Tentacle
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;


        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> procChance;
        private static ConfigEntry<float> damagePerSecond;
        private static ConfigEntry<float> baseLength;
        private static ConfigEntry<float> stackLength;

        private static ConfigEntry<int> baseAmount;
        private static ConfigEntry<int> stackAmount;

        private static ConfigEntry<float> tickRate;
        private static ConfigEntry<float> tetherRadius;

        private static GameObject tetherPrefab;



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
            string configSection = "Frayed Tentacle";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            procChance = SivsItemsPlugin.config.Bind<float>(configSection, "Proc Chance (0 - 1)", 0.5f, "How often the item should proc, on a scale of 0 (0%) to 1 (100%).");
            damagePerSecond = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Per Second", 1f, "How much damage the tether will do overall. Used as a multiplier for the damage that procced the tether; i.e., 1 = 100% damage.");
            baseLength = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Duration", 3f, "How long, in seconds, the tether will last.");
            stackLength = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Duration per Stack", 0.5f, "How long, in seconds, each stack will extend the tether's duration.");
            baseAmount = SivsItemsPlugin.config.Bind<int>(configSection, "Tether Amount", 1, "How many tethers a single character can have.");
            stackAmount = SivsItemsPlugin.config.Bind<int>(configSection, "Tether Amount per Stack", 1, "How many tethers, per stack, a character's cap is increased by.");
            tickRate = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Tick Rate", 0.25f, "The interval in which tethers apply damage. Smaller values means quicker damage.");
            tetherRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Radius", 65f, "Tethers can be up to this value, in meters, in length before breaking.");
        }

        private static void RegisterItem()
        {

            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "Tentacle";
            itemDef.nameToken = "TENTACLE_NAME";
            itemDef.descriptionToken = "TENTACLE_DESCRIPTION";
            itemDef.loreToken = "TENTACLE_LORE";
            itemDef.pickupToken = "TENTACLE_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.Utility };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "HeadCenter",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(30f, 0f, 0f),
                    localPos = new Vector3(0f, -0.003f, -0.122f),
                    localScale = Vector3.one * 0.5f
                }
            });/*
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });*/
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("TENTACLE_NAME", "Frayed Tentacle");
            LanguageAPI.Add("TENTACLE_PICKUP", "Chance to tether yourself to an enemy.");
            LanguageAPI.Add("TENTACLE_DESCRIPTION", "<style=cIsUtility>" + (procChance.Value * 100) + "% chance on hit</style> to tether yourself to an enemy, dealing <style=cIsDamage>" + (damagePerSecond.Value * 100) + "% TOTAL damage</style> per second for <style=cIsUtility>" + baseLength.Value + " seconds</style> <style=cStack>(+" + stackAmount.Value + " second per stack)</style>. Tether up to <style=cIsUtility>" + baseAmount.Value + "</style> enemy <style=cStack>(+" + stackAmount.Value + " per stack)</style> at a time.");
            //LanguageAPI.Add("TENTACLE_LORE", "A");
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.Start += (On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (!self.gameObject.GetComponent<TentacleTetherController>())
                {
                    TentacleTetherController c = self.gameObject.AddComponent<TentacleTetherController>();
                    c.attachedObject = self.gameObject;
                }
            };
            On.RoR2.GlobalEventManager.OnHitEnemy += (On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) =>
            {
                orig.Invoke(self, damageInfo, victim);
                if (damageInfo.attacker && victim)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (attackerBody)
                    {
                        if (attackerBody.inventory)
                        {
                            int count = attackerBody.inventory.GetItemCount(itemDef);
                            if (attackerBody.gameObject && count > 0)
                            {
                                TentacleTetherController tetherController = attackerBody.gameObject.GetComponent<TentacleTetherController>();
                                if (tetherController)
                                {
                                    if (tetherController.tethers.Count < tetherController.maxTethers && damageInfo.procCoefficient > 0)
                                    {
                                        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                                        if (victimBody)
                                        {
                                            if (!tetherController.IsAlreadyTethered(victimBody.healthComponent))
                                            {
                                                float distance = Vector3.Distance(attackerBody.transform.position, victimBody.transform.position);
                                                if(distance <= tetherRadius.Value)
                                                {
                                                    float chance = (procChance.Value * 100) * damageInfo.procCoefficient;
                                                    if (Util.CheckRoll(chance, attackerBody.master))
                                                    {
                                                        GameObject newTether = GameObject.Instantiate(tetherPrefab, attackerBody.gameObject.transform.position, Quaternion.identity, attackerBody.gameObject.transform);
                                                        TentacleTether tether = newTether.GetComponent<TentacleTether>();
                                                        tether.attachedController = tetherController;
                                                        tetherController.tethers.Add(tether);
                                                        tether.owner = attackerBody.gameObject;
                                                        tether.target = victimBody.mainHurtBox;
                                                        tether.lifeTime = baseLength.Value + (stackLength.Value * (count - 1));
                                                        tether.baseDamage = (damageInfo.damage * damagePerSecond.Value) * damageInfo.procCoefficient;
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
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayTentacle");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupTentacle");
            tetherPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("LightningTether");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            tetherPrefab.AddComponent<NetworkIdentity>();
            tetherPrefab.AddComponent<TentacleTether>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matTentacle");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matTentacleLightning");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class TentacleTetherController : MonoBehaviour
        {
            public GameObject attachedObject;
            private CharacterBody attachedBody
            {
                get
                {
                    return this.attachedObject.GetComponent<CharacterBody>();
                }
            }
            public int maxTethers
            {
                get
                {
                    int count = this.attachedBody.inventory.GetItemCount(itemDef);
                    return baseAmount.Value + (stackAmount.Value * (count - 1));
                }
            }

            public List<TentacleTether> tethers = new List<TentacleTether>();

            public bool IsAlreadyTethered(HealthComponent healthComponent)
            {
                return tethers.Find(n => n.target.healthComponent == healthComponent);
            }
        }

        public class TentacleTether : MonoBehaviour
        {
            public GameObject owner;

            public TentacleTetherController attachedController;

            private CharacterBody ownerBody
            {
                get
                {
                    return this.owner.GetComponent<CharacterBody>();
                }
            }

            public HurtBox target;

            public float baseDamage;

            private GameObject targetObject
            {
                get
                {
                    return target.healthComponent.gameObject;
                }
            }

            public float lifeTime;

            private float stopwatch;

            private float tickStopwatch;
            private float damagePerTick
            {
                get
                {
                    return (Tentacle.damagePerSecond.Value * tickRate.Value);
                }
            }

            private void Start()
            {
                if (!this.targetObject || !this.owner || !this.attachedController)
                {
                    GameObject.DestroyImmediate(this);
                    return;
                }
                this.stopwatch = 0f;
                this.tickStopwatch = 0f;
            }

            private void FixedUpdate()
            {
                this.stopwatch += Time.fixedDeltaTime;
                this.tickStopwatch += Time.fixedDeltaTime;
                float distance = Vector3.Distance(this.ownerBody.transform.position, this.target.transform.position);
                if (!this.target.healthComponent || !this.target.healthComponent.alive || distance >= tetherRadius.Value || this.stopwatch >= this.lifeTime)
                {
                    this.attachedController.tethers.Remove(this);
                    GameObject.DestroyImmediate(this.gameObject);
                }
                if (this.tickStopwatch >= tickRate.Value && this.target.healthComponent.alive)
                {
                    OrbManager.instance.AddOrb(new LightningOrb()
                    {
                        arrivalTime = tickRate.Value,
                        attacker = this.owner,
                        bouncesRemaining = 0,
                        lightningType = LightningOrb.LightningType.Ukulele,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 0f,
                        damageCoefficientPerBounce = 1f,
                        damageColorIndex = DamageColorIndex.Item,
                        inflictor = this.owner,
                        origin = this.ownerBody.mainHurtBox.transform.position,
                        target = this.target,
                        range = tetherRadius.Value,
                        isCrit = this.ownerBody.RollCrit(),
                        teamIndex = this.ownerBody.teamComponent.teamIndex,
                        damageValue = this.damagePerTick * this.baseDamage,
                        damageType = DamageType.Generic,
                        bouncedObjects = new List<HealthComponent>(),
                        speed = 50f,
                        targetsToFindPerBounce = 0,
                    });
                    this.tickStopwatch = 0f;
                }
            }


        }


    }
    public static class MiniWispOnKill
    {
        public static ItemDef itemDef;
        private static GameObject pickupPrefab;

        public static GameObject miniWispBody;
        public static GameObject miniWispMaster;
        public static CharacterSpawnCard miniWispSpawnCard;


        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<int> baseSummonLimit;
        private static ConfigEntry<int> stackSummonLimit;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterBody();
            CreateSpawnCard();
            RegisterItem();
            RegisterItemDisplayRules();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Abandoned Wisp";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseSummonLimit = SivsItemsPlugin.config.Bind<int>(configSection, "Summon Limit", 1, "The amount of Little Wisps that can be summoned at a time.");
            stackSummonLimit = SivsItemsPlugin.config.Bind<int>(configSection, "Summon Limit per Stack", 1, "The summon limit on Little Wisps will be increased by this number.");
        }


        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "MiniWispOnKill";
            itemDef.nameToken = "MINIWISPONKILL_NAME";
            itemDef.descriptionToken = "MINIWISPONKILL_DESCRIPTION";
            itemDef.loreToken = "MINIWISPONKILL_LORE";
            itemDef.pickupToken = "MINIWISPONKILL_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texMiniWispIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.Utility, ItemTag.OnKillEffect };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                
            });

            SivsItemsPlugin.allItemDefs.Add(itemDef);

        }
        private static void RegisterItemDisplayRules()
        {
        }

        private static void CreateSpawnCard()
        {
            miniWispSpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
            miniWispSpawnCard.directorCreditCost = 0;
            miniWispSpawnCard.forbiddenAsBoss = true;
            miniWispSpawnCard.prefab = miniWispMaster;
            miniWispSpawnCard.noElites = false;
            miniWispSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Air;
        }

        private static void RegisterBody()
        {
            SkillLocator component = miniWispBody.GetComponent<SkillLocator>();

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Wisp1Monster.ChargeEmbers));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 2f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.skillName = "FireEmbers";


            component.primary = miniWispBody.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.primary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            SivsItems_ContentPack.skillDefs.Add(mySkillDef);
            SivsItems_ContentPack.skillFamilies.Add(newFamily);
            SivsItems_ContentPack.bodyPrefabs.Add(miniWispBody);
            SivsItems_ContentPack.masterPrefabs.Add(miniWispMaster);
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("MINIWISP_BODY_NAME", "Little Wisp");
            LanguageAPI.Add("MINIWISPONKILL_NAME", "Abandoned Wisp");
            LanguageAPI.Add("MINIWISPONKILL_PICKUP", "Summon little wisps on kill.");
            LanguageAPI.Add("MINIWISPONKILL_DESCRIPTION", "On kill, summon a <style=cIsDamage>Little Wisp</style> that fights along side you. Have up to <style=cIsUtility>"+baseSummonLimit.Value+"</style> Little Wisp <style=cStack>(+"+stackSummonLimit.Value+" per stack)</style> at a time.");
            //LanguageAPI.Add("MINIWISPONKILL_LORE", "A");
        }

        private static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) =>
            {
                orig.Invoke(self, damageReport);
                if(damageReport != null)
                {
                    if(damageReport.attacker != null)
                    {
                        CharacterBody attacker = damageReport.attackerBody;
                        if (attacker)
                        {
                            Inventory inventory = attacker.inventory;
                            if (inventory)
                            {
                                int count = inventory.GetItemCount(itemDef);
                                if (count > 0)
                                {

                                    CharacterMaster master = damageReport.attackerMaster;
                                    if (master)
                                    {
                                        int wispCount = 0;
                                        int summonLimit = baseSummonLimit.Value + (stackSummonLimit.Value * (count - 1));
                                        CharacterMaster prefabMaster = miniWispMaster.GetComponent<CharacterMaster>();
                                        CharacterMaster[] allies = new CharacterMaster[CharacterMaster.readOnlyInstancesList.Count];
                                        CharacterMaster.readOnlyInstancesList.CopyTo(allies, 0);
                                        foreach (var item in allies.ToList<CharacterMaster>().FindAll(n => n.teamIndex == master.teamIndex))
                                        {
                                            if (item.masterIndex == prefabMaster.masterIndex)
                                            {
                                                wispCount++;
                                            }
                                        }
                                        //Chat.AddMessage("Character "+master.name+" has "+wispCount+" / "+summonLimit+" wisp(s)");
                                        if (wispCount < summonLimit)
                                        {
                                            SpawnCard spawnCard = miniWispSpawnCard;
                                            DirectorPlacementRule placementRule = new DirectorPlacementRule
                                            {
                                                placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                                                minDistance = 5f,
                                                maxDistance = 10f,
                                                position = attacker.gameObject.transform.position
                                            };
                                            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng);
                                            directorSpawnRequest.teamIndexOverride = new TeamIndex?(attacker.teamComponent.teamIndex);
                                            directorSpawnRequest.summonerBodyObject = attacker.gameObject;
                                            directorSpawnRequest.ignoreTeamMemberLimit = true;
                                            DirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
                                            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
        }
        private static void UnpackAssetBundle()
        {
            miniWispBody = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("MiniWispBody");
            miniWispMaster = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("MiniWispMaster");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupAbandonedWisp");
            miniWispBody.AddComponent<NetworkIdentity>();
            miniWispMaster.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matMiniWisp");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matWispNapkin");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matMiniWispFire");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matMiniWispGlow");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matInverseDistortion2");
            mat.shader = Shader.Find("Hopoo Games/FX/Distortion");
        }
    }
    public static class Geode
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;
        private static BuffDef buffDef;


        public static ConfigEntry<float> dropChance;
        private static ConfigEntry<float> baseBuffLength;
        private static ConfigEntry<float> stackBuffLength;
        private static ConfigEntry<float> baseArmorBuff;
        private static ConfigEntry<float> stackArmorBuff;
        private static ConfigEntry<float> baseRegenMultiplier;
        private static ConfigEntry<float> stackRegenMultiplier;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            RegisterBuff();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Mourning Geode";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseBuffLength = SivsItemsPlugin.config.Bind<float>(configSection, "Buff Length", 1.5f, "The amount of time, in seconds, the Geode's buff lasts.");
            stackBuffLength = SivsItemsPlugin.config.Bind<float>(configSection, "Buff Length per Stack", 0.3f, "The amount of time, in seconds, the Geode's buff is extended when stacking the item.");
            baseArmorBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Armor Buff", 25f, "The amount of armor the Geode's buff grants. Refer to the RoR2 Wiki for more information on armor.");
            stackArmorBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Armor Buff per Stack", 10f, "The amount of bonus armor stacking the Geode grants.");
            baseRegenMultiplier = SivsItemsPlugin.config.Bind<float>(configSection, "Regen Multiplier", 1.85f, "The amount your regeneration is multiplied by by the Geode. 1.85 gives a +85% bonus.");
            stackRegenMultiplier = SivsItemsPlugin.config.Bind<float>(configSection, "Regen Multiplier per stack", 0.35f, "The amount the regeneration bonus is increased by per stack.");

        }

        private static void RegisterItem()
        {

            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "Geode";
            itemDef.nameToken = "GEODE_NAME";
            itemDef.descriptionToken = "GEODE_DESCRIPTION";
            itemDef.loreToken = "GEODE_LORE";
            itemDef.pickupToken = "GEODE_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texGeodeIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Utility, ItemTag.Healing, ItemTag.OnKillEffect };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }


        private static void RegisterItemDisplayRules()
        {

            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(27.15f, -189.531f, 99.601f),
                    localPos = new Vector3(0.074f, -0.012f, 0.096f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-14.99f, -103.16f, -83.9f),
                    localPos = new Vector3(-1.1f, 1.19f, 3.24f),
                    localScale = Vector3.one * 1f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });


        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("GEODE_NAME", "Mourning Geode");
            LanguageAPI.Add("GEODE_PICKUP", "Gain temporary armor and regen on kill.");
            LanguageAPI.Add("GEODE_DESCRIPTION", "On kill, gain <style=cIsUtility>" + baseArmorBuff.Value + " armor</style> <style=cStack>(+" + stackArmorBuff.Value + " armor per stack)</style> and <style=cIsHealing>+" + ((baseRegenMultiplier.Value - 1) * 100) + "% regeneration</style> <style=cStack>(+" + (stackRegenMultiplier.Value * 100) + "% per stack)</style> for " + baseBuffLength.Value + " second(s) <style=cStack>(+" + stackBuffLength.Value + " second(s) per stack)</style>.");
            LanguageAPI.Add("GEODE_LORE", "To those who trample Mother Earth's fields,\n\nTo those who tear down Her mighty mountains,\n\nTo those who pollute Her sparkling waters,\n\nAnd to those who send Her children to the grave,\n\nForget not the fury of those who mourn.");
        }

        private static void RegisterBuff()
        {
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "GeodeArmor";
            buffDef.isDebuff = false;
            buffDef.canStack = false;
            buffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffBodyArmorIcon");
            buffDef.buffColor = new Color(0.5f, 0.2f, 0.5f);
            buffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(buffDef);
        }


        private static void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (self.inventory)
                {
                    int count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        if (self.HasBuff(Geode.buffDef))
                        {
                            float armorIncrease = baseArmorBuff.Value + (stackArmorBuff.Value * (count - 1));
                            float regenMultiplier = baseRegenMultiplier.Value + (stackRegenMultiplier.Value * (count - 1));
                            int rackCount = self.inventory.GetItemCount(RoR2.RoR2Content.Items.IncreaseHealing.itemIndex);
                            for (int i = 0; i < rackCount; i++)
                            {
                                regenMultiplier *= 2;
                            }
                            float length = baseBuffLength.Value + (stackBuffLength.Value * (count - 1));
                            Reflection.SetPropertyValue<float>(self, "armor", self.armor + armorIncrease);
                            Reflection.SetPropertyValue<float>(self, "regen", self.regen * regenMultiplier);
                        }
                    }

                }
            };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) =>
            {
                orig.Invoke(self, damageReport);
                if(damageReport != null)
                {
                    if (damageReport.attacker != null)
                    {
                        CharacterBody attacker = damageReport.attackerBody;
                        if (attacker)
                        {
                            if (attacker.inventory)
                            {
                                int count = attacker.inventory.GetItemCount(itemDef);
                                if (count > 0)
                                {
                                    float length = baseBuffLength.Value + (stackBuffLength.Value * (count - 1));
                                    attacker.ClearTimedBuffs(Geode.buffDef);
                                    attacker.AddTimedBuff(Geode.buffDef, length);
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayGeode");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupGeode");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGeode");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
        }


    }
    public static class LemurianArmor
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;
        private static BuffDef buffDef;


        public static ConfigEntry<float> dropChance;
        private static ConfigEntry<float> baseBuffLength;
        private static ConfigEntry<float> stackBuffLength;
        private static ConfigEntry<float> baseArmorBuff;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            RegisterBuff();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Scale Mail";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseBuffLength = SivsItemsPlugin.config.Bind<float>(configSection, "Buff Length", 2f, "The amount of time, in seconds, Scail Mails buff lasts.");
            stackBuffLength = SivsItemsPlugin.config.Bind<float>(configSection, "Buff Length per Stack", 2f, "The amount of time, in seconds, Scale Mails buff is extended when stacking the item.");
            baseArmorBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Armor Buff", 5f, "The amount of armor Scale Mail's buff grants. Refer to the RoR2 Wiki for more information on armor.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "LemurianArmor";
            itemDef.nameToken = "LEMURIANARMOR_NAME";
            itemDef.descriptionToken = "LEMURIANARMOR_DESCRIPTION";
            itemDef.loreToken = "LEMURIANARMOR_LORE";
            itemDef.pickupToken = "LEMURIANARMOR_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texLemurianArmor.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Utility };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterItemDisplayRules()
        {

            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });/*
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(27.15f, -189.531f, 99.601f),
                    localPos = new Vector3(0.074f, -0.012f, 0.096f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-14.99f, -103.16f, -83.9f),
                    localPos = new Vector3(-1.1f, 1.19f, 3.24f),
                    localScale = Vector3.one * 1f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });*/

        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("LEMURIANARMOR_NAME", "Scale Mail");
            LanguageAPI.Add("LEMURIANARMOR_PICKUP", "Gain bonus armor when taking damage.");
            LanguageAPI.Add("LEMURIANARMOR_DESCRIPTION", "On being hit, gain a stacking buff that grants <style=cIsUtility>"+baseArmorBuff.Value+"</style> bonus armor per stack for <style=cIsDamage>"+baseBuffLength.Value+"</style> <style=cStack>(+"+stackBuffLength.Value+" per stack)</style> second(s).");
            //LanguageAPI.Add("LEMURIANARMOR_LORE", "");
        }

        private static void RegisterBuff()
        {
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "TempArmorOnHit";
            buffDef.isDebuff = false;
            buffDef.canStack = true;
            buffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffGenericShield");
            buffDef.eliteDef = null;
            buffDef.buffColor = new Color(0.5f, 0.1f, 0.7f);
            SivsItems_ContentPack.buffDefs.Add(buffDef);
        }


        private static void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (self.inventory)
                {
                    int count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        if (self.HasBuff(LemurianArmor.buffDef))
                        {
                            float armorIncrease = baseArmorBuff.Value * self.GetBuffCount(buffDef);
                            Reflection.SetPropertyValue<float>(self, "armor", self.armor + armorIncrease);
                        }
                    }

                }
            };
            On.RoR2.GlobalEventManager.OnHitEnemy += (On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) =>
            {
                orig.Invoke(self, damageInfo, victim);
                if (victim != null)
                {
                    CharacterBody cb = victim.GetComponent<CharacterBody>();
                    if(cb != null)
                    {
                        Inventory i = cb.inventory;
                        if(i != null)
                        {
                            int count = i.GetItemCount(itemDef);
                            if(count > 0)
                            {
                                int buffCount = cb.GetBuffCount(buffDef);
                                float duration = (baseBuffLength.Value + (stackBuffLength.Value * (count - 1))) * damageInfo.procCoefficient;

                                cb.ApplyBuff(buffDef.buffIndex, 1, duration);
                                //cb.AddTimedBuff(buffDef, duration);
                            }
                        }
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayLemurianArmor");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupLemurianArmor");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matLemurianArmor");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
        }


    }
    public static class BeetlePlush
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject displayPrefabSit;
        private static GameObject pickupPrefab;
        private static BuffDef buffDef;


        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> baseRadius;
        private static ConfigEntry<float> stackRadius;
        private static ConfigEntry<float> baseRegenBuff;
        private static ConfigEntry<float> stackRegenBuff;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            RegisterBuff();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Workers Bond";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Effect Radius", 15f, "The size, in meters, of the area Workers Bond will check for allies.");
            stackRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Effect Radius per Stack", 5f, "The size, in meters, in which the Effect Radius will be increased by stacking the item.");
            baseRegenBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Regen Multiplier", 1.25f, "The amount your regeneration is multiplied by. 1.25 gives a bonus of +25%.");
            stackRegenBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Regen Multiplier per Stack", .25f, "The amount your regeneration bonus increased per stack.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "BeetlePlush";
            itemDef.nameToken = "BEETLEPLUSH_NAME";
            itemDef.descriptionToken = "BEETLEPLUSH_DESCRIPTION";
            itemDef.loreToken = "BEETLEPLUSH_LORE";
            itemDef.pickupToken = "BEETLEPLUSH_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texBeetlePlushIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Healing, ItemTag.Utility };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-35.4f, -9.26f, 13.47f),
                    localPos = new Vector3(-0.179f, 0.439f, -0.09f),
                    localScale = Vector3.one * 0.46f
                }
            });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(27.15f, -189.531f, 99.601f),
                    localPos = new Vector3(0.074f, -0.012f, 0.096f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-14.99f, -103.16f, -83.9f),
                    localPos = new Vector3(-1.1f, 1.19f, 3.24f),
                    localScale = Vector3.one * 1f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("BEETLEPLUSH_NAME", "Worker's Bond");
            LanguageAPI.Add("BEETLEPLUSH_PICKUP", "Having allies nearby boosts regeneration.");
            LanguageAPI.Add("BEETLEPLUSH_DESCRIPTION", "Having allies within <style=cIsUtility>" + baseRadius.Value + "m</style> <style=cStack>(+" + stackRadius.Value + "m per stack)</style> increases your regeneration by <style=cIsHealing>+" + ((baseRegenBuff.Value - 1) * 100) + "%</style> <style=cStack>(" + (stackRegenBuff.Value * 100) + "% per stack)</style>.");
            LanguageAPI.Add("BEETLEPLUSH_LORE", "This world is cruel, friend.\n\nTake a look outside the tunnels. All facets of life, conspiring towards our end.\n\nThere's many who would see us culled, oppressed, and subjugated. But with my hand outstretched, I will not let you drown.\n\nThe only thing you need in this world is a true friend. I will not let you go. I will not let you down.");
        }

        private static void RegisterBuff()
        {

            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "BeetlePlushRegen";
            buffDef.isDebuff = false;
            buffDef.canStack = false;
            buffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffRegenBoostIcon");
            buffDef.buffColor = new Color(1f, 1f, 0.5f);
            buffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(buffDef);
        }


        private static void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (self.inventory)
                {
                    int count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        if (self.HasBuff(buffDef))
                        {
                            float regenMultiplier = baseRegenBuff.Value + (stackRegenBuff.Value * (count - 1));
                            int rackCount = self.inventory.GetItemCount(RoR2.RoR2Content.Items.IncreaseHealing.itemIndex);
                            for (int i = 0; i < rackCount; i++)
                            {
                                regenMultiplier *= 2;
                            }
                            Reflection.SetPropertyValue<float>(self, "regen", self.regen * regenMultiplier);
                        }
                    }

                }
            };
            On.RoR2.CharacterBody.FixedUpdate += (On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                Inventory inventory = self.inventory;
                if (!inventory)
                {
                    return;
                }
                int count = inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    float radius = baseRadius.Value + (stackRadius.Value * (count - 1));
                    if (FindClosestAlly(self.gameObject, self.gameObject.transform.position, self.teamComponent.teamIndex, radius))
                    {
                        self.AddTimedBuff(buffDef, 1f);
                    }
                }
            };
        }

        private static HurtBox FindClosestAlly(GameObject sender, Vector3 position, TeamIndex teamIndex, float radius)
        {
            BullseyeSearch search = new BullseyeSearch();
            search.searchOrigin = position;
            search.searchDirection = Vector3.zero;
            search.teamMaskFilter = TeamMask.none;
            search.teamMaskFilter.AddTeam(teamIndex);
            search.filterByLoS = false;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.maxDistanceFilter = radius;
            search.RefreshCandidates();
            HurtBox hurtBox = (from v in search.GetResults()
                               where v.healthComponent.gameObject != sender
                               select v).FirstOrDefault<HurtBox>();
            return hurtBox;
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayBeetlePlush");
            displayPrefabSit = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayBeetlePlushSitting");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupBeetlePlush");
            displayPrefab.AddComponent<NetworkIdentity>();
            displayPrefabSit.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matBeetlePlush");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
        }


    }
    public static class ImpEye
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;
        private static BuffDef buffDef;

        private static GameObject effectPrefab;

        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> hiddenBleedChance;

        private static ConfigEntry<float> baseArmorDebuff;
        private static ConfigEntry<float> stackArmorDebuff;
        private static ConfigEntry<float> debuffMoveSpeedCoefficient;
        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            RegisterBuff();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Imps Eye";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseArmorDebuff = SivsItemsPlugin.config.Bind<float>(configSection, "Armor Debuff", 10f, "The amount of armor Imps Eye will remove from the target.");
            stackArmorDebuff = SivsItemsPlugin.config.Bind<float>(configSection, "Armor Debuff per Stack", 5f, "The amount of additional armor Imps Eye will remove when stacking.");
            debuffMoveSpeedCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Movement Speed Coefficient", 0.5f, "The amount of movement speed removed from the target by Imps Eye. Set to values higher than 1 if you like to live dangerously.");
            hiddenBleedChance = SivsItemsPlugin.config.Bind<float>(configSection, "Hidden Bleed Chance", 3.5f, "Adds a hidden bleed chance, similarly to Predatory Instincts or Harvesters Scythe. Range is from 0 to 100.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "ImpEye";
            itemDef.nameToken = "IMPEYE_NAME";
            itemDef.descriptionToken = "IMPEYE_DESCRIPTION";
            itemDef.loreToken = "IMPEYE_LORE";
            itemDef.pickupToken = "IMPEYE_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texEyeIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.Utility };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Head",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(0f, 0.303f, 0.167f),
                    localScale = Vector3.one * 0.4f
                }
            });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(27.15f, -189.531f, 99.601f),
                    localPos = new Vector3(0.074f, -0.012f, 0.096f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Head",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(236.95f, 0f, 0f),
                    localPos = new Vector3(0.392f, 2.958f, -1.015f),
                    localScale = Vector3.one * 3.15f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Head",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(198.104f, 106.914f, -91.64999f),
                    localPos = new Vector3(-1.282005f, 1.762984f, 0.3611287f),
                    localScale = Vector3.one * 3.340857f
                },
                new ItemDisplayRule
                {
                    childName = "Head",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(195.291f, 253.104f, -270.943f),
                    localPos = new Vector3(1.288987f, 1.762985f, 0.338928f),
                    localScale = Vector3.one * 3.340857f
                },
                new ItemDisplayRule
                {
                    childName = "Head",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(189.522f, 118.388f, -93.59f),
                    localPos = new Vector3(-1.053009f, 2.679019f, 0.2590138f),
                    localScale = Vector3.one * 3.228076f
                },
                new ItemDisplayRule
                {
                    childName = "Head",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(193.757f, 241.53f, -271.418f),
                    localPos = new Vector3(1.063996f, 2.684004f, 0.2520248f),
                    localScale = Vector3.one * 3.228076f
                },
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "ThighL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(1.624f, 171.151f, 98.14f),
                    localPos = new Vector3(0.148f, -0.007f, 0.033f),
                    localScale = Vector3.one * 0.2f
                }
            });
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("IMPEYE_NAME", "Imp's Eye");
            LanguageAPI.Add("IMPEYE_PICKUP", "Bleeding enemies now have their armor and movement speed reduced.");
            LanguageAPI.Add("IMPEYE_DESCRIPTION", "<style=cDeath>Bleeding</style> now <style=cIsUtility>reduces armor</style> by <style=cIsDamage>" + baseArmorDebuff.Value + "</style> <style=cStack>(+" + stackArmorDebuff.Value + " per stack)</style> and <style=cIsUtility>slows enemies</style> by <style=cIsDamage>-" + ((1 - debuffMoveSpeedCoefficient.Value) * 100) + "%</style>.");
            //LanguageAPI.Add("IMPEYE_LORE", "A");
        }

        private static void RegisterBuff()
        {
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "ImpEyeDebuff";
            buffDef.isDebuff = true;
            buffDef.canStack = true;
            buffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffSlow50Icon");
            buffDef.eliteDef = null;
            buffDef.buffColor = new Color(0.6f, 0.2f, 0.2f);
            SivsItems_ContentPack.buffDefs.Add(buffDef);
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (self.HasBuff(ImpEye.buffDef))
                {
                    int count = self.GetBuffCount(ImpEye.buffDef);
                    float armorReduction = baseArmorDebuff.Value + (stackArmorDebuff.Value * (count - 1));
                    Reflection.SetPropertyValue<float>(self, "armor", self.armor - armorReduction);
                    Reflection.SetPropertyValue<float>(self, "moveSpeed", self.moveSpeed * debuffMoveSpeedCoefficient.Value);
                }
            };
            On.RoR2.GlobalEventManager.OnHitEnemy += (On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) =>
            {
                orig.Invoke(self, damageInfo, victim);
                if (damageInfo.attacker)
                {
                    CharacterBody component2 = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (!component2.inventory)
                    {
                        return;
                    }
                    if (component2 && victim)
                    {
                        CharacterBody component = victim.GetComponent<CharacterBody>();
                        if (component != null)
                        {
                            int count = component2.inventory.GetItemCount(ImpEye.itemDef);
                            if (count > 0)
                            {
                                if(Util.CheckRoll(hiddenBleedChance.Value, component2.master) && !damageInfo.procChainMask.HasProc(ProcType.BleedOnHit))
                                {
                                    ProcChainMask procChainMask2 = damageInfo.procChainMask;
                                    procChainMask2.AddProc(ProcType.BleedOnHit);
                                    DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed, 3f * damageInfo.procCoefficient, 1f);
                                }
                            }
                            if (component.HasBuff(RoR2.RoR2Content.Buffs.Bleeding.buffIndex))
                            {
                                if (count > 0)
                                {
                                    int buffCount = component.GetBuffCount(ImpEye.buffDef);
                                    if (buffCount < count)
                                    {
                                        component.AddTimedBuff(buffDef, Mathf.Infinity, Mathf.Abs(buffCount - count));
                                    }
                                }
                            }
                        }
                    }
                }
                
            };
            On.RoR2.CharacterBody.OnBuffFinalStackLost += (On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef) =>
            {
                orig.Invoke(self, buffDef);
                if (buffDef == BuffCatalog.GetBuffDef(RoR2.RoR2Content.Buffs.Bleeding.buffIndex))
                {
                    if (self.HasBuff(ImpEye.buffDef))
                    {
                        self.ClearTimedBuffs(ImpEye.buffDef);
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayImpEye");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupImpEye");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            effectPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("ImpEyeEffect");
            effectPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matImpEye");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matEyeDeBuffBillboard");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matEyeDeBuffBillboardPupil");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

    }
    public static class RevengeBonfire
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;


        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> baseDamageToGrudgeCoefficient;
        private static ConfigEntry<float> stackDamageToGrudgeCoefficient;

        private static ConfigEntry<float> minimumGrudgeToProc;

        private static ConfigEntry<float> bonfireDuration;
        private static ConfigEntry<float> bonfireDamageCoefficient;
        private static ConfigEntry<float> bonfireTickRate;
        private static ConfigEntry<float> bonfireRadius;

        private static GameObject bonfirePrefab;

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
            string configSection = "Vengeful Cinders";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseDamageToGrudgeCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Proc Chance (0 - 1)", 0.5f, "How often the item should proc, on a scale of 0 (0%) to 1 (100%).");
            stackDamageToGrudgeCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Per Second", 1f, "How much damage the tether will do overall. Used as a multiplier for the damage that procced the tether; i.e., 1 = 100% damage.");
            minimumGrudgeToProc = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Duration", 3f, "How long, in seconds, the tether will last.");
            bonfireDuration = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Duration per Stack", 0.5f, "How long, in seconds, each stack will extend the tether's duration.");
            bonfireDamageCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Duration per Stack", 0.5f, "How long, in seconds, each stack will extend the tether's duration.");
            bonfireTickRate = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Duration per Stack", 0.5f, "How long, in seconds, each stack will extend the tether's duration.");
            bonfireRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Tether Duration per Stack", 0.5f, "How long, in seconds, each stack will extend the tether's duration.");
        }

        private static void RegisterItem()
        {

            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "???";
            itemDef.nameToken = "???_NAME";
            itemDef.descriptionToken = "???_DESCRIPTION";
            itemDef.loreToken = "???_LORE";
            itemDef.pickupToken = "???_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Scrap };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("TENTACLE_NAME", "Frayed Tentacle");
            LanguageAPI.Add("TENTACLE_PICKUP", "Chance to tether yourself to an enemy.");
            LanguageAPI.Add("TENTACLE_DESCRIPTION", "");
            //LanguageAPI.Add("TENTACLE_LORE", "A");
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.Start += (On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (!self.gameObject.GetComponent<RevengeBonfireController>())
                {
                    RevengeBonfireController c = self.gameObject.AddComponent<RevengeBonfireController>();
                    c.attachedObject = self.gameObject;
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            //displayPrefab = Main.assetBundle.LoadAsset<GameObject>("DisplayTentacle");
            //pickupPrefab = Main.assetBundle.LoadAsset<GameObject>("PickupTentacle");
            //displayPrefab.AddComponent<NetworkIdentity>();
            //pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matTentacle");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matTentacleLightning");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class RevengeBonfireController : MonoBehaviour
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
        }
    }
    public static class BeetleDropBoots
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        public static ConfigEntry<float> dropChance;


        public static ConfigEntry<float> minimumActivationSpeed;

        public static ConfigEntry<float> baseDamageCoefficient;
        public static ConfigEntry<float> stackDamageCoefficient;
        public static ConfigEntry<float> baseFallDamageGuard;
        public static ConfigEntry<float> stackFallDamageGuard;

        public static ConfigEntry<float> radius;

        private static GameObject impactEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/BeetleGuardGroundSlam");
        private static GameObject hitEffectPrefab = EntityStates.BeetleGuardMonster.GroundSlam.hitEffectPrefab;


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
            string configSection = "Chitin Hammer";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            minimumActivationSpeed = SivsItemsPlugin.config.Bind<float>(configSection, "Activation Speed", 20f, "The speed at which you need to be falling in order for Chitin Hammer to take effect.");
            baseDamageCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Coefficient", 4.5f, "The damage coefficient of Chitin Hammer's impact.");
            stackDamageCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Coefficient per Stack", 1.2f, "The damage coefficient stacking Chitin Hammer grants.");
            baseFallDamageGuard = SivsItemsPlugin.config.Bind<float>(configSection, "Fall Damage Reduction", 0.1f, "The amount of fall damage reduced, as a percentage, when Chitin Hammer takes effect. 0.1 equals a 10% reduction.");
            stackFallDamageGuard = SivsItemsPlugin.config.Bind<float>(configSection, "Fall Damage Reduction per Stack", 0.05f, "The amount of fall damage reduced per stack.");
            radius = SivsItemsPlugin.config.Bind<float>(configSection, "Attack Radius", 5f, "The size of Chitin Hammer's impact attack, in meters.");
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "BeetleDropBoots";
            itemDef.nameToken = "BEETLEDROPBOOTS_NAME";
            itemDef.descriptionToken = "BEETLEDROPBOOTS_DESCRIPTION";
            itemDef.loreToken = "BEETLEDROPBOOTS_LORE";
            itemDef.pickupToken = "BEETLEDROPBOOTS_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texBeetleHammerIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.Utility };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                /*new ItemDisplayRule
                {
                    childName = "HeadCenter",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(30f, 0f, 0f),
                    localPos = new Vector3(0f, -0.003f, -0.122f),
                    localScale = Vector3.one * 0.5f
                }*/
            });/*
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });*/
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("BEETLEDROPBOOTS_NAME", "Chitin Hammer");
            LanguageAPI.Add("BEETLEDROPBOOTS_PICKUP", "Hitting the ground at a great enough speed damages nearby enemies and reduces fall damage.");
            LanguageAPI.Add("BEETLEDROPBOOTS_DESCRIPTION", "Hitting the ground at <style=cIsUtility>"+(minimumActivationSpeed.Value)+" m/s</style> or faster damages nearby enemies for <style=cIsDamage>"+(baseDamageCoefficient.Value*100)+"%</style> <style=cStack>(+"+(stackDamageCoefficient.Value*100)+"% per stack)</style> and <style=cIsUtility>reduces fall damage by "+(baseFallDamageGuard.Value*100)+"%</style> <style=cStack>(+"+(stackFallDamageGuard.Value*100)+"% per stack)</style>. Fall damage <style=cIsUtility>cannot be reduced below 1</style>.");
            //LanguageAPI.Add("BEETLEDROPBOOTS_LORE", "A");
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.Start += (On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (!self.gameObject.GetComponent<BeetleHammerController>())
                {
                    BeetleHammerController c = self.gameObject.AddComponent<BeetleHammerController>();
                    c.attachedObject = self.gameObject;
                }
            };
            On.RoR2.HealthComponent.TakeDamage += (On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) =>
            {

                if (damageInfo.damageType == DamageType.FallDamage)
                {
                    if (self.body != null)
                    {
                        CharacterBody body = self.body;
                        if (body)
                        {
                            Inventory i = body.inventory;
                            if (i)
                            {
                                int count = i.GetItemCount(itemDef);
                                if (count > 0)
                                {
                                    BeetleHammerController bhc = body.gameObject.GetComponent<BeetleHammerController>();
                                    if (bhc && bhc.IsActive)
                                    {
                                        float reductionCoefficient = baseFallDamageGuard.Value + (stackFallDamageGuard.Value * (count - 1));
                                        float reduction = Mathf.Clamp(damageInfo.damage * (Mathf.Clamp01(Mathf.Abs(1-reductionCoefficient))), 0, damageInfo.damage - 1);
                                        damageInfo.damage -= reduction;
                                        Debug.LogFormat("Reduced fall damage by {0}", new object[]{
                                            reduction
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                orig.Invoke(self, damageInfo);
            };
            On.RoR2.CharacterMotor.OnHitGround += (On.RoR2.CharacterMotor.orig_OnHitGround orig, CharacterMotor self, CharacterMotor.HitGroundInfo hitGroundInfo) =>
            {
                orig.Invoke(self, hitGroundInfo);
                if (self)
                {
                    BeetleHammerController bhc = self.gameObject.GetComponent<BeetleHammerController>();
                    if (bhc)
                    {
                        if (bhc.attachedBody)
                        {
                            Inventory i = bhc.attachedBody.inventory;
                            if (i)
                            {
                                int count = i.GetItemCount(itemDef);
                                if(count > 0)
                                {
                                    if(hitGroundInfo.velocity.y <= -Mathf.Abs(minimumActivationSpeed.Value))
                                    {
                                        bhc.FireAttack();
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayBeetleHammer");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupBeetleHammer");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matBeetleHammer");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
        }

        public class BeetleHammerController : MonoBehaviour
        {
            public GameObject attachedObject;

            public CharacterBody attachedBody
            {
                get
                {
                    return this.attachedObject.GetComponent<CharacterBody>();
                }
            }

            public CharacterMotor attachedMotor
            {
                get
                {
                    return this.attachedObject.GetComponent<CharacterMotor>();
                }
            }

            public Rigidbody attachedRigidbody
            {
                get
                {
                    return this.attachedObject.GetComponent<Rigidbody>();
                }
            }

            public ChildLocator attachedChildLocator
            {
                get
                {
                    return this.attachedObject.GetComponentInChildren<ChildLocator>();
                }
            }

            private BlastAttack attack;

            private BlastAttack UpdateBlastAttack()
            {
                Transform position = this.attachedObject.transform;
                if (this.attachedChildLocator)
                {
                    position = attachedChildLocator.FindChild("Base");
                }
                float damageCoefficient = baseDamageCoefficient.Value;
                if (this.attachedBody.inventory != null)
                {
                    int count = this.attachedBody.inventory.GetItemCount(itemDef);
                    damageCoefficient = (baseDamageCoefficient.Value + (stackDamageCoefficient.Value * (count - 1)));
                }
                return new BlastAttack
                {
                    attacker = this.attachedObject,
                    baseDamage = this.attachedBody.damage * damageCoefficient,
                    baseForce = 200f,
                    crit = this.attachedBody.RollCrit(),
                    damageColorIndex = DamageColorIndex.Item,
                    damageType = DamageType.Generic,
                    procCoefficient = 1,
                    radius = radius.Value,
                    position = position.position,
                    falloffModel = BlastAttack.FalloffModel.None,
                    teamIndex = TeamComponent.GetObjectTeam(this.attachedObject),
                    inflictor = this.attachedObject,
                    bonusForce = Vector3.up * 10f,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(hitEffectPrefab),
                };
            }

            public bool IsActive
            {
                get
                {
                    if (this.attachedMotor)
                    {
                        if(attachedMotor.velocity.y <= -Mathf.Abs(minimumActivationSpeed.Value))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public void FireAttack()
            {
                if (this.attachedBody)
                {
                    this.attack = UpdateBlastAttack();
                    if (impactEffectPrefab)
                    {
                        Transform position = this.attachedObject.transform;
                        if (this.attachedChildLocator)
                        {
                            position = attachedChildLocator.FindChild("Base");
                        }
                        EffectData ed = new EffectData
                        {
                            origin = position.position,
                            start = position.position,
                            scale = 1f,
                            rotation = this.attachedBody.modelLocator.modelBaseTransform.rotation,
                        };
                        EffectManager.SpawnEffect(impactEffectPrefab, ed, true);
                    }
                    this.attack.Fire();

                }
            }
        }
    }
    public static class Tarbine
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;
        private static BuffDef buffDef;

        private static ProcType tarBulletOnHit;


        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> baseAttackSpeedBuff;
        private static ConfigEntry<float> stackAttackSpeedBuff;

        private static ConfigEntry<float> followUpDuration;

        private static ConfigEntry<float> baseTarPelletDamageCoefficient;
        private static ConfigEntry<float> stackTarPelletDamageCoefficient;
        private static float tarPelletProcCoefficient = 0f;



        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            RegisterBuff();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Frenzied Tarbine";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseAttackSpeedBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Attack Speed Bonus", 1.2f, "The modifier to your attack speed. 1.2 equals a +20% bonus.");
            stackAttackSpeedBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Attack Speed Bonus per Stack", 0.1f, "The amount your attack speed bonus is increased by when stacking the item.");
            followUpDuration = SivsItemsPlugin.config.Bind<float>(configSection, "Follow-Up Duration", 0.5f, "The time, in seconds, you have to hit an enemy again in order to proc Frenzied Tarbine.");
            baseTarPelletDamageCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Tar Pellet Damage Coefficient", 0.2f, "The damage dealt by Frenzied Tarbines pellets, as a coefficient. 0.2 equals 20% damage.");
            stackTarPelletDamageCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Tar Pellet Damage Coefficient per Stack", 0.05f, "The increase to Frenzied Tarbines pellets damage when stacking the item.");
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "Tarbine";
            itemDef.nameToken = "TARBINE_NAME";
            itemDef.descriptionToken = "TARBINE_DESCRIPTION";
            itemDef.loreToken = "TARBINE_LORE";
            itemDef.pickupToken = "TARBINE_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTarbineIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }
        private static void RegisterProcType()
        {
            tarBulletOnHit = (ProcType)1000;
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("TARBINE_NAME", "Frenzied Tarbine");
            LanguageAPI.Add("TARBINE_PICKUP", "Striking enemies rapidly starts up a barrage of tar bullets.");
            LanguageAPI.Add("TARBINE_DESCRIPTION", "Increases attack speed by <style=cIsDamage>" + ((baseAttackSpeedBuff.Value - 1) * 100) + "%</style> <style=cStack>(+" + (stackAttackSpeedBuff.Value * 100) + "% per stack)</style>. Striking an enemy again within <style=cIsUtility>" + followUpDuration.Value + " seconds</style> fires a tar bullet for <style=cIsDamage>" + (baseTarPelletDamageCoefficient.Value * 100) + "% TOTAL damage</style> <style=cStack>(+" + (stackTarPelletDamageCoefficient.Value * 100) + "% per stack)</style> and applies <style=cIsUtility>Tar</style>.");
            //LanguageAPI.Add("TARBINE_LORE", "A");
        }

        private static void RegisterBuff()
        {

            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "TarbineReady";
            buffDef.isDebuff = false;
            buffDef.canStack = true;
            buffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffBleedingIcon");
            buffDef.buffColor = new Color(0.25f, 0.25f, 0.2f);
            buffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(buffDef);
        }

        private static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += (On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) => {
                orig.Invoke(self, damageInfo, victim);
                if (damageInfo.attacker)
                {
                    CharacterBody attackingBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (attackingBody && victim)
                    {
                        if (!attackingBody.inventory)
                        {
                            return;
                        }
                        int count = attackingBody.inventory.GetItemCount(itemDef);
                        if (count > 0)
                        {
                            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                            if (victimBody)
                            {
                                if (!damageInfo.procChainMask.HasProc(tarBulletOnHit) && damageInfo.procCoefficient > 0)
                                {
                                    if (victimBody.HasBuff(buffDef))
                                    {
                                        float damageCoefficient = baseTarPelletDamageCoefficient.Value + (stackTarPelletDamageCoefficient.Value * (count - 1));
                                        Vector3 aimVector = (victimBody.mainHurtBox.transform.position - attackingBody.mainHurtBox.transform.position).normalized;
                                        BulletAttack b = new BulletAttack
                                        {
                                            bulletCount = (uint)1,
                                            aimVector = aimVector,
                                            origin = attackingBody.mainHurtBox.transform.position,
                                            damage = damageInfo.damage * damageCoefficient,
                                            damageColorIndex = DamageColorIndex.Default,
                                            damageType = DamageType.ClayGoo,
                                            falloffModel = BulletAttack.FalloffModel.None,
                                            maxDistance = EntityStates.ClayBruiser.Weapon.MinigunFire.bulletMaxDistance,
                                            force = 10f,
                                            hitMask = LayerIndex.CommonMasks.bullet,
                                            minSpread = EntityStates.ClayBruiser.Weapon.MinigunFire.bulletMinSpread,
                                            maxSpread = EntityStates.ClayBruiser.Weapon.MinigunFire.bulletMaxSpread,
                                            isCrit = attackingBody.RollCrit(),
                                            owner = damageInfo.attacker,
                                            muzzleName = "Base",
                                            smartCollision = false,
                                            procChainMask = default(ProcChainMask),
                                            procCoefficient = tarPelletProcCoefficient,
                                            radius = 0f,
                                            sniper = false,
                                            stopperMask = LayerIndex.CommonMasks.bullet,
                                            weapon = null,
                                            tracerEffectPrefab = EntityStates.ClayBruiser.Weapon.MinigunFire.bulletTracerEffectPrefab,
                                            spreadPitchScale = 1f,
                                            spreadYawScale = 1f,
                                            queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                                            hitEffectPrefab = EntityStates.ClayBruiser.Weapon.MinigunFire.bulletHitEffectPrefab,
                                            HitEffectNormal = EntityStates.ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                                        };
                                        b.procChainMask.AddProc(tarBulletOnHit);
                                        b.Fire();

                                    }
                                    else
                                    {
                                        victimBody.ClearTimedBuffs(buffDef);
                                        victimBody.AddTimedBuff(buffDef, followUpDuration.Value * damageInfo.procCoefficient);
                                    }
                                }


                            }
                        }
                    }

                }
                

            };
            On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                Inventory inventory = self.inventory;
                if (!inventory)
                {
                    return;
                }
                int count = self.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    float attackSpeedBuff = (baseAttackSpeedBuff.Value + (stackAttackSpeedBuff.Value * (count - 1)));
                    Reflection.SetPropertyValue<float>(self, "attackSpeed", self.attackSpeed * attackSpeedBuff);

                }

            };

        }


        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayTarbine");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupTarbine");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matTrimSheetClayBruiser");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
        }


    }
    public static class BisonShield
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;
        private static BuffDef activeBuffDef;
        private static BuffDef inActiveBuffDef;

        private static GameObject chargeEffectPrefab;

        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> cooldownDuration;

        private static ConfigEntry<float> baseSprintSpeedBuff;
        private static ConfigEntry<float> stackSprintSpeedBuff;

        private static ConfigEntry<float> baseDashDamageCoefficient;
        private static ConfigEntry<float> stackDashDamageCoefficient;

        private static float knockback = 200f;
        private static Vector3 dashForceVector = new Vector3(0f, 0f, 100f);

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            RegisterBuff();
            Hooks();
        }
        private static void ReadWriteConfig()
        {
            string configSection = "Bighorn Buckler";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            cooldownDuration = SivsItemsPlugin.config.Bind<float>(configSection, "Cooldown Duration", 3f, "The amount of time, in seconds, required by Bighorn Buckler to cooldown after dashing.");
            baseSprintSpeedBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Sprint Modifier", 1.25f, "The modifier to your sprinting speed, as a coefficient. 1.25 equals a +25% bonus.");
            stackSprintSpeedBuff = SivsItemsPlugin.config.Bind<float>(configSection, "Sprint Modifier per Stack", 0.15f, "The amount the sprinting modifier is increased by when stacking the item.");
            baseDashDamageCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Coefficient", 2.5f, "The damage Bighorn Bucklers dash deals, as a coefficient. 2.5 equals 250% damage.");
            stackDashDamageCoefficient = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Coefficient per Stack", 1.75f, "The increase to Bighorn Bucklers dash damage when stacking the item.");
        }
        private static void RegisterItem()
        {

            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "BisonShield";
            itemDef.nameToken = "BISONSHIELD_NAME";
            itemDef.descriptionToken = "BISONSHIELD_DESCRIPTION";
            itemDef.loreToken = "BISONSHIELD_LORE";
            itemDef.pickupToken = "BISONSHIELD_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texBisonShieldIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.SprintRelated };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "LowerArmL",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-5.615f, 209.896f, 3.079f),
                    localPos = new Vector3(-0.002956f, -0.18262f, 0.014969f),
                    localScale = Vector3.one * 0.6f
                }
            });

            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("BISONSHIELD_NAME", "Bighorn Buckler");
            LanguageAPI.Add("BISONSHIELD_PICKUP", "Damage enemies while sprinting.");
            LanguageAPI.Add("BISONSHIELD_DESCRIPTION", "Increases <style=cIsUtility>sprinting speed</style> by <style=cIsUtility>" + ((baseSprintSpeedBuff.Value - 1) * 100) + "%</style> <style=cStack>(+" + (stackSprintSpeedBuff.Value * 100) + "% per stack)</style>. While sprinting, <style=cIsUtility>damage nearby enemies</style> for <style=cIsDamage>" + (baseDashDamageCoefficient.Value * 100) + "%</style> <style=cStack>(+" + (stackDashDamageCoefficient.Value * 100) + "% per stack)</style>, gaining <style=cIsDamage>bonus damage</style> the <style=cIsUtility>faster you are moving</style>. Recharges after <style=cIsDamage>" + cooldownDuration.Value + "</style> second(s).");
            //LanguageAPI.Add("BISONSHIELD_LORE", "To those who trample Mother Earth's fields,\n\nTo those who tear down Her mighty mountains,\n\nTo those who pollute Her sparkling waters,\n\nAnd to those who send Her children to the grave,\n\nForget not the fury of those who mourn.");
        }

        private static void RegisterBuff()
        {
            activeBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            activeBuffDef.name = "BisonShieldReady";
            activeBuffDef.isDebuff = false;
            activeBuffDef.canStack = false;
            activeBuffDef.iconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texBisonShieldActive");
            activeBuffDef.buffColor = new Color(0.9f, 0.5f, 0.4f);;
            activeBuffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(activeBuffDef);

            inActiveBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            inActiveBuffDef.name = "BisonShieldCooldown";
            inActiveBuffDef.isDebuff = true;
            inActiveBuffDef.canStack = true;
            inActiveBuffDef.iconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texBisonShieldInactive");
            inActiveBuffDef.buffColor = new Color(1f, 1f, 1f);
            inActiveBuffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(inActiveBuffDef);
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.Start += (On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (!self.gameObject.GetComponent<BisonShieldController>())
                {
                    BisonShieldController bsc = self.gameObject.AddComponent<BisonShieldController>();
                    bsc.attachedObject = self.gameObject;
                }
            };

            On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                Inventory inventory = self.inventory;
                if (!inventory)
                {
                    return;
                }
                int count = self.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    if (self.isSprinting && self.HasBuff(activeBuffDef))
                    {
                        float sprintSpeedBuff = (baseSprintSpeedBuff.Value + (stackSprintSpeedBuff.Value * (count - 1)));
                        Reflection.SetPropertyValue<float>(self, "moveSpeed", self.moveSpeed * sprintSpeedBuff);
                    }
                }
            };
            On.RoR2.CharacterBody.OnBuffFinalStackLost += (On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef) =>
            {
                orig.Invoke(self, buffDef);
                if (buffDef == inActiveBuffDef)
                {
                    Inventory inventory = self.inventory;
                    if (!inventory)
                    {
                        return;
                    }
                    int count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        self.AddBuff(activeBuffDef);
                    }
                }
            };
            On.RoR2.CharacterBody.OnInventoryChanged += (On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                Inventory inventory = self.inventory;
                if (!inventory)
                {
                    return;
                }
                int count = self.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    if (!self.HasBuff(activeBuffDef) && !self.HasBuff(inActiveBuffDef))
                    {
                        self.AddBuff(activeBuffDef);
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayBisonShield");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupBisonShield");
            chargeEffectPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("BisonShieldChargeEffect");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matBisonShield");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matBisonShieldCharge");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matBisonTracer");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class BisonShieldController : MonoBehaviour
        {
            public GameObject attachedObject;

            public CharacterBody attachedBody
            {
                get
                {
                    return this.attachedObject.GetComponent<CharacterBody>();
                }
            }

            private GameObject dashInstance;

            private OverlapAttack attack;

            private void Awake()
            {
                this.attachedObject = this.gameObject;
            }

            private void FixedUpdate()
            {
                if (this.attachedBody)
                {
                    Inventory i = this.attachedBody.inventory;
                    if (i)
                    {
                        int count = i.GetItemCount(BisonShield.itemDef);
                        if (count > 0)
                        {
                            if (this.attachedBody.isSprinting && this.attachedBody.HasBuff(activeBuffDef))
                            {
                                float damageCoefficient = baseDashDamageCoefficient.Value + (stackDashDamageCoefficient.Value * (count - 1));
                                if (!this.dashInstance)
                                {
                                    Transform transform = this.attachedBody.modelLocator.modelTransform;
                                    this.dashInstance = GameObject.Instantiate(BisonShield.chargeEffectPrefab, transform.position, transform.rotation, transform);
                                    this.dashInstance.transform.Translate(new Vector3(0f, 1f, 0f));
                                    this.attack = new OverlapAttack()
                                    {
                                        attacker = this.attachedObject,
                                        hitBoxGroup = this.dashInstance.GetComponent<HitBoxGroup>(),
                                        hitEffectPrefab = EntityStates.Bison.Charge.hitEffectPrefab,
                                        attackerFiltering = AttackerFiltering.AlwaysHit,
                                        procCoefficient = 1f,
                                        damage = this.attachedBody.damage * damageCoefficient,
                                        damageType = DamageType.Stun1s,
                                        pushAwayForce = knockback,
                                        isCrit = this.attachedBody.RollCrit(),
                                        procChainMask = default(ProcChainMask),
                                        forceVector = dashForceVector,
                                        teamIndex = this.attachedBody.teamComponent.teamIndex,
                                    };
                                }
                                else
                                {
                                    this.attack.damage = (this.attachedBody.damage * damageCoefficient) * (this.attachedBody.moveSpeed / (this.attachedBody.baseMoveSpeed * this.attachedBody.sprintingSpeedMultiplier));
                                    this.attack.Fire();
                                }
                            }
                            else
                            {
                                if (this.dashInstance)
                                {
                                    this.attack = null;
                                    GameObject.DestroyImmediate(this.dashInstance);
                                    if (!this.attachedBody.HasBuff(inActiveBuffDef))
                                    {
                                        this.attachedBody.RemoveBuff(activeBuffDef);
                                        for (int x = 1; x < cooldownDuration.Value + 1; x++)
                                        {
                                            this.attachedBody.AddTimedBuff(inActiveBuffDef, x);
                                        }
                                    }
                                }

                            }

                        }
                        else
                        {
                            if (this.dashInstance)
                            {
                                this.attack = null;
                                GameObject.DestroyImmediate(this.dashInstance);
                            }
                        }

                    }
                }
            }
        }
    }
    public static class FlameGland
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;


        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> procChance;
        private static ConfigEntry<float> baseLength;
        private static ConfigEntry<float> stackLength;

        private static GameObject onHitEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXLemurianBruiserFireballImpact");



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
            string configSection = "Living Furnace";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            procChance = SivsItemsPlugin.config.Bind<float>(configSection, "Proc Chance", 10f, "The chance to ignite a target, out of 100.");
            baseLength = SivsItemsPlugin.config.Bind<float>(configSection, "Ignition Duration", 2f, "The amount of time, in seconds, the burning debuff lasts.");
            stackLength = SivsItemsPlugin.config.Bind<float>(configSection, "Ignition Duration per Second", 2f, "The amount of time, in seconds, the burning debuff is extended by when stacking the item.");
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "FlameGland";
            itemDef.nameToken = "FLAMEGLAND_NAME";
            itemDef.descriptionToken = "FLAMEGLAND_DESCRIPTION";
            itemDef.loreToken = "FLAMEGLAND_LORE";
            itemDef.pickupToken = "FLAMEGLAND_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texFlameGlandIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("FLAMEGLAND_NAME", "Living Furnace");
            LanguageAPI.Add("FLAMEGLAND_PICKUP", "Chance to ignite enemies on hit.");
            LanguageAPI.Add("FLAMEGLAND_DESCRIPTION", "<style=cIsDamage>" + (procChance.Value) + "% chance on hit</style> to <style=cIsDamage>ignite</style> enemies for <style=cIsUtility>" + baseLength.Value + " seconds</style> <style=cStack>(+" + stackLength.Value + " seconds per stack)</style>. Ignited enemies burn for <style=cIsDamage>240%</style> base damage.");
            //LanguageAPI.Add("FLAMEGLAND_LORE", "A");
        }

        private static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += (On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) =>
            {
                orig.Invoke(self, damageInfo, victim);
                if (damageInfo.attacker)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (attackerBody && victim)
                    {
                        if (attackerBody.inventory)
                        {
                            int count = attackerBody.inventory.GetItemCount(itemDef);
                            if (count > 0)
                            {
                                CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                                if (victimBody)
                                {
                                    if (attackerBody.master)
                                    {
                                        float chance = procChance.Value;
                                        if (Util.CheckRoll(chance, attackerBody.master))
                                        {
                                            if (onHitEffectPrefab)
                                            {
                                                EffectManager.SimpleImpactEffect(onHitEffectPrefab, damageInfo.position, victim.transform.position.normalized, true);
                                            }
                                            float duration = baseLength.Value + (stackLength.Value * (count - 1));
                                            victimBody.AddTimedBuff(RoR2.RoR2Content.Buffs.OnFire.buffIndex, duration);
                                        }

                                    }
                                    
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayFlameGland");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupFlameGland");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matFlameGland");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGlandFire");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGlandGlow");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }


    }
    public static class ArmorWhenStationary
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        public static ConfigEntry<float> dropChance;

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
            string configSection = "No Place Like Home";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "ArmorWhenStationary";
            itemDef.nameToken = "ARMORWHENSTATIONARY_NAME";
            itemDef.descriptionToken = "ARMORWHENSTATIONARY_DESCRIPTION";
            itemDef.loreToken = "ARMORWHENSTATIONARY_LORE";
            itemDef.pickupToken = "ARMORWHENSTATIONARY_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("ARMORWHENSTATIONARY_NAME", "No Place Like Home");
            LanguageAPI.Add("ARMORWHENSTATIONARY_PICKUP", "");
            LanguageAPI.Add("ARMORWHENSTATIONARY_DESCRIPTION", "");
            //LanguageAPI.Add("ARMORWHENSTATIONARY_LORE", "A");
        }

        private static void Hooks()
        {

        }

        private static void UnpackAssetBundle()
        {
            //displayPrefab = Main.assetBundle.LoadAsset<GameObject>("DisplayTentacle");
            //pickupPrefab = Main.assetBundle.LoadAsset<GameObject>("PickupTentacle");
            //displayPrefab.AddComponent<NetworkIdentity>();
            //pickupPrefab.AddComponent<NetworkIdentity>();
            //Material mat = Main.assetBundle.LoadAsset<Material>("matTentacle");
        }

    }
    public static class AlloyFeather
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        public static ConfigEntry<float> dropChance;

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
            string configSection = "Alloy Feather";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "AlloyFeather";
            itemDef.nameToken = "ALLOYFEATHER_NAME";
            itemDef.descriptionToken = "ALLOYFEATHER_DESCRIPTION";
            itemDef.loreToken = "ALLOYFEATHER_LORE";
            itemDef.pickupToken = "ALLOYFEATHER_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }
        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("???_NAME", "");
            LanguageAPI.Add("???_PICKUP", "");
            LanguageAPI.Add("???_DESCRIPTION", "");
            //LanguageAPI.Add("???_LORE", "A");
        }

        private static void Hooks()
        {

        }

        private static void UnpackAssetBundle()
        {
            //displayPrefab = Main.assetBundle.LoadAsset<GameObject>("DisplayTentacle");
            //pickupPrefab = Main.assetBundle.LoadAsset<GameObject>("PickupTentacle");
            //displayPrefab.AddComponent<NetworkIdentity>();
            //pickupPrefab.AddComponent<NetworkIdentity>();
            //Material mat = Main.assetBundle.LoadAsset<Material>("matTentacle");
        }

    }
    public static class Mushroom
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        private static GameObject mushrumSporeObject;

        public static SpawnCard mushroomSpawnCard;

        public static ConfigEntry<float> dropChance;

        public static ConfigEntry<int> baseMushroomLimit;
        public static ConfigEntry<int> stackMushroomLimit;
        public static ConfigEntry<float> baseMushroomDPS;
        public static ConfigEntry<float> stackMushroomDPS;
        public static ConfigEntry<float> baseMushroomRadius;
        public static ConfigEntry<float> stackMushroomRadius;
        public static ConfigEntry<float> mushroomLife;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            CreateSpawnCard();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Mushrum Spore";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseMushroomLimit = SivsItemsPlugin.config.Bind<int>(configSection, "Mushrum Amount", 5, "The max amount of Mushrums you can grow.");
            stackMushroomLimit = SivsItemsPlugin.config.Bind<int>(configSection, "Mushrum Amount per Stack", 3, "The amount of Mushrums you can grow by stacking the item.");
            baseMushroomDPS = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Per Second", 1f, "The amount of damage that a Mushrum spore will deal over the course of a second.");
            stackMushroomDPS = SivsItemsPlugin.config.Bind<float>(configSection, "Damage Per Second Per Stack", 0.25f, "The amount of damage that stacking Mushrum Spore will grant.");
            baseMushroomRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Effect Radius", 15f, "The size of the Mushrums AOE, in meters.");
            stackMushroomRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Effect Radius Per Stack", 6.5f, "The amount the Mushrums AOE increases by when stacking the item.");
            mushroomLife = SivsItemsPlugin.config.Bind<float>(configSection, "Duration", 5f, "The amount of time, in seconds, a Mushrum Spore will last before disappearing.");
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "AoEMushroom";
            itemDef.nameToken = "AOEMUSHROOM_NAME";
            itemDef.descriptionToken = "AOEMUSHROOM_DESCRIPTION";
            itemDef.loreToken = "AOEMUSHROOM_LORE";
            itemDef.pickupToken = "AOEMUSHROOM_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.Utility };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }

        private static void CreateSpawnCard()
        {
            mushroomSpawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            mushroomSpawnCard.prefab = mushrumSporeObject;
            mushroomSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            mushroomSpawnCard.directorCreditCost = 0;
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("AOEMUSHROOM_NAME", "Mushrum Spore");
            LanguageAPI.Add("AOEMUSHROOM_PICKUP", "Grow a mushroom on kill that damages and slows nearby enemies.");
            LanguageAPI.Add("AOEMUSHROOM_DESCRIPTION", "On kill, grow a mushroom that damages enemies within <style=cIsUtility>"+(baseMushroomRadius.Value)+"m</style> <style=cStack>(+"+(stackMushroomRadius.Value)+"m per stack)</style> for <style=cIsDamage>"+(baseMushroomDPS.Value*100)+"% per second</style> <style=cStack>(+"+(stackMushroomDPS.Value * 100)+"% per stack)</style>, slowing them by <style=cIsDamage>-20%</style>. Lasts for <style=cIsUtility>"+(mushroomLife.Value)+" second(s)</style>. Grow up to <style=cIsUtility>"+(baseMushroomLimit.Value)+" mushroom(s)</style> <style=cStack>(+"+(stackMushroomLimit.Value)+" per stack)</style> at a time.");
            //LanguageAPI.Add("AOEMUSHROOM_LORE", "A");
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.Awake += (On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if(self.gameObject.GetComponent<MushrumSporeManager>() == null)
                {
                    self.gameObject.AddComponent<MushrumSporeManager>().attachedObject = self.gameObject;
                }
            };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) =>
            {
                orig.Invoke(self, damageReport);
                if(damageReport.attacker && damageReport.attackerMaster)
                {
                    if(damageReport.victim)
                    {
                        Inventory i = damageReport.attackerMaster.inventory;
                        if (i)
                        {
                            int count = i.GetItemCount(itemDef);
                            if(count > 0)
                            {
                                CharacterBody body = damageReport.attackerBody;
                                if (body)
                                {
                                    MushrumSporeManager msm = body.gameObject.GetComponent<MushrumSporeManager>();
                                    if(msm != null)
                                    {
                                        int limit = baseMushroomLimit.Value + (stackMushroomLimit.Value * (count - 1));
                                        int mushrooms = msm.mushrooms.Count;
                                        if(mushrooms < limit)
                                        {
                                            DirectorPlacementRule placementRule = new DirectorPlacementRule
                                            {
                                                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                                                minDistance = 5f,
                                                maxDistance = 10f,
                                                position = damageReport.victim.gameObject.transform.position
                                            };
                                            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(mushroomSpawnCard, placementRule, RoR2Application.rng);
                                            directorSpawnRequest.teamIndexOverride = new TeamIndex?(damageReport.attackerBody.teamComponent.teamIndex);
                                            directorSpawnRequest.summonerBodyObject = damageReport.attacker.gameObject;
                                            directorSpawnRequest.ignoreTeamMemberLimit = true;
                                            GameObject obj = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                                            if(obj != null)
                                            {
                                                EntityStateMachine esm = obj.GetComponent<EntityStateMachine>();
                                                esm.SetState(new MushrumSproutState
                                                {
                                                    ownerInfo =
                                                    {
                                                        owner = damageReport.attacker,
                                                        ownerBodyComponent = body,
                                                        baseDamageStat = body.damage,
                                                        ownerManagerComponent = msm,
                                                        stackCount = count
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                                
                            }
                        }
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayMushrum");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupMushrum");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            mushrumSporeObject = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("MushrumSpore");
            mushrumSporeObject.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(mushrumSporeObject, "EnemyItems", "UnpackAssetBundle", 4165);
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matMushrum");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matMushrumSpore");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class MushrumSporeManager : MonoBehaviour
        {
            public GameObject attachedObject;

            public CharacterBody attachedBody
            {
                get
                {
                    return attachedObject.GetComponent<CharacterBody>();
                }
            }
            public List<GameObject> mushrooms = new List<GameObject>();
        }

        public struct MushrumOwnerInformation
        {
            public GameObject owner;

            public CharacterBody ownerBodyComponent;

            public MushrumSporeManager ownerManagerComponent;

            public int stackCount;

            public float baseDamageStat;
        }

        public class MushrumSproutState : BaseState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                base.PlayAnimation("Base", "Spawn", "Spawn.playbackRate", baseDuration);
                if(this.ownerInfo.ownerManagerComponent != null)
                {
                    this.ownerInfo.ownerManagerComponent.mushrooms.Add(this.outer.gameObject);
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if(base.isAuthority && this.fixedAge >= baseDuration)
                {
                    this.outer.SetState(new MushrumIdleState
                    {
                        ownerInfo = this.ownerInfo
                    });
                }
            }

            public static float baseDuration = 0.5f;

            public MushrumOwnerInformation ownerInfo;
        }

        public class MushrumIdleState : BaseState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                base.PlayAnimation("Base", "Idle");
                this.duration = mushroomLife.Value;
                if (this.ownerInfo.owner != null)
                {

                    ChildLocator cl = this.outer.gameObject.GetComponent<ChildLocator>();
                    if (cl)
                    {
                        Transform t = cl.FindChild("Root");
                        if (t)
                        {

                            CharacterBody body = this.ownerInfo.ownerBodyComponent;
                            if (body)
                            {
                                wardInstance = UnityEngine.GameObject.Instantiate(wardObject, t.position, t.rotation);
                                wardInstance.transform.parent = t;
                                wardInstance.GetComponent<TeamFilter>().teamIndex = TeamComponent.GetObjectTeam(this.ownerInfo.owner);
                                float damageCoefficient = baseMushroomDPS.Value + (stackMushroomDPS.Value * (ownerInfo.stackCount - 1));
                                float lifetime = mushroomLife.Value;
                                float radius = baseMushroomRadius.Value + (stackMushroomRadius.Value * (ownerInfo.stackCount - 1));
                                wardInstance.transform.localScale = (Vector3.one * 10) * radius;
                                ProjectileController component = wardInstance.GetComponent<ProjectileController>();
                                if (component)
                                {
                                    component.procChainMask = default(ProcChainMask);
                                    component.procCoefficient = 1f;
                                    component.owner = this.ownerInfo.owner;
                                    component.Networkowner = this.ownerInfo.owner;
                                }
                                ProjectileDamage component2 = wardInstance.GetComponent<ProjectileDamage>();
                                if (component2)
                                {
                                    component2.damage = body.damage;
                                    component2.crit = body.RollCrit();
                                    component2.force = 0f;
                                    component2.damageColorIndex = DamageColorIndex.Item;
                                }
                                ProjectileDotZone component3 = wardInstance.GetComponent<ProjectileDotZone>();
                                if (component3)
                                {
                                    component3.fireFrequency = 4f;
                                    component3.resetFrequency = 4f;
                                    component3.damageCoefficient = (damageCoefficient / component3.fireFrequency);
                                    component3.lifetime = lifetime;

                                }
                                Transform fx = wardInstance.transform.Find("FX");
                                if (fx)
                                {
                                    fx.localScale = Vector3.one;
                                }

                            }
                            
                        }
                    }
                }
                else
                {
                    Debug.LogFormat("{0}: MushrumIdleState could not find owner object in MushrumOwnerInformation struct. Destroying object", new object[]
                    {
                        this.outer.gameObject.name
                    });
                    EntityState.Destroy(this.outer.gameObject);
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if(base.isAuthority && this.fixedAge >= duration)
                {
                    EntityState.Destroy(this.outer.gameObject);
                }
            }

            public override void OnExit()
            {
                base.OnExit();
                if (this.ownerInfo.ownerManagerComponent != null)
                {
                    this.ownerInfo.ownerManagerComponent.mushrooms.Remove(this.outer.gameObject);
                }
                if (this.wardInstance)
                {
                    EntityState.Destroy(this.wardInstance);
                }
            }

            private float duration;

            public MushrumOwnerInformation ownerInfo;

            private GameObject wardInstance;

            public static GameObject wardObject = Resources.Load<GameObject>("Prefabs/Projectiles/SporeGrenadeProjectileDotZone");
        }
    }
    public static class BrassMechanism
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        public static ConfigEntry<float> dropChance;

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
            string configSection = "Brass Mechanism";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "???";
            itemDef.nameToken = "???_NAME";
            itemDef.descriptionToken = "???_DESCRIPTION";
            itemDef.loreToken = "???_LORE";
            itemDef.pickupToken = "???_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Scrap };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("BRASSMECHANISM_NAME", "");
            LanguageAPI.Add("BRASSMECHANISM_PICKUP", "");
            LanguageAPI.Add("BRASSMECHANISM_DESCRIPTION", "");
            //LanguageAPI.Add("BRASSMECHANISM_LORE", "A");
        }

        private static void Hooks()
        {

        }

        private static void UnpackAssetBundle()
        {
            //displayPrefab = Main.assetBundle.LoadAsset<GameObject>("DisplayTentacle");
            //pickupPrefab = Main.assetBundle.LoadAsset<GameObject>("PickupTentacle");
            //displayPrefab.AddComponent<NetworkIdentity>();
            //pickupPrefab.AddComponent<NetworkIdentity>();
            //Material mat = Main.assetBundle.LoadAsset<Material>("matTentacle");
        }

    }
    public static class Grief
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        public static BuffDef buffDef;

        public static EffectDef effectDef;

        private static GameObject procEffectPrefab;

        public static ConfigEntry<float> dropChance;

        public static ConfigEntry<float> baseBuffDuration;
        public static ConfigEntry<float> stackBuffDuration;

        public static ConfigEntry<float> damageModifier;
        public static ConfigEntry<float> armorBonus;
        public static ConfigEntry<float> moveSpeedModifier;
        public static ConfigEntry<float> attackSpeedModifier;
        public static ConfigEntry<float> regenModifier;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterItem();
            RegisterItemDisplayRules();
            RegisterEffects();
            RegisterBuff();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "The Second Stage";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseBuffDuration = SivsItemsPlugin.config.Bind<float>(configSection, "Grief Duration", 5f, "The amount of time, in seconds, The Second Stages buff lasts. ");
            stackBuffDuration = SivsItemsPlugin.config.Bind<float>(configSection, "Grief Duration per Stack", 3f, "How long the buff is increased by when stacking The Second Stage.");
            damageModifier = SivsItemsPlugin.config.Bind<float>(configSection, "Grief Damage Modifier", 2f, "The multiplier applied to the characters damage stat.");
            armorBonus = SivsItemsPlugin.config.Bind<float>(configSection, "Grief Armor Bonus", 150f, "The amount of armor gained by the character.");
            moveSpeedModifier = SivsItemsPlugin.config.Bind<float>(configSection, "Grief Move Speed Modifier", 1.5f, "The multiplier applied to the characters move speed stat.");
            attackSpeedModifier = SivsItemsPlugin.config.Bind<float>(configSection, "Grief Attack Speed Modifier", 1.5f, "The multiplier applied to the characters attack speed stat");
            regenModifier = SivsItemsPlugin.config.Bind<float>(configSection, "Grief Regeneration Modifier", 5f, "The multiplier applied to the characters regeneration.");
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "Grief";
            itemDef.nameToken = "GRIEF_NAME";
            itemDef.descriptionToken = "GRIEF_DESCRIPTION";
            itemDef.loreToken = "GFIEF_LORE";
            itemDef.pickupToken = "GRIEF_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Utility, ItemTag.Healing };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }

        private static void RegisterEffects()
        {
            effectDef = new EffectDef();
            effectDef.prefab = procEffectPrefab;
            SivsItems_ContentPack.effectDefs.Add(effectDef);
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("GRIEF_NAME", "The Second Stage");
            LanguageAPI.Add("GRIEF_PICKUP", "Gain a boost to all stats when an ally is killed.");
            LanguageAPI.Add("GRIEF_DESCRIPTION", "When an ally is killed, gain a <style=cIsUtility>boost to all stats</style> for <style=cIsUtility>"+(baseBuffDuration.Value)+" seconds</style> <style=cStack>(+"+(stackBuffDuration.Value)+" seconds per stack)</style>.\n\nDamage is increased by <style=cIsDamage>+"+((damageModifier.Value-1)*100)+ "%</style>, armor is increased by <style=cIsUtility>" + (armorBonus.Value)+ "</style>, movement speed is increased by <style=cIsUtility>+" + ((moveSpeedModifier.Value-1)*100)+ "%</style>, attack speed is increased by <style=cIsDamage>+" + ((moveSpeedModifier.Value - 1) * 100) + "%</style>, and regeneration is increased by <style=cIsHealing>"+(regenModifier.Value*100)+"%</style>.");
            string lore = "Denial, wrath, bargaining, depression, and acceptance."+Environment.NewLine+Environment.NewLine
                +"The commonly associated five stages - though not everyone experiences them in order." + Environment.NewLine + Environment.NewLine
                +"Those who deny have yet to come to terms with their loss; they wish to escape." + Environment.NewLine + Environment.NewLine
                +"Those who bargain seek to lessen the blow; they wish to undo." + Environment.NewLine + Environment.NewLine
                +"Those who are depressed are processing their loss; they wish to mourn." + Environment.NewLine + Environment.NewLine
                +"Those who accept have found peace; they wish to move on." + Environment.NewLine + Environment.NewLine
                +"And those who are overcome with fury seek justice; they wish to destroy.";
            LanguageAPI.Add("GRIEF_LORE", lore);
        }
        private static void RegisterBuff()
        {
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "Grief";
            buffDef.isDebuff = false;
            buffDef.canStack = false;
            buffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffAttackSpeedOnCritIcon");
            buffDef.buffColor = new Color(1f, 0.9f, 0f);
            buffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(buffDef);
        }
        private static void Hooks()
        {
            On.RoR2.CharacterBody.RecalculateStats += (On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                if (self.inventory)
                {
                    int count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        if (self.HasBuff(Grief.buffDef))
                        {
                            Reflection.SetPropertyValue<float>(self, "damage", self.damage * damageModifier.Value);
                            Reflection.SetPropertyValue<float>(self, "armor", self.armor + armorBonus.Value);
                            Reflection.SetPropertyValue<float>(self, "attackSpeed", self.attackSpeed * attackSpeedModifier.Value);
                            Reflection.SetPropertyValue<float>(self, "moveSpeed", self.moveSpeed * moveSpeedModifier.Value);
                            Reflection.SetPropertyValue<float>(self, "regen", self.regen * regenModifier.Value);
                        }
                    }

                }
            };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) =>
            {
                orig.Invoke(self, damageReport);
                HealthComponent victim = damageReport.victim;
                if (victim)
                {
                    if (damageReport.victimBody)
                    {
                        TeamIndex victimTeam = damageReport.victimTeamIndex;
                        IReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(victimTeam);
                        foreach (var teamMateTeamComponent in teamMembers)
                        {
                            CharacterBody teammateBody = teamMateTeamComponent.body;
                            if (teammateBody)
                            {
                                Inventory i = teammateBody.inventory;
                                if (i)
                                {
                                    int count = i.GetItemCount(itemDef);
                                    if(count > 0)
                                    {
                                        EffectManager.SpawnEffect(effectDef.index, new EffectData
                                        {
                                            origin = teammateBody.modelLocator.modelBaseTransform.position,
                                            rotation = teammateBody.modelLocator.modelBaseTransform.rotation,
                                            scale = teammateBody.modelLocator.modelBaseTransform.localScale.magnitude,
                                        }, true);
                                        float duration = baseBuffDuration.Value + (stackBuffDuration.Value*(count - 1));
                                        teammateBody.AddTimedBuff(buffDef, duration);
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayGriefFlower");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupGriefFlower");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            procEffectPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("GriefRageProc");
            procEffectPrefab.AddComponent<NetworkIdentity>();
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGriefFlower");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGriefOverlay");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGriefProc");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matGriefProc2");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

    }
    public static class LunarConsumeProcs
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;

        public static ConfigEntry<float> dropChance;

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
            string configSection = "Lunar Designs";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");

        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "???";
            itemDef.nameToken = "???_NAME";
            itemDef.descriptionToken = "???_DESCRIPTION";
            itemDef.loreToken = "???_LORE";
            itemDef.pickupToken = "???_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texTentacleIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.Scrap };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Lunar;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterItemDisplayRules()
        {
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
               {
                   new ItemDisplayRule
                   {
                       childName = "HeadCenter",
                       ruleType = ItemDisplayRuleType.ParentedPrefab,
                       followerPrefab = displayPrefab,
                       localAngles = new Vector3(30f, 0f, 0f),
                       localPos = new Vector3(0f, -0.003f, -0.122f),
                       localScale = Vector3.one * 0.5f
                   }
               });
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Chest",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, -81.1f, 0f),
                    localPos = new Vector3(0f, 0.021f, -0.319f),
                    localScale = Vector3.one * 0.56f
                }
            });
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("LUNARCONSUMEPROCS_NAME", "");
            LanguageAPI.Add("LUNARCONSUMEPROCS_PICKUP", "");
            LanguageAPI.Add("LUNARCONSUMEPROCS_DESCRIPTION", "");
            //LanguageAPI.Add("LUNARCONSUMEPROCS_LORE", "A");
        }

        private static void Hooks()
        {

        }

        private static void UnpackAssetBundle()
        {
            //displayPrefab = Main.assetBundle.LoadAsset<GameObject>("DisplayTentacle");
            //pickupPrefab = Main.assetBundle.LoadAsset<GameObject>("PickupTentacle");
            //displayPrefab.AddComponent<NetworkIdentity>();
            //pickupPrefab.AddComponent<NetworkIdentity>();
            //Material mat = Main.assetBundle.LoadAsset<Material>("matTentacle");
        }

    }
    public static class NullSeed
    {
        public static ItemDef itemDef;
        public static GameObject displayPrefab;
        private static GameObject pickupPrefab;


        public static ConfigEntry<float> dropChance;

        private static ConfigEntry<float> baseRadius;
        private static ConfigEntry<float> baseCooldown;
        private static ConfigEntry<float> stackCooldown;

        private static ConfigEntry<float> baseFuse;

        private static BuffDef activeBuffDef;
        private static BuffDef inactiveBuffDef;

        private static GameObject deathNova;

        public static void Init()
        {
            UnpackAssetBundle();
            ReadWriteConfig();
            RegisterLanguageTokens();
            RegisterBuffs();
            RegisterItem();
            RegisterItemDisplayRules();
            Hooks();
        }

        private static void ReadWriteConfig()
        {
            string configSection = "Null Seed";
            dropChance = SivsItemsPlugin.config.Bind<float>(configSection, "Drop Chance", 0.1f, "The chance of the item dropping from its respective monster. Range is 0% to 100%, meaning a guaranteed chance.");
            baseFuse = SivsItemsPlugin.config.Bind<float>(configSection, "Fuse", 3, "The time, in seconds, it takes for Null Seeds effect to detonate.");
            baseRadius = SivsItemsPlugin.config.Bind<float>(configSection, "Detonation Radius", 10f, "The size, in meters, of Null Seeds detonation. Probably shouldnt increase this too much.");
            baseCooldown = SivsItemsPlugin.config.Bind<float>(configSection, "Cooldown", 30f, "The amount of time, in seconds, required for Null Seed to recharge.");
            stackCooldown = SivsItemsPlugin.config.Bind<float>(configSection, "Cooldown per Stack", -2f, "The deduction of Null Seeds cooldown when stacking it. Should be negative.");
        }

        private static void RegisterItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();
            itemDef.name = "NullSeed";
            itemDef.nameToken = "NULLSEED_NAME";
            itemDef.descriptionToken = "NULLSEED_DESCRIPTION";
            itemDef.loreToken = "NULLSEED_LORE";
            itemDef.pickupToken = "NULLSEED_PICKUP";
            itemDef.pickupModelPrefab = pickupPrefab;
            itemDef.pickupIconSprite = SivsItemsPlugin.assetBundle.LoadAsset<Sprite>("texNullSeedIcon.png");
            itemDef.hidden = false;
            itemDef.tags = new ItemTag[] { ItemTag.OnKillEffect, ItemTag.Damage, ItemTag.WorldUnique };
            itemDef.canRemove = true;
            itemDef.tier = ItemTier.Boss;
            SivsItemsPlugin.allItemDefs.Add(itemDef);
        }

        private static void RegisterItemDisplayRules()
        {

            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(-90f, 0f, 0f),
                    localPos = new Vector3(1f, 2f, 0f),
                    localScale = Vector3.one
                }
            });/*
            idrs.Add("mdlHuntress", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlToolbot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlMage", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlCroco", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlEngi", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlEngiTurret", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlMerc", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlTreebot", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlCaptain", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlLoader", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });
            idrs.Add("mdlScav", new ItemDisplayRule[]
            {
                new ItemDisplayRule
                {
                    childName = "Base",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(1f, 1f, 0f),
                    localScale = Vector3.one
                }
            });*/
        }
        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("NULLSEED_NAME", "Null Seed");
            LanguageAPI.Add("NULLSEED_PICKUP", "Annihilates ALL nearby characters on kill.");
            LanguageAPI.Add("NULLSEED_DESCRIPTION", "On kill, enemies spawn a <style=cArtifact>nullifying nova</style> that <style=cIsDamage>instantly kills</style> <style=cDeath>ALL nearby characters</style> within <style=cIsUtility>"+(baseRadius.Value)+"m</style>. Recharges after <style=cIsUtility>"+(baseCooldown.Value)+" seconds</style> <style=cStack>("+stackCooldown.Value+" seconds per stack)</style>. Detonation fuse scales with attack speed.");
            LanguageAPI.Add("NULLSEED_LORE", "STATE YOUR NAME FOR THE RECORD.\n\n...CONFIRMED. YOUR CASE WILL NOW BE REVIEWED. PLEASE REMAIN PATIENT AS WE GO OVER YOUR FILE.\n\nIDENTIFICATION: UNTRANSLATABLE.\n\nSPECIES: UNKNOWN; GENETIC EXPERIMENT.\n\nWANTED FOR EXCESS ACCUMULATION OF POWER; ESTIMATED THREAT LEVEL: MAXIMUM.\n\nYOUR SENTENCE IS: TWO HUNDRED MILLENIA IN SOLITARY CONFINEMENT. OFFENDING CONTRABAND WILL BE CONFISCATED AND SUBSEQUENTLY DISPOSED OF.\n\nYOU WILL NOW BE ESCORTED TO YOUR CELL. MAKE NO ATTEMPT TO RESIST; ESCAPE IS IMPOSSIBLE.");
        }

        private static void RegisterBuffs()
        {
            activeBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            activeBuffDef.name = "NullSeedReady";
            activeBuffDef.isDebuff = false;
            activeBuffDef.canStack = false;
            activeBuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffNullifiedIcon");
            activeBuffDef.buffColor = new Color(1f, 0.75f, 0.8f);
            activeBuffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(activeBuffDef);

            inactiveBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            inactiveBuffDef.name = "NullSeedCooldown";
            inactiveBuffDef.isDebuff = true;
            inactiveBuffDef.canStack = true;
            inactiveBuffDef.iconSprite = Resources.Load<Sprite>("Textures/BuffIcons/texBuffNullifyStackIcon");
            inactiveBuffDef.buffColor = new Color(0.5f, 0.2f, 0.4f);
            inactiveBuffDef.eliteDef = null;
            SivsItems_ContentPack.buffDefs.Add(inactiveBuffDef);
        }

        private static void Hooks()
        {
            On.RoR2.CharacterBody.OnBuffFinalStackLost += (On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef) =>
            {
                orig.Invoke(self, buffDef);
                if (buffDef == inactiveBuffDef)
                {
                    Inventory inventory = self.inventory;
                    if (!inventory)
                    {
                        return;
                    }
                    int count = self.inventory.GetItemCount(itemDef);
                    if (count > 0)
                    {
                        self.AddBuff(activeBuffDef);
                    }
                }
            };
            On.RoR2.CharacterBody.OnInventoryChanged += (On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) =>
            {
                orig.Invoke(self);
                Inventory inventory = self.inventory;
                if (!inventory)
                {
                    return;
                }
                int count = self.inventory.GetItemCount(itemDef);
                if (count > 0)
                {
                    if (!self.HasBuff(activeBuffDef) && !self.HasBuff(inactiveBuffDef))
                    {
                        self.AddBuff(activeBuffDef);
                    }
                }
            };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) =>
            {
                orig.Invoke(self, damageReport);
                if (damageReport != null)
                {
                    if (damageReport.attacker != null)
                    {
                        CharacterBody attackingBody = damageReport.attackerBody;
                        if (attackingBody != null)
                        {
                            Inventory i = attackingBody.inventory;
                            if (i)
                            {
                                int count = i.GetItemCount(itemDef);
                                if (attackingBody.HasBuff(activeBuffDef))
                                {
                                    Vector3 position = damageReport.damageInfo.position;
                                    GameObject bombInstance = GameObject.Instantiate(deathNova, position, Quaternion.identity);
                                    EntityStateMachine esm = bombInstance.GetComponent<EntityStateMachine>();
                                    if (esm)
                                    {
                                        float radius = (baseRadius.Value / 2.1f);
                                        esm.SetState(new NullSeedNovaBombState()
                                        {
                                            owner = damageReport.attacker,
                                            deathExplosionRadius = radius,
                                            outer = esm,
                                            duration = baseFuse.Value / attackingBody.attackSpeed
                                        });
                                    }
                                    attackingBody.RemoveBuff(activeBuffDef);
                                    if (count > 0)
                                    {
                                        attackingBody.ClearTimedBuffs(inactiveBuffDef);
                                        float duration = Mathf.Max((baseCooldown.Value + (stackCooldown.Value * (count - 1))), 0.5f);
                                        for (int x = 1; x < Mathf.Ceil(duration + 1); x++)
                                        {
                                            attackingBody.AddTimedBuff(inactiveBuffDef, x);
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                
            };
        }

        private static void UnpackAssetBundle()
        {
            displayPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("DisplayNullSeed");
            pickupPrefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("PickupNullSeed");
            deathNova = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("NullSeedNovaBomb");
            displayPrefab.AddComponent<NetworkIdentity>();
            pickupPrefab.AddComponent<NetworkIdentity>();
            deathNova.AddComponent<NetworkIdentity>();
            EntityStateMachine esm = deathNova.GetComponent<EntityStateMachine>();
            if (esm)
            {
                esm.initialStateType = new SerializableEntityStateType(typeof(Uninitialized));
                esm.mainStateType = new SerializableEntityStateType(typeof(NullSeedNovaBombState));
            }
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matNullSeed");
            mat.shader = Shader.Find("Hopoo Games/FX/Solid Parallax");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matNullSeedContainer");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matNullSeedGlow");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        public class NullSeedNovaBombState : EntityStates.BaseState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                this.muzzleTransform = base.gameObject.transform;
                if (this.muzzleTransform)
                {
                    this.portalPosition = new Vector3?(this.muzzleTransform.position);
                    if (deathPreExplosionVFX)
                    {
                        this.deathPreExplosionInstance = UnityEngine.Object.Instantiate<GameObject>(deathPreExplosionVFX, this.muzzleTransform.position, this.muzzleTransform.rotation);
                        this.deathPreExplosionInstance.transform.parent = this.muzzleTransform;
                        this.deathPreExplosionInstance.transform.localScale = Vector3.one * deathExplosionRadius;
                        ScaleParticleSystemDuration component = this.deathPreExplosionInstance.GetComponent<ScaleParticleSystemDuration>();
                        if (component)
                        {
                            component.newDuration = duration;
                        }
                    }
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (this.muzzleTransform)
                {
                    this.portalPosition = new Vector3?(this.muzzleTransform.position);
                }
                if (base.fixedAge >= duration)
                {
                    if (!this.hasFiredVoidPortal)
                    {
                        EntityState.Destroy(this.deathPreExplosionInstance);
                        this.hasFiredVoidPortal = true;
                        this.FireVoidPortal();
                        return;
                    }
                    if (NetworkServer.active && base.fixedAge >= duration + 4f)
                    {
                        EntityState.Destroy(base.gameObject);
                    }
                }
            }

            private void FireVoidPortal()
            {
                if (NetworkServer.active && this.portalPosition != null)
                {
                    Collider[] array = Physics.OverlapSphere(this.portalPosition.Value, this.deathExplosionRadius, LayerIndex.entityPrecise.mask);
                    CharacterBody[] array2 = new CharacterBody[array.Length];
                    int count = 0;
                    for (int i = 0; i < array.Length; i++)
                    {
                        CharacterBody characterBody = Util.HurtBoxColliderToBody(array[i]);
                        if (characterBody && !(characterBody == base.characterBody) && Array.IndexOf<CharacterBody>(array2, characterBody, 0, count) == -1)
                        {
                            array2[count++] = characterBody;
                        }
                    }
                    foreach (CharacterBody characterBody2 in array2)
                    {
                        if (characterBody2 && characterBody2.healthComponent)
                        {
                            characterBody2.healthComponent.Suicide(this.owner, this.owner, DamageType.VoidDeath);
                        }
                    }
                    if (deathExplosionEffect)
                    {
                        EffectManager.SpawnEffect(deathExplosionEffect, new EffectData
                        {
                            origin = (Vector3)this.portalPosition,
                            scale = deathExplosionRadius
                        }, true);
                    }
                }
            }

            public static GameObject deathPreExplosionVFX = EntityStates.NullifierMonster.DeathState.deathPreExplosionVFX;

            public static GameObject deathExplosionEffect = EntityStates.NullifierMonster.DeathState.deathExplosionEffect;

            public GameObject owner;

            public float duration;

            public float deathExplosionRadius;

            private GameObject deathPreExplosionInstance;

            private Transform muzzleTransform;

            private bool hasFiredVoidPortal;

            private Vector3? portalPosition;
        }

    }
}
