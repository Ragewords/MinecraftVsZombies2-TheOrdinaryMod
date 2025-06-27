using System.Linq;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using MVZ2.Vanilla.Properties;
using MVZ2Logic;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.goldenApple)]
    public class GoldenApple : ContraptionBehaviour
    {
        public GoldenApple(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_ENEMY_MELEE_ATTACK, PostEnemyMeleeAttackCallback);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelProperty("Evoked", entity.IsEvoked());
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.SetEvoked(true);
            entity.PlaySound(VanillaSoundID.sparkle);
        }

        private void PostEnemyMeleeAttackCallback(VanillaLevelCallbacks.EnemyMeleeAttackParams param, CallbackResult result)
        {
            var enemy = param.enemy;
            var target = param.target;
            if (!target.IsEntityOf(VanillaContraptionID.goldenApple))
                return;
            if (!target.IsHostile(enemy))
                return;
            if (target.IsAIFrozen())
                return;
            var callbackParam = new EntityCallbackParams(enemy);
            enemy.Level.Triggers.RunCallback(VanillaLevelCallbacks.POST_ENTITY_REINCARNATE, callbackParam);
            if (target.IsEvoked())
            {
                var mutant = target.SpawnWithParams(VanillaEnemyID.mutantZombie, enemy.Position);
                mutant.Charm(target.GetFaction());
                enemy.Spawn(VanillaEffectID.mindControlLines, enemy.GetCenter());
                enemy.Neutralize();
                enemy.Remove();
                enemy.PlaySound(VanillaSoundID.charmed);
                enemy.PlaySound(VanillaSoundID.odd);
            }
            else
            {
                var game = Global.Game;
                var rng = target.RNG;
                var grid = enemy.GetGrid();
                var validEnemies = enemyPool.Where(id =>
                {
                    if (!game.GetUnlockedEnemies().Contains(id))
                        return false;
                    return grid.CanSpawnEntity(id);
                });
                if (validEnemies.Count() <= 0)
                    return;
                var enemyID = validEnemies.Random(rng);
                var random = target.SpawnWithParams(enemyID, enemy.Position);
                random.Charm(target.GetFaction());
                enemy.Spawn(VanillaEffectID.mindControlLines, enemy.GetCenter());
                enemy.Neutralize();
                enemy.Remove();
                enemy.PlaySound(VanillaSoundID.charmed);
                enemy.PlaySound(VanillaSoundID.floop);
            }
            target.Remove();
        }
        private static NamespaceID[] enemyPool = new NamespaceID[]
        {
            VanillaEnemyID.zombie,
            VanillaEnemyID.leatherCappedZombie,
            VanillaEnemyID.ironHelmettedZombie,
            VanillaEnemyID.skeleton,
            VanillaEnemyID.gargoyle,
            VanillaEnemyID.ghost,
            VanillaEnemyID.mummy,
            VanillaEnemyID.necromancer,
            VanillaEnemyID.skelebomb,
            VanillaEnemyID.spider,
            VanillaEnemyID.caveSpider,
            VanillaEnemyID.ghast,
            VanillaEnemyID.motherTerror,
            VanillaEnemyID.parasiteTerror,
            VanillaEnemyID.silverfish,
            VanillaEnemyID.mesmerizer,
            VanillaEnemyID.berserker,
            VanillaEnemyID.anubisand,
            VanillaEnemyID.curseer,
            VanillaEnemyID.reflectiveBarrierZombie,
            VanillaEnemyID.talismanZombie,
            VanillaEnemyID.wickedHermitZombie,
            VanillaEnemyID.shikaisenZombie,
            VanillaEnemyID.emperorZombie,
            VanillaEnemyID.tanookiZombie,
            VanillaEnemyID.imp,
            VanillaEnemyID.skeletonHorse,
            VanillaEnemyID.skeletonWarrior,
            VanillaEnemyID.skeletonMage
        };
        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_ENEMY_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("EnemyRNG");
        private static readonly NamespaceID ID = VanillaContraptionID.goldenApple;
    }
}
