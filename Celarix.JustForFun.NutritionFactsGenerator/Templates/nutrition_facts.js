// Document load
document.addEventListener('DOMContentLoaded', () => {
    showPanel(0);
    setNavButtonStates();
});

// Navigation controls
let currentPanelIndex = 0;
const panels = document.querySelectorAll('.row > div');
const totalPanels = panels.length;

function showPanel(index) {
    panels.forEach((panel, i) => {
        if (i === index) {
            panel.classList.add('panel-shown');
            panel.classList.remove('panel-hidden');
        } else {
            panel.classList.add('panel-hidden');
            panel.classList.remove('panel-shown');
        }
    });
    currentPanelIndex = index;
}

function setNavButtonStates() {
    const canGoBackToStart = currentPanelIndex > 0;
    const canGoBack = currentPanelIndex > 0;
    const canGoForward = currentPanelIndex < totalPanels - 1;

    document.getElementById('back-to-nutrition-facts').classList.toggle('nav-button-active', canGoBackToStart);
    document.getElementById('back-one-page').classList.toggle('nav-button-active', canGoBack);
    document.getElementById('forward-one-page').classList.toggle('nav-button-active', canGoForward);
}

document.getElementById('back-to-nutrition-facts').addEventListener('click', () => {
    if (currentPanelIndex > 0) {
        showPanel(0);
        setNavButtonStates();
    }
});

document.getElementById('back-one-page').addEventListener('click', () => {
    if (currentPanelIndex > 0) {
        showPanel(currentPanelIndex - 1);
        setNavButtonStates();
    }
});

document.getElementById('forward-one-page').addEventListener('click', () => {
    if (currentPanelIndex < totalPanels - 1) {
        showPanel(currentPanelIndex + 1);
        setNavButtonStates();
    }
});

// Formatting helpers
function siPrefixed(value, unit) {
    if (value < 1e-30) {
        // Use scientific notation for extremely small values
        return `${value.toExponential(3)} ${unit}`;
    }
    if (value >= 1e-30 && value < 1e-27) {
        return `${(value * 1e30).toFixed(3)} quecto${unit}`;
    }
    if (value >= 1e-27 && value < 1e-24) {
        return `${(value * 1e27).toFixed(3)} ronto${unit}`;
    }
    if (value >= 1e-24 && value < 1e-21) {
        return `${(value * 1e24).toFixed(3)} yocto${unit}`;
    }
    if (value >= 1e-21 && value < 1e-18) {
        return `${(value * 1e21).toFixed(3)} zepto${unit}`;
    }
    if (value >= 1e-18 && value < 1e-15) {
        return `${(value * 1e18).toFixed(3)} atto${unit}`;
    }
    if (value >= 1e-15 && value < 1e-12) {
        return `${(value * 1e15).toFixed(3)} femto${unit}`;
    }
    if (value >= 1e-12 && value < 1e-9) {
        return `${(value * 1e12).toFixed(3)} pico${unit}`;
    }
    if (value >= 1e-9 && value < 1e-6) {
        return `${(value * 1e9).toFixed(3)} nano${unit}`;
    }
    if (value >= 1e-6 && value < 1e-3) {
        return `${(value * 1e6).toFixed(3)} micro${unit}`;
    }
    if (value >= 1e-3 && value < 1) {
        return `${(value * 1e3).toFixed(3)} milli${unit}`;
    }
    if (value >= 1 && value < 1e3) {
        return `${value.toFixed(3)} ${unit}`;
    }
    if (value >= 1e3 && value < 1e6) {
        return `${(value / 1e3).toFixed(3)} kilo${unit}`;
    }
    if (value >= 1e6 && value < 1e9) {
        return `${(value / 1e6).toFixed(3)} mega${unit}`;
    }
    if (value >= 1e9 && value < 1e12) {
        return `${(value / 1e9).toFixed(3)} giga${unit}`;
    }
    if (value >= 1e12 && value < 1e15) {
        return `${(value / 1e12).toFixed(3)} tera${unit}`;
    }
    if (value >= 1e15 && value < 1e18) {
        return `${(value / 1e15).toFixed(3)} peta${unit}`;
    }
    if (value >= 1e18 && value < 1e21) {
        return `${(value / 1e18).toFixed(3)} exa${unit}`;
    }
    if (value >= 1e21 && value < 1e24) {
        return `${(value / 1e21).toFixed(3)} zetta${unit}`;
    }
    if (value >= 1e24 && value < 1e27) {
        return `${(value / 1e24).toFixed(3)} yotta${unit}`;
    }
    if (value >= 1e27 && value < 1e30) {
        return `${(value / 1e27).toFixed(3)} ronna${unit}`;
    }
    if (value >= 1e30) {
        return `${(value / 1e30).toFixed(3)} quetta${unit}`;
    }

    // Use scientific notation as a fallback for extremely large values
    return `${value.toExponential(3)} ${unit}`;
}

