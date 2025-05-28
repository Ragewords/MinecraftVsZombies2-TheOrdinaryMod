using MVZ2.GameContent.Contraptions;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.largeArrow)]
    public class LargeArrow : ProjectileBehaviour
    {
        public LargeArrow(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetRepeatTimer(entity, new FrameTimer(15));
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            projectile.Velocity += projectile.Velocity.normalized * 0.3f;

            var rotation = projectile.RenderRotation;
            rotation.x += projectile.Velocity.magnitude * 30;
            rotation.x %= 360;
            projectile.RenderRotation = rotation;
            var shootTimer = GetRepeatTimer(projectile);
            shootTimer.Run();
            if (shootTimer.Expired)
            {
                var shootParams = projectile.GetShootParams();
                shootParams.projectileID = VanillaProjectileID.arrow;
                shootParams.damage = 20;
                shootParams.soundID = null;
                shootParams.velocity = projectile.Velocity.normalized * 10;
                projectile.ShootProjectile(shootParams);
                shootTimer.ResetTime(15);
            }
        }
        private static readonly NamespaceID ID = VanillaProjectileID.largeArrow;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_REPEAT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("RepeatTimer");
        public static FrameTimer GetRepeatTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_REPEAT_TIMER);
        public static void SetRepeatTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_REPEAT_TIMER, timer);
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            if (damage == null)
                return;
            var projecitle = hitResult.Projectile;
            bool fatal = true;
            float spentDamage = 0;
            CheckDamageResult(damage.ShieldResult);
            CheckDamageResult(damage.ArmorResult);
            CheckDamageResult(damage.BodyResult);

            if (!fatal)
            {
                projecitle.Remove();
            }

            var dmg = projecitle.GetDamage();
            dmg -= spentDamage;
            projecitle.SetDamage(dmg);

            if (dmg <= 0)
            {
                projecitle.Remove();
            }

            void CheckDamageResult(DamageResult result)
            {
                if (result != null)
                {
                    if (result.Fatal)
                    {
                        spentDamage += result.SpendAmount;
                    }
                    else
                    {
                        fatal = false;
                    }
                }
            }
        }
    }
}
