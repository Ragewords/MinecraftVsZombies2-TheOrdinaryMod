﻿using MVZ2.Models;

namespace MVZ2.Entities
{
    public class ArmorModelInterface : EntityModelInterface
    {
        public ArmorModelInterface(EntityController ctrl) : base(ctrl) { }
        protected override Model GetModel()
        {
            return controller.Model.GetArmorModel();
        }
    }
}
