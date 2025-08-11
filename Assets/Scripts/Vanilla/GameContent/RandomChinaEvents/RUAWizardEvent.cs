using MukioI18n;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using Tools;

namespace MVZ2.GameContent.RandomChinaEvents
{
    [RandomChinaEventDefinition(VanillaRandomChinaEventNames.ruaWizard)]
    public class RUAWizardEvent : RandomChinaEventDefinition
    {
        public RUAWizardEvent(string nsp, string path) : base(nsp, path, NAME)
        {
        }
        public override void Run(Entity contraption, RandomGenerator rng)
        {
            var level = contraption.Level;
            foreach (var plant in level.FindEntities(e => e.Type == EntityTypes.PLANT && e.ExistsAndAlive()))
            {
                plant.AddBuff<RUAWizardBuff>();
            }
            contraption.PlaySound(VanillaSoundID.pirouette);
        }
        [TranslateMsg("随机瓷器事件名称", VanillaStrings.CONTEXT_RANDOM_CHINA_EVENT_NAME)]
        public const string NAME = "你是巫师吗";
    }
}
