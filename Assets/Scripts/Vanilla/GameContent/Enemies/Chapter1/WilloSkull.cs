using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.willoSkull)]
    public class WilloSkull : MeleeEnemy
    {
        public WilloSkull(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 20);
            if (!entity.HasBuff<GhostBuff>())
            {
                entity.AddBuff<GhostBuff>();
            }
        }
        public override void PreTakeDamage(DamageInput input)
        {
            base.PreTakeDamage(input);
            if (input.Effects.HasEffect(VanillaDamageEffects.FIRE))
            {
                input.Multiply(0);
            }
        }
    }
}
