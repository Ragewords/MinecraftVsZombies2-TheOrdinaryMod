﻿using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
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
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationInt("HealthState", entity.GetHealthState(2));
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            var gas = entity.Level.Spawn(VanillaEffectID.mummyGas, entity.Position, entity);
            gas.SetFaction(entity.GetFaction());
            gas.SetScale(entity.GetScale());
            var zombie = entity.Level.Spawn(VanillaEnemyID.zombie, entity.Position, entity);
            zombie.SetFactionAndDirection(entity.GetFaction());
            zombie.SetScale(entity.GetScale());
            if (entity.RNG.Next(20) < 6)
                zombie.EquipArmor<LeatherCap>();
            else if (entity.RNG.Next(20) >= 6 && entity.RNG.Next(20) <= 8)
                zombie.EquipArmor<IronHelmet>();
            entity.PlaySound(VanillaSoundID.poisonCast);
            entity.PostFragmentDeath(info);
            entity.Remove();
        }
    }
}
