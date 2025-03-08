class UnitSystem {
    constructor(name) {
        this.name = name;
        this.conversions = {};
        this.units = [];
    }

    addConversion(toUnitSystemName, conversionFactor) {
        this.conversions[toUnitSystemName] = conversionFactor;
    }

    addUnit(unit) {
        this.units.push(unit);
    }

    makeQuantity(value) {
        return new Quantity(this, value);
    }
}

class Unit {
    constructor(name, plural, symbol, multipleOfLast) {
        this.name = name;
        this.plural = plural;
        this.symbol = symbol;
        this.multipleOfLast = multipleOfLast;
    }
}

class Quantity {
    constructor(unitSystem, value) {
        // The value is implicitly in the first unit in unitSystem.units.
        this.unitSystem = unitSystem;
        this.value = value;
    }

    to(unitSystem) {
        if (this.unitSystem === unitSystem) {
            return this;
        }

        const conversionFactor = this.unitSystem.conversions[unitSystem.name];
        if (conversionFactor === undefined) {
            throw new Error(`No conversion factor from ${this.unitSystem.name} to ${unitSystem.name}.`);
        }

        return new Quantity(unitSystem, this.value * conversionFactor);
    }

    multiply(factor) {
        return new Quantity(this.unitSystem, this.value * factor);
    }

    toString() {
        const inSmallestFittingUnit = this.findSmallestFittingUnit();
        const unit = this.unitSystem.units[inSmallestFittingUnit.unitIndex];
        const valueInUnit = inSmallestFittingUnit.valueInUnit;

        if (valueInUnit === 0 || valueInUnit === 1) {
            // Easiest case.
            return `${to3DecimalPlaces(this.value)} ${unit.name}`;
        }

        if (valueInUnit < 1) {
            // Use the smaller prefixes.
            if (valueInUnit <= 1e30) {
                // That is, unless we're smaller than all prefixes. In that case, we'll just use scientific
                // notation.
                let mantissa = valueInUnit;
                let power = 0;
                while (mantissa < 1) {
                    mantissa *= 10;
                    power -= 1;
                }
                return `${to3DecimalPlaces(mantissa)} × 10^${power} ${unit.name}`;
            }

            const prefixAndMultiplier = getPrefixAndMultiplier(valueInUnit);
            const multipliedValue = valueInUnit * prefixAndMultiplier.multiplier;
            return `${to3DecimalPlaces(multipliedValue)} ${prefixAndMultiplier.prefix}${unit.plural}`;
        }

        if (valueInUnit > 1) {
            // Use the larger prefixes.
            if (valueInUnit >= 1e30) {
                // That is, unless we're larger than all prefixes. In that case, we'll just use scientific
                // notation.
                let mantissa = valueInUnit;
                let power = 0;
                while (mantissa >= 10) {
                    mantissa /= 10;
                    power += 1;
                }
                return `${to3DecimalPlaces(mantissa)} × 10^${power} ${unit.name}`;
            }

            const prefixAndMultiplier = getPrefixAndMultiplier(valueInUnit);
            const multipliedValue = valueInUnit / prefixAndMultiplier.multiplier;
            return `${to3DecimalPlaces(multipliedValue)} ${prefixAndMultiplier.prefix}${unit.plural}`;
        }
    }

    // Let's say we have a quantity of 35 inches. We know that the first unit in the US Customary length
    // system is the inch, which has index 0. We need to check if there's a larger unit that has a multiple
    // of less than 35 inches. Of course, unit index 1 is the foot, which has a multiple of 12. So we can
    // convert 35 inches to 2.917 feet. In this case, feet become the "smallest fitting unit".
    findSmallestFittingUnit() {
        if (this.unitSystem.units.length === 1) {
            return {
                unitIndex: 0,
                valueInUnit: this.value
            };
        }

        const units = this.unitSystem.units;
        let valueInCurrentUnit = this.value;
        let unitIndex = 0;
        let nextUnitIndex = 1;
        while ((nextUnitIndex < this.unitSystem.units.length)
            && (valueInCurrentUnit >= units[nextUnitIndex].multipleOfLast)) {
            valueInCurrentUnit /= units[nextUnitIndex].multipleOfLast;
            unitIndex = nextUnitIndex;
            nextUnitIndex++;
        }

        return {
            unitIndex: unitIndex,
            valueInUnit: valueInCurrentUnit
        };
    }
}

to3DecimalPlaces = (number) => {
    return number.toFixed(3);
}

