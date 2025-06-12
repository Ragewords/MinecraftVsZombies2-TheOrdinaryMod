using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.mummy)]
    public class Mummy : MeleeEnemy
    {
        public Mummy(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var level = entity.Level;
            if (level.IsWaterLane(entity.GetLane()))
            {
                entity.AddBuff<BoatBuff>();
                entity.SetAnimationBool("HasBoat", true);
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            var param = entity.GetSpawnParams();
            param.SetProperty(EngineEntityProps.SCALE, entity.GetScale());
            entity.Spawn(VanillaEffectID.mummyGas, entity.Position, param);
            entity.SpawnWithParams(VanillaEnemyID.zombie, entity.Position);
            entity.PlaySound(VanillaSoundID.poisonCast);
            entity.Remove();

            var fragment = entity.CreateFragment();
            Fragment.AddEmitSpeed(fragment, 500);
        }
    }
}
