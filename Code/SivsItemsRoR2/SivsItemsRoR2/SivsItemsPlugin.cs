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
    [BepInPlugin(MODUID, "SivsItems", "0.1.1")] // put your own name and version here
    [R2APISubmoduleDependency(nameof(CommandHelper),nameof(PrefabAPI), nameof(ItemDropAPI), nameof(ResourcesAPI), nameof(DirectorAPI), nameof(LanguageAPI))] // need these dependencies for the mod to work properly

    public class SivsItemsPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Sivelos.SivsItems"; // put your own names here
        public const string MODPATH = "C:\\Users\\devin\\source\\repos\\SivsItems\\SivsItems\\Main.cs";
        public static string MODPREFIX = "@SivsItems:";


        public static event Action awake;

        public static event Action start;



        public static ConfigFile config;

        public static ContentPack contentPack;

        public static List<ItemDef> allItemDefs = new List<ItemDef>();

        public static List<EquipmentDef> allEquipDefs = new List<EquipmentDef>();

        public static AssetBundle assetBundle = Assets.LoadAssetBundle(SivsItemsRoR2.Properties.Resources.sivsitems);

        public static List<BodyToItemPair> enemyItemDrops = new List<BodyToItemPair>();

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

        // Token: 0x0600000B RID: 11 RVA: 0x000023C0 File Offset: 0x000005C0
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
            //PiggyBank.Init();

            //Equipment
            //Heart.Init();

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
            //EnemyItemChest.Init();



            IL.RoR2.BuffCatalog.Init += FixBuffCatalog;

            On.RoR2.ArenaMissionController.EndRound += (On.RoR2.ArenaMissionController.orig_EndRound orig, ArenaMissionController self) => {
                orig.Invoke(self);
                Debug.LogFormat("Reward Spawn Position: \n - x: {0}\n - y: {1}\n - z: {2}", new object[]
                {
                    self.rewardSpawnPosition.transform.position.x,
                    self.rewardSpawnPosition.transform.position.y,
                    self.rewardSpawnPosition.transform.position.z,
                });
            };

            SivsItems_ContentPack.CreateContentPack();
        }

        internal static void FixBuffCatalog(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.Next.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.buffDefs)))
            {
                return;
            }

            c.Remove();
            c.Emit(OpCodes.Ldsfld, typeof(ContentManager).GetField(nameof(ContentManager.buffDefs)));
        }

        private void SivsItems_Start()
        {
            RegisterDroppables();
            RegisterVanillaBodyItemPairs();
        }
        private void RegisterVanillaBodyItemPairs()
        {
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/TitanBody"), RoR2.RoR2Content.Items.Knurl.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/ImpBossBody"), RoR2.RoR2Content.Items.BleedOnHitAndExplode.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/BeetleQueen2Body"), RoR2.RoR2Content.Items.BeetleGland.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/VagrantBody"), RoR2.RoR2Content.Items.NovaOnLowHealth.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/MagmaWormBody"), RoR2.RoR2Content.Items.FireballsOnHit.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/ElectricWormBody"), RoR2.RoR2Content.Items.LightningStrikeOnHit.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/RoboBallBossBody"), RoR2.RoR2Content.Items.RoboBallBuddy.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/ClayBossBody"), RoR2.RoR2Content.Items.SiphonOnLowHealth.itemIndex));
            enemyItemDrops.Add(new BodyToItemPair(Resources.Load<GameObject>("Prefabs/CharacterBodies/GrandParentBody"), RoR2.RoR2Content.Items.ParentEgg.itemIndex));
        }

        private void RegisterDroppables()
        {
            string bodyAddress = "Prefabs/CharacterBodies/";
            MakeDroppable(BeetlePlush.itemDef.itemIndex, BeetlePlush.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "BeetleBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(MiniWispOnKill.itemDef.itemIndex, MiniWispOnKill.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "WispBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Tentacle.itemDef.itemIndex, Tentacle.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "JellyfishBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Geode.itemDef.itemIndex, Geode.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "GolemBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(LemurianArmor.itemDef.itemIndex, LemurianArmor.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "LemurianBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(ImpEye.itemDef.itemIndex, ImpEye.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "ImpBody").GetComponent<CharacterBody>(), true);
            //MakeDroppable(Mushroom.itemDef.itemIndex, Mushroom.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "MiniMushroomBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Tarbine.itemDef.itemIndex, Tarbine.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "ClayBruiserBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(BisonShield.itemDef.itemIndex, BisonShield.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "BisonBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(BeetleDropBoots.itemDef.itemIndex, BeetleDropBoots.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "BeetleGuardBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(Grief.itemDef.itemIndex, Grief.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "ParentBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(FlameGland.itemDef.itemIndex, FlameGland.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "LemurianBruiserBody").GetComponent<CharacterBody>(), true);
            MakeDroppable(NullSeed.itemDef.itemIndex, NullSeed.dropChance.Value, Resources.Load<GameObject>(bodyAddress + "NullifierBody").GetComponent<CharacterBody>(), true);
        }

        public static void MakeDroppable(ItemIndex index, float dropChance, CharacterBody droppingBodyBodyComponent, bool addAsTeleporterBossDrop = false)
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += (On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport) => {
                orig.Invoke(self, damageReport);
                if(damageReport != null)
                {
                    if (damageReport.attacker != null)
                    {
                        if (damageReport.victim != null)
                        {
                            if (damageReport.victimBody != null)
                            {
                                if (damageReport.victimBodyIndex == droppingBodyBodyComponent.bodyIndex)
                                {
                                    if (damageReport.attackerMaster != null)
                                    {
                                        CharacterMaster attacker = damageReport.attackerMaster;
                                        float chance = dropChance;
                                        if (Util.CheckRoll(chance, attacker))
                                        {
                                            GameObject gameObject = damageReport.victim.gameObject;
                                            Transform transform = gameObject.transform;
                                            Vector3 position = transform.position;
                                            Vector3 rotation = Quaternion.AngleAxis((float)UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
                                            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(index), damageReport.victimBody.transform.position + Vector3.up * 1.5f, rotation);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            };
            if (addAsTeleporterBossDrop)
            {
                GameObject bodyObject = droppingBodyBodyComponent.gameObject;
                DeathRewards dr = bodyObject.GetComponent<DeathRewards>();
                if (dr)
                {
                    if(dr.bossPickup.pickupName == String.Empty)
                    {
                        PickupIndex pi = PickupCatalog.FindPickupIndex(index);
                        PickupDef pd = PickupCatalog.GetPickupDef(pi);
                        if (pd != null)
                        {
                            dr.bossPickup.pickupName = pd.internalName;
                        }
                    }
                }
            }
            enemyItemDrops.Add(new BodyToItemPair(droppingBodyBodyComponent.gameObject, index));
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
            public BodyToItemPair(GameObject bodyPrefab, ItemIndex index)
            {
                this.bodyPrefab = bodyPrefab;
                this.masterPrefab = bodyPrefab.GetComponent<CharacterBody>().masterObject;
                this.itemIndex = index;
            }

            public GameObject bodyPrefab;

            public GameObject masterPrefab;

            public ItemIndex itemIndex;
        }
    }
    public static class Assets
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

    internal static class SivsItems_ContentPack
    {
        public static List<GameObject> bodyPrefabs = new List<GameObject>();

        public static List<GameObject> masterPrefabs = new List<GameObject>();

        public static List<GameObject> projectilePrefabs = new List<GameObject>();

        public static List<Run> gameModePrefabs = new List<Run>();

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
            SivsItemsPlugin.contentPack = new ContentPack
            {
                bodyPrefabs = bodyPrefabs.ToArray(),
                masterPrefabs = masterPrefabs.ToArray(),
                projectilePrefabs = projectilePrefabs.ToArray(),
                gameModePrefabs = gameModePrefabs.ToArray(),
                networkedObjectPrefabs = networkedObjectPrefabs.ToArray(),
                skillDefs = skillDefs.ToArray(),
                skillFamilies = skillFamilies.ToArray(),
                sceneDefs = sceneDefs.ToArray(),
                itemDefs = itemDefs.ToArray(),
                equipmentDefs = equipmentDefs.ToArray(),
                buffDefs = buffDefs.ToArray(),
                eliteDefs = eliteDefs.ToArray(),
                unlockableDefs = unlockableDefs.ToArray(),
                survivorDefs = survivorDefs.ToArray(),
                artifactDefs = artifactDefs.ToArray(),
                effectDefs = effectDefs.ToArray(),
                surfaceDefs = surfaceDefs.ToArray(),
                networkSoundEventDefs = networkSoundEventDefs.ToArray(),
                musicTrackDefs = musicTrackDefs.ToArray(),
                gameEndingDefs = gameEndingDefs.ToArray(),
                entityStateConfigurations = entityStateConfigurations.ToArray(),
                entityStateTypes = entityStateTypes.ToArray(),
            };
            On.RoR2.ContentManager.SetContentPacks += AddContent;
        }
        private static void AddContent(On.RoR2.ContentManager.orig_SetContentPacks orig, List<ContentPack> newContentPacks)
        {
            newContentPacks.Add(SivsItemsPlugin.contentPack);
            orig(newContentPacks);
        }
    }
}