getPrefixAndMultiplier = (number) => {
    if (number >= 1e-30 && number <= 1e-27) { return { prefix: 'quecto', multiplier: 1e-30 }; }
    if (number >= 1e-27 && number <= 1e-24) { return { prefix: 'ronto', multiplier: 1e-27 }; }
    if (number >= 1e-24 && number <= 1e-21) { return { prefix: 'yocto', multiplier: 1e-24 }; }
    if (number >= 1e-21 && number <= 1e-18) { return { prefix: 'zepto', multiplier: 1e-21 }; }
    if (number >= 1e-18 && number <= 1e-15) { return { prefix: 'atto', multiplier: 1e-18 }; }
    if (number >= 1e-15 && number <= 1e-12) { return { prefix: 'femto', multiplier: 1e-15 }; }
    if (number >= 1e-12 && number <= 1e-9) { return { prefix: 'pico', multiplier: 1e-12 }; }
    if (number >= 1e-9 && number <= 1e-6) { return { prefix: 'nano', multiplier: 1e-9 }; }
    if (number >= 1e-6 && number <= 1e-3) { return { prefix: 'micro', multiplier: 1e-6 }; }
    if (number >= 1e-3 && number <= 1) { return { prefix: 'milli', multiplier: 1e-3 }; }
    if (number >= 1 && number <= 1e3) { return { prefix: '', multiplier: 1 }; }
    if (number >= 1e3 && number <= 1e6) { return { prefix: 'kilo', multiplier: 1e3 }; }
    if (number >= 1e6 && number <= 1e9) { return { prefix: 'mega', multiplier: 1e6 }; }
    if (number >= 1e9 && number <= 1e12) { return { prefix: 'giga', multiplier: 1e9 }; }
    if (number >= 1e12 && number <= 1e15) { return { prefix: 'tera', multiplier: 1e12 }; }
    if (number >= 1e15 && number <= 1e18) { return { prefix: 'peta', multiplier: 1e15 }; }
    if (number >= 1e18 && number <= 1e21) { return { prefix: 'exa', multiplier: 1e18 }; }
    if (number >= 1e21 && number <= 1e24) { return { prefix: 'zetta', multiplier: 1e21 }; }
    if (number >= 1e24 && number <= 1e27) { return { prefix: 'yotta', multiplier: 1e24 }; }
    if (number >= 1e27 && number <= 1e30) { return { prefix: 'ronna', multiplier: 1e27 }; }
    if (number >= 1e30 && number <= 1e33) { return { prefix: 'quetta', multiplier: 1e30 }; }
}

getNameAndMultiplier = (number) => {
    if (number >= 1 && number <= 1e9) { return { name: '', multiplier: 1 }; }
    if (number >= 1e9 && number <= 1e12) { return { name: 'billion', multiplier: 1e9 }; }
    if (number >= 1e12 && number <= 1e15) { return { name: 'trillion', multiplier: 1e12 }; }
    if (number >= 1e15 && number <= 1e18) { return { name: 'quadrillion', multiplier: 1e15 }; }
    if (number >= 1e18 && number <= 1e21) { return { name: 'quintillion', multiplier: 1e18 }; }
    if (number >= 1e21 && number <= 1e24) { return { name: 'sextillion', multiplier: 1e21 }; }
    if (number >= 1e24 && number <= 1e27) { return { name: 'septillion', multiplier: 1e24 }; }
    if (number >= 1e27 && number <= 1e30) { return { name: 'octillion', multiplier: 1e27 }; }
    if (number >= 1e30 && number <= 1e33) { return { name: 'nonillion', multiplier: 1e30 }; }
    if (number >= 1e33 && number <= 1e36) { return { name: 'decillion', multiplier: 1e33 }; }
    if (number >= 1e36 && number <= 1e39) { return { name: 'undecillion', multiplier: 1e36 }; }
    if (number >= 1e39 && number <= 1e42) { return { name: 'duodecillion', multiplier: 1e39 }; }
    if (number >= 1e42 && number <= 1e45) { return { name: 'tredecillion', multiplier: 1e42 }; }
    if (number >= 1e45 && number <= 1e48) { return { name: 'quattuordecillion', multiplier: 1e45 }; }
    if (number >= 1e48 && number <= 1e51) { return { name: 'quindecillion', multiplier: 1e48 }; }
    if (number >= 1e51 && number <= 1e54) { return { name: 'sexdecillion', multiplier: 1e51 }; }
    if (number >= 1e54 && number <= 1e57) { return { name: 'septendecillion', multiplier: 1e54 }; }
    if (number >= 1e57 && number <= 1e60) { return { name: 'octodecillion', multiplier: 1e57 }; }
    if (number >= 1e60 && number <= 1e63) { return { name: 'novemdecillion', multiplier: 1e60 }; }
    if (number >= 1e63 && number <= 1e66) { return { name: 'vigintillion', multiplier: 1e63 }; }
    if (number >= 1e66 && number <= 1e69) { return { name: 'unvigintillion', multiplier: 1e66 }; }
    if (number >= 1e69 && number <= 1e72) { return { name: 'duovigintillion', multiplier: 1e69 }; }
    if (number >= 1e72 && number <= 1e75) { return { name: 'trevigintillion', multiplier: 1e72 }; }
    if (number >= 1e75 && number <= 1e78) { return { name: 'quattuorvigintillion', multiplier: 1e75 }; }
    if (number >= 1e78 && number <= 1e81) { return { name: 'quinvigintillion', multiplier: 1e78 }; }
    if (number >= 1e81 && number <= 1e84) { return { name: 'sexvigintillion', multiplier: 1e81 }; }
    if (number >= 1e84 && number <= 1e87) { return { name: 'septenvigintillion', multiplier: 1e84 }; }
    if (number >= 1e87 && number <= 1e90) { return { name: 'octovigintillion', multiplier: 1e87 }; }
    if (number >= 1e90 && number <= 1e93) { return { name: 'novemvigintillion', multiplier: 1e90 }; }
    if (number >= 1e93 && number <= 1e96) { return { name: 'trigintillion', multiplier: 1e93 }; }
}

