using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2Logic;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.tinyBlaze)]
    public class TinyBlaze : EffectBehaviour
    {
        public TinyBlaze(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var collisionMask = EntityCollisionHelper.MASK_ALL;
            entity.CollisionMaskFriendly = collisionMask;
            entity.CollisionMaskHostile = collisionMask;
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            collideBuffer.Clear();
            collideDetector.DetectMultiple(entity, collideBuffer);
            foreach (var collider in collideBuffer)
            {
                var other = collider.Entity;
                if (entity.IsHostile(other))
                {
                    collider.TakeDamage(DAMAGE, new DamageEffectList(VanillaDamageEffects.FIRE, VanillaDamageEffects.MUTE), entity);
                }
            }
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            if (collision.Other.IsFrost())
            {
                var stain = collision.Entity;
                Freeze(stain);
            }
        }
        public static void Freeze(Entity stain)
        {
            stain.PlaySound(VanillaSoundID.fizz);
            stain.Remove();
        }
        public static Entity UpdateBlaze(LevelEngine level, Vector3 position, Entity spawner, SpawnParams param)
        {
            var foundStain = FindBlazeAtPosition(level, position);
            if (foundStain.ExistsAndAlive())
            {
                foundStain.Timeout = foundStain.GetMaxTimeout();
                return foundStain;
            }
            else
            {
                return level.Spawn(VanillaEffectID.tinyBlaze, position, spawner, param);
            }
        }
        public static Entity FindBlazeAtPosition(LevelEngine level, Vector3 position)
        {
            var bounds = GetDetectionBounds(position);

            var mask = EntityCollisionHelper.MASK_EFFECT;
            resultsBuffer.Clear();
            level.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, mask, mask, resultsBuffer);
            return resultsBuffer.FirstOrDefault(c => c.Entity.IsEntityOf(VanillaEffectID.tinyBlaze))?.Entity;
        }
        private static Bounds GetDetectionBounds(Vector3 position)
        {
            var center = position;
            var def = Global.Game.GetEntityDefinition(VanillaEffectID.tinyBlaze);
            var size = def.GetSize() * 0.5f;
            size.y = 800;
            return new Bounds(center, size);
        }
        public const float MAX_FADE_SECONDS = 0.5f;
        public const float DAMAGE = 4 / 3;
        private static List<IEntityCollider> resultsBuffer = new List<IEntityCollider>();
        private Detector collideDetector = new CollisionDetector();
        private List<IEntityCollider> collideBuffer = new List<IEntityCollider>();
    }
}