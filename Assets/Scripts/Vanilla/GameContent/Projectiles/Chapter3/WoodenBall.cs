using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Buffs.Projectiles;
using MVZ2.GameContent.Contraptions;
using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using MVZ2.Vanilla.Properties;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.woodenBall)]
    public class WoodenBall : ProjectileBehaviour, IHellfireIgniteBehaviour
    {
        public WoodenBall(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            float angleSpeed = -projectile.Velocity.x * 2.5f;
            projectile.RenderRotation += Vector3.forward * angleSpeed;

            UpdateIgnited(projectile);
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var projectile = hitResult.Projectile;

            var dmg = projectile.GetDamage();
            dmg -= 10f;
            projectile.SetDamage(dmg);

            var hitCount = GetHitCount(projectile);
            hitCount++;
            SetHitCount(projectile, hitCount);
            if (hitCount >= MAX_HIT_COUNT)
            {
                hitResult.Pierce = false;
                return;
            }

            var vel = projectile.Velocity;
            var vel2D = new Vector2(vel.x, vel.z);
            var speed = vel2D.magnitude;
            vel.x = Mathf.Sign(vel.x) * speed * 0.5f;

            var lane = projectile.GetLane();
            var zSpeed = speed / 2 * Mathf.Sqrt(3);
            int zDir;
            if (lane <= 0)
            {
                zDir = -1;
            }
            else if (lane >= projectile.Level.GetMaxLaneCount() - 1)
            {
                zDir = 1;
            }
            else
            {
                zDir = projectile.RNG.Next(2) * 2 - 1;
            }
            vel.z = zDir * zSpeed;
            projectile.Velocity = vel;
        }
        public void Ignite(Entity entity, Entity hellfire, bool cursed)
        {
            var igniteBuff = entity.GetFirstBuff<HellfireIgnitedBuff>();
            if (igniteBuff == null)
            {
                igniteBuff = entity.AddBuff<HellfireIgnitedBuff>();
            }
            if (!HellfireIgnitedBuff.GetCursed(igniteBuff) && cursed)
            {
                HellfireIgnitedBuff.Curse(igniteBuff);
            }
        }
        private void UpdateIgnited(Entity projectile)
        {
            ignitedBuffBuffer.Clear();
            projectile.GetBuffs<HellfireIgnitedBuff>(ignitedBuffBuffer);

            // 在水中移除普通火焰。
            if (projectile.IsInWater())
            {
                for (int i = ignitedBuffBuffer.Count - 1; i >= 0; i--)
                {
                    var buff = ignitedBuffBuffer[i];
                    if (!HellfireIgnitedBuff.GetCursed(buff))
                    {
                        projectile.RemoveBuff(buff);
                        ignitedBuffBuffer.RemoveAt(i);
                    }
                }
            }

            // 更新模型。
            var igniteState = 0;
            if (ignitedBuffBuffer.Count > 0)
            {
                if (ignitedBuffBuffer.Any(b => HellfireIgnitedBuff.GetCursed(b)))
                {
                    igniteState = 2;
                }
                else
                {
                    igniteState = 1;
                }
            }
            projectile.SetModelProperty("IgniteState", igniteState);
        }
        private List<Buff> ignitedBuffBuffer = new List<Buff>();
        public static int GetHitCount(Entity entity) => entity.GetBehaviourField<int>(PROP_HIT_COUNT);
        public static void SetHitCount(Entity entity, int value) => entity.SetBehaviourField(PROP_HIT_COUNT, value);
        public static readonly VanillaEntityPropertyMeta<int> PROP_HIT_COUNT = new VanillaEntityPropertyMeta<int>("HitCount");
        public const int MAX_HIT_COUNT = 3;
    }
}
