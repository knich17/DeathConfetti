using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using Unity;
using Unity.Netcode;
using UnityEngine;

namespace DeathConfetti
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class DeathConfettiPlugin : BaseUnityPlugin
    {
        public const string modGUID = "K.DeathConfetti";
        public const string modName = "Death Confetti";
        public const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        void Awake()
        {
            var BepInExLogSource = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            BepInExLogSource.LogMessage(modGUID + " has loaded succesfully.");

            harmony.PatchAll(typeof(DeathConfettiPatch));
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB))]
    [HarmonyPatch("KillPlayerClientRpc")]
    class DeathConfettiPatch
    {
        public static GameObject eggPrefab;

        [HarmonyPatch(typeof(Terminal), "Start")]
        [HarmonyPostfix]
        private static void Init()
        {
            eggPrefab = StartOfRound.Instance.allItemsList.itemsList[0].spawnPrefab;

            for (int i = 0; i < StartOfRound.Instance.allItemsList.itemsList.Count; i++)
            {
                if (StartOfRound.Instance.allItemsList.itemsList[i].name == "EasterEgg") //itemName = "Easter Egg"
                {
                    eggPrefab = StartOfRound.Instance.allItemsList.itemsList[i].spawnPrefab;
                    break;
                }
            }
        }

        [HarmonyPostfix]
        static void Postfix(ref PlayerControllerB __instance)
        {
            // Logic adapted from StunGrenadeItem.ExplodeStunGrenade()
            Vector3 position = __instance.thisPlayerBody.position;
            GameObject val = UnityEngine.Object.Instantiate<GameObject>(eggPrefab, position, Quaternion.identity);
            StunGrenadeItem egg = val.GetComponent<StunGrenadeItem>();

            Transform parent;
            if (egg.isInElevator)
            {
                parent = StartOfRound.Instance.elevatorTransform;
            }
            else
            {
                parent = RoundManager.Instance.mapPropsContainer.transform;
            }

            UnityEngine.Object.Instantiate<GameObject>(egg.stunGrenadeExplosion, val.transform.position, Quaternion.identity, parent);
            egg.itemAudio.PlayOneShot(egg.explodeSFX);
            WalkieTalkie.TransmitOneShotAudio(egg.itemAudio, egg.explodeSFX, 1f);

            egg.DestroyObjectInHand(null);
        }
    }
}