using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Models;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Models;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.levitation)]
    public class LevitationBuff : BuffDefinition
    {
        public LevitationBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(EngineEntityProps.GRAVITY, NumberOperator.Set, -0.2f));
            AddModelInsertion(LogicModelHelper.ANCHOR_CENTER, VanillaModelKeys.levitationStars, VanillaModelID.levitationStars);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMER, new FrameTimer(30));
        }

        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);

            var timer = buff.GetProperty<FrameTimer>(PROP_TIMER);
            timer.Run();
            if (timer.Expired)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("Timer");
        public const int MAX_TIMEOUT = 150;
    }
}