const metricLength = new UnitSystem('Metric length');
const metricMass = new UnitSystem('Metric mass');
const metricVolume = new UnitSystem('Metric volume');
const metricEnergy = new UnitSystem('Metric energy');

const usCustomaryLength = new UnitSystem('US Customary length');
const usCustomaryMass = new UnitSystem('US Customary mass');
const usCustomaryVolume = new UnitSystem('US Customary volume');
const usCustomaryEnergy = new UnitSystem('US Customary energy');

const time = new UnitSystem('Time');

buildUnits = () => {
    // Metric length
    metricLength.addUnit(new Unit('meter', 'meters', 'm', 1));

    // Metric mass
    metricMass.addUnit(new Unit('gram', 'grams', 'g', 1));

    // Metric volume
    metricVolume.addUnit(new Unit('liter', 'liters', 'L', 1));
    metricVolume.addUnit(new Unit('meter cubed', 'meters cubed', 'm³', 1000));

    // Metric energy
    metricEnergy.addUnit(new Unit('joule', 'joules', 'J', 1));

    // US Customary length
    usCustomaryLength.addUnit(new Unit('inch', 'inches', 'in', 1));
    usCustomaryLength.addUnit(new Unit('foot', 'feet', 'ft', 12));
    usCustomaryLength.addUnit(new Unit('yard', 'yards', 'yd', 3));
    usCustomaryLength.addUnit(new Unit('mile', 'miles', 'mi', 1760));

    // US Customary mass
    usCustomaryMass.addUnit(new Unit('ounce', 'ounces', 'oz', 1));
    usCustomaryMass.addUnit(new Unit('pound', 'pounds', 'lb', 16));
    usCustomaryMass.addUnit(new Unit('ton', 'tons', 'ton', 2000));

    // US Customary volume
    usCustomaryVolume.addUnit(new Unit('fluid ounce', 'fluid ounces', 'fl oz', 1));
    usCustomaryVolume.addUnit(new Unit('pint', 'pints', 'pt', 16));
    usCustomaryVolume.addUnit(new Unit('quart', 'quarts', 'qt', 32));
    usCustomaryVolume.addUnit(new Unit('gallon', 'gallons', 'gal', 128));

    // US Customary energy
    usCustomaryEnergy.addUnit(new Unit('calorie (thermochemical)', 'calories (thermochemical)', 'calₜₕ', 1));
    usCustomaryEnergy.addUnit(new Unit('Calorie (dietary)', 'Calories (dietary)', 'Kcal', 1000));

    // Time
    time.addUnit(new Unit('second', 'seconds', 's', 1));
    time.addUnit(new Unit('minute', 'minutes', 'min', 60));
    time.addUnit(new Unit('hour', 'hours', 'hr', 60));
    time.addUnit(new Unit('day', 'days', 'd', 24));
    time.addUnit(new Unit('week', 'weeks', 'wk', 7));
    time.addUnit(new Unit('year', 'years', 'yr', 52.1775));
    time.addUnit(new Unit('decade', 'decades', 'dec', 10));
    time.addUnit(new Unit('century', 'centuries', 'c', 10));
    time.addUnit(new Unit('millennium', 'millennia', 'm', 10));
    time.addUnit(new Unit('eon', 'eons', 'e', 1e9));
    time.addUnit(new Unit('universe-age', 'universe-ages', 'U', 13.7));

    // Add conversion factors between first units
    metricLength.addConversion(usCustomaryLength.name, 39.3701);    // 1 meter = 39.3701 inches
    metricMass.addConversion(usCustomaryMass.name, 0.035274);    // 1 gram = 0.035274 ounces
    metricVolume.addConversion(usCustomaryVolume.name, 33.814);    // 1 liter = 33.814 fluid ounces
    metricEnergy.addConversion(usCustomaryEnergy.name, 0.239006);    // 1 joule = 0.239006 calories (thermochemical)

    usCustomaryLength.addConversion(metricLength.name, 0.0254);    // 1 inch = 0.0254 meters
    usCustomaryMass.addConversion(metricMass.name, 28.3495);    // 1 ounce = 28.3495 grams
    usCustomaryVolume.addConversion(metricVolume.name, 0.0295735);    // 1 fluid ounce = 0.0295735 liters
    usCustomaryEnergy.addConversion(metricEnergy.name, 4.184);    // 1 calorie (thermochemical) = 4.184 joules
};

