using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.InfinitePowerBeacon.Recipes;

namespace Celarix.JustForFun.InfinitePowerBeacon.Simulation
{
	internal sealed class Simulation
	{
		private IDictionary<string, ItemInfo> items;
		private long smeltingCost;
		private long compressionCost;
		private long hypercompressionCost;
		private decimal miningTime;
		private long killingCost;

		public SimulationResult RunSimulation()
		{
			items = ItemListGenerator.GenerateItemList();
			var steps = new List<SimulationStep>
			{
				new SimulationStep(
					[
						new SimulationItem("Hypercompressed Block of Infinite Power", 81 + 49 + 25 + 9)
					],
					[],
					[],
					0L,
					0L,
					0L,
					0m,
					0L)
			};

			while (true)
			{
				steps.Add(RunStep(steps.Last()));

				var lastStepNumber = steps.Count;
				Console.WriteLine($"Step {lastStepNumber}:");

				foreach (var simulationItem in steps.Last().AllItems)
				{
					Console.WriteLine($"  {simulationItem}");
				}

				if (steps.Last().AllItemsTerminal)
				{
					// Handle the circular Ultraheavy Netherite Upgrade Template.
					// First, find the number of Ultraheavy Netherite Upgrade Templates we need.
					var lastStep = steps.Last();
					var ultraheavyNetheriteUpgradeTemplateCount = lastStep.MinedItems.First(i => i.ItemReference == "Ultraheavy Netherite Upgrade Template").Quantity;
					// We only need one Ultraheavy Netherite Upgrade Template, and we need 8 diamonds for every other Ultraheavy Netherite Upgrade Template.
					// So, we need 8 * (ultraheavyNetheriteUpgradeTemplateCount - 1) diamonds.
					var diamondCount = 8 * (ultraheavyNetheriteUpgradeTemplateCount - 1);
					var newMinedItems = lastStep.MinedItems.Where(i =>
							i.ItemReference is not ("Ultraheavy Netherite Upgrade Template"
								or "Netherite Upgrade Template"))
						.ToList();
					newMinedItems.Add(new SimulationItem("Ultraheavy Netherite Upgrade Template", 1));
					// Create a new step with the diamond requirement and the single templates.
					steps.Add(new SimulationStep([new SimulationItem("Diamond", diamondCount)],
						newMinedItems,
						lastStep.KilledItems,
						0L,
						0L,
						0L,
						0m,
						0L));
					
					// Now make a step to mine the diamonds.
					steps.Add(new SimulationStep([new SimulationItem("Diamond Ore", diamondCount)],
						steps.Last().MinedItems,
						steps.Last().KilledItems,
						0L,
						0L,
						0L,
						0m,
						0L));
					
					// Finally, make a step with the Diamond Ore in the minedItems list.
					steps.Add(new SimulationStep([],
						[
							.. steps.Last().MinedItems,
							new SimulationItem("Diamond Ore", diamondCount),
						],
						steps.Last().KilledItems,
						0L,
						0L,
						0L,
						0.4m * diamondCount,
						0L));
					
					// i am good programmer
					return new SimulationResult(steps);
				}
			}
		}

