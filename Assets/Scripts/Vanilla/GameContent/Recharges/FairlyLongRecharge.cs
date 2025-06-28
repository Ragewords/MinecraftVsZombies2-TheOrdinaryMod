using MVZ2.Vanilla;
using PVZEngine.Definitions;
using PVZEngine.Level;

namespace MVZ2.GameContent.Recharges
{
    [RechargeDefinition(VanillaRechargeNames.fairlyLongTime)]
    public class FairlyLongRecharge : RechargeDefinition
    {
        public FairlyLongRecharge(string nsp, string name) : base(nsp, name)
        {
            SetProperty(EngineRechargeProps.START_MAX_RECHARGE, 1050);
            SetProperty(EngineRechargeProps.MAX_RECHARGE, 1800);
            SetProperty(EngineRechargeProps.QUALITY, 10);
            SetProperty(EngineRechargeProps.NAME, VanillaStrings.RECHARGE_VERY_LONG);
        }
    }
}
