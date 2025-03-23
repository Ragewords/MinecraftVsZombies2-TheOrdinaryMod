using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
            contraption.HealEffects(HEAL_PER_FRAME * (3 - contraption.GetHealthState(3)), contraption);
            if (contraption.GetHealthState(3) > 0)
                contraption.SetAnimationFloat("HealthUP", 0);
            else
                contraption.SetAnimationFloat("HealthUP", 1);
        }
        protected override void UpdateLogic(Entity contraption)
        {
            base.UpdateLogic(contraption);
            bool evoked = contraption.HasBuff<DreamCrystalEvocationBuff>();
            contraption.SetEvoked(evoked);
            contraption.SetAnimationInt("HealthState", contraption.GetHealthState(3));
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
        public const float HEAL_PER_FRAME = 2f;
        public static FrameTimer GetHealthUpTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_HEALTH_UP_TIMER);
        public static void SetHealthUpTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, PROP_HEALTH_UP_TIMER, value);
        public static bool GetHealthUp(Entity entity) => entity.GetBehaviourField<bool>(ID, PROP_HEALTH_UP);
        public static void SetHealthUp(Entity entity, bool value) => entity.SetBehaviourField(ID, PROP_HEALTH_UP, value);
        public static readonly VanillaEntityPropertyMeta PROP_HEALTH_UP_TIMER = new VanillaEntityPropertyMeta("HealthUpTimer");
        public static readonly VanillaEntityPropertyMeta PROP_HEALTH_UP = new VanillaEntityPropertyMeta("HealthUp");
        private static readonly NamespaceID ID = VanillaContraptionID.dreamCrystal;
    }
}
