using Celarix.JustForFun.NutritionFactsGenerator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Data
{
    internal static class ServingInfoMassData
    {
        public static Table Elements()
        {
            return new Table("servingSizeInfo", "Serving Size Info")
                .AddColumn("ssiPerServing", new TableColumn("Per Serving", "inputs.servingSize").WithMassRows())
                .AddColumn("ssiPerUnit", new TableColumn("Per Unit", "inputs.servingSize / inputs.unitsPerServing", "inputs.foodIsDiscrete").WithMassRows())
                .AddColumn("ssiPerContainer", new TableColumn("Per Container", "inputs.containerNetWeight").WithMassRows());
        }
    }
}
