using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.dimensionHammer)]
    public class DimensionHammer : ContraptionBehaviour
    {
        public DimensionHammer(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity hammer)
        {
            base.Init(hammer);
            hammer.Timeout = hammer.GetMaxTimeout();
        }
        protected override void UpdateLogic(Entity hammer)
        {
            base.UpdateLogic(hammer);
            var evocationTime = GetEvocationTime(hammer);
            evocationTime++;
            if (evocationTime == START_TIME)
            {
                hammer.TriggerAnimation("Attack");
            }
            if (evocationTime == FLING_TIME)
            {
                hammer.PlaySound(VanillaSoundID.fling);
            }
            if (evocationTime == THROW_TIME)
            {
                hammer.PlaySound(VanillaSoundID.thump);
                Smash(hammer, hammer.GetDamage(), hammer.GetFaction());
            }
            if (evocationTime == THROWN_TIME)
            {
                hammer.SetAnimationBool("Attacked", true);
            }
            if (evocationTime > THROWN_TIME)
            {
                hammer.Timeout--;
            }
            SetEvocationTime(hammer, evocationTime);

            var tint = hammer.GetTint();
            tint.a = hammer.Timeout / (float)hammer.GetMaxTimeout();
            hammer.SetTint(tint);
            if (hammer.Timeout <= 0)
            {
                hammer.Remove();
            }
        }
        public override bool CanEvoke(Entity entity)
        {
            return false;
        }
        public static void Smash(Entity entity, float damage, int faction)
        {
            var range = entity.GetRange();
            foreach (Entity target in entity.Level.FindEntities(e => CanStun(entity, e)))
            {
                target.TakeDamage(damage, new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.DAMAGE_BOTH_ARMOR_AND_BODY, VanillaDamageEffects.MUTE), entity);
                if (target.Type == EntityTypes.ENEMY)
                {
                    var distance = (target.Position - entity.Position).magnitude;
                    var speed = 10;
                    target.Velocity = target.Velocity + Vector3.up * speed;
                    if (target.CanDeactive())
                        target.Stun(60);
                }
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    var angle = i * 36 + j * 12;
                    var param = entity.GetShootParams();
                    param.projectileID = VanillaProjectileID.arrowBullet;
                    param.damage = entity.GetDamage() / 3f;
                    param.velocity = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * Mathf.CeilToInt(15 / (j + 1));
                    var bullet = entity.ShootProjectile(param);
                }
            }

            var fragment = entity.GetOrCreateFragment();
            Fragment.AddEmitSpeed(fragment, 500);
        }
        private static bool CanStun(Entity self, Entity target)
        {
            return target.GetMainCollider().CheckSphere(self.GetCenter(), 80) && self.IsHostile(target) && target.IsOnGround;
        }
        public static int GetEvocationTime(Entity entity) => entity.GetBehaviourField<int>(ID, PROP_EVOCATION_TIME);
        public static void SetEvocationTime(Entity entity, int value) => entity.SetBehaviourField(ID, PROP_EVOCATION_TIME, value);
        private static readonly NamespaceID ID = VanillaContraptionID.dimensionHammer;
        public static readonly VanillaEntityPropertyMeta<int> PROP_EVOCATION_TIME = new VanillaEntityPropertyMeta<int>("EvocationTime");
        public const int START_TIME = 20;
        public const int FLING_TIME = 30;
        public const int THROW_TIME = 35;
        public const int THROWN_TIME = 40;
    }
}

