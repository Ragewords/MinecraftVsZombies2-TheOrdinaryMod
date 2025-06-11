using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.anvil)]
    public class Anvil : ContraptionBehaviour
    {
        public Anvil(string nsp, string name) : base(nsp, name)
        {
            smashDetector = new CollisionDetector();
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskFriendly |= EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY | EntityCollisionHelper.MASK_OBSTACLE | EntityCollisionHelper.MASK_BOSS;
            entity.CollisionMaskHostile |= EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY | EntityCollisionHelper.MASK_OBSTACLE | EntityCollisionHelper.MASK_BOSS;
        }
        protected override void UpdateLogic(Entity contraption)
        {
            base.UpdateLogic(contraption);
            contraption.SetModelHealthStateByCount(3);
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state != EntityCollisionHelper.STATE_ENTER)
                return;
            if (!collision.Collider.IsForMain())
                return;
            var anvil = collision.Entity;
            if (anvil.Velocity == Vector3.zero)
                return;
            var other = collision.Other;
            if (!CanSmash(anvil, other))
                return;
            float damageModifier = Mathf.Clamp(anvil.Velocity.magnitude, 0, 1);
            collision.OtherCollider.TakeDamage(1800 * damageModifier, new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BOTH_ARMOR_AND_BODY), anvil);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            bool hurt = false;
            foreach (var target in entity.Level.GetEntities(EntityTypes.PLANT))
            {
                if (target.Health < target.GetMaxHealth() && target.GetDefinitionID() != VanillaContraptionID.anvil)
                {
                    target.HealEffects(target.GetMaxHealth() / 3, entity);
                    if (entity.RNG.Next(25) == 1)
                        hurt = true;
                    else
                        entity.PlaySound(VanillaSoundID.anvil_fix);
                }
            }
            if (hurt)
            {
                entity.TakeDamage(entity.GetMaxHealth() / 3, new DamageEffectList(VanillaDamageEffects.MUTE), entity);
                if (entity.Health <= entity.GetMaxHealth() / 3)
                    entity.PlaySound(VanillaSoundID.anvil_fix_break);
                else
                    entity.PlaySound(VanillaSoundID.anvil_fix);
            }
        }
        public override void PostContactGround(Entity anvil, Vector3 velocity)
        {
            base.PostContactGround(anvil, velocity);

            if (velocity != Vector3.zero)
            {
                smashBuffer.Clear();
                smashDetector.DetectMultiple(anvil, smashBuffer);
                foreach (var target in smashBuffer)
                {
                    var other = target.Entity;
                    if (CanSmash(anvil, other))
                    {
                        float damageModifier = Mathf.Clamp(velocity.magnitude, 0, 1);
                        target.TakeDamage(1800 * damageModifier, new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BOTH_ARMOR_AND_BODY), anvil);
                    }
                }
            }


            anvil.PlaySound(VanillaSoundID.anvil);

            var grid = anvil.GetGrid();
            if (grid == null)
                return;
            var selfGridLayers = anvil.GetGridLayersToTake();
            foreach (var layer in selfGridLayers)
            {
                var ent = grid.GetLayerEntity(layer);
                if (CanSmash(anvil, ent))
                {
                    ent.Die(new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.SELF_DAMAGE), anvil, null);
                }
            }
            foreach (Entity target in anvil.Level.FindEntities(e => CanStun(anvil, e)))
            {
                if (target.Type == EntityTypes.ENEMY)
                {
                    var distance = (target.Position - anvil.Position).magnitude;
                    var speed = 20 * Mathf.Lerp(1f, 0.5f, distance / 120);
                    target.Velocity = target.Velocity + Vector3.up * speed;
                    if (target.CanDeactive())
                        target.Stun(30);
                }
            }
        }
        public static bool CanSmash(Entity anvil, Entity other)
        {
            if (anvil == null || other == null)
                return false;
            if (other == anvil)
                return false;
            if (!other.IsVulnerableEntity())
                return false;
            if (anvil.IsHostile(other))
                return true;
            var selfGridLayers = anvil.GetGridLayersToTake();
            var otherGridLayers = other.GetGridLayersToTake();
            if (selfGridLayers == null || otherGridLayers == null)
                return false;
            return selfGridLayers.Any(l => otherGridLayers.Contains(l));
        }
        private static bool CanStun(Entity self, Entity target)
        {
            return target.GetMainCollider().CheckSphere(self.GetCenter(), 120) && self.IsHostile(target) && target.IsOnGround;
        }
        private List<IEntityCollider> smashBuffer = new List<IEntityCollider>();
        private Detector smashDetector;
    }
}
