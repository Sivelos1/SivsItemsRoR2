using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Skills;
using RoR2.Orbs;
using RoR2.ContentManagement;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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

namespace SivsItems
{
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, "SivsItems", "0.1.5")] // put your own name and version here
    [R2APISubmoduleDependency(nameof(CommandHelper),nameof(PrefabAPI), nameof(ItemDropAPI), nameof(ResourcesAPI), nameof(DirectorAPI), nameof(LanguageAPI))] // need these dependencies for the mod to work properly

    public class SivsItemsPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Sivelos.SivsItems"; // put your own names here
        public const string MODPATH = "C:\\Users\\devin\\source\\repos\\SivsItems\\SivsItems\\Main.cs";
        public static string MODPREFIX = "@SivsItems:";


        public static event Action awake;

        public static event Action start;

        public static ConfigFile config;

        public static ContentPack contentPack = new ContentPack();

        public static List<ItemDef> allItemDefs = new List<ItemDef>();

        public static List<EquipmentDef> allEquipDefs = new List<EquipmentDef>();

        public static AssetBundle assetBundle = Assets.LoadAssetBundle(SivsItemsRoR2.Properties.Resources.sivsitems);

        public static List<BodyToItemPair> enemyItemDrops = new List<BodyToItemPair>();

        public static Dictionary<string, BodyToItemPair> bodyToItemPairs = new Dictionary<string, BodyToItemPair>();

        public static SivsItemsPlugin instance;

        public SivsItemsPlugin()
        {
            SivsItemsPlugin.awake += this.SivsItems_Awake;
            SivsItemsPlugin.start += this.SivsItems_Start;
        }

        public void Awake()
        {
            Action action = SivsItemsPlugin.awake;
            bool flag = action == null;
            if (!flag)
            {
                action();
            }
        }

        public void Start()
        {
            Action action = SivsItemsPlugin.start;
            bool flag = action == null;
            if (!flag)
            {
                action();
            }
        }

        

        private void SivsItems_Awake()
        {
            config = base.Config;
            R2API.Utils.CommandHelper.AddToConsoleWhenReady();

            //Common
            //GlassShield.Init();

            //Uncommon
            SameEliteDamageBonus.Init();

            //Rare


            //Lunar
            Wheel.Init();
            //PiggyBank.Init();

            //Equipment
            Heart.Init();

            //Others

            //Enemy Items
            BeetlePlush.Init();
            MiniWispOnKill.Init();
            Tentacle.Init();
            Geode.Init();
            LemurianArmor.Init();
            ImpEye.Init();
            //Mushroom.Init();
            Tarbine.Init();
            BisonShield.Init();
            BeetleDropBoots.Init();
            Grief.Init();
            FlameGland.Init();
            NullSeed.Init();

            //Other Stuff

            new ContentPacks().Initialize();
        }


        private void SivsItems_Start()
        {
            RegisterDroppables();
            RegisterVanillaBodyItemPairs();
            Hooks();
        }
        private void RegisterVanillaBodyItemPairs()
        {
            
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("TitanBody"), RoR2.RoR2Content.Items.Knurl));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("ImpBossBody"), RoR2.RoR2Content.Items.BleedOnHitAndExplode));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("BeetleQueen2Body"), RoR2.RoR2Content.Items.BeetleGland));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("VagrantBody"), RoR2.RoR2Content.Items.NovaOnLowHealth));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("MagmaWormBody"), RoR2.RoR2Content.Items.FireballsOnHit));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("ElectricWormBody"), RoR2.RoR2Content.Items.LightningStrikeOnHit));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("RoboBallBossBody"), RoR2.RoR2Content.Items.RoboBallBuddy));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("ClayBossBody"), RoR2.RoR2Content.Items.SiphonOnLowHealth));
            enemyItemDrops.Add(new BodyToItemPair(BodyCatalog.FindBodyIndex("GrandParentBody"), RoR2.RoR2Content.Items.ParentEgg));
        }
        
        private void RegisterDroppables()
        {
            string bodyAddress = "Prefabs/CharacterBodies/";
            MakeDroppable(BeetlePlush.itemDef, BeetlePlush.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "BeetleBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(MiniWispOnKill.itemDef, MiniWispOnKill.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "WispBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Tentacle.itemDef, Tentacle.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "JellyfishBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Geode.itemDef, Geode.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "GolemBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(LemurianArmor.itemDef, LemurianArmor.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "LemurianBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(ImpEye.itemDef, ImpEye.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "ImpBody").GetComponent<CharacterBody>(), true);
            //MakeDroppable(Mushroom.itemDef, Mushroom.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "MiniMushroomBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Tarbine.itemDef, Tarbine.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "ClayBruiserBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(BisonShield.itemDef, BisonShield.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "BisonBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(BeetleDropBoots.itemDef, BeetleDropBoots.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "BeetleGuardBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Grief.itemDef, Grief.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "ParentBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(FlameGland.itemDef, FlameGland.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "LemurianBruiserBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(NullSeed.itemDef, NullSeed.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "NullifierBody").GetComponent<CharacterBody>(), true);
        }

        public static void MakeDroppable(ItemDef itemDef, float dropChance, CharacterBody droppingBodyBodyComponent, bool addAsTeleporterBossDrop = false)
        {
            if (addAsTeleporterBossDrop)
            {
                GameObject bodyObject = droppingBodyBodyComponent.gameObject;
                DeathRewards dr = bodyObject.GetComponent<DeathRewards>();
                if (dr)
                {
                    if(dr.bossPickup.pickupName == String.Empty)
                    {
                        PickupIndex pi = PickupCatalog.FindPickupIndex(itemDef.itemIndex);
                        PickupDef pd = PickupCatalog.GetPickupDef(pi);
                        if (pd != null)
                        {
                            dr.bossPickup.pickupName = pd.internalName;
                        }
                    }
                }
            }
            if (!bodyToItemPairs.ContainsKey(droppingBodyBodyComponent.gameObject.name))
            {
                Debug.LogFormat("SivsItems: Registered item {1} to be dropped from body {0} ({2}% chance)", new object[]
                {
                    droppingBodyBodyComponent.gameObject.name,
                    itemDef.nameToken,
                    dropChance
                });
                bodyToItemPairs.Add(droppingBodyBodyComponent.gameObject.name, new BodyToItemPair(droppingBodyBodyComponent.bodyIndex, itemDef, dropChance));
            }
            
        }
        private static void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) => {
                orig.Invoke(self, damageReport);
                if (damageReport != null)
                {
                    if (damageReport.attacker != null)
                    {
                        if (damageReport.victim != null)
                        {
                            if (damageReport.victimBody != null)
                            {
                                
                                string victimBodyName = damageReport.victimBody.gameObject.name.Replace("(Clone)", "");

                                BodyToItemPair bip = new BodyToItemPair();
                                bip.isValid = false;
                                try
                                {
                                    bip = bodyToItemPairs[victimBodyName];
                                }
                                catch (KeyNotFoundException)
                                {
                                    return;
                                }
                                if (bip.isValid)
                                {
                                    if (damageReport.attackerMaster != null)
                                    {
                                        CharacterMaster attacker = damageReport.attackerMaster;
                                        float chance = bip.dropChance;
                                        if (Util.CheckRoll(chance, attacker))
                                        {
                                            GameObject gameObject = damageReport.victim.gameObject;
                                            Transform transform = gameObject.transform;
                                            Vector3 position = transform.position;
                                            Vector3 rotation = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 10f + Vector3.forward * 5f);
                                            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(bip.itemDef.itemIndex), damageReport.victimBody.transform.position + Vector3.up * 1.5f, rotation);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [ConCommand(commandName = "CreateAllPickups", flags = ConVarFlags.None, helpText = "Creates pickups for all items and equipment in SivsItems. Used mostly for debugging purposes.")]
        private static void CreateAllPickups(ConCommandArgs args)
        {
            Debug.LogFormat("SivsItems: Starting CreateAllPickups (sender: {0})", new object[]{
                args.senderMasterObject.name
            });
            foreach (var def in allItemDefs)
            {
                Debug.LogFormat("   - SivsItems: Creating pickup for Item Index {0}", new object[]{
                    def.itemIndex.ToString()
                });
                GameObject gameObject = args.senderBody.gameObject;
                Transform transform = gameObject.transform;
                Vector3 position = transform.position;
                Quaternion rotation = transform.rotation;
                Ray ray = args.senderBody.inputBank ? args.senderBody.inputBank.GetAimRay() : new Ray(position, rotation * Vector3.forward);
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(def.itemIndex), args.senderBody.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + ray.direction * 2f);
            }
            foreach (var def in allEquipDefs)
            {
                Debug.LogFormat("   - SivsItems: Creating pickup for Item Index {0}", new object[]{
                    def.equipmentIndex.ToString()
                });
                GameObject gameObject = args.senderBody.gameObject;
                Transform transform = gameObject.transform;
                Vector3 position = transform.position;
                Quaternion rotation = transform.rotation;
                Ray ray = args.senderBody.inputBank ? args.senderBody.inputBank.GetAimRay() : new Ray(position, rotation * Vector3.forward);
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(def.equipmentIndex), args.senderBody.transform.position + Vector3.up * 1.5f, Vector3.up * 20f + ray.direction * 2f);
            }
            Debug.LogFormat("   - SivsItems: Done!", new object[]{
            });
        }
        public struct BodyToItemPair
        {
            public BodyToItemPair(BodyIndex bodyIndex, ItemDef itemDef, float dropChance = 0f)
            {
                this.bodyIndex = bodyIndex;
                this.itemDef = itemDef;
                this.dropChance = dropChance;
                this.isValid = true;
            }

            public BodyIndex bodyIndex;

            public ItemDef itemDef;

            public float dropChance;

            public bool isValid;
        }
    }
    internal static class Assets
    {
        public static AssetBundle LoadAssetBundle(Byte[] resourceBytes)
        {
            //Check to make sure that the byte array supplied is not null, and throw an appropriate exception if they are.
            if (resourceBytes == null) throw new ArgumentNullException(nameof(resourceBytes));

            //Actually load the bundle with a Unity function.
            var bundle = AssetBundle.LoadFromMemory(resourceBytes);

            return bundle;
        }
    }
    
    internal static class ItemDisplays
    {
        internal static List<string> vitalItemDisplayBodies = new List<string>
        {
            "CommandoBody",
            "HuntressBody",
            "Bandit2Body",
            "ToolbotBody",
            "MageBody",
            "TreebotBody",
            "LoaderBody",
            "MercBody",
            "CaptainBody",
            "CrocoBody",
            "EngiBody",
            "EngiTurretBody",
            "EngiWalkerTurretBody",
            "EquipmentDroneBody",
            "ScavBody",
        };
        internal static GameObject FindCharacterModelFromVanillaBodyName(string bodyName)
        {
            GameObject result = null;
            GameObject cb = Resources.Load<GameObject>("Prefabs/CharacterBodies/" + bodyName);
            if (cb != null)
            {
                ModelLocator ml = cb.GetComponent<ModelLocator>();
                if (ml != null)
                {
                    result = ml.modelTransform.gameObject;
                }
            }
            return result;
        }
        internal static ItemDisplayRuleSet GetIDRSFromModelObject(GameObject model)
        {
            ItemDisplayRuleSet result = null;
            if (model != null)
            {
                CharacterModel cm = model.GetComponent<CharacterModel>();
                if (cm != null)
                {
                    result = cm.itemDisplayRuleSet;
                }
            }
            return result;
        }
        //For copy paste purposes
        private static void RegisterItemDisplayRules(GameObject displayPrefab, ItemDef itemDef)
        {
            Dictionary<string, ItemDisplayRuleSet> vitalIdrs = ItemDisplays.GetVitalBodiesIDRS();
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CommandoBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["HuntressBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["Bandit2Body"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["ToolbotBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["MageBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["TreebotBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["LoaderBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["MercBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CaptainBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["CrocoBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EngiBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EngiTurretBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["EquipmentDroneBody"], itemDef, new DisplayRuleGroup
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
            ItemDisplays.AddItemDisplayToIDRS(vitalIdrs["ScavBody"], itemDef, new DisplayRuleGroup
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
        internal static void AddItemDisplayToIDRS(ItemDisplayRuleSet idrs, ItemDef itemDef, DisplayRuleGroup displayRules)
        {
            idrs.SetDisplayRuleGroup(itemDef, displayRules);
            idrs.GenerateRuntimeValues();
        }
        internal static void AddEquipDisplayToIDRS(ItemDisplayRuleSet idrs, EquipmentDef equipmentDef, DisplayRuleGroup displayRules)
        {
            idrs.SetDisplayRuleGroup(equipmentDef, displayRules);
            idrs.GenerateRuntimeValues();
        }

        internal static Dictionary<string, ItemDisplayRuleSet> GetVitalBodiesIDRS()
        {
            Dictionary<string, ItemDisplayRuleSet> result = new Dictionary<string, ItemDisplayRuleSet>();
            foreach (var bodyName in vitalItemDisplayBodies)
            {
                result.Add(bodyName, GetIDRSFromModelObject(FindCharacterModelFromVanillaBodyName(bodyName)));
            }
            return result;
        }
    }
    internal class ContentPacks : IContentPackProvider
    {
        public string identifier
        {
            get
            {
                return SivsItemsPlugin.MODUID;
            }
        }

        public void Initialize()
        {
            SivsItems_ContentPack.CreateContentPack(); //bring items and equipment into the content pack
            ContentManager.collectContentPackProviders += this.ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            //this.contentPack.identifier = this.identifier;
            this.contentPack.bodyPrefabs.Add(SivsItems_ContentPack.bodyPrefabs.ToArray());
            this.contentPack.masterPrefabs.Add(SivsItems_ContentPack.masterPrefabs.ToArray());
            this.contentPack.projectilePrefabs.Add(SivsItems_ContentPack.projectilePrefabs.ToArray());
            this.contentPack.gameModePrefabs.Add(SivsItems_ContentPack.gameModePrefabs.ToArray());
            this.contentPack.networkedObjectPrefabs.Add(SivsItems_ContentPack.networkedObjectPrefabs.ToArray());
            this.contentPack.skillDefs.Add(SivsItems_ContentPack.skillDefs.ToArray());
            this.contentPack.skillFamilies.Add(SivsItems_ContentPack.skillFamilies.ToArray());
            this.contentPack.sceneDefs.Add(SivsItems_ContentPack.sceneDefs.ToArray());
            this.contentPack.itemDefs.Add(SivsItems_ContentPack.itemDefs.ToArray());
            this.contentPack.equipmentDefs.Add(SivsItems_ContentPack.equipmentDefs.ToArray());
            this.contentPack.buffDefs.Add(SivsItems_ContentPack.buffDefs.ToArray());
            this.contentPack.eliteDefs.Add(SivsItems_ContentPack.eliteDefs.ToArray());
            this.contentPack.unlockableDefs.Add(SivsItems_ContentPack.unlockableDefs.ToArray());
            this.contentPack.survivorDefs.Add(SivsItems_ContentPack.survivorDefs.ToArray());
            this.contentPack.artifactDefs.Add(SivsItems_ContentPack.artifactDefs.ToArray());
            this.contentPack.effectDefs.Add(SivsItems_ContentPack.effectDefs.ToArray());
            this.contentPack.surfaceDefs.Add(SivsItems_ContentPack.surfaceDefs.ToArray());
            this.contentPack.networkSoundEventDefs.Add(SivsItems_ContentPack.networkSoundEventDefs.ToArray());
            this.contentPack.musicTrackDefs.Add(SivsItems_ContentPack.musicTrackDefs.ToArray());
            this.contentPack.gameEndingDefs.Add(SivsItems_ContentPack.gameEndingDefs.ToArray());
            this.contentPack.entityStateConfigurations.Add(SivsItems_ContentPack.entityStateConfigurations.ToArray());
            this.contentPack.entityStateTypes.Add(SivsItems_ContentPack.entityStateTypes.ToArray());
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        internal ContentPack contentPack
        {
            get
            {
                return SivsItemsPlugin.contentPack;
            }
        }
    }
    internal static class SivsItems_ContentPack
    {
        public static List<GameObject> bodyPrefabs = new List<GameObject>();

        public static List<GameObject> masterPrefabs = new List<GameObject>();

        public static List<GameObject> projectilePrefabs = new List<GameObject>();

        public static List<GameObject> gameModePrefabs = new List<GameObject>();

        public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();

        public static List<SkillDef> skillDefs = new List<SkillDef>();

        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();

        public static List<SceneDef> sceneDefs = new List<SceneDef>();

        public static List<ItemDef> itemDefs = new List<ItemDef>();

        public static List<EquipmentDef> equipmentDefs = new List<EquipmentDef>();

        public static List<BuffDef> buffDefs = new List<BuffDef>();

        public static List<EliteDef> eliteDefs = new List<EliteDef>();

        public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();

        public static List<SurvivorDef> survivorDefs = new List<SurvivorDef>();

        public static List<ArtifactDef> artifactDefs = new List<ArtifactDef>();

        public static List<EffectDef> effectDefs = new List<EffectDef>();

        public static List<SurfaceDef> surfaceDefs = new List<SurfaceDef>();

        public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();

        public static List<MusicTrackDef> musicTrackDefs = new List<MusicTrackDef>();

        public static List<GameEndingDef> gameEndingDefs = new List<GameEndingDef>();

        public static List<EntityStateConfiguration> entityStateConfigurations = new List<EntityStateConfiguration>();

        public static List<Type> entityStateTypes = new List<Type>();
        public static void CreateContentPack()
        {
            foreach (var item in SivsItemsPlugin.allItemDefs)
            {
                itemDefs.Add(item);
            }
            foreach (var item in SivsItemsPlugin.allEquipDefs)
            {
                equipmentDefs.Add(item);
            }
            foreach (var item in buffDefs)
            {
                Debug.LogFormat("- Adding buff {0} to content pack", new object[]
                {
                    item.name
                });
            }
        }
    }
}



