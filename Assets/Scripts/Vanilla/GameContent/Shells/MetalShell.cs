﻿using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Shells;
using PVZEngine.Damages;
using PVZEngine.Level;

namespace MVZ2.GameContent.Shells
{
    [ShellDefinition(VanillaShellNames.metal)]
    public class MetalShell : ShellDefinition
    {
        public MetalShell(string nsp, string name) : base(nsp, name)
        {
            SetProperty(VanillaShellProps.HIT_SOUND, VanillaSoundID.shieldHit);
            SetProperty(VanillaShellProps.BLOCKS_SLICE, true);
        }
    }
}
