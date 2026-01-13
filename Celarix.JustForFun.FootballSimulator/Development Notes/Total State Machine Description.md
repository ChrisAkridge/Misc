# Football Simulator: Total System State Description

_January 10, 2026_

Programming is more about back-and-forth iteration than I first expected getting into the industry. The much more elegant design of the static Decision and Outcome classes and the GameState machine were pretty clean! They did still have some problems - unit testing is very tedious due to massively complex decision flows and runaway exponential conditional growth. I'm happy with the individual play decision and outcome code, but the post-play code for injuries? Building quarter box scores? Player handling? Not my favorite.

Let's envision this as a set of three layered state machines: the system machine, the game machine, and the play machine. The play machine is what's currently implemented in Outcome and Decision and I'm happy with that as is. System represents the entire thing, whereas Game represents a single active game. A lot of these states will largely be "do this and then move unconditionally to the next state" which is unconventional in terms of state machines but I'll take it, especially as it makes testing a bit simpler. Let's map it out.

Each of these state machines take individual steps. They have a `record` object which represents their current state and all the information tied to it. There is a method on each layer that moves a single step in the state machine - anywhere you read _go to_, this means the method ends returning a new `record` value with the next state to run. Then the method must be invoked again to go to the next state.

Each state also reports a user-friendly message describing what it has done. There will be additional methods that can be run any time to query much more rich data about the simulator.

## Changes to Injuries, Trades, and the Draft

Tracking per-player injuries and counting players out, along with drafting new players mid-game, is just too finicky for my taste. Instead, a team's roster remains fixed during a game, and an injury on a player represents some loss of strength on the team's strength categories for that player. This is added to a new table `InjuryRecoveryInfo`:

- `InjuryRecoveryInfoID`
- `TeamID`
- `Strength`: The name of the team strength affected. If multiple strengths are affected on the same play, multiple rows are added.
- `StrengthDelta`: How much strength was lost.
- `RecoverOn`: Representing the idea of a player returning from injury or fully recovering, this date represents when the strength will be added back to the team. When a game is started after this date, that is when the strength is recovered. This is chosen as some random number of days after kickoff of the current game.
- `Recovered`: Indicates if the strength has already been recovered. This flag is written at the end of the game; if a partial game is resumed, it will be with the pre-recovery strengths, so we'd need to add them back.

Trading between teams comes after a season and is much simplified. Each team puts a random number between 0 and 2 players (there will be standard deviation calculations here at some point) up for trade, then teams draw at random from the pool of players up for trade. The concept of the draft is removed.

Players can retire at the end of a season. The normal distribution will be picked such that most players retire with 3-5 years but some will last up to 10-15 years. When a player retires, a new one is drafted into the missing slot.

## End-of-Period Logic

A static method taking a starting period number will inform the game state machine how to handle the end of the previous period. It produces an object with the following properties:

- `PeriodDisplay`: 1Q, 2Q, 3Q, 4Q, then OT, 2OT, 3OT, and so on, based on one-indexed period number.
- `Duration`: 15 minutes for periods 1 through 4, 10 minutes otherwise.
- `CoinToss`: True for any period number % 4 == 1. Via coin toss, a team is selected as the receiving team for the opening kickoff of this period.
- `CoinTossLoserReceives`: True for any period number % 4 == 3.

If `CoinToss || CoinTossLoserReceives`, the current drive ends.

## The System State Machine

- **Start**: The initial state, when the application is first launched. We first check if the database exists - if not, go to **InitializeDatabase**, if so, go to **PrepareForGame**.
- **PrepareForGame**: Checks the game records in the database and goes to a state based on what is found:
		- No season exists or all existing seasons are completed with full playoffs (15 games): Go to **InitializeNextSeason**.
		- A season exists and is complete but has no summary: Go to **WriteSummaryForSeason**.
		- There is one season with unplayed games (preseason, regular season, or playoffs): Go to **LoadGame**.
		- There is one season with all preseason and regular season games played, but no playoff games exist: Go to **InitializeWildCardRound**.
		- There is one season with all preseason and regular season games played along with exactly 8 completed playoff games: Go to **InitializeDivisionalRound**.
		- There is one season with all preseason and regular season games played along with exactly 12 completed playoff games: Go to **InitializeConferenceChampionships**.
		- There is one season with all preseason and regular season games played along with exactly 14 completed playoff games: Go to **InitializeSuperBowl**.
		- There is a game record with some `TeamDriveRecord` rows but is not marked completed: Go to **ResumePartialGame**.
		- There is a game marked complete but has no summary: Go to **WriteSummaryForGame**.
