using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Difficulties;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Pickups;
using MVZ2.GameContent.Seeds;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.gunpowderBarrel)]
    public class GunpowderBarrel : ContraptionBehaviour
    {
        public GunpowderBarrel(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new ColorModifier(EngineEntityProps.COLOR_OFFSET, PROP_COLOR_OFFSET));
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetBombRNG(entity, new RandomGenerator(entity.RNG.Next()));
            var productionTimer = new FrameTimer(entity.RNG.Next(PRODUCTION_TIME_START_MIN, PRODUCTION_TIME_START_MAX));
            SetProductionTimer(entity, productionTimer);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (!entity.Level.IsIZombie())
            {
                ProductionUpdate(entity);
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelProperty("Furious", IsFurious(entity));
        }
        public override void PostDeath(Entity entity, DeathInfo deathInfo)
        {
            base.PostDeath(entity, deathInfo);
            if (deathInfo.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER))
                return;
            var damage = entity.GetDamage() * entity.Level.GetGunpowderDamageMultiplier();
            var range = entity.GetRange();
            var effects = new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE);
            entity.ExplodeAgainstFriendly(entity.GetCenter(), range, entity.GetFaction(), damage, effects);

            Explosion.Spawn(entity, entity.GetCenter(), range);

            entity.PlaySound(VanillaSoundID.explosion);
            entity.Level.Triggers.RunCallbackFiltered(VanillaLevelCallbacks.POST_CONTRAPTION_DETONATE, new EntityCallbackParams(entity), entity.GetDefinitionID());
        }
        public override bool CanEvoke(Entity entity)
        {
            if (IsFurious(entity))
                return false;
            return base.CanEvoke(entity);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            SetFurious(entity, true);
            WhiteFlashBuff.AddToEntity(entity, 15);
            entity.PlaySound(VanillaSoundID.fuse);
        }
        public static FrameTimer GetProductionTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_PRODUCTION_TIMER);
        public static void SetProductionTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_PRODUCTION_TIMER, timer);
        public static bool IsFurious(Entity entity) => entity.GetBehaviourField<bool>(PROP_FURIOUS);
        public static void SetFurious(Entity entity, bool value) => entity.SetBehaviourField(PROP_FURIOUS, value);
        public static RandomGenerator GetBombRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_RNG);
        public static void SetBombRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_RNG, value);
        private void ProductionUpdate(Entity entity)
        {
            var productionTimer = GetProductionTimer(entity);
            productionTimer.Run(entity.GetProduceSpeed());
            if (entity.Level.IsNoEnergy())
            {
                productionTimer.Frame = productionTimer.MaxFrame;
            }

            var color = entity.GetProperty<Color>(PROP_COLOR_OFFSET);
            float colorValue = color.a;
            if (productionTimer.Frame < 30)
            {
                colorValue = Mathf.Lerp(1, 0, productionTimer.Frame / 30f);
            }
            else
            {
                colorValue = Mathf.Max(0, colorValue - 1 / 30f);
            }
            color.r = 1;
            color.g = 1;
            color.b = 1;
            color.a = colorValue;
            entity.SetProperty(PROP_COLOR_OFFSET, color);

            if (productionTimer.Expired)
            {
                var pickupID = IsFurious(entity) ? VanillaPickupID.furiousGunpowder : VanillaPickupID.gunpowder;
                var bombProduceLimit = IsFurious(entity) ? 10 : 5;
                if (entity.IsFriendlyEntity())
                {
                    if (entity.RNG.Next(100) < bombProduceLimit)
                    {
                        ProduceBomb(entity);
                    }
                    else
                    {
                        var spawnParams = entity.GetSpawnParams();
                        spawnParams.SetProperty(VanillaEntityProps.DAMAGE, entity.GetDamage());
                        spawnParams.SetProperty(VanillaEntityProps.RANGE, entity.GetRange());
                        entity.Produce(pickupID);
                        entity.PlaySound(VanillaSoundID.throwSound);
                    }
                }
                else
                {
                    var redstoneDefinition = entity.Level.Content.GetEntityDefinition(pickupID);
                    var energyValue = redstoneDefinition?.GetEnergyValue() ?? 50;
                    entity.Level.AddEnergy(-energyValue);
                }
                productionTimer.ResetTime(1080);
            }
        }
        private void ProduceBomb(Entity entity)
        {
            var bombID = bombPool.Random(GetBombRNG(entity));
            if (!IsFurious(entity))
            {
                var spawnParams = entity.GetSpawnParams();
                spawnParams.SetProperty(BlueprintPickup.PROP_BLUEPRINT_ID, VanillaBlueprintID.FromEntity(bombID));
                entity.Produce(VanillaPickupID.blueprintPickup, spawnParams);
                entity.PlaySound(VanillaSoundID.throwSound);
            }
            else
            {
                var bomb = entity.SpawnWithParams(bombID, entity.Position);
                bomb.Velocity = new Vector3(bomb.RNG.Next(-8f, 8f), 8, 0);
                bomb.Trigger();
            }
        }
        private static NamespaceID[] bombPool = new NamespaceID[]
        {
            VanillaContraptionID.tnt,
            VanillaContraptionID.blackHoleBomb
        };

        public const int PRODUCTION_TIME_START_MIN = 90;
        public const int PRODUCTION_TIME_START_MAX = 360;
        public const int PRODUCTION_TIME = 1080;
        private static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_PRODUCTION_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("ProductionTimer");
        private static readonly VanillaEntityPropertyMeta<bool> PROP_FURIOUS = new VanillaEntityPropertyMeta<bool>("fury");
        private static readonly VanillaEntityPropertyMeta<Color> PROP_COLOR_OFFSET = new VanillaEntityPropertyMeta<Color>("color_offset");
        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("RNG");
        public static readonly NamespaceID ID = VanillaContraptionID.gunpowderBarrel;
    }
}
