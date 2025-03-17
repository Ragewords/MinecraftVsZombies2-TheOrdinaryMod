using System.Collections.Generic;
using MVZ2.Vanilla.Level;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using static MVZ2.GameContent.Buffs.Level.LevelEasyBuff;

namespace MVZ2.GameContent.Buffs.Level
{
    [BuffDefinition(VanillaBuffNames.Level.levelLunatic)]
    public class LevelLunaticBuff : BuffDefinition
    {
        public LevelLunaticBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(VanillaLevelProps.SPAWN_POINTS_POWER, NumberOperator.AddMultiplie, 0.3f));
            AddAura(new ContraptionAura());
            AddAura(new EnemyAura());
            AddAura(new BlueprintAura());
        }

        public class ContraptionAura : AuraEffectDefinition
        {
            public ContraptionAura() : base()
            {
                BuffID = VanillaBuffID.lunaticContraption;
                UpdateInterval = 30;
            }

            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var level = auraEffect.Source.GetLevel();
                results.AddRange(level.GetEntities(EntityTypes.PLANT));
            }
        }
        public class EnemyAura : AuraEffectDefinition
        {
            public EnemyAura() : base()
            {
                BuffID = VanillaBuffID.lunaticEnemy;
                UpdateInterval = 30;
            }

            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var level = auraEffect.Source.GetLevel();
                results.AddRange(level.GetEntities(EntityTypes.ENEMY));
            }
        }
        public class BlueprintAura : AuraEffectDefinition
        {
            public BlueprintAura() : base()
            {
                BuffID = VanillaBuffID.SeedPack.lunaticBlueprint;
                UpdateInterval = 30;
            }

            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var level = auraEffect.Source.GetLevel();
                results.AddRange(level.GetAllSeedPacks());
            }
        }
    }
}
