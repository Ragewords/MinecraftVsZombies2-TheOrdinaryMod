using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.dreamCrystal)]
    public class DreamCrystal : ContraptionBehaviour
    {
        public DreamCrystal(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity contraption)
        {
            base.Init(contraption);
        }
        protected override void UpdateAI(Entity contraption)
        {
            base.UpdateAI(contraption);
            var health = contraption.Health;
            var maxHealth = contraption.GetMaxHealth();
            contraption.HealEffects(HEAL_PER_FRAME * (2 - health / maxHealth), contraption);
        }
        protected override void UpdateLogic(Entity contraption)
        {
            base.UpdateLogic(contraption);
            bool evoked = contraption.HasBuff<DreamCrystalEvocationBuff>();
            contraption.SetEvoked(evoked);
            contraption.SetModelDamagePercent();
            contraption.SetAnimationBool("Evoked", evoked);
        }

        protected override void OnEvoke(Entity contraption)
        {
            base.OnEvoke(contraption);
            contraption.SetEvoked(true);
            contraption.Health = contraption.GetMaxHealth();
            contraption.AddBuff<DreamCrystalEvocationBuff>();
            contraption.PlaySound(VanillaSoundID.sparkle);
        }
        public const float HEAL_PER_FRAME = 4 / 3;
        public static FrameTimer GetHealthUpTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_HEALTH_UP_TIMER);
        public static void SetHealthUpTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, PROP_HEALTH_UP_TIMER, value);
        public static bool GetHealthUp(Entity entity) => entity.GetBehaviourField<bool>(ID, PROP_HEALTH_UP);
        public static void SetHealthUp(Entity entity, bool value) => entity.SetBehaviourField(ID, PROP_HEALTH_UP, value);
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_HEALTH_UP_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("HealthUpTimer");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_HEALTH_UP = new VanillaEntityPropertyMeta<bool>("HealthUp");
        private static readonly NamespaceID ID = VanillaContraptionID.dreamCrystal;
    }
}
