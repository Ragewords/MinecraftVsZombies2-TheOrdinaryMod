using MVZ2.GameContent.Recharges;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.SeedPacks
{
    [BuffDefinition(VanillaBuffNames.SeedPack.commandBlockBlueprint)]
    public class CommandBlockBlueprintBuff : BuffDefinition
    {
        public CommandBlockBlueprintBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(EngineSeedProps.COST, NumberOperator.Set, 50));
            AddModifier(new NamespaceIDModifier(EngineSeedProps.RECHARGE_ID, NEXT_RECHARGE_ID));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            var game = Global.Game;
            var rechargeID = buff.GetSeedPack().GetRechargeID();
            var rechargeDef = game.GetRechargeDefinition(rechargeID);
            if (rechargeDef == null)
                return;
            switch (rechargeDef.GetQuality())
            {
                case NONE:
                    buff.SetProperty(NEXT_RECHARGE_ID, VanillaRechargeID.shortTime);
                    break;
                case SHORT:
                    buff.SetProperty(NEXT_RECHARGE_ID, VanillaRechargeID.mediumTime);
                    break;
                case MEDIUM:
                    buff.SetProperty(NEXT_RECHARGE_ID, VanillaRechargeID.longTime);
                    break;
                case LONG:
                    buff.SetProperty(NEXT_RECHARGE_ID, VanillaRechargeID.veryLongTime);
                    break;
                case VERY_LONG:
                    buff.SetProperty(NEXT_RECHARGE_ID, VanillaRechargeID.fairlyLongTime);
                    break;
            }
        }
        public const int NONE = 0;
        public const int SHORT = 1;
        public const int MEDIUM = 2;
        public const int LONG = 3;
        public const int VERY_LONG = 6;
        public static readonly VanillaBuffPropertyMeta<NamespaceID> NEXT_RECHARGE_ID = new VanillaBuffPropertyMeta<NamespaceID>("RechargeID");
    }
}
