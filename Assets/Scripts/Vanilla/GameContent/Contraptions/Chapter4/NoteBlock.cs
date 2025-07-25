using System.Collections.Generic;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.noteBlock)]
    public class NoteBlock : DispenserFamily
    {
        public NoteBlock(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
            SetWaveTimer(entity, new FrameTimer(WAVE_INTERVAL));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            ShootTick(entity);
            var waveTimer = GetWaveTimer(entity);
            waveTimer.Run(entity.GetAttackSpeed());
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("Loud", entity.HasBuff<NoteBlockLoudBuff>());
        }
        public override void OnShootTick(Entity entity)
        {
            var children = GetNoteChildren(entity);
            if (children != null)
            {
                children.RemoveAll(id => !id.Exists(entity.Level));
                if (children.Count >= MAX_NOTE_COUNT)
                {
                    return;
                }
            }
            base.OnShootTick(entity);
        }
        public override Entity Shoot(Entity entity)
        {
            var projectile = base.Shoot(entity);
            if (projectile != null)
            {
                projectile.SetParent(entity);
                var h = projectile.RNG.NextFloat();
                var color = Color.HSVToRGB(h, 1, 1);
                projectile.SetTint(color);
                PlayHarpSound(entity);

                AddNoteChild(entity, projectile);
            }
            return projectile;
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.AddBuff<NoteBlockLoudBuff>();
            entity.PlaySound(VanillaSoundID.ufo);
            entity.PlaySound(VanillaSoundID.growBig);

            if (entity.HasBuff<NoteBlockChargedBuff>())
            {
                entity.Level.ShakeScreen(15, 0, 90);
                foreach (var target in entity.Level.FindEntities(e => e.IsHostile(entity) && e.IsVulnerableEntity()))
                {
                    target.TakeDamage(12 * entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.MUTE), entity);
                    if (target.IsEntityOf(VanillaBossID.theGiant))
                    {
                        TheGiant.Stun(target, 150);
                    }
                    else if (target.Type == EntityTypes.ENEMY)
                    {
                        target.Stun(300);
                    }
                }
                entity.Spawn(VanillaEffectID.amplifiedRoar, entity.GetCenter());
                entity.RemoveBuffs<NoteBlockChargedBuff>();
                entity.PlaySound(VanillaSoundID.giantRoar, 2);
            }
        }
        public override bool CanEvoke(Entity entity)
        {
            if (entity.HasBuff<NoteBlockLoudBuff>())
            {
                return false;
            }
            return base.CanEvoke(entity);
        }
        protected override int GetTimerTime(Entity entity)
        {
            return FIRE_INTERVAL;
        }
        public static void PlayHarpSound(Entity entity)
        {
            var pitch = entity.RNG.NextFloat() + 0.5f;
            var volume = entity.HasBuff<NoteBlockLoudBuff>() ? 5 : 1;
            entity.PlaySound(VanillaSoundID.harp, pitch, volume);
        }
        public static void SonicWave(Entity entity)
        {
            var waveTimer = GetWaveTimer(entity);
            if (waveTimer.Expired)
            {
                var rangeMultiplier = entity.HasBuff<NoteBlockLoudBuff>() ? 2 : 1;
                var param = entity.GetSpawnParams();
                param.SetProperty(VanillaEntityProps.DAMAGE, entity.GetDamage() / 5 * 3);
                param.SetProperty(EngineEntityProps.SIZE, new Vector3(240, 5, 240) * rangeMultiplier);
                var wave = entity.Spawn(VanillaEffectID.soundwave, entity.GetCenter(), param);
                Soundwave.SetLoud(wave, entity.HasBuff<NoteBlockLoudBuff>());
                waveTimer.Reset();
            }
        }
        public static List<EntityID> GetNoteChildren(Entity entity)
        {
            return entity.GetBehaviourField<List<EntityID>>(PROP_NOTE_CHILDREN);
        }
        public static void AddNoteChild(Entity entity, Entity child)
        {
            var children = GetNoteChildren(entity);
            if (children == null)
            {
                children = new List<EntityID>();
                entity.SetBehaviourField(PROP_NOTE_CHILDREN, children);
            }
            children.Add(new EntityID(child));
        }
        public const int FIRE_INTERVAL = 45;
        public const int WAVE_INTERVAL = 5;
        public const int MAX_NOTE_COUNT = 10;
        public static FrameTimer GetWaveTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_WAVE_TIMER);
        public static void SetWaveTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_WAVE_TIMER, timer);
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_WAVE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("WaveTimer");
        private static readonly VanillaEntityPropertyMeta<List<EntityID>> PROP_NOTE_CHILDREN = new VanillaEntityPropertyMeta<List<EntityID>>("NoteChildren");
    }
}