		private SimulationStep RunStep(SimulationStep previousStep)
		{
			var newStepItems = new List<SimulationItem>();
			var minedItems = new List<SimulationItem>(previousStep.MinedItems);
			var killedItems = new List<SimulationItem>(previousStep.KilledItems);
			
			long stepHypercompressionCost = 0;
			long stepCompressionCost = 0;
			long stepSmeltingCost = 0;
			long stepKillingCost = 0;
			decimal stepMiningTime = 0;
			
			foreach (var previousStepItem in previousStep.NonTerminalItems)
			{
				if (!items.TryGetValue(previousStepItem.ItemReference, out var itemInfo))
				{
					// If we did this right, this should never happen
					// ugh... except for the upgrade templates
					// which are special cases
					if (previousStepItem.ItemReference is "Ultraheavy Netherite Upgrade Template"
					    or "Netherite Upgrade Template")
					{
						// just pretend we can mine it
						minedItems.Add(new SimulationItem(previousStepItem.ItemReference, previousStepItem.Quantity));
						continue;
					}
					
					throw new InvalidOperationException($"Unknown item reference: {previousStepItem.ItemReference}");
				}
				
				switch (itemInfo.MadeBy)
				{
					case CompressionRecipe compressionRecipe:
						newStepItems.Add(new SimulationItem(compressionRecipe.IngredientReference, previousStepItem.Quantity * compressionRecipe.IngredientCount));
						stepCompressionCost += previousStepItem.Quantity;

						break;
					case CraftingRecipe craftingRecipe:
					{
						var requiredCraftings = (int)Math.Ceiling((decimal)previousStepItem.Quantity / craftingRecipe.ResultCount);
						newStepItems.AddRange(craftingRecipe.IngredientReferences.Select(i => new SimulationItem(i, requiredCraftings)));

						break;
					}
					case HypercompressionRecipe hypercompressionRecipe:
						newStepItems.Add(new SimulationItem(hypercompressionRecipe.IngredientReference, previousStepItem.Quantity * hypercompressionRecipe.IngredientCount));
						stepHypercompressionCost += previousStepItem.Quantity;

						break;
					case KillRecipe killRecipe:
						// Terminal.
						killedItems.Add(new SimulationItem(killRecipe.MobReference, previousStepItem.Quantity));
						stepKillingCost += previousStepItem.Quantity;

						break;
					case MiningRecipe miningRecipe:
						// Terminal.
						minedItems.Add(new SimulationItem(miningRecipe.BlockReference, previousStepItem.Quantity));
						stepMiningTime += miningRecipe.MiningTime * previousStepItem.Quantity;

						break;
					case OtherRecipe otherRecipe:
						newStepItems.Add(new SimulationItem(otherRecipe.RequirementReference, previousStepItem.Quantity));

						break;
					case PurifyingRecipe purifyingRecipe:
						newStepItems.Add(new SimulationItem(purifyingRecipe.OverworldItemReference, previousStepItem.Quantity));
						newStepItems.Add(new SimulationItem(purifyingRecipe.NetherItemReference, previousStepItem.Quantity));
						newStepItems.Add(new SimulationItem(purifyingRecipe.EndItemReference, previousStepItem.Quantity));
						newStepItems.Add(new SimulationItem(purifyingRecipe.ItemToPurifyReference, previousStepItem.Quantity));

						break;
					case SmeltingRecipe smeltingRecipe:
						newStepItems.Add(new SimulationItem(smeltingRecipe.IngredientReference, previousStepItem.Quantity));
						stepSmeltingCost += previousStepItem.Quantity;

						break;
					case SmithingRecipe smithingRecipe:
						newStepItems.Add(new SimulationItem(smithingRecipe.BaseItemReference, previousStepItem.Quantity));
						newStepItems.Add(new SimulationItem(smithingRecipe.AdditiveItemReference, previousStepItem.Quantity));
						newStepItems.Add(new SimulationItem(smithingRecipe.TemplateReference, previousStepItem.Quantity));

						break;
					default:
						throw new InvalidOperationException($"Unknown recipe type: {itemInfo.MadeBy.GetType().Name}");
				}
			}
			
			newStepItems = MergeDuplicateItems(newStepItems);
			minedItems = MergeDuplicateItems(minedItems);
			killedItems = MergeDuplicateItems(killedItems);

			return new SimulationStep(newStepItems,
				minedItems,
				killedItems,
				stepHypercompressionCost,
				stepCompressionCost,
				stepKillingCost,
				stepMiningTime,
				stepSmeltingCost);
		}

		private static List<SimulationItem> MergeDuplicateItems(List<SimulationItem> itemsWithDuplicates)
		{
			var mergedItems = new Dictionary<string, long>();

			foreach (var item in itemsWithDuplicates)
			{
				if (mergedItems.ContainsKey(item.ItemReference)) { mergedItems[item.ItemReference] += item.Quantity; }
				else { mergedItems.Add(item.ItemReference, item.Quantity); }
			}

			return mergedItems.Select(i => new SimulationItem(i.Key, i.Value)).ToList();
		}
	}
}
