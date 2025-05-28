using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.frankensteinHead)]
    public class FrankensteinHeadEnemy : BossBehaviour
    {
        public FrankensteinHeadEnemy(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetStateTimer(entity, new FrameTimer(SHOOT_COOLDOWN));
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 60f);
            entity.AddBuff<FrankensteinSteelBuff>();
            SetFireTimer(entity, new FrameTimer(45));
            SetMode(entity, GUN_MODE);
            entity.SetAnimationBool("Jet", false);
        }
        protected override void UpdateAI(Entity enemy)
        {
            var shootTimer = GetStateTimer(enemy);
            shootTimer.Run(enemy.GetAttackSpeed());
            var state = GetState(enemy);
            var mode = GetMode(enemy);
            Vector3 target = GetMoveTarget(enemy);
            var posi = enemy.Position;
            posi.x = posi.x * 0.8f + target.x * 0.2f;
            posi.z = posi.z * 0.8f + target.z * 0.2f;
            enemy.Position = posi;

            switch (state)
            {
                case SUBSTATE_GUN_READY:
                    if (shootTimer.Expired)
                    {
                        enemy.SetAnimationBool("JawOpen", true);
                        shootTimer.ResetTime(SHOOT_DURATION);
                        var pos = enemy.Position;
                        var level = enemy.Level;
                        pos.x = level.GetEntityColumnX(enemy.IsFacingLeft() ? enemy.RNG.Next(level.GetMaxColumnCount() - 4, level.GetMaxColumnCount() - 1) : enemy.RNG.Next(0, 3));
                        var lane = enemy.RNG.Next(level.GetMaxLaneCount());
                        pos.z = level.GetEntityLaneZ(lane);
                        SetMoveTarget(enemy, pos);
                        enemy.PlaySound(VanillaSoundID.gunReload);
                        SetState(enemy, SUBSTATE_PRE_FIRE);
                    }
                    break;
                case SUBSTATE_PRE_FIRE:
                    if (shootTimer.Expired)
                    {
                        enemy.SetAnimationBool("JawOpen", true);
                        shootTimer.ResetTime(SHOOT_DURATION / 2);
                        SetState(enemy, SUBSTATE_GUN_FIRE);
                    }
                    break;
                case SUBSTATE_GUN_FIRE:
                    if (shootTimer.Expired)
                    {
                        switch (mode)
                        {
                            case GUN_MODE:
                                {
                                    var fireTimer = GetFireTimer(enemy);
                                    fireTimer.Run(enemy.GetAttackSpeed());
                                    if (fireTimer.Expired)
                                    {
                                        enemy.SetAnimationBool("JawOpen", false);
                                        shootTimer.ResetTime(SHOOT_COOLDOWN);
                                        fireTimer.Reset();
                                        SetState(enemy, SUBSTATE_GUN_READY);
                                        SetMode(enemy, MISSILE_MODE);
                                    }
                                    else
                                    {
                                        var bullet = enemy.ShootProjectile(new ShootParams()
                                        {
                                            projectileID = VanillaProjectileID.bullet,
                                            position = new Vector3(enemy.Position.x - 20, enemy.Position.y - 18, enemy.Position.z),
                                            velocity = enemy.GetShotVelocity() * (enemy.IsFacingLeft() ? -1 : 1),
                                            damage = enemy.GetDamage() * 0.1f,
                                            faction = enemy.GetFaction(),
                                            soundID = VanillaSoundID.gunShot,
                                        });
                                    }
                                }
                                break;
                            case MISSILE_MODE:
                                {
                                    var missileSpeed = enemy.GetShotVelocity();
                                    var missile = enemy.ShootProjectile(new ShootParams()
                                    {
                                        projectileID = VanillaProjectileID.missile,
                                        position = new Vector3(enemy.Position.x, enemy.Position.y - 20, enemy.Position.z),
                                        velocity = missileSpeed * (enemy.IsFacingLeft() ? -1 : 1),
                                        damage = enemy.GetDamage() * 2,
                                        faction = enemy.GetFaction(),
                                        soundID = VanillaSoundID.missile
                                    });
                                    var pos = enemy.Position;
                                    pos.x += 80;
                                    SetMoveTarget(enemy, pos);
                                    SetMode(enemy, GUN_MODE);
                                    enemy.SetAnimationBool("JawOpen", false);
                                    shootTimer.ResetTime(SHOOT_COOLDOWN);
                                    SetState(enemy, SUBSTATE_GUN_READY);
                                }
                                break;
                        }
                    }
                    break;
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            if (!entity.Parent.ExistsAndAlive())
            {
                entity.Die(new DamageEffectList(VanillaDamageEffects.NO_NEUTRALIZE), entity);
                return;
            }
        }
        public override void PreTakeDamage(DamageInput damageInfo, CallbackResult result)
        {
            base.PreTakeDamage(damageInfo, result);
            if (damageInfo.Amount > 300)
            {
                damageInfo.SetAmount(300);
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            entity.SetAnimationBool("Jet", true);
            entity.SetAnimationBool("JawOpen", false);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            var headEffect = entity.Level.Spawn(VanillaEffectID.frankensteinHead, entity.Position, entity);
            headEffect.Velocity += new Vector3(entity.GetFacingX() * 10, 1, 0);
            headEffect.SetDisplayScale(new Vector3(-entity.GetFacingX(), 1, 1));
            FrankensteinHead.SetSteelPhase(headEffect, true);
            entity.PlaySound(VanillaSoundID.explosion);
            entity.PlaySound(VanillaSoundID.powerOff);
            var expPart = entity.Level.Spawn(VanillaEffectID.explosion, entity.Position, entity);
            expPart.SetSize(Vector3.one * 60);
            entity.Remove();
        }
        public static FrameTimer GetStateTimer(Entity enemy)
        {
            return enemy.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        }
        public static void SetStateTimer(Entity enemy, FrameTimer value)
        {
            enemy.SetBehaviourField(ID, PROP_STATE_TIMER, value);
        }
        public static FrameTimer GetFireTimer(Entity enemy)
        {
            return enemy.GetBehaviourField<FrameTimer>(ID, PROP_FIRE_TIMER);
        }
        public static void SetFireTimer(Entity enemy, FrameTimer value)
        {
            enemy.SetBehaviourField(ID, PROP_FIRE_TIMER, value);
        }
        public static int GetState(Entity enemy)
        {
            return enemy.GetBehaviourField<int>(ID, PROP_STATE);
        }
        public static void SetState(Entity enemy, int value)
        {
            enemy.SetBehaviourField(ID, PROP_STATE, value);
        }
        public static int GetMode(Entity enemy)
        {
            return enemy.GetBehaviourField<int>(ID, PROP_MODE);
        }
        public static void SetMode(Entity enemy, int value)
        {
            enemy.SetBehaviourField(ID, PROP_MODE, value);
        }
        public static Vector3 GetMoveTarget(Entity boss)
        {
            return boss.GetBehaviourField<Vector3>(ID, PROP_MOVE_TARGET);
        }
        public static void SetMoveTarget(Entity boss, Vector3 target)
        {
            boss.SetBehaviourField(ID, PROP_MOVE_TARGET, target);
        }
        public static readonly NamespaceID ID = VanillaBossID.frankensteinHead;
        public const int SHOOT_COOLDOWN = 300;
        public const int SHOOT_DURATION = 20;
        private static readonly VanillaEntityPropertyMeta<Vector3> PROP_MOVE_TARGET = new VanillaEntityPropertyMeta<Vector3>("MoveTarget");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_FIRE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("FireTimer");
        public static readonly VanillaEntityPropertyMeta<int> PROP_STATE = new VanillaEntityPropertyMeta<int>("State");
        public static readonly VanillaEntityPropertyMeta<int> PROP_MODE = new VanillaEntityPropertyMeta<int>("Mode");
        private const int SUBSTATE_GUN_READY = 0;
        private const int SUBSTATE_PRE_FIRE = 1;
        private const int SUBSTATE_GUN_FIRE = 2;
        private const int GUN_MODE = 0;
        private const int MISSILE_MODE = 1;
    }
}