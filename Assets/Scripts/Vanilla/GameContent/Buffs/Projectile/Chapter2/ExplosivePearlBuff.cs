using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Projectiles
{
    [BuffDefinition(VanillaBuffNames.explosivePearl)]
    public class ExplosivePearlBuff : BuffDefinition
    {
        public ExplosivePearlBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_PROJECTILE_HIT, PostProjectileHitCallback);
            AddModifier(new ColorModifier(EngineEntityProps.COLOR_OFFSET, new Color(1, 0, 0, 0.5f)));
        }

        private void PostProjectileHitCallback(VanillaLevelCallbacks.PostProjectileHitParams param, CallbackResult result)
        {
            var hit = param.hit;
            var projectile = hit.Projectile;
            var buff = projectile.GetFirstBuff<ExplosivePearlBuff>();
            if (buff == null)
                return;
            projectile.Explode(projectile.GetCenter(), 120, projectile.GetFaction(), 200, new DamageEffectList(VanillaDamageEffects.IGNORE_ARMOR));
            Explosion.Spawn(projectile, projectile.GetCenter(), 120);
            projectile.PlaySound(VanillaSoundID.explosion);
            projectile.Level.ShakeScreen(15, 0, 10);
            var count = buff.GetProperty<int>(PROP_HIT_COUNT);
            count++;
            buff.SetProperty(PROP_HIT_COUNT, count);
            if (count >= 3)
                projectile.Remove();
        }
        public static readonly VanillaBuffPropertyMeta<int> PROP_HIT_COUNT = new VanillaBuffPropertyMeta<int>("hitCount");
    }
}