buildUnits();

cookieMass = () => metricMass.makeQuantity(30);

cookieCalories = () => metricEnergy.makeQuantity(620712);   // 620.712 kJ = 148 Calories
cookieFat = () => metricMass.makeQuantity(7.4);
cookieSaturatedFat = () => metricMass.makeQuantity(2.4);
cookieTransFat = () => metricMass.makeQuantity(0.2);
cookiePolyunsaturatedFat = () => metricMass.makeQuantity(2.5);
cookieMonounsaturatedFat = () => metricMass.makeQuantity(1.9);
cookieSodium = () => metricMass.makeQuantity(0.093);
cookieCarbohydrates = () => metricMass.makeQuantity(20);
cookieFiber = () => metricMass.makeQuantity(0.6);
cookieSugars = () => metricMass.makeQuantity(9.9);
cookieProtein = () => metricMass.makeQuantity(1.5);
cookieCalcium = () => metricMass.makeQuantity(0.0063);
cookieIron = () => metricMass.makeQuantity(0.0017);
cookiePotassium = () => metricMass.makeQuantity(0.0513);
cookieCaffeine = () => metricMass.makeQuantity(0.0033);

cookieCalorieDailyValue = () => time.makeQuantity(6393.6);  // 148 Calories / 2000 Calories/day, Calories cancel out so we're left with time
cookieFatDailyValue = () => time.makeQuantity(7776); // 9% DV
cookieSaturatedFatDailyValue = () => time.makeQuantity(10368); // 12% DV
cookieSodiumDailyValue = () => time.makeQuantity(3456); // 4% DV
cookieCarbohydratesDailyValue = () => time.makeQuantity(6048); // 7% DV
cookieFiberDailyValue = () => time.makeQuantity(1728); // 2% DV
cookieSugarsDailyValue = () => time.makeQuantity(17280); // 20% DV
cookieProteinDailyValue = () => time.makeQuantity(1036.8); // 1.2% DV
cookieCalciumDailyValue = () => time.makeQuantity(544.32); // 0.63% DV
cookieIronDailyValue = () => time.makeQuantity(6912); // 8% DV
cookiePotassiumDailyValue = () => time.makeQuantity(864); // 1% DV

walkingBurnOffTime = () => time.makeQuantity(2400); // 40 minutes of walking @ 3 MPH
runningBurnOffTime = () => time.makeQuantity(840); // 14 minutes of running @ 6 MPH
bikingBurnOffTime = () => time.makeQuantity(1200); // 20 minutes of biking @ 10 MPH
walkingBurnOffDistance = () => metricLength.makeQuantity(3218.69); // 2 miles
runningBurnOffDistance = () => metricLength.makeQuantity(2253.08); // 1.4 miles
bikingBurnOffDistance = () => metricLength.makeQuantity(5364.48); // 3.333 miles

cookieVolume = () => metricVolume.makeQuantity(0.009554);   // total guess, 1.75" diameter, 0.333" thick = 0.583 cubic inches = 0.009554 liters

