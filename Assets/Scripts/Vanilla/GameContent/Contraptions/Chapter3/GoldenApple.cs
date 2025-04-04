using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using System.Linq;
using System;
using Tools;
using MVZ2.GameContent.Buffs.Enemies;
using static UnityEngine.EventSystems.EventTrigger;
using PVZEngine.Triggers;

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
            SetEnemyRNG(entity, new RandomGenerator(entity.RNG.Next()));
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("Evoked", entity.IsEvoked());
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.SetEvoked(true);
            entity.PlaySound(VanillaSoundID.sparkle);
        }

        private void PostEnemyMeleeAttackCallback(Entity enemy, Entity target)
        {
            if (!target.IsEntityOf(VanillaContraptionID.goldenApple))
                return;
            if (!target.IsHostile(enemy))
                return;
            if (target.IsAIFrozen())
                return;
            if (enemy.GetDefinitionID() == VanillaEnemyID.dullahan || enemy.GetDefinitionID() == VanillaEnemyID.dullahanHead)
            {
                enemy.Level.Triggers.RunCallback(VanillaLevelCallbacks.POST_ENTITY_REINCARNATE, c => c(enemy));
            }
            if (target.IsEvoked())
            {
                var mutant = target.Spawn(VanillaEnemyID.mutantZombie, enemy.Position);
                mutant.Charm(target.GetFaction());
                enemy.Spawn(VanillaEffectID.mindControlLines, enemy.GetCenter());
                enemy.Neutralize();
                enemy.Remove();
                enemy.PlaySound(VanillaSoundID.charmed);
                enemy.PlaySound(VanillaSoundID.odd);
            }
            else
            {
                var rng = GetEnemyRNG(target);
                NamespaceID[] pool = enemyPool;
                var targetID = pool.Random(rng);
                var random = target.Spawn(targetID, enemy.Position);
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
            VanillaEnemyID.imp,
            VanillaEnemyID.skeletonHorse,
        };
        public static RandomGenerator GetEnemyRNG(Entity contraption) => contraption.GetBehaviourField<RandomGenerator>(ID, PROP_ENEMY_RNG);
        public static void SetEnemyRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_ENEMY_RNG, value);
        public static readonly VanillaEntityPropertyMeta PROP_ENEMY_RNG = new VanillaEntityPropertyMeta("EnemyRNG");
        private static readonly NamespaceID ID = VanillaContraptionID.goldenApple;
    }
}
