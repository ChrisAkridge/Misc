using Celarix.JustForFun.NutritionFactsGenerator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Data
{
    internal static class CalorieInfoData
    {
        public static Table Elements()
        {
            return new Table("calorieInfo", "Calorie Info")
                .AddColumn("ciPerGram", new TableColumn("Per Gram", "(inputs.calories / inputs.servingSize)", "inputs.servingSizeUnit.toLowerCase() == 'grams'")
                    .WithEnergyRows())
                .AddColumn("ciPerMilliliter", new TableColumn("Per Milliliter", "(inputs.calories / inputs.servingSize)", "inputs.servingSizeUnit.toLowerCase() == 'milliliters'")
                    .WithEnergyRows())
                .AddColumn("ciPerServing", new TableColumn("Per Serving", "inputs.calories")
                    .WithEnergyRows())
                .AddColumn("ciPerUnit", new TableColumn("Per Unit", "(inputs.calories / (inputs.servingSize / inputs.unitsPerServing))", "inputs.foodIsDiscrete")
                    .WithEnergyRows())
                .AddColumn("ciPerContainer", new TableColumn("Per Container", "(inputs.calories / inputs.servingSize) * inputs.containerNetWeight")
                    .WithEnergyRows());
        }
    }
}
