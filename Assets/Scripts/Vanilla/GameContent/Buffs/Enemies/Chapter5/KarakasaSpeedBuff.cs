using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.karakasaSpeed)]
    public class KarakasaSpeedBuff : BuffDefinition
    {
        public KarakasaSpeedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(VanillaEnemyProps.SPEED, NumberOperator.Multiply, PROP_SPEED));
        }
        public static void SetSpeed(Buff buff, float value)
        {
            buff.SetProperty(PROP_SPEED, value);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;

            if (entity.IsOnGround)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<float> PROP_SPEED = new VanillaBuffPropertyMeta<float>("Speed", 1);
    }
}
