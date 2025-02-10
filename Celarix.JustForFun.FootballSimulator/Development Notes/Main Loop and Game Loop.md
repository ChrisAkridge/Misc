# Football Simulator: Main Loop and Game Loop

In Football Simulator, once we have a valid season schedule that includes an 80-game preseason and a 320-game regular season, we can actually start playing games. The overall program runs through one season at a time, playing one game at a time, until all games are complete for a season, including the postseason. Then, a draft occurs to draft more players for the 40 teams, followed by schedule generation for the next season, then playing those games one at a time. The program remembers, via the database state, where it is in a game, so if it crashes or the computer crashes, it can pick up where it left off.

The main program loop is fairly simple:

```

CheckDatabaseAndInitializeIfNotPresent();

if (GameWasInProgress)
{
	GameLoop();
}
else if (GetNextGame() != null)
{
	GameLoop();
}
else
{
	RunDraft();
	GenerateNextSeasonSchedule();
	GetNextGame();
	GameLoop();
}

```

## The Game Loop

### Normal Distributions

The heart of Football Simulator is in its way of generating random outcomes, which isn't with an evenly-distributed RNG, but with a sampling of various normal distributions of various means and standard deviations. Each normal distribution uses the following parameters:

- `MeanAtZero`
- `StandardDeviationAtZero`
- `MeanReductionPerUnitValue`
- `StandardDeviationIncreasePerUnitValue`

...of which two are used to construct the `MathNet.Numerics.Distributions.Normal` instance from the MathNet library. Basically, we want a normal distribution centered on a certain value, with a certain width of standard deviation. For everything that uses a normal distribution, we want to get a value close to the center most of the time, but with a small chance of something truly extreme.

The distribution is sampled with the parameters and a value, which is a `double`. Depending on if the value is negative or not, we set the parameters of the `Normal` instance like so:

- If negative, `mean = MeanAtZero - (MeanReductionPerUnitValue * Math.Abs(value))` and `stddev = parameters.StandardDeviationAtZero`. Here, we push the mean left based on the magnitude of value, scaled by how much we want that magnitude to matter.
- If non-negative, `mean = MeanAtZero` and `stddev = StandardDeviationAtZero + (StandardDeviationIncreasePerUnitValue * value)`. Here, we leave the mean right where it is, but widen the distribution based on the magnitude of value, again scaled by how much we want that magnitude to matter.

This is probably not very well done. Let's see how we'll want to use it in practice, first.

One thing that I do want is to store the parameters for fixed distributions in the database by name, so that we can tweak them outside of code in case we get weird results.

### Team Strengths

Each team has a number of strengths, which are numbers that represent how strong the team is overall at various aspects of the game. The strengths are the sums of the strengths of a team's players in each category. The strengths for each team are:

- Running offense
- Running defense
- Passing offense
- Passing defense
- Offensive line
- Defensive line
- Kicking
- Field goal
- Kick return
- Kick return defense
- Clock management

Strengths are increased very slightly when a successful play of that type is ran and decreased very slightly when an unsuccessful play of that type is ran. Injuries can occur to players, which represent large but temporary losses to the strength which mostly go away when the player returns from injury. Teams don't know their strengths, nor the opponent's strengths, but these are used to calculate play outcomes.

### Step 1: Get Next Game

The season opens with 80 preseason games, which are played in order. 320 regular season games follow, then a postseason schedule is generated based on the standings of each team. All games except the Hall of Fame Game and the Super Bowl are played at the home team's stadium, but the Super Bowl rotates across different stadiums each season and the Hall of Fame Game is always played in Canton, OH. The Super Bowl is also able to go to Canton, OH.

### Step 2: Weather Initialization

Each of the 40 teams has a home stadium (the Giants and the Jets share one), and we start by taking the generated schedule, getting the first non-complete game from it, and examining its weather properties, which are field temperature, precipitation, and wind speed. The database keeps track of the average temperatures (per season month), precipitation days per month, and average wind speed over the season for each stadium. We must then convert it down into observed weather for that particular day.

Any stadium marked as being indoors will have a temperature of 72°F and a wind speed of 0 MPH, regardless of regional climate data.

#### Temperature

1. Get the average temperature for the stadium in the month the game is playing.
2. Convert the kickoff time stored in the database to the local team's timezone (Alaska, Pacific, Mountain, Central, Eastern, or UTC+6 for Vostok Station).
3. We'll take the easy way out to figure what how the time-of-day will affect the local temperature and just say that we add/subtract 2 degrees for each hour away from 3:00pm local time it is. For Antarctica, we flip that, so later times are colder and earlier times are warmer, due to it being in the opposite season of the rest of the league.
4. Sample a normal distribution with a mean of *the time-adjusted temperature* and a standard deviation of *5°F*.

#### Precipitation

1. Get the average number of precipitation days for the stadium in the month the game is playing.
2. Divide that by the number of days in the month to get a per-day chance of precipitation.
3. If a random number between 0 and 1 is less than or equal to the per-day chance, there is precipitation. If the temperature is >= 45°F, the precipitation is rain, if it's between 32°F and 45°F, there's a 50/50 chance of rain or snow, and if it's under 32°F, it's snowing.

#### Wind Speed and Direction

This is sampled from a normal distribution with a mean of *the average wind speed for that stadium* and a standard deviation of *8 MPH*, capped at 0 on the low end. The wind can be blowing in one of four directions, chosen randomly with equal odds:

