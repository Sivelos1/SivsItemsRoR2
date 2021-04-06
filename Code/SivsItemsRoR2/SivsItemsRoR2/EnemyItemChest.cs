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
    public static class EnemyItemChest
    {

        public static GameObject prefab;
        public static InteractableSpawnCard isc;
        public static void Init()
        {
            UnpackAssetBundle();
            RegisterLanguageTokens();
            RegisterInteractable();
        }

        private static void RegisterInteractable()
        {
            EnemyItemChestController eicc = prefab.AddComponent<EnemyItemChestController>();
            EntityStateMachine esm = prefab.GetComponent<EntityStateMachine>();
            if (esm)
            {
                esm.initialStateType = new SerializableEntityStateType(typeof(EntityStates.EnemyItemChest.Idle));
                PurchaseInteraction pi = prefab.GetComponent<PurchaseInteraction>();
                if (pi)
                {
                    pi.onPurchase.AddListener(new UnityEngine.Events.UnityAction<Interactor>(eicc.Open));
                }
            }
            DirectorAPI.Helpers.AddNewInteractable(new DirectorCard { 
                spawnCard = isc,
                minimumStageCompletions = 0,
                selectionWeight = 100,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                allowAmbushSpawn = true,
                preventOverhead = false,
            }, DirectorAPI.InteractableCategory.Chests);
        }

        private static void RegisterLanguageTokens()
        {
            LanguageAPI.Add("ENEMYITEMCHEST_NAME", "Trial Chest");
            LanguageAPI.Add("ENEMYITEMCHEST_CONTEXT", "Open Trial Chest");
        }

        private static void UnpackAssetBundle()
        {
            prefab = SivsItemsPlugin.assetBundle.LoadAsset<GameObject>("EnemyItemChest");
            PrefabAPI.RegisterNetworkPrefab(prefab);
            isc = SivsItemsPlugin.assetBundle.LoadAsset<InteractableSpawnCard>("iscEnemyItemChest");
            Material mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matEnemyItemChest");
            mat.shader = Shader.Find("Hopoo Games/Deferred/Standard");
            mat = SivsItemsPlugin.assetBundle.LoadAsset<Material>("matEnemyChestGlowNeutral");
            mat.shader = Shader.Find("Hopoo Games/FX/Cloud Remap");
        }

        
        public class EnemyItemChestController : MonoBehaviour
        {
            public void Open(Interactor i)
            {
                Chat.AddMessage("meem");
                PurchaseInteraction pi = this.gameObject.GetComponent<PurchaseInteraction>();
                EntityStateMachine esm = this.gameObject.GetComponent<EntityStateMachine>();
                if (esm)
                {
                    esm.SetNextState(new EntityStates.EnemyItemChest.Active());
                    esm.SetNextStateToMain();
                    Chat.AddMessage(esm.state.ToString());
                    pi.onPurchase.RemoveAllListeners();
                }
                pi.lastActivator = i;
            }
        }

    }
}

namespace EntityStates.EnemyItemChest
{
    public class Idle : BaseState
    {
        public DirectorCard enemyCard;

        private ChildLocator childLocator;

        public override void OnEnter()
        {
            base.OnEnter();

            this.enemyCard = ClassicStageInfo.instance.monsterSelection.Evaluate(Run.instance.runRNG.nextNormalizedFloat);
            Debug.LogFormat("{0}: Drew card for {1}", new object[]
            {
                    base.gameObject.GetInstanceID(),
                    this.enemyCard.spawnCard.prefab.name
            });
            childLocator = this.GetModelChildLocator();
            Transform symbol = childLocator.FindChild("Symbol");
            if (symbol && this.enemyCard != null)
            {
                GameObject model = null;
                GameObject prefab = enemyCard.spawnCard.prefab;
                if (prefab)
                {
                    GameObject body = prefab.gameObject;
                    if (body && body.GetComponent<CharacterBody>())
                    {
                        model = body.GetComponent<CharacterBody>().modelLocator.modelTransform.gameObject;
                    }
                }
                if (model)
                {
                    GameObject hologram = GameObject.Instantiate(model, symbol.position, Quaternion.identity);
                    hologram.transform.parent = symbol;
                    hologram.transform.localPosition = Vector3.zero;
                    hologram.transform.localScale = Vector3.one * 1f;
                }
            }
        }

        public void BeginPhase()
        {
            this.outer.SetNextState(new EntityStates.EnemyItemChest.Active());
            this.outer.SetNextStateToMain();
        }

    }

    public class Active : EntityState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            this.particles = this.GetModelChildLocator().FindChild("ActiveParticles").gameObject;
            if (particles)
            {
                particles.gameObject.SetActive(true);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (particles)
            {
                particles.gameObject.SetActive(false);
            }
        }

        private GameObject particles;
    }

    public class Opened : EntityState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Body", "Opened");
        }
    }
}
