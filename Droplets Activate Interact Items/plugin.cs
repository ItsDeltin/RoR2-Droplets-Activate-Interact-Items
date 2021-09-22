using System;
using BepInEx;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Deltin
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.deltin.dropletsactivateinteractitems", "Droplets Activate Interact Items", "1.0.0")]
    public class Droplets_Activate_Interact_Items : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log { get; private set; }

        public void Awake()
        {
            Log = Logger;
            DropletConfig.Configure(Logger, Config);

            // Monster tooth
            IL.RoR2.HealthPickup.OnTriggerStay += il =>
            {
                var c = new ILCursor(il);
                // Post if-statement
                c.GotoNext(
                    x => x.MatchLdloc(0),
                    x => x.MatchCallvirt<CharacterBody>("get_healthComponent"),
                    x => x.MatchStloc(1)
                );

                c.Emit(OpCodes.Ldloc_0); // Emit local variable that contains the character body.
                c.EmitDelegate<Action<CharacterBody>>(characterBody => ExecuteDroplet(characterBody, DropletConfig.MonsterTooth));
            };

            // Money
            IL.RoR2.MoneyPickup.OnTriggerStay += il =>
            {
                var c = new ILCursor(il);
                // Post if-statement
                c.GotoNext(
                    MoveType.After,
                    x => x.MatchLdloc(0),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<MoneyPickup>("teamFilter"),
                    x => x.MatchCallvirt<TeamFilter>("get_teamIndex")
                );
                c.Index++; // Skip bne.un.s

                c.Emit(OpCodes.Ldarg_1); // Emit money collider.
                c.EmitDelegate<Action<Collider>>(collider => {
                    var body = collider.GetComponent<CharacterBody>();
                    if (body)
                        ExecuteDroplet(body, DropletConfig.GhorsTome);
                });
            };

            // Ammo pickup
            On.RoR2.SkillLocator.ApplyAmmoPack += (orig, skillLocator) => {
                orig(skillLocator);
                ExecuteDroplet(skillLocator.GetComponent<CharacterBody>(), DropletConfig.Bandolier);
            };

            // Original firework count balancing on interaction.
            IL.RoR2.GlobalEventManager.OnInteractionBegin += il =>
            {
                var c = new ILCursor(il);
                // Find 4 + itemCount * 4
                c.GotoNext(
                    MoveType.Before,
                    x => x.MatchLdcI4(4),
                    x => x.MatchLdloc(5),
                    x => x.MatchLdcI4(4),
                    x => x.MatchMul(),
                    x => x.MatchAdd()
                );
                c.RemoveRange(5);

                // Add the firework item count variable.
                c.Emit(OpCodes.Ldloc_S, (byte)5);

                // Replace the firework count formula.
                c.EmitDelegate<Func<int, int>>(fireworksCount => (int)DropletConfig.InteractFireworkCount.Evaluate(fireworksCount));
            };
        }

        void ExecuteDroplet(CharacterBody body, InteractableEventSourceConfig balance)
        {
            var inventory = body.inventory;
            if (!inventory) return;

            // Fireworks
            int fireworkCount = inventory.GetItemCount(RoR2Content.Items.Firework);
            if (fireworkCount > 0)
            {
                // Create the FireworkLauncher.
                FireworkLauncher launcher = UnityEngine.Object.Instantiate<GameObject>(
                    original: Resources.Load<GameObject>("Prefabs/FireworkLauncher"),
                    position: body.corePosition, 
                    rotation: Quaternion.identity).GetComponent<FireworkLauncher>();

                launcher.owner = body.gameObject;
                launcher.crit = Util.CheckRoll(body.crit, body.master);
                launcher.remaining = balance.GetFireworkCount(fireworkCount);
                launcher.damageCoefficient = balance.GetFireworkDamageCoefficient(fireworkCount);
            }

            // Squid
            int squidCount = inventory.GetItemCount(RoR2Content.Items.Squid);
            if (squidCount > 0)
            {
                SpawnCard spawnCard = Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscSquidTurret");
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 5f,
                    maxDistance = 25f,
                    position = body.corePosition
                };
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, RoR2Application.rng);
                directorSpawnRequest.teamIndexOverride = new TeamIndex?(TeamIndex.Player);
                directorSpawnRequest.summonerBodyObject = body.gameObject;
                DirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
                directorSpawnRequest2.onSpawnedServer += result => {
                    if (!result.success)
                        return;
                    var squidMaster = result.spawnedInstance.GetComponent<CharacterMaster>();
                    squidMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, balance.GetSquidHealth(squidCount));
                    squidMaster.inventory.GiveItem(RoR2Content.Items.BoostAttackSpeed, balance.GetSquidAttackSpeed(squidCount));
                };
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }

        // Debugging
        public void Update()
        {
            var itemHotkeys = new (KeyCode key, ItemDef item)[] {
                (KeyCode.F1, RoR2Content.Items.Tooth),
                (KeyCode.F3, RoR2Content.Items.Bandolier),
                (KeyCode.F2, RoR2Content.Items.BonusGoldPackOnKill),
                (KeyCode.F4, RoR2Content.Items.Firework),
                (KeyCode.F5, RoR2Content.Items.Squid),
            };

            foreach (var itemHotkey in itemHotkeys)
            if (Input.GetKeyDown(itemHotkey.key))
            {
                //Get the player body to use a position:	
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                //And then drop our defined item in front of the player.
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemHotkey.item.itemIndex), transform.position, transform.forward * 20f);
            }
        }
    }
}