using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.funnelenser)]
    public class Funnelenser : DispenserFamily
    {
        public Funnelenser(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 60f);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            ShootTick(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("AIFrozen", entity.IsAIFrozen());
            entity.SetAnimationBool("Upgrade", entity.HasBuff<FunnelenserEvokedBuff>());
        }
        public override bool CanEvoke(Entity entity)
        {
            if (entity.HasBuff<FunnelenserEvokedBuff>())
            {
                return false;
            }
            return base.CanEvoke(entity);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.AddBuff<FunnelenserEvokedBuff>();
            entity.PlaySound(VanillaSoundID.ufo);
        }
    }
}