updateCookieStats = () => {
    // Get the number in the "cookies" element using jQuery
    let cookies = $("#cookies").val();
    
    let cookieNameAndMultiplier = getNameAndMultiplier(cookies);
    const cookieMantissa = cookies / cookieNameAndMultiplier.multiplier;
    $("#cookie-display").text(`${to3DecimalPlaces(cookieMantissa)} ${cookieNameAndMultiplier.name} cookies`);

    const mass = cookieMass().multiply(cookies);
    $("#cookie-mass").text(`Mass: ${mass.toString()} | ${mass.to(usCustomaryMass).toString()}`);

    const calories = cookieCalories().multiply(cookies);
    $("#cookie-calories").text(`Calories: ${calories.toString()} | ${calories.to(usCustomaryEnergy).toString()}`);

    const caloriesDV = cookieCalorieDailyValue().multiply(cookies);
    $("#cookie-calories-dv").text(`Calories: ${caloriesDV.toString()} of energy (per Daily Value requirements)`);

    const fat = cookieFat().multiply(cookies);
    $("#cookie-fat").text(`Fat: ${fat.toString()} | ${fat.to(usCustomaryMass).toString()}`);

    const fatDV = cookieFatDailyValue().multiply(cookies);
    $("#cookie-fat-dv").text(`Fat: ${fatDV.toString()} of fat (per Daily Value requirements)`);

    const saturatedFat = cookieSaturatedFat().multiply(cookies);
    $("#cookie-saturated-fat").text(`Saturated fat: ${saturatedFat.toString()} | ${saturatedFat.to(usCustomaryMass).toString()}`);

    const saturatedFatDV = cookieSaturatedFatDailyValue().multiply(cookies);
    $("#cookie-saturated-fat-dv").text(`Saturated fat: ${saturatedFatDV.toString()} of saturated fat (per Daily Value requirements)`);

    const sodium = cookieSodium().multiply(cookies);
    $("#cookie-sodium").text(`Sodium: ${sodium.toString()} | ${sodium.to(usCustomaryMass).toString()}`);

    const sodiumDV = cookieSodiumDailyValue().multiply(cookies);
    $("#cookie-sodium-dv").text(`Sodium: ${sodiumDV.toString()} of sodium (per Daily Value requirements)`);

    const carbohydrates = cookieCarbohydrates().multiply(cookies);
    $("#cookie-carbs").text(`Carbohydrates: ${carbohydrates.toString()} | ${carbohydrates.to(usCustomaryMass).toString()}`);

    const carbohydratesDV = cookieCarbohydratesDailyValue().multiply(cookies);
    $("#cookie-carbs-dv").text(`Carbohydrates: ${carbohydratesDV.toString()} of carbohydrates (per Daily Value requirements)`);

    const fiber = cookieFiber().multiply(cookies);
    $("#cookie-fiber").text(`Fiber: ${fiber.toString()} | ${fiber.to(usCustomaryMass).toString()}`);

    const fiberDV = cookieFiberDailyValue().multiply(cookies);
    $("#cookie-fiber-dv").text(`Fiber: ${fiberDV.toString()} of fiber (per Daily Value requirements)`);

    const sugars = cookieSugars().multiply(cookies);
    $("#cookie-sugar").text(`Sugars: ${sugars.toString()} | ${sugars.to(usCustomaryMass).toString()}`);

    const sugarsDV = cookieSugarsDailyValue().multiply(cookies);
    $("#cookie-sugar-dv").text(`Sugars: ${sugarsDV.toString()} of sugars (per Daily Value requirements)`);

    const protein = cookieProtein().multiply(cookies);
    $("#cookie-protein").text(`Protein: ${protein.toString()} | ${protein.to(usCustomaryMass).toString()}`);

    const proteinDV = cookieProteinDailyValue().multiply(cookies);
    $("#cookie-protein-dv").text(`Protein: ${proteinDV.toString()} of protein (per Daily Value requirements)`);

    const calcium = cookieCalcium().multiply(cookies);
    $("#cookie-calcium").text(`Calcium: ${calcium.toString()} | ${calcium.to(usCustomaryMass).toString()}`);

    const calciumDV = cookieCalciumDailyValue().multiply(cookies);
    $("#cookie-calcium-dv").text(`Calcium: ${calciumDV.toString()} of calcium (per Daily Value requirements)`);

    const iron = cookieIron().multiply(cookies);
    $("#cookie-iron").text(`Iron: ${iron.toString()} | ${iron.to(usCustomaryMass).toString()}`);

    const ironDV = cookieIronDailyValue().multiply(cookies);
    $("#cookie-iron-dv").text(`Iron: ${ironDV.toString()} of iron (per Daily Value requirements)`);
};

$("#cookies").change(updateCookieStats);