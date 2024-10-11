using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.InfinitePowerBeacon.Recipes;

namespace Celarix.JustForFun.InfinitePowerBeacon
{
	internal static class ItemListGenerator
	{
		public static IDictionary<string, ItemInfo> GenerateItemList()
		{
			var items = new Dictionary<string, ItemInfo>();
			
			Add(items, new ItemInfo("Hypercompressed Block of Infinite Power", Hypercompressing("Compressed Block of Infinite Power", 9)));
			Add(items, new ItemInfo("Compressed Block of Infinite Power", Compressing("Block of Infinite Power", 9)));
			Add(items, new ItemInfo("Block of Infinite Power", CraftingSingleIngredient(1, "Infinite Power Crystal", 9)));
			
			Add(items, new ItemInfo("Infinite Power Crystal", Crafting(1,
				"Pickaxe's Heart",
				"Shovel's Heart",
				"Axe's Heart",
				"Sword's Heart",
				"Hoe's Heart",
				"Helmet's Heart",
				"Chestplate's Heart",
				"Leggings' Heart",
				"Boots' Heart")));
			
			// Add the tool hearts
			var toolTypes = new[]
			{
				"Pickaxe", "Shovel", "Axe", "Sword", "Hoe"
			};

			var toolMaterialNames = new[]
			{
				"Wooden", "Coal", "Stone", "Iron", "Gold",
				"Emerald", "Diamond", "Netherite", "Whitestone"
			};

			foreach (var toolType in toolTypes)
			{
				var ingredientReferences = toolMaterialNames.Select(m => $"Ultraheavy {m} {toolType}").ToArray();
				Add(items, new ItemInfo($"{toolType}'s Heart", Crafting(1, ingredientReferences)));
			}
			
			// Add the armor hearts
			var armorTypeNames = new[]
			{
				"Helmet's", "Chestplate's", "Leggings'", "Boots'"
			};

			var armorTypes = new[]
			{
				"Helmet", "Chestplate", "Leggings", "Boots"
			};

			var armorMaterialNames = new[]
			{
				"Leather", "Coal", "Chainmail", "Iron", "Gold", "Emerald", "Diamond", "Netherite", "Whitestone"
			};

			foreach (var armorType in armorTypes)
			{
				var heartName = armorType switch
				{
					"Helmet" => "Helmet's Heart",
					"Chestplate" => "Chestplate's Heart",
					"Leggings" => "Leggings' Heart",
					"Boots" => "Boots' Heart",
					_ => throw new InvalidOperationException($"Unknown armor type '{armorType}'.")
				};
				var ingredientReferences = armorMaterialNames.Select(m => $"Ultraheavy {m} {armorType}").ToArray();
				Add(items, new ItemInfo($"{heartName}", Crafting(1, ingredientReferences)));
			}
			
			// Add the ultraheavy tools
			var hypercompressedBlocksNeededForEachToolType = new[]
			{
				3, 1, 3, 2, 2
			};

			var oakLogsNeededForEachToolType = new[]
			{
				2, 2, 2, 1, 2
			};

			var hypercompressedToolBlockNames = new[]
			{
				"Hypercompressed Oak Log",
				"Hypercompressed Block of Coal",
				// stone handled separately
				"Hypercompressed Block of Iron",
				"Hypercompressed Block of Gold",
				"Hypercompressed Block of Emerald",
				"Hypercompressed Block of Diamond",
				// Netherite handled separately
				// Whitestone handled separately
			};
			
			var ultraheavyOutputs = new[]
			{
				"Ultraheavy Wooden",
				"Ultraheavy Coal",
				// stone handled separately
				"Ultraheavy Iron",
				"Ultraheavy Gold",
				"Ultraheavy Emerald",
				"Ultraheavy Diamond",
				// Netherite handled separately
				// Whitestone handled separately
			};

			for (var i = 0; i < 5; i++)
			{
				var toolType = toolTypes[i];
				var hypercompressedBlocksNeeded = hypercompressedBlocksNeededForEachToolType[i];
				var oakLogsNeeded = oakLogsNeededForEachToolType[i];

				for (var j = 0; j < 6; j++)
				{
					var hypercompressedBlockName = hypercompressedToolBlockNames[j];
					var ultraheavyOutput = $"{ultraheavyOutputs[j]} {toolType}";

					Add(items, new ItemInfo(ultraheavyOutput, Crafting(1,
						Enumerable
							.Repeat(hypercompressedBlockName, hypercompressedBlocksNeeded)
							.Concat(Enumerable.Repeat("Oak Log", oakLogsNeeded))
							.ToArray())));
				}
			}

			// Add the ultraheavy stone tools
			for (var i = 0; i < toolTypes.Length; i++)
			{
				var toolType = toolTypes[i];
				var hypercompressedCobblestoneNeeded = hypercompressedBlocksNeededForEachToolType[i];
				var oakPlanksNeeded = oakLogsNeededForEachToolType[i];

				Add(items,
					new ItemInfo($"Ultraheavy Stone {toolType}",
						Hypercompressing($"Pile of Superheavy Stone {toolType}s", 1)));

				Add(items,
					new ItemInfo($"Pile of Superheavy Stone {toolType}s",
						CraftingSingleIngredient(1, $"Superheavy Stone {toolType}", 9)));
				Add(items, new ItemInfo($"Superheavy Stone {toolType}", Crafting(1,
					Enumerable
						.Repeat("Hypercompressed Cobblestone", hypercompressedCobblestoneNeeded)
						.Concat(Enumerable
							.Repeat("Oak Planks", oakPlanksNeeded))
						.ToArray())));
			}
			
			// Add the ultraheavy Netherite tools

			foreach (var toolType in toolTypes)
			{
				Add(items,
					new ItemInfo($"Ultraheavy Netherite {toolType}",
						Smithing($"Ultraheavy Diamond {toolType}", "Hypercompressed Block of Netherite",
							"Ultraheavy Netherite Upgrade Template")));
			}
			
			// Add the ultraheavy Whitestone tools
			foreach (var toolType in toolTypes)
			{
				Add(items, new ItemInfo($"Ultraheavy Whitestone {toolType}", Purifying("Totem of Undying", "Nether Star", "Dragon's Breath", $"Ultraheavy Diamond {toolType}")));	
			}

			// Add the armor hearts
			var hypercompressedArmorBlockNames = new[]
			{
				"Hypercompressed Block of Leather",
				"Hypercompressed Block of Coal",
				"Hypercompressed Block of Iron Chains",
				"Hypercompressed Block of Iron",
				"Hypercompressed Block of Gold",
				"Hypercompressed Block of Emerald",
				"Hypercompressed Block of Diamond"
				// Netherite handled separately
				// Whitestone handled separately
			};

			var ultraheavyArmorOutputs = new[]
			{
				"Ultraheavy Leather",
				"Ultraheavy Coal",
				"Ultraheavy Chainmail",
				"Ultraheavy Iron",
				"Ultraheavy Gold",
				"Ultraheavy Emerald",
				"Ultraheavy Diamond"
				// Netherite handled separately
				// Whitestone handled separately
			};

			var hypercompressedBlocksNeededForEachArmorType = new[]
			{
				5, 8, 7, 4
			};

			for (var i = 0; i < armorTypes.Length; i++)
			{
				var armorType = armorTypes[i];
				var hypercompressedBlocksNeeded = hypercompressedBlocksNeededForEachArmorType[i];

				for (var j = 0; j < 7; j++)
				{
					var hypercompressedBlockName = hypercompressedArmorBlockNames[j];
					var ultraheavyOutput = $"{ultraheavyArmorOutputs[j]} {armorType}";

					Add(items, new ItemInfo(ultraheavyOutput, CraftingSingleIngredient(1, hypercompressedBlockName, hypercompressedBlocksNeeded)));
				}
			}
			
			// Add the ultraheavy Netherite armor
			foreach (var armorType in armorTypes)
			{
				Add(items,
					new ItemInfo($"Ultraheavy Netherite {armorType}",
						Smithing($"Ultraheavy Diamond {armorType}", "Hypercompressed Block of Netherite",
							"Ultraheavy Netherite Upgrade Template")));
			}

			// Add the ultraheavy Whitestone armor
			foreach (var armorType in armorTypes)
			{
				Add(items,
					new ItemInfo($"Ultraheavy Whitestone {armorType}",
						Purifying("Totem of Undying", "Nether Star", "Dragon's Breath",
							$"Ultraheavy Diamond {armorType}")));
			}

			// Add the wood path
			Add(items, new ItemInfo("Hypercompressed Oak Log", Hypercompressing("Compressed Oak Log", 9)));
			Add(items, new ItemInfo("Compressed Oak Log", Compressing("Oak Log", 9)));
			Add(items, new ItemInfo("Oak Log", Mining("Oak Log", false, MiningTier.Anything, 0.4m)));
			Add(items, new ItemInfo("Oak Planks", Crafting(4, "Oak Log")));
			
			// Add the stone path
			Add(items, new ItemInfo("Hypercompressed Cobblestone", Hypercompressing("Compressed Cobblestone", 9)));
			Add(items, new ItemInfo("Compressed Cobblestone", Compressing("Cobblestone", 9)));
			Add(items, new ItemInfo("Cobblestone", Mining("Stone", true, MiningTier.WoodOrGold, 0.4m)));
			
			// Add the coal path
			Add(items, new ItemInfo("Hypercompressed Block of Coal", Hypercompressing("Compressed Block of Coal", 9)));
			Add(items, new ItemInfo("Compressed Block of Coal", Compressing("Block of Coal", 9)));
			Add(items, new ItemInfo("Block of Coal", CraftingSingleIngredient(1, "Coal", 9)));
			Add(items, new ItemInfo("Coal", Mining("Coal Ore", true, MiningTier.WoodOrGold, 0.4m)));
			
			// Add the iron path
			Add(items, new ItemInfo("Hypercompressed Block of Iron", Hypercompressing("Compressed Block of Iron", 9)));
			Add(items, new ItemInfo("Compressed Block of Iron", Compressing("Block of Iron", 9)));
			Add(items, new ItemInfo("Block of Iron", CraftingSingleIngredient(1, "Iron Ingot", 9)));
			Add(items, new ItemInfo("Iron Ingot", Smelting("Raw Iron")));
			Add(items, new ItemInfo("Raw Iron", Mining("Iron Ore", true, MiningTier.StoneOrCopper, 0.4m)));
			
			// Add the gold path
			Add(items, new ItemInfo("Hypercompressed Block of Gold", Hypercompressing("Compressed Block of Gold", 9)));
			Add(items, new ItemInfo("Compressed Block of Gold", Compressing("Block of Gold", 9)));
			Add(items, new ItemInfo("Block of Gold", CraftingSingleIngredient(1, "Gold Ingot", 9)));
			Add(items, new ItemInfo("Gold Ingot", Smelting("Raw Gold")));
			Add(items, new ItemInfo("Raw Gold", Mining("Gold Ore", true, MiningTier.IronOrEmerald, 0.4m)));
			
			// Add the emerald path
			Add(items, new ItemInfo("Hypercompressed Block of Emerald", Hypercompressing("Compressed Block of Emerald", 9)));
			Add(items, new ItemInfo("Compressed Block of Emerald", Compressing("Block of Emerald", 9)));
			Add(items, new ItemInfo("Block of Emerald", CraftingSingleIngredient(1, "Emerald", 9)));
			Add(items, new ItemInfo("Emerald", Mining("Emerald Ore", true, MiningTier.IronOrEmerald, 0.4m)));
			
			// Add the diamond path
			Add(items, new ItemInfo("Hypercompressed Block of Diamond", Hypercompressing("Compressed Block of Diamond", 9)));
			Add(items, new ItemInfo("Compressed Block of Diamond", Compressing("Block of Diamond", 9)));
			Add(items, new ItemInfo("Block of Diamond", CraftingSingleIngredient(1, "Diamond", 9)));
			Add(items, new ItemInfo("Diamond", Mining("Diamond Ore", true, MiningTier.Diamond, 0.4m)));
			
			// Add the Netherite path
			Add(items, new ItemInfo("Hypercompressed Block of Netherite", Hypercompressing("Compressed Block of Netherite", 9)));
			Add(items, new ItemInfo("Compressed Block of Netherite", Compressing("Block of Netherite", 9)));
			Add(items, new ItemInfo("Block of Netherite", CraftingSingleIngredient(1, "Netherite Ingot", 9)));
			Add(items, new ItemInfo("Netherite Ingot", Crafting(1,
				"Gold Ingot",
				"Gold Ingot",
				"Gold Ingot",
				"Gold Ingot",
				"Netherite Scrap",
				"Netherite Scrap",
				"Netherite Scrap",
				"Netherite Scrap")));
			Add(items, new ItemInfo("Netherite Scrap", Smelting("Ancient Debris")));
			Add(items, new ItemInfo("Ancient Debris", Mining("Ancient Debris", true, MiningTier.Diamond, 1.25m)));
			
			// Add the Whitestone path
			Add(items, new ItemInfo("Hypercompressed Block of Whitestone", Hypercompressing("Compressed Block of Whitestone", 9)));
			Add(items, new ItemInfo("Compressed Block of Whitestone", Compressing("Block of Whitestone", 9)));
			Add(items, new ItemInfo("Block of Whitestone", CraftingSingleIngredient(1, "Whitestone", 9)));
			
			Add(items, new ItemInfo("Purifying Lens", Crafting(1,
				"Glass",
				"Glass",
				"Glass",
				"Glowstone Dust",
				"Glowstone Dust",
				"Glowstone Dust",
				"Glass",
				"Glass",
				"Glass")));
			Add(items, new ItemInfo("Altar of Ages", Crafting(1,
				"End Stone",
				"End Crystal",
				"End Stone",
				"Glowstone",
				"Nether Star",
				"Glowstone",
				"Obsidian",
				"Diamond",
				"Obsidian")));
			
			Add(items, new ItemInfo("Totem of Undying", Killing("Evoker", 1m)));
			// Ancient Debris is already added
			Add(items, new ItemInfo("Dragon's Breath", Other("Glass Bottle")));
			
			// Add the Leather path
			Add(items, new ItemInfo("Hypercompressed Block of Leather", Hypercompressing("Compressed Block of Leather", 9)));
			Add(items, new ItemInfo("Compressed Block of Leather", Compressing("Block of Leather", 9)));
			Add(items, new ItemInfo("Block of Leather", CraftingSingleIngredient(1, "Leather", 9)));
			Add(items, new ItemInfo("Leather", Killing("Cow", 0.5m)));
			
			// Add the Chainmail path
			Add(items, new ItemInfo("Hypercompressed Block of Iron Chains", Hypercompressing("Compressed Block of Iron Chains", 9)));
			Add(items, new ItemInfo("Compressed Block of Iron Chains", Compressing("Block of Iron Chains", 9)));
			Add(items, new ItemInfo("Block of Iron Chains", CraftingSingleIngredient(1, "Iron Chains", 9)));
			Add(items, new ItemInfo("Iron Chains", CraftingSingleIngredient(1, "Iron Nugget", 4)));
			Add(items, new ItemInfo("Iron Nugget", CraftingSingleIngredient(9, "Iron Ingot", 1)));
			
			Add(items, new ItemInfo("Glass Bottle", CraftingSingleIngredient(1, "Glass", 3)));
			Add(items, new ItemInfo("Glass", Smelting("Sand")));
			Add(items, new ItemInfo("Glowstone Dust", Mining("Glowstone", false, MiningTier.Anything, 0.45m, 2, 4)));
			Add(items, new ItemInfo("End Stone", Mining("End Stone", false, MiningTier.Anything, 0.05m)));
			Add(items, new ItemInfo("End Crystal", Crafting(1,
				"Glass",
				"Glass",
				"Glass",
				"Glass",
				"Eye of Ender",
				"Glass",
				"Glass",
				"Ghast Tear",
				"Glass")));
			Add(items, new ItemInfo("Nether Star", Killing("Wither", 1m)));
			Add(items, new ItemInfo("Obsidian", Mining("Obsidian", false, MiningTier.Diamond, 1.85m)));
			
			Add(items, new ItemInfo("Sand", Mining("Sand", false, MiningTier.Anything, 0.05m)));
			Add(items, new ItemInfo("Glowstone", CraftingSingleIngredient(1, "Glowstone Dust", 4)));
			Add(items, new ItemInfo("Eye of Ender", Crafting(1,
				"Blaze Powder",
				"Ender Pearl")));
			Add(items, new ItemInfo("Ghast Tear", Killing("Ghast", 0.5m)));
			
			Add(items, new ItemInfo("Blaze Powder", CraftingSingleIngredient(2, "Blaze Rod", 1)));
			Add(items, new ItemInfo("Ender Pearl", Killing("Enderman", 0.5m)));
			
			Add(items, new ItemInfo("Blaze Rod", Killing("Blaze", 0.5m)));
			
			return items;
		}
		
