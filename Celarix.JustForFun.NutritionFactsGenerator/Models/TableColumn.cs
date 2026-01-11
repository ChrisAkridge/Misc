using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Models
{
    internal sealed class TableColumn(string label,
        string expressionValueName,
        string? displayIfExpression = null)
    {
        private readonly List<TableRow> tableRows = new();

        public string Label { get; } = label;
        public string ExpressionValueName { get; } = expressionValueName;
        public string? DisplayIfExpression { get; } = displayIfExpression;
        public IReadOnlyList<TableRow> TableRows => tableRows;

        public TableColumn AddRow(TableRow row)
        {
            tableRows.Add(row);
            return this;
        }

        public TableColumn WithMassRows()
        {
            tableRows.Add(new TableRow("Grams", "val", "toGramsPrefixed"));
            tableRows.Add(new TableRow("Planck masses", "val / 2.17645e-5", "toPlanckMasses"));
            tableRows.Add(new TableRow("Ounces (US food labellling)", "val / 28", "toOunces"));
            tableRows.Add(new TableRow("Ounces", "val / 28.349523125", "toOunces"));
            tableRows.Add(new TableRow("Pounds", "val / 453.59237", "toPounds"));
            tableRows.Add(new TableRow("Tons", "val / 907184.74", "toTons"));
            tableRows.Add(new TableRow("Average US men", "val / 90264.88163", "count"));
            tableRows.Add(new TableRow("Average US women", "val / 77927.16917", "count"));
            tableRows.Add(new TableRow("5 pounds", "val / 2267.96185", "count"));
            tableRows.Add(new TableRow("20 pounds", "val / 9071.8474", "count"));
            tableRows.Add(new TableRow("50 pounds", "val / 22679.6185", "count"));
            tableRows.Add(new TableRow("100 pounds", "val / 45359.237", "count"));
            tableRows.Add(new TableRow("Electron-volts of relativistic energy", "val / 1.8e-33", "toElectronVolts"));
            tableRows.Add(new TableRow("Electrons", "val / 9.11e-28", "count"));
            tableRows.Add(new TableRow("Protons", "val / 1.673e-24", "count"));
            tableRows.Add(new TableRow("Neutron", "val / 1.675e-24", "count"));
            tableRows.Add(new TableRow("Oganesson<sub>294</sub> atoms", "val / 4.9e-22", "count"));
            tableRows.Add(new TableRow("Joules of relativistic energy", "val / 1.1e-14", "toJoules"));
            tableRows.Add(new TableRow("Lunar masses", "val / 7.346e25", "toLunarMasses"));
            tableRows.Add(new TableRow("Earth masses", "val / 6e27", "toEarthMasses"));
            tableRows.Add(new TableRow("Solar masses", "val / 2e33", "toSolarMasses"));
            tableRows.Add(new TableRow("Milky Way masses", "val / 2.98e45", "toMilkyWayMasses"));
            tableRows.Add(new TableRow("Observable universe masses", "val / 1.5e56", "toUniverseMasses"));
            return this;
        }

        public TableColumn WithEnergyRows()
        {
            tableRows.Add(new TableRow("Joules", "val * 4184", "toJoules"));
            tableRows.Add(new TableRow("Thermochemical calories", "val * 1000", "toThermochemicalCalories"));
            tableRows.Add(new TableRow("Calories", "val", "toDietaryCalories"));
            tableRows.Add(new TableRow("Electron-Volts", "val * 2.611e22", "toElectronVolts"));
            tableRows.Add(new TableRow("Foot-pounds", "val * 3086", "toElectronVolts"));
            tableRows.Add(new TableRow("British Thermal Units", "val * 3.966", "toBritishThermalUnits"));
            tableRows.Add(new TableRow("Ergs", "val * 4.184e10", "toErgs"));
            tableRows.Add(new TableRow("Foes", "val * 4.184e-41", "toFoes"));
            tableRows.Add(new TableRow("Planck Energies", "val * 2.139e-6", "toPlanckEnergies"));
            tableRows.Add(new TableRow("% Daily Value of Calories", "(val / inputs.dailyEnergyIntake) * 100", "toPercent"));
            tableRows.Add(new TableRow("Duration of Calories", "toDuration((val / inputs.dailyEnergyIntake) * 86400)", "none"));
            tableRows.Add(new TableRow("Duration that 1 watt can be sustained", "toDuration(val * 4184)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 1.5 volts can be sustained", "toDuration((val * 4184) / 1.5)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 5 volts can be sustained", "toDuration((val * 4184) / 5)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 12 volts can be sustained", "toDuration((val * 4184) / 12)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 120 volts can be sustained", "toDuration((val * 4184) / 120)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 240 volts can be sustained", "toDuration((val * 4184) / 240)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 480 volts can be sustained", "toDuration((val * 4184) / 480)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 100 kilovolts can be sustained", "toDuration((val * 4184) / 1e5)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 800 kilovolts can be sustained", "toDuration((val * 4184) / 8e5)", "none"));
            tableRows.Add(new TableRow("Duration that 1 ampere across 800 kilovolts can be sustained", "toDuration((val * 4184) / 8e5)", "none"));
            tableRows.Add(new TableRow("Mass of TNT", "val", "toGramsPrefixed"));
            tableRows.Add(new TableRow("Heating effect on 1 gram of water", "heatingEffectOnWater(val * 4184, 1)", "none"));
            tableRows.Add(new TableRow("Heating effect on 1 kilogram of water", "heatingEffectOnWater(val * 4184, 1e3)", "none"));
            tableRows.Add(new TableRow("Heating effect on 1 megagram of water", "heatingEffectOnWater(val * 4184, 1e6)", "none"));
            tableRows.Add(new TableRow("Relativistic mass", "val * 4.655e-14", "toGramsPrefixed"));
            tableRows.Add(new TableRow("Barrel of oil equivalent", "val / 6.837e-7", "count"));
            tableRows.Add(new TableRow("LED light bulb operation duration (10 W)", "toDuration((val * 4184) / 10)", "none"));
            tableRows.Add(new TableRow("Incandescant light bulb operation duration (100 W)", "toDuration((val * 4184) / 100)", "none"));
            tableRows.Add(new TableRow("Typical PC PSU operation duration (650 W)", "toDuration((val * 4184) / 650)", "none"));
            tableRows.Add(new TableRow("Household operation duration (24 kW)", "toDuration((val * 4184) / 24e3)", "none"));
            tableRows.Add(new TableRow("Clear channel AM station operation duration (50 kW)", "toDuration((val * 4184) / 50e3)", "none"));
            tableRows.Add(new TableRow("Duga OTH radar operation duration (10 MW)", "toDuration((val * 4184) / 10e6)", "none"));
            tableRows.Add(new TableRow("Global power output duration (19.6 TW)", "toDuration((val * 4184) / 19.6e12)", "none"));
            tableRows.Add(new TableRow("Sun power output duration (38.28 YW)", "toDuration((val * 4184) / 38.28e24)", "none"));
            tableRows.Add(new TableRow("Sun power output duration (38.28 YW)", "toDuration((val * 4184) / 38.28e24)", "none"));
            tableRows.Add(new TableRow("mass-energies of the observable universe", "val * 3e-67", "count"));
            return this;
        }
    }
}
