using System.Collections.Generic;
using MVZ2.Vanilla.SeedPacks;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Level;

namespace MVZ2.GameContent.Buffs.Level
{
    [BuffDefinition(VanillaBuffNames.Level.commandBlockBlueprint)]
    public class CommandBlockSeedPackBuff : BuffDefinition
    {
        public CommandBlockSeedPackBuff(string nsp, string name) : base(nsp, name)
        {
            AddAura(new SeedAura());
        }

        public class SeedAura : AuraEffectDefinition
        {
            public SeedAura() : base()
            {
                BuffID = VanillaBuffID.SeedPack.commandBlockBlueprint;
                UpdateInterval = 4;
            }

            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var level = auraEffect.Source.GetLevel();
                foreach (var seed in level.GetAllSeedPacks())
                {
                    var seedDef = seed?.Definition;
                    var isCommandBlock = seed?.IsCommandBlock();
                    if (isCommandBlock == false)
                        continue;
                    if (seedDef == null)
                        continue;
                    if (seedDef.GetCost() <= 100)
                        continue;
                    results.Add(seed);
                }
            }
        }
    }
}
