using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.MinecraftStatisticsPrinter.Blocks
{
    internal abstract class Block
    {
        public abstract string Name { get; }
        public abstract BigInteger Density { get; }
        public abstract BlockVolume Volume { get; }
        public abstract int StackSize { get; }
        
        public abstract MinimumMiningToolMaterial MinimumMiningToolMaterial { get; }
        public abstract MiningTool MiningTool { get; }
        public abstract int HandMiningTime { get; }
        public abstract int WoodMiningTime { get; }
        public abstract int StoneMiningTime { get; }
        public abstract int IronMiningTime { get; }
        public abstract int DiamondMiningTime { get; }
        public abstract int GoldMiningTime { get; }
        public abstract int NetheriteMiningTime { get; }
        
        public void PrintBasicData(StringBuilder builder, BigInteger count, int tabLevel)
        {
            var tabs = string.Concat(Enumerable.Repeat("    ", tabLevel));
            
            builder.AppendLine($"{tabs}- Count: {count.PrintNumber()}");

            var totalVolume = Volume switch
            {
                BlockVolume.FullBlock => count,
                BlockVolume.ThreeQuartersBlock => (count * 4) / 3,
                BlockVolume.HalfBlock => count / 2,
                BlockVolume.OneQuarterBlock => count / 4,
                BlockVolume.OneEighthBlock => count / 8,
                _ => throw new ArgumentOutOfRangeException()
            };

            var totalMass = totalVolume * Density;
            builder.AppendLine($"{tabs}- Volume: {totalVolume.PrintNumber()} m^3");
            builder.AppendLine($"{tabs}- Mass: {totalMass.PrintNumber()} kg");

            var inventorySlots = count.CeilingDivide(StackSize);
            var singleChests = inventorySlots.CeilingDivide(27);
            var playerInventories = inventorySlots.CeilingDivide(36);
            var doubleChests = inventorySlots.CeilingDivide(54);
            var doubleChestsWithShulkerBoxes = inventorySlots.CeilingDivide(1458);

            builder.AppendLine($"{tabs}- Single Chests/Ender Chests/Shulker Boxes: {singleChests.PrintNumber()}");
            builder.AppendLine($"{tabs}- Player Inventories: {playerInventories.PrintNumber()}");
            builder.AppendLine($"{tabs}- Double Chests: {doubleChests.PrintNumber()}");
            builder.AppendLine($"{tabs}- Double Chests with Shulker Boxes: {doubleChestsWithShulkerBoxes.PrintNumber()}");
            
            builder.AppendLine($"{tabs}- Area: Square {count.Sqrt()} meters on an edge");
            // builder.AppendLine($"{tabs}- Volume: Cube {count.Cbrt()} meters on an edge");
            
            PrintMiningData(builder, count, 0);
        }

        private void PrintMiningData(StringBuilder builder, BigInteger count, int tabLevel)
        {
            var tabs = string.Concat(Enumerable.Repeat("    ", tabLevel));
            var minimumToolName = $"{MinimumMiningToolMaterial} {MiningTool}";
            builder.AppendLine($"{tabs}- Minimum mining tool: {minimumToolName}");

            var requiredMinimumTools = MinimumMiningToolMaterial switch
            {
                MinimumMiningToolMaterial.Hand => 1,
                MinimumMiningToolMaterial.Wood => count.CeilingDivide(59),
                MinimumMiningToolMaterial.Stone => count.CeilingDivide(131),
                MinimumMiningToolMaterial.Iron => count.CeilingDivide(250),
                MinimumMiningToolMaterial.Diamond => count.CeilingDivide(1561),
                _ => throw new ArgumentOutOfRangeException()
            };
            var minimumBaseMaterialName = MinimumMiningToolMaterial switch
            {
                MinimumMiningToolMaterial.Hand => null,
                MinimumMiningToolMaterial.Wood => "Wooden Planks",
                MinimumMiningToolMaterial.Stone => "Cobblestone",
                MinimumMiningToolMaterial.Iron => "Iron Ingot",
                MinimumMiningToolMaterial.Diamond => "Diamond",
                _ => throw new ArgumentOutOfRangeException()
            };
            int baseMaterialPerTool = MiningTool switch
            {
                MiningTool.Pickaxe => 3,
                MiningTool.Shovel => 1,
                MiningTool.Axe => 3,
                MiningTool.Sword => 1,
                _ => throw new ArgumentOutOfRangeException()
            };
            var requiredMinimumBaseMaterial = requiredMinimumTools
                * baseMaterialPerTool;
            var requiredMinimumBaseBlocks = MinimumMiningToolMaterial switch
            {
                MinimumMiningToolMaterial.Hand => 1,
                MinimumMiningToolMaterial.Wood => requiredMinimumBaseMaterial.CeilingDivide(4),
                MinimumMiningToolMaterial.Stone => requiredMinimumBaseMaterial,
                MinimumMiningToolMaterial.Iron => (requiredMinimumBaseMaterial * 10).CeilingDivide(22),
                MinimumMiningToolMaterial.Diamond => (requiredMinimumBaseMaterial * 10).CeilingDivide(22),
                _ => throw new ArgumentOutOfRangeException()
            };
            var minimumBaseBlockName = MinimumMiningToolMaterial switch
            {
                MinimumMiningToolMaterial.Hand => null,
                MinimumMiningToolMaterial.Wood => "Logs",
                MinimumMiningToolMaterial.Stone => "Cobblestone",
                MinimumMiningToolMaterial.Iron => "Iron Ore",
                MinimumMiningToolMaterial.Diamond => "Diamond Ore",
                _ => throw new ArgumentOutOfRangeException()
            };
            var requiredMinimumSticks = requiredMinimumTools * 2;
            var requiredMinimumPlanks = requiredMinimumSticks.CeilingDivide(2);
            var requiredMinimumLogs = requiredMinimumPlanks.CeilingDivide(4);

            builder.AppendLine($"{tabs}    - Required {minimumToolName}s: {requiredMinimumTools.PrintNumber()}");

            if (MinimumMiningToolMaterial != MinimumMiningToolMaterial.Hand)
            {
                builder.AppendLine($"{tabs}    - Required {minimumBaseMaterialName}: {requiredMinimumBaseMaterial.PrintNumber()}");
                builder.AppendLine($"{tabs}        - Required {minimumBaseBlockName}: {requiredMinimumBaseBlocks.PrintNumber()}");
                builder.AppendLine($"{tabs}    - Required Sticks: {requiredMinimumSticks.PrintNumber()}");
                builder.AppendLine($"{tabs}        - Required Wooden Planks: {requiredMinimumPlanks.PrintNumber()}");
                builder.AppendLine($"{tabs}            - Required Logs: {requiredMinimumLogs.PrintNumber()}");
            }

            var requiredEnchantedNetheriteTools = count.CeilingDivide(8124);
            var requiredDiamonds = baseMaterialPerTool * requiredEnchantedNetheriteTools;
            var requiredDiamondOre = (requiredDiamonds * 10).CeilingDivide(22);
            var requiredSticks = requiredEnchantedNetheriteTools * 2;
            var requiredPlanks = requiredSticks.CeilingDivide(2);
            var requiredLogs = requiredSticks.CeilingDivide(4);
            var requiredNetheriteScrapAndGoldIngots = requiredEnchantedNetheriteTools * 4;
            var requiredGoldOre = (requiredNetheriteScrapAndGoldIngots * 10).CeilingDivide(22);
            var requiredSmeltingTime = requiredNetheriteScrapAndGoldIngots * 20;
            var requiredBlocksOfCoal = requiredEnchantedNetheriteTools.CeilingDivide(80);
            var requiredCoal = requiredBlocksOfCoal * 9;
            var requiredCoalOre = (requiredCoal * 10).CeilingDivide(22);
            var requiredLapisLazuliOre = (requiredEnchantedNetheriteTools * 10).CeilingDivide(143);

            builder.AppendLine($"{tabs}- Required Netherite {MiningTool}s with Unbreaking III and Efficiency V: {requiredEnchantedNetheriteTools.PrintNumber()}");
            builder.AppendLine($"{tabs}    - Required Diamonds: {requiredDiamonds.PrintNumber()}");
            builder.AppendLine($"{tabs}        - Required Diamond Ore: {requiredDiamondOre.PrintNumber()}");
            builder.AppendLine($"{tabs}    - Required Sticks: {requiredSticks.PrintNumber()}");
            builder.AppendLine($"{tabs}        - Required Wooden Planks: {requiredPlanks.PrintNumber()}");
            builder.AppendLine($"{tabs}            - Required Logs: {requiredLogs.PrintNumber()}");
            builder.AppendLine($"{tabs}    - Required Ancient Debris: {requiredNetheriteScrapAndGoldIngots.PrintNumber()}");
            builder.AppendLine($"{tabs}    - Required Gold Ingots: {requiredNetheriteScrapAndGoldIngots.PrintNumber()}");
            builder.AppendLine($"{tabs}        - Required Gold Ore: {requiredGoldOre.PrintNumber()}");
            builder.AppendLine($"{tabs}    - Total Smelting Time: {requiredSmeltingTime.PrintNumber()} seconds");
            builder.AppendLine($"{tabs}        - {(requiredSmeltingTime / 86400).PrintNumber()} days");
            builder.AppendLine($"{tabs}        - {(requiredSmeltingTime / 31557600).PrintNumber()} years");
            builder.AppendLine($"{tabs}        - Required Blocks of Coal: {requiredBlocksOfCoal.PrintNumber()}");
            builder.AppendLine($"{tabs}            - Required Coal: {requiredCoal.PrintNumber()}");
            builder.AppendLine($"{tabs}                - Required Coal Ore: {requiredCoalOre.PrintNumber()}");
            builder.AppendLine($"{tabs}    - Required Lapis Lazuli: {requiredEnchantedNetheriteTools.PrintNumber()}");
            builder.AppendLine($"{tabs}    - Required Lapis Lazuli Ore: {requiredLapisLazuliOre.PrintNumber()}");
            
            // TODO: add mining time info using block hardnesses
        }
    }
}
