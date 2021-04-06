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
            ItemDisplayRuleDict idrs = new ItemDisplayRuleDict(new ItemDisplayRule[]
            {
                /*new ItemDisplayRule
                {
                    childName = "Root",
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = displayPrefab,
                    localAngles = new Vector3(0f, 0f, 0f),
                    localPos = new Vector3(0f, 2.5f, 0f),
                    localScale = Vector3.one
                }*/
            });
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