		private static void Add(IDictionary<string, ItemInfo> items, ItemInfo item)
		{
			if (!items.TryAdd(item.Name, item))
			{
				throw new InvalidOperationException($"An item with the name '{item.Name}' already exists.");
			}
		}

		private static MiningRecipe Mining(string blockReference, bool requiresPickaxe, MiningTier requiredMiningTier, decimal miningTime, int minimumDrops = 1, int maximumDrops = 1) =>
					new MiningRecipe(blockReference, requiresPickaxe, requiredMiningTier, miningTime, minimumDrops, maximumDrops);
		
		private static CraftingRecipe Crafting(int resultCount, params string[] ingredientReferences) =>
			new CraftingRecipe(resultCount, ingredientReferences);

		private static CraftingRecipe CraftingSingleIngredient(int resultCount, string ingredientReference, int ingredientCount) =>
			new CraftingRecipe(resultCount, Enumerable.Repeat(ingredientReference, ingredientCount).ToArray());

		private static SmeltingRecipe Smelting(string ingredientReference) => new SmeltingRecipe(ingredientReference);

		private static SmithingRecipe Smithing(string baseItemReference, string upgradeItemReference,
			string templateReference) =>
			new SmithingRecipe(baseItemReference, upgradeItemReference, templateReference);

		private static CompressionRecipe Compressing(string ingredientReference, int ingredientCount) =>
			new CompressionRecipe(ingredientReference, ingredientCount);
		
		private static HypercompressionRecipe Hypercompressing(string ingredientReference, int ingredientCount) =>
			new HypercompressionRecipe(ingredientReference, ingredientCount);

		private static PurifyingRecipe Purifying(string overworldItemReference, string netherItemReference,
			string endItemReference, string itemToPurifyReference) =>
			new PurifyingRecipe(overworldItemReference, netherItemReference, endItemReference, itemToPurifyReference);
		
		private static KillRecipe Killing(string mobReference, decimal dropRate) => new KillRecipe(mobReference, dropRate);
		
		private static OtherRecipe Other(string requirementReference) => new OtherRecipe(requirementReference);
	}
}
