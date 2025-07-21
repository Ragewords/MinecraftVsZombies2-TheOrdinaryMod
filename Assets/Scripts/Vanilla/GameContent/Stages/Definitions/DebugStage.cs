using MVZ2.GameContent.Artifacts;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.ProgressBars;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Definitions;
using PVZEngine.Level;

namespace MVZ2.GameContent.Stages
{
    [StageDefinition(VanillaStageNames.debug)]
    public partial class DebugStage : StageDefinition
    {
        public DebugStage(string nsp, string name) : base(nsp, name)
        {
            //AddBehaviour(new ConveyorStageBehaviour(this));
        }
        public override void OnStart(LevelEngine level)
        {
            base.OnStart(level);
            ClassicStart(level);
            //ConveyorStart(level);
            level.LevelProgressVisible = true;
            level.SetProgressBarToBoss(VanillaProgressBarID.nightmare);
            level.SetTriggerActive(true);
            var cartRef = level.GetCartReference();
            level.SpawnCarts(cartRef, VanillaLevelExt.CART_START_X, 20);
        }
        public override void OnUpdate(LevelEngine level)
        {
            base.OnUpdate(level);
            level.SetEnergy(9990);
            level.SetStarshardSlotCount(5);
            level.SetStarshardCount(3);
            level.CheckGameOver();
        }
        private void ClassicStart(LevelEngine level)
        {
            level.SetEnergy(9990);
            level.SetSeedSlotCount(10);
            level.FillSeedPacks(new NamespaceID[]
            {
                VanillaContraptionID.dispenser,
                VanillaContraptionID.furnace,
                VanillaContraptionID.dreamCrystal,
                VanillaContraptionID.lightningOrb,
                VanillaContraptionID.devourer,
                VanillaContraptionID.blackHoleBomb,
                VanillaEnemyID.ironHelmettedZombie,
                VanillaEnemyID.tanookiZombie,
                VanillaEnemyID.shikaisenZombie,
                VanillaBossID.frankenstein,
            });
            level.SetArtifactSlotCount(3);
            level.ReplaceArtifacts(new NamespaceID[]
            {
                VanillaArtifactID.greedyVacuum,
                VanillaArtifactID.almanac,
                VanillaArtifactID.dreamKey,
            });
            level.SetRechargeSpeed(9999999);
        }
        private void ConveyorStart(LevelEngine level)
        {
            level.SetConveyorSlotCount(10);
            level.AddConveyorSeedPack(VanillaBossID.slenderman);
        }
    }
}