1. Towards the home endzone
2. Towards the away endzone
3. Laterally from the top of the field (toward the cameras)
4. Laterally from the bottom of the field (away from the cameras)

Vostok Station always gets a fifth type, down onto the field from above.

### Step 3: The Coin Toss

The home team is given the choice to call heads or tails, and it chooses either with 50% odds. The team that wins the coin toss can elect to kick or receive. This is the first major decision.

#### Decision 1: Kickoff to Start the Game, or Receive?

Teams have a kick return strength and a kick defense strength, but the teams don't know that. What they do know is overall season statistics for kick returns, and later, they'll know more about kick returns in this particular game. Let's call the team that won the coin toss Team A, and the other team Team B. Insane teams ALWAYS opt to **kick**, and will ALWAYS **onside kick**.

Team A computes the kick returns of Team B and where each was stopped. But if Team B had any kick return touchdowns, Team A doesn't want that risk and will elect to **receive**. Otherwise, if the average return by B is less than 40 yards, Team A opts to **kick**.

### Step 4: Kickoffs

These rules apply both for kickoffs to open each half, and kickoffs after scoring plays.

#### Decision 2: Onside or normal kickoff?

Insane teams ALWAYS **onside kick** and ultra-conservative teams NEVER onside kick. Otherwise, a team will onside kick if they're down by at least 1 score for each 3 minutes left on the clock (so, to onside kick with 12:00 left in the 2nd, they'd need to be down by 11 scores). A team will also opt to **onside kick** if its opponent has either given up 2 onside kicks during the season, or has given up 1 surprise onside kick (a kick which wasn't done in the down-by-a-lot scenario).

Otherwise, the team will **kickoff normally**.

#### Outcome 1: Normal Kickoff

A normal kickoff, barring penalties, is placed at the kicking team's 35 yard line. The distance the kick travels is based on the team's kicking strength, with 1 point of strength equalling 1 yard of distance. An unadjusted distance is calculated by a normal distribution with a mean of *the kicking strength* and a standard deviation of *2 yards*. If the wind is toward either endzone, the ball gains or loses 0.1 yards of distance for each 1 MPH of wind. If the wind is blowing laterally, its distance is unaffected. If the wind is blowing straight down, the ball's distance is multiplied by *0.99^x* where x = 10 times the wind speed in MPH.

Kicks can travel out of bounds. The higher the kicking strength, the less likely this is, with the odds equal to *0.999^x* where x = the kicking strength. However, if the wind is blowing laterally, that is multiplied by *1.02^x* where x = the wind speed.

The outcome of the kickoff depends on where the ball lands. If it goes out the sides of the field for any reason, that is a kickoff out of bounds penalty and the receiving team gets 1st and 10 at their own 40, with 15 penalty yards given to the kicking team. If the ball goes out the back of the endzone, or if the receiving team calls fair catch in their own endzone (see Decision 3 below), it's 1st and 10 at the receiving team's 25 yard line.

If the ball is caught by a receiving team player where it lands, the receiver may elect to return it (see Decision 3), in which case it becomes a rushing play in terms of outcomes and decisions, but using the kick return and kick return defense strengths of each team. But, if due to strong headwinds, the ball lands closer to the kicking team, the odds of them recovering it go up.

The odds of a receiving team recovery are capped at 95% for any part of the field behind them, and goes down to 1% at the back of the receiving team's endzone via the following method. 

1. Take the section of the field between the kickoff location and the back of the receiving team's endzone (usually 75 yards).
2. Take the ratio between the receiving team's kick return defense strength and the kicking team's kick return strength. For example, if the former is 1000 and the latter is 2000, the ratio is 2:1, or 0.67 of the way between the two points of the field (just past the receiving team's 40). This is where the 47% recovery odds are (that is, 50% of the range between 1% and 95%).
3. The recovery odds decrease linearly in those two zones, from 95% to 47% in the "kicking team zone" (in the example, between the receiving team's 40 and the kicking team's 35) and from 47% to 1% in the "receiving team zone" (from the receiving team's 40 back).
4. The RNG generates a number between 0 and 1, and if that's less than or equal to the odds of recovery, the kicking team recovers the ball.

If the recovery by the kicking team occurs less than 10 yards in front of the kickoff location, the receiving team automatically gets 1st down wherever the ball was recovered. If recovered in the kicking team's endzone by the kicking team, or it goes out the back or sides of the kicking team's endzone, it is automatically a safety against the kicking team. If the kick is recovered by the receiving team in the kicking team's endzone, it is a touchdown.

Thus, the outcomes of a normal kickoff are:

- Touchback, 1st and 10 for the receiving team from their 25
- Kickoff out of bounds, 1st and 10 for the receiving team from their 40 and 15 penalty yards for the kicking team
- Kick recovered by receiving team...
	- On the field, eligible for return or fair catch
	- In their own endzone, eligible for return or fair catch
	- In the kicking team's endzone, automatic touchdown for receiving team
- Kick recovered by kicking team...
	- On the field
		- Less than 10 yards in front of the kickoff location, 1st down for the receiving team at the spot of recovery (ball is immediately dead)
		- More than 10 yards, 1st down for the kicking team at the spot of recovery (ball is immediately dead)
	- In the receiving team's endzone, automatic touchdown for kicking team
	- In the kicking team's endzone, automatic safety against kicking team
- Kick out the back or sides of the receiving team's endzone, automatic safety against kicking team

#### Decision 3: Return the Kick or Fair Catch?