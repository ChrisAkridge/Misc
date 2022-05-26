using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.MinecraftStatisticsPrinter.Blocks
{
    internal sealed class Stone : Block
    {
        public override string Name => "Stone";
        public override BigInteger Density => 1600;
        public override int StackSize => 64;
        public override BlockVolume Volume => BlockVolume.FullBlock;
        public override MinimumMiningToolMaterial MinimumMiningToolMaterial => MinimumMiningToolMaterial.Wood;
        public override MiningTool MiningTool => MiningTool.Pickaxe;
        public override int HandMiningTime => 7500;
        public override int WoodMiningTime => 1150;
        public override int StoneMiningTime => 600;
        public override int IronMiningTime => 400;
        public override int DiamondMiningTime => 300;
        public override int GoldMiningTime => 200;
        public override int NetheriteMiningTime => 150;

        public void Print(StringBuilder builder, BigInteger count, int tabLevel)
        {
            var tabs = string.Concat(Enumerable.Repeat("    ", tabLevel));
            builder.AppendLine($"{tabs}- Mines into {count.PrintNumber()} Cobblestone");
            builder.AppendLine($"{tabs}- Crafts into {((count / 4) * 4).PrintNumber()} Stone Bricks");
            builder.AppendLine($"{tabs}- Crafts into {(count * 2).PrintNumber()} Stone Slabs");
            builder.AppendLine($"{tabs}- Crafts into {(count / 3).PrintNumber()} Stone Pickaxes");
            builder.AppendLine($"{tabs}    - Plus {((count / 3) * 2).PrintNumber()} Sticks");
            builder.AppendLine($"{tabs}- Crafts into {(count).PrintNumber()} Stone Shovels");
            builder.AppendLine($"{tabs}    - Plus {(count * 2).PrintNumber()} Sticks");
            builder.AppendLine($"{tabs}- Crafts into {(count / 3).PrintNumber()} Stone Axes");
            builder.AppendLine($"{tabs}    - Plus {((count / 3) * 2).PrintNumber()} Sticks");
            builder.AppendLine($"{tabs}- Crafts into {(count / 2).PrintNumber()} Stone Swords");
            builder.AppendLine($"{tabs}    - Plus {((count / 2) * 2).PrintNumber()} Sticks");
            builder.AppendLine($"{tabs}- Crafts into {(count / 2).PrintNumber()} Stone Hoes");
            builder.AppendLine($"{tabs}    - Plus {((count / 2) * 2).PrintNumber()} Sticks");
            builder.AppendLine($"{tabs}- Crafts into {(count / 2).PrintNumber()} Stone Pressure Plates");
            builder.AppendLine($"{tabs}- Crafts into {count.PrintNumber()} Stone Buttons");
            builder.AppendLine($"{tabs}- Crafts into {(count / 3).PrintNumber()} Redstone Repeaters");
            builder.AppendLine($"{tabs}    - Plus {((count / 3) * 2).PrintNumber()} Redstone Torches");
            builder.AppendLine($"{tabs}        - Plus {((count / 3) * 2).PrintNumber()} Sticks");
            builder.AppendLine($"{tabs}        - Plus {((count / 3) * 2).PrintNumber()} Redstone Dust");
            builder.AppendLine($"{tabs}    - Plus {(count / 3).PrintNumber()} Redstone Dust");
            builder.AppendLine($"{tabs}- Crafts into {(count / 3).PrintNumber()} Redstone Comparators");
            builder.AppendLine($"{tabs}    - Plus {((count / 3) * 3).PrintNumber()} Redstone Torches");
            builder.AppendLine($"{tabs}        - Plus {((count / 3) * 3).PrintNumber()} Sticks");
            builder.AppendLine($"{tabs}        - Plus {((count / 3) * 3).PrintNumber()} Redstone Dust");
            builder.AppendLine($"{tabs}    - Plus {(count / 3).PrintNumber()} Nether Quartz");
            builder.AppendLine($"{tabs}- Crafts into {(count / 3).PrintNumber()} Stonecutters");
            builder.AppendLine($"{tabs}    - Plus {(count / 3).PrintNumber()} Iron Ingots");
        }
    }
}
