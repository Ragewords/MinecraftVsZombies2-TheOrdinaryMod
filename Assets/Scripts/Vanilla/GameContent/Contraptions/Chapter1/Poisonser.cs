using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.poisonser)]
    public class Poisonser : DispenserFamily
    {
        public Poisonser(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
            var timer = new FrameTimer(60)
            {
                Frame = 0
            };
            SetStateTimer(entity, timer);
            entity.SetAnimationBool("Evoked", false);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            var timer = GetStateTimer(entity);
            if (!entity.IsEvoked())
            {
                ShootTick(entity);
                timer.Run();
                entity.SetAnimationBool("Evoked", !timer.Expired);
                return;
            }
        }
        public override Entity Shoot(Entity entity)
        {
            var projectile = base.Shoot(entity);
            projectile.Timeout = Mathf.CeilToInt(entity.GetRange() / entity.GetShotVelocity().magnitude);
            return projectile;
        }

        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var timer = GetStateTimer(entity);
            timer.Reset();
            var param = entity.GetShootParams();
            param.projectileID = VanillaProjectileID.poisonPotion;
            param.damage = 0;
            entity.ShootProjectile(param);
        }
        public static void SetStateTimer(Entity entity, FrameTimer timer)
        {
            entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        }
        public static FrameTimer GetStateTimer(Entity entity)
        {
            return entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        }
        public static readonly NamespaceID ID = VanillaContraptionID.poisonser;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
    }
}
