using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.dreamCrystal)]
    public class DreamCrystal : ContraptionBehaviour
    {
        public DreamCrystal(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void UpdateAI(Entity contraption)
        {
            base.UpdateAI(contraption);
            var health = contraption.Health;
            var maxHealth = contraption.GetMaxHealth();
            var heal_multipiler = GetDividedValue3(health, maxHealth);
            contraption.HealEffects(HEAL_PER_FRAME * heal_multipiler, contraption);
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
        public int GetDividedValue3(float inputValue, float maxValue)
        {
            var firstThird = maxValue / 3f;
            var secondThird = firstThird * 2f;
            inputValue = Mathf.Clamp(inputValue, 0f, maxValue);

            if (inputValue < firstThird) return 3;
            else if (inputValue < secondThird) return 2;
            else return 1;
        }
        public const float HEAL_PER_FRAME = 4 / 3;
    }
}