function toScientificNotationIfNeeded(value) {
    if (value < 1e-3 || value >= 1e9) {
        return value.toExponential(3);
    } else {
        return value.toFixed(3);
    }
}

function subscriptDocumentFragment(text, subscript) {
    const fragment = document.createDocumentFragment();
    fragment.append(
        document.createTextNode(text)
    );
    const sub = document.createElement("sub");
    sub.textContent = subscript;
    fragment.append(sub);
    return fragment;
}

function none(value) { return value; }
function count(value) { return `${toScientificNotationIfNeeded(value)}`; }
function toPercent(percent) { return `${toScientificNotationIfNeeded(percent)}%`; }
function toGrams(grams) { return `${toScientificNotationIfNeeded(grams)} grams`; }
function toGramsPrefixed(grams) { return siPrefixed(grams, 'grams'); }
function toPlanckMasses(planckMasses) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(planckMasses)} M`, 'P'); }
function toOunces(ounces) { return `${toScientificNotationIfNeeded(ounces)} ounces`; }
function toPounds(pounds) { return `${toScientificNotationIfNeeded(pounds)} pounds`; }
function toTons(tons) { return `${toScientificNotationIfNeeded(tons)} tons`; }
function toLunarMasses(lunarMasses) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(lunarMasses)} M`, '🌔︎'); }
function toEarthMasses(earthMasses) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(earthMasses)} M`, '🜨'); }
function toSolarMasses(solarMasses) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(solarMasses)} M`, '☉'); }
function toMilkyWayMasses(milkyWayMasses) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(milkyWayMasses)} M`, 'MW'); }
function toUniverseMasses(universeMasses) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(universeMasses)} M`, 'U'); }
function toJoules(joules) { return siPrefixed(joules, 'joules'); }
function toThermochemicalCalories(calories) { return subscriptDocumentFragment(`${siPrefixed(calories, 'cal')}`, 'th'); }
function toDietaryCalories(calories) { return `${toScientificNotationIfNeeded(calories)} Calories`; }
function toElectronVolts(electronVolts) { return siPrefixed(electronVolts, 'electron-volts'); }
function toFootPounds(footPounds) { return `${toScientificNotationIfNeeded(footPounds)} ft�lb`; }
function toBritishThermalUnits(btu) { return `${toScientificNotationIfNeeded(btu)} BTU`; }
function toErgs(ergs) { return siPrefixed(ergs, 'ergs'); }
function toFoes(foes) { return `${toScientificNotationIfNeeded(foes)} foe`; }
function toPlanckEnergies(planckEnergies) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(planckEnergies)} E`, 'P'); }
function toDuration(seconds) {
    if (seconds < 60) {
        return `${seconds.toFixed(3)}s`;
    } else if (seconds < 3600) {
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        return `${minutes}m${remainingSeconds.toFixed(3)}s`;
    } else if (seconds < 86400) {
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const remainingSeconds = seconds % 60;
        return `${hours}h${minutes}m${remainingSeconds.toFixed(3)}s`;
    } else if (seconds < (86400 * 7)) {
        const days = Math.floor(seconds / 86400);
        const hours = Math.floor((seconds % 86400) / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const remainingSeconds = seconds % 60;
        return `${days}d${hours}h${minutes}m${remainingSeconds.toFixed(3)}s`;
    } else {
        const years = Math.floor(seconds / (86400 * 365));
        const days = Math.floor((seconds % (86400 * 365)) / 86400);
        const hours = Math.floor((seconds % 86400) / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const remainingSeconds = seconds % 60;
        return `${years}y${days}d${hours}h${minutes}m${remainingSeconds.toFixed(3)}s`;
    }
}
function heatingEffectOnWater(joules, waterMassGrams) {
    var liquidWaterLeft = waterMassGrams;
    var steamMade = 0;
    var temperature = 20;

    const specificHeatCapacityWater = 4.184; // J/g�C
    const temperatureChange = joules / (waterMassGrams * specificHeatCapacityWater);
    if (temperatureChange > 80) {
        // Some of this energy goes into evaporating the water, compute how much is evaporated
        const energyToBoilWater = waterMassGrams * 2260; // J/g
        const excessEnergy = joules - (waterMassGrams * specificHeatCapacityWater * 80);
        steamMade = excessEnergy / 2260;
        liquidWaterLeft -= steamMade;
        temperature = 100;

        if (liquidWaterLeft == 0) {
            // All water has been evaporated, we can focus the remaining energy into
            // superheating the steam
            const specificHeatCapacitySteam = 2.03; // J/g�C
            const superheatTemperatureChange = excessEnergy / (steamMade * specificHeatCapacitySteam);
            temperature += superheatTemperatureChange;
        }
    }

    return `Achieved ${temperature.toFixed(3)}�C, ${liquidWaterLeft.toFixed(3)} g liquid water left, ${steamMade.toFixed(3)} g steam made.`;
}
function toSeconds(seconds) { return siPrefixed(seconds, 'seconds'); }
function toMinutes(minutes) { return `${toScientificNotationIfNeeded(minutes)} minutes`; }
function toHours(hours) { return `${toScientificNotationIfNeeded(hours)} hours`; }
function toMeridiems(meridiems) { return `${toScientificNotationIfNeeded(meridiems)} meridiems`; }
function toDays(days) { return `${toScientificNotationIfNeeded(days)} days`; }
function toWeeks(weeks) { return `${toScientificNotationIfNeeded(weeks)} weeks`; }
function to28DayMonths(months) { return `${toScientificNotationIfNeeded(months)} months (28 days)`; }
function to29DayMonths(months) { return `${toScientificNotationIfNeeded(months)} months (29 days)`; }
function to30DayMonths(months) { return `${toScientificNotationIfNeeded(months)} months (30 days/standard)`; }
function to31DayMonths(months) { return `${toScientificNotationIfNeeded(months)} months (31 days)`; }
function toAverageMonths(months) { return `${toScientificNotationIfNeeded(months)} months (average)`; }
function toCommonYears(years) { return `${toScientificNotationIfNeeded(years)} common years`; }
function toLeapYears(years) { return `${toScientificNotationIfNeeded(years)} leap years`; }
function toAverageYears(years) { return `${toScientificNotationIfNeeded(years)} years (average)`; }
function to1LeapYearDecades(decades) { return `${toScientificNotationIfNeeded(decades)} decades (1 leap year)`; }
function to2LeapYearDecades(decades) { return `${toScientificNotationIfNeeded(decades)} decades (2 leap years)`; }
function to3LeapYearDecades(decades) { return `${toScientificNotationIfNeeded(decades)} decades (3 leap years)`; }
function toAverageDecades(decades) { return `${toScientificNotationIfNeeded(decades)} decades (average)`; }
function toStandardDecades(decades) { return `${toScientificNotationIfNeeded(decades)} decades (standard)`; }
function to24LeapYearCenturies(centuries) { return `${toScientificNotationIfNeeded(centuries)} centuries (24 leap years)`; }
function to25LeapYearCenturies(centuries) { return `${toScientificNotationIfNeeded(centuries)} centuries (25 leap years)`; }
function toAverageCenturies(centuries) { return `${toScientificNotationIfNeeded(centuries)} centuries (average)`; }
function toStandardCenturies(centuries) { return `${toScientificNotationIfNeeded(centuries)} centuries (standard)`; }
function toLeapYearCycles(cycles) { return `${toScientificNotationIfNeeded(cycles)} leap year cycles`; }
function to242LeapYearMillennia(millennia) { return `${toScientificNotationIfNeeded(millennia)} millennia (242 leap years)`; }
function to243LeapYearMillennia(millennia) { return `${toScientificNotationIfNeeded(millennia)} millennia (243 leap years)`; }
function toAverageMillennia(millennia) { return `${toScientificNotationIfNeeded(millennia)} millennia (average)`; }
function toStandardMillennia(millennia) { return `${toScientificNotationIfNeeded(millennia)} millennia (standard)`; }
function to242499999LeapYearEons(eons) { return `${toScientificNotationIfNeeded(eons)} eons (242,499,999 leap years)`; }
function to242500000LeapYearEons(eons) { return `${toScientificNotationIfNeeded(eons)} eons (242,500,000 leap years)`; }
function toUniverseAges(universeAges) { return `${toScientificNotationIfNeeded(universeAges)} universe ages`; }
function toLightMeters(lightMeters) { return siPrefixed(lightMeters, 'light-meters'); }
function toLightInches(lightInches) { return `${toScientificNotationIfNeeded(lightInches)} light-inches`; }
function toLightFeet(lightFeet) { return `${toScientificNotationIfNeeded(lightFeet)} light-feet`; }
function toLightYards(lightYards) { return `${toScientificNotationIfNeeded(lightYards)} light-yards`; }
function toLightMiles(lightMiles) { return `${toScientificNotationIfNeeded(lightMiles)} light-miles`; }
function toLightAstronomicalUnits(lightAu) { return `${toScientificNotationIfNeeded(lightAu)} light-AU`; }
function toLightParsecs(lightParsecs) { return `${toScientificNotationIfNeeded(lightParsecs)} light-parsecs`; }
function toPlanckLengths(planckLengths) { return subscriptDocumentFragment(`${toScientificNotationIfNeeded(planckLengths)} L`, 'P'); }
function toMeters(meters) { return siPrefixed(meters, 'meters'); }
function toInches(inches) { return `${toScientificNotationIfNeeded(inches)} inches`; }
function toFeet(feet) { return `${toScientificNotationIfNeeded(feet)} feet`; }
function toYards(yards) { return `${toScientificNotationIfNeeded(yards)} yards`; }
function toMiles(miles) { return `${toScientificNotationIfNeeded(miles)} miles`; }
function toAstronomicalUnits(au) { return `${toScientificNotationIfNeeded(au)} AU`; }
function toParsecs(parsecs) { return `${toScientificNotationIfNeeded(parsecs)} parsecs`; }
function toLightSeconds(lightSeconds) { return siPrefixed(lightSeconds, 'light-seconds'); }
function toLightMinutes(lightMinutes) { return `${toScientificNotationIfNeeded(lightMinutes)} light-minutes`; }
function toLightHours(lightHours) { return `${toScientificNotationIfNeeded(lightHours)} light-hours`; }
function toLightDays(lightDays) { return `${toScientificNotationIfNeeded(lightDays)} light-days`; }
function toLightYears(lightYears) { return `${toScientificNotationIfNeeded(lightYears)} light-years`; }

function onUpdate(inputs) {
    {{OnUpdate}}
}

// Auto-generated
{{GeneratedJS}}