- **InitializeDatabase**: Create the database file, add teams and stadiums, add initial rosters, then go to **InitializeNextSeason**.
- **InitializeNextSeason**: Generate the preseason and regular season schedule for the next season. From an empty database, this is the 2014 season. Otherwise, find the highest season year in the database and add 1. Go to **LoadGame**.
- **InitializeWildCardRound**: Determine division standings (using tiebreakers when necessary) and seed each of the 5 division winners per conference as seeds 1-5. Pick 3 wild card teams from the remaining 30 teams (best 3 in each conference left). Create game records and save them, then go to **LoadGame**. See Appendix A: Playoffs for more details.
- **InitializeDivisionalRound**: Determine winners of the Wild Card games and create game records for the divisional round. Go to **LoadGame**.
- **InitializeConferenceChampionships**: Determine winners of the Divisional Round games and create game records for the conference championships. Go to **LoadGame**.
- **InitializeSuperBowl**: Determine winners of the Conference Championships and create a game record for the Super Bowl. Go to **LoadGame**.
- **ResumePartialGame**: Finds the one game that hasn't been completed and initializes the game state machine with the partial state. The `TeamDriveRecord` rows track their elapsed time, so set the game clock appropriately. Go to **InGame**.
- **LoadGame**: Finds the next game to play (the first unplayed game when the season's games are sorted by start time ascending), checks for any injury recoveries available and applies them, initializes the game state machine, and goes to **InGame**. If there are no games to play, go to **PrepareForGame**.
- **InGame**: The game state machine is active here - the method that moves the system state machine to the next state calls the game state machine, which moves itself internally and then signals an outcome to the system loop. The outcomes are:
	- **GameContinues**: The system state machine has no need to take action. Go to **InGame**.
	- **GameCompleted**: The game has completed. Go to **PostGame**.
- **PostGame**: The game record is updated in the database. We set `Recovered` on all recovered injuries. Go to **WriteSummaryForGame**, unless this game was the season's Super Bowl, in which case go to **WriteSummaryForSeason**.
- **WriteSummaryForGame**: Given the `TeamDriveRecords`, write a summary for the game to the database. If this step fails, we write a basic summary instead. Go to **PrepareForGame**.
- **WriteSummaryForSeason**: Given a condensed list of game results for all 40 teams, write a summary for the season to the database, then mark the season as complete. If this step fails, we write a basic summary instead. Go to **PrepareForGame**.
- **Error**: An unrecoverable error has occurred at any point in any of the state machines. This is a terminal state. Any failure to read or write to the database causes this state immediately.

## The Game State Machine

- **Start**: The initial state of the game state machine, after the system machine has fully initialized. If resuming from a partial game, the most recent `TeamDriveRecord` stores the result of the last drive, which indicates if the next play is a kickoff or a first down for the other team. Otherwise, from a new game, it will be a kickoff. Simulate a basic coin toss to figure out who receives in the first quarter and the third quarter. Go to **EvaluatingPlay**.
- **EvaluatingPlay**: The play state machine is active here - again, the method that moves the system state machine to the next state calls the play state machine, which moves itself internally and then signals an outcome to the game loop. The play state machine, again, is already implemented in full in the Decisions and Outcomes namespaces.
	- **InProgress**: The play evaluation is still underway - for instance, the team may have opted for an onside kick, which moved the play state machine to its onside kick attempt outcome state, but that has to be ran first. Go to **EvaluatingPlay**.
	- **PlayEvaluationComplete**: The play evaluation is complete and the game state has been modified - for instance, a second down results from a 3-yard rush, or a field goal scored 3 points. Go to **AdjustStrengths**.
- **AdjustStrengths**: Each play causes an extremely small increase in certain strengths for both teams, so evaulate this and adjust the strengths. Go to **DeterminePlayersOnPlay**.
- **DeterminePlayersOnPlay**. Rather backward, but we evaluate the play first and then figure out who was active on it. Each play will list the number of players of broad positions that would be involved with it - this is due to some plays needing multiple players of note. Consider a kickoff (kicker) returned by the receiving team (rusher), fumbled and recovered by the kicking team (defender)... The maximum number of players on the play is all of them; that is, 22. Then go to **InjuryCheck**.
- **InjuryCheck**: For all involved players, each has a small chance per play to be injured. If any are, adjust the strength corresponding to the player's position down, add an injury recovery row per strength reduced and go to **AdjustClock**.
- **AdjustClock**: Determine how much time came off the clock during the play and go to **PostPlayCheck**.
- **PostPlayCheck**: We start with a `PostPlayDecision` of `RunNextPlay`. If the last play was a scoring play AND it's overtime AND both teams have had possession during any overtime period AND the current score is not tied, the decision becomes `EndGameWin`. Otherwise, if the period has ended, we call the static method above to figure out what to do for the start of the next period. If it's the fourth quarter or any overtime period AND the score is not tied, the decision becomes `EndGameWin`. If it's overtime AND the score is tied AND it's the regular season, the decision becomes `EndGameTie`. Otherwise, the decision becomes `NextPeriod`. If the play resulted in a change of possession by any means AND the static method did not indicate a kickoff to start the next period, the decision becomes `EndPossession`. Depending on the decision, go to:
	- `RunNextPlay`: **EvaluatingPlay**
	- `EndGameWin` or `EndGameTie`: **EndGame**. (save `TeamDriveRecord`)
	- `NextPeriod`: **StartNextPeriod**. (save `TeamDriveRecord` if the static method indicates a kickoff is required)
	- `EndOfPossession`: **EvaluatingPlay**. (save `TeamDriveRecord`; this decision does go to the same state as `RunNextPlay` but needs to write the drive first)
- **StartNextPeriod**: Some periods require a kickoff from one team to the other (sometimes with a coin toss) - if so, set up a kickoff and save a `TeamDriveRecord`. In all cases, set up the clock for the next period and go to **EvaluatingPlay**.
- **EndGame**: The final drive record is saved and then the game record is updated and marked complete. The updated team strengths are saved to the database. This is the final state of the game state machine, reaching this state indicates an outcome of **GameCompleted** to the system state machine.

## Appendix A: Playoffs

- The winners of the 5 divisions in each conference are in, along with the 3 teams in each conference who remain and have the highest ranking amongst teams in their conference.
- The 5 division winners get the 1 through 5 seeds in order of their ranking amongst other division winners. Wild cards get 6 through 8 in the same manner.
- The playoff games are as follows:
	- AFC Wild Card 1: #8 @ #1
	- AFC Wild Card 2: #7 @ #2
	- AFC Wild Card 3: #6 @ #3
	- AFC Wild Card 4: #5 @ #4
	- NFC Wild Card 1: #8 @ #1
	- NFC Wild Card 2: #7 @ #2
	- NFC Wild Card 3: #6 @ #3
	- NFC Wild Card 4: #5 @ #4
	- AFC Divisional Round 1: (lowest remaining seed) @ (highest remaining seed)
	- AFC Divisional Round 2: (second lowest seed) @ (second highest seed)
	- NFC Divisional Round 1: (lowest remaining seed) @ (highest remaining seed)
	- NFC Divisional Round 2: (second lowest seed) @ (second highest seed)
	- AFC Championship: (lowest remaining seed) @ (highest remaining seed)
	- NFC Championship: (lowest remaining seed) @ (highest remaining seed)
	- Super Bowl: (AFC winner) vs. (NFC Winner)
- The designated home team in the Super Bowl is the AFC in even-numbered years and the NFC in odd-numbered years. The actual stadium is any of the 40 chosen at random.
- Wild Card games are played on the Saturday and Sunday after Week 17, 4 games at 4:25pm and 4 games at 8:20pm, 4 per night.
- Divisional Round games are played on the next Saturday and Sunday, 2 games at 4:25pm and 2 games at 8:20pm, 2 per night.
- Conference Championship games are played on that next Sunday, the AFC game at 4:25pm and the NFC at 8:20pm.
- The Super Bowl is played two weeks later on that Sunday, at 6:05pm.