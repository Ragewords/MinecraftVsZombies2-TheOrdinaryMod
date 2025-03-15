using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using MVZ2Logic.Models;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.rain)]
    public class Rain : EffectBehaviour
    {

        #region 公有方法
        public Rain(string nsp, string name) : base(nsp, name)
        {
            SetProperty(VanillaEntityProps.UPDATE_BEFORE_GAME, true);
            SetProperty(VanillaEntityProps.UPDATE_AFTER_GAME_OVER, true);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.Level.AddLoopSoundEntity(VanillaSoundID.rain, entity.ID);
            entity.SetSortingLayer(SortingLayers.foreground);
            entity.SetSortingOrder(9999);
        }
        #endregion
    }
}