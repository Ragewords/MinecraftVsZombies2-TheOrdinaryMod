using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.ruaWizard)]
    public class RUAWizardBuff : BuffDefinition
    {
        public RUAWizardBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new Vector3Modifier(VanillaEntityProps.SHOT_VELOCITY, NumberOperator.Add, ADD_DIAGONAL));
            AddTrigger(VanillaLevelCallbacks.POST_CONTRAPTION_SHOT, PostContraptionShotCallback);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMER, new FrameTimer(MAX_TIMEOUT));
            buff.SetProperty(ADD_DIAGONAL, DIAGONAL_1);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var contraption = buff.GetEntity();
            if (contraption == null)
                return;
            var timer = buff.GetProperty<FrameTimer>(PROP_TIMER);
            timer.Run();
            if (timer.Expired)
                buff.Remove();
        }
        private void PostContraptionShotCallback(EntityCallbackParams param, CallbackResult result)
        {
            var entity = param.entity;
            if (entity == null)
                return;
            if (!entity.HasBuff<RUAWizardBuff>())
                return;
            var buff = entity.GetFirstBuff<RUAWizardBuff>();
            var diag = buff.GetProperty<Vector3>(ADD_DIAGONAL);
            if (diag == DIAGONAL_1)
                buff.SetProperty(ADD_DIAGONAL, DIAGONAL_2);
            else
                buff.SetProperty(ADD_DIAGONAL, DIAGONAL_1);
        }
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("Timer");
        public static readonly VanillaBuffPropertyMeta<Vector3> ADD_DIAGONAL = new VanillaBuffPropertyMeta<Vector3>("AddDiagonal");
        public const int MAX_TIMEOUT = 300;
        public Vector3 DIAGONAL_1 = Vector3.forward * 10;
        public Vector3 DIAGONAL_2 = Vector3.back * 10;
    }
}
