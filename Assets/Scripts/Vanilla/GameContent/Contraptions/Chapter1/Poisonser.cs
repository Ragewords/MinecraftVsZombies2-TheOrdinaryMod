using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.poisonser)]
    public class Poisonser : DispenserFamily
    {
        public Poisonser(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
            SetStateTimer(entity, new FrameTimer(60));
            entity.SetAnimationBool("Evoked", false);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            var timer = GetStateTimer(entity);
            if (!entity.IsEvoked())
            {
                ShootTick(entity);
                timer.Run();
                if (timer.Expired)
                {
                    entity.SetAnimationBool("Evoked", false);
                }
                return;
            }
        }
        public override Entity Shoot(Entity entity)
        {
            var projectile = base.Shoot(entity);
            projectile.Timeout = Mathf.CeilToInt(entity.GetRange() / entity.GetShotVelocity().magnitude);
            return projectile;
        }

        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var timer = GetStateTimer(entity);
            timer.Reset();
            entity.SetAnimationBool("Evoked", true);
            entity.PlaySound(VanillaSoundID.poisonGas);
            var level = entity.Level;
            int startLine = -1;
            int endLine = 1;
            int startCol = 1;
            int endCol = 3;
            var lane = entity.GetLane();
            var column = entity.GetColumn();
            if (lane == 0)
            {
                endLine = 0;
            }
            if (lane == level.GetMaxLaneCount() - 1)
            {
                startLine = 0;
            }
            if (column == level.GetMaxColumnCount() - 1)
            {
                endCol = 1;
            }

            for (int i = startLine; i <= endLine; i++)
            {
                for (int j = startCol; j <= endCol; j++)
                {
                    var x = entity.Position.x + level.GetGridWidth() * entity.GetFacingX() * j;
                    var z = entity.Position.z + level.GetGridHeight() * i;
                    var y = level.GetGroundY(x, z);
                    Vector3 gasPos = new Vector3(x, y, z);
                    var gas = level.Spawn(VanillaProjectileID.poisonGas, gasPos, entity);
                    gas.SetFaction(entity.GetFaction());
                }
            }
        }
        public static void SetStateTimer(Entity entity, FrameTimer timer)
        {
            entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        }
        public static FrameTimer GetStateTimer(Entity entity)
        {
            return entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        }
        public static readonly NamespaceID ID = VanillaContraptionID.poisonser;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
    }
}
