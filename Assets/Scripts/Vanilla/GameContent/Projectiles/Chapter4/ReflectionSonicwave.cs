using MVZ2.GameContent.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.reflectionSonicwave)]
    public class ReflectionSonicwave : ProjectileBehaviour
    {
        public ReflectionSonicwave(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new Vector3Modifier(EngineEntityProps.DISPLAY_SCALE, NumberOperator.Multiply, PROP_DISPLAY_SCALE_MULTIPLIER));
            AddModifier(new Vector3Modifier(VanillaEntityProps.SHADOW_SCALE, NumberOperator.Multiply, PROP_DISPLAY_SCALE_MULTIPLIER));
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskFriendly |= EntityCollisionHelper.MASK_ENEMY;
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            var dmg = projectile.GetDamage();
            projectile.SetProperty(PROP_DISPLAY_SCALE_MULTIPLIER, Vector3.one * Mathf.Min(5, Mathf.Pow(dmg / 8f, 0.5f)));
            SetHitProtected(projectile, false);
        }
        protected override void PreHitEntity(ProjectileHitInput hit, DamageInput damage, CallbackResult result)
        {
            base.PreHitEntity(hit, damage, result);
            if (IsHitProtected(hit.Projectile))
            {
                result.SetFinalValue(false);
            }
            if (hit.Other.GetDefinitionID() == VanillaContraptionID.noteBlock)
            {
                damage.SetAmount(0);
            }
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var note = hitResult.Projectile;
            Reflect(note, hitResult.Other);

            SetHitProtected(note, true);

            var dmg = note.GetDamage(true);
            dmg--;
            note.SetDamage(dmg);
            if (dmg <= 0)
            {
                note.Remove();
            }
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            var note = collision.Entity;
            var noteBlock = collision.Other;
            if (!collision.OtherCollider.IsMainCollider())
                return;
            if (!note.IsFriendly(noteBlock))
                return;
            if (noteBlock.IsDead)
                return;
            var dmg = note.GetDamage(true);
            dmg++;
            note.SetDamage(dmg);
            note.Velocity = noteBlock.GetFacingDirection() * note.Velocity.magnitude;
            SetHitProtected(note, false);
            note.ClearIgnoredProjectileColliders();
        }
        public static void Reflect(Entity note, Entity other)
        {
            var vel = note.Velocity;
            var magnitude = vel.magnitude;
            if (other.Position.x < note.PreviousPosition.x)
            {
                vel = Vector3.right * magnitude;
            }
            else
            {
                vel = Vector3.left * magnitude;
            }
            note.Velocity = vel;
        }
        public static void SetHitProtected(Entity note, bool value) => note.SetBehaviourField(PROP_HIT_PROTECTED, value);
        public static bool IsHitProtected(Entity note) => note.GetBehaviourField<bool>(PROP_HIT_PROTECTED);
        private static readonly VanillaEntityPropertyMeta<Vector3> PROP_DISPLAY_SCALE_MULTIPLIER = new VanillaEntityPropertyMeta<Vector3>("DisplayScaleMultiplier");
        private static readonly VanillaEntityPropertyMeta<bool> PROP_HIT_PROTECTED = new VanillaEntityPropertyMeta<bool>("HitProtected");
    }
}