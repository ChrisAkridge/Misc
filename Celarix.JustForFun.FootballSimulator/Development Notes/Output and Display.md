# Football Simulator: Output and Display

So we have our tri-layer state machines put together. We have our schedule generator, database layer, and all the code to run a full game, one step at a time. Now we actually need to have a way to ask what's happening.

We use Serilog as our logging library, and we might want to use that to write some events somewhere. We also use `PlayContext.LastPlayDescriptionTemplate` which can be filled in later with actual player names and team names. Let's start from the top and figure out what we want to output, without thinking about where we're putting it quite yet.

## What We Want to Output

### Layer 1: In Game

Let's start at the lowest level and work our way up. We want to display all of this in at least one of the outputs, but maybe not all of it.

- Game week (using playoff names such as "AFC Divisional Round" when needed)
- Kickoff date and time
- Air temperature
- Wind speed and direction
- Away team abbreviation and logo
- Away team current score
- Home team abbreviation and logo
- Home team current score
- Possession indicator
- Down and distance (or kickoff/XP attempt/free kick)
- Current field position
- Away timeouts remaining
- Home timeouts remaining
- Period number
- Game clock (we don't implement a play clock)

This is the basic info you'd see on a scorebug, but since we just have these and not the full video we'd see for a real football game broadcast, we need to show a play history. Mostly this can be `PlayContext.LastPlayDescriptionTemplate`, mostly, but we may want to look at some of the state history to look for key events:

- Turnovers

We can also show some "big moment" indicators that let the user know when they may want to pay closer attention:

- Team in redzone
- Team behind own 5 yard line
- Turnover (again)
- Near end of game and the trailing team is within 1 or 2 scores
- Scoring play of any kind
- The offensive one-point safety, naturally!

Also, when we first enter the game, we can pull recent games out of the database to display their results. Since only one game is ever active at once, these will able to be static throughout the entire game. Rather than picking "n most recent", we can apply a bit of cleverness. All games in the current season can be assigned a relevance score, higher is better, where the factors are:

- How recent the game is, decays FAST as showing Week 1 games in Week 9 doesn't matter much
- If the game has at least one team in either of the divisions of the current game (i.e. if we're playing CIN @ IND, we can favor games from i.e. JAX, BAL, HOU, CLE, etc.)
- If the game was an upset (a team close to the top/bottom of the conference lost/won to a team close to the bottom/top of the conference)
- If the game had a lot of scoring (combined or by one team)
- If the game had a lot of safeties (since that's weird)
- If the game was a tie

We'd only have to show a little bit of info - away and home abbreviations, away and home scores, week number:

```
+---------+
| IND  42 |
| CIN  45 |
|  Week 1 |
|   Final |
+---------+
```

We'd also want to add the next n games that will be played just to know what's going on. We also want to show either the division leaders (for week 9 and earlier) or the current playoff seeds (for week 10 and later). No need for elimination scenarios, that's quite complicated to actually compute.

### Layer 2: Between Games

Here, we'd want to show the next game about to be played: full team names, logos, stadium, average temperature and wind speed for that stadium, kickoff time, week number. We'd also want to show win-loss records for both teams, division standings, and if week 10 or later, current playoff seed.

We'd also want to show the full league standings in division groupings. We'd show a table per each of the 10 divisions:

- Team abbreviation
- Wins, losses, ties (don't show ties if they're 0, prefer 2-3 instead of 2-3-0, looks cleaner)
- Division standing is implied by the order, so don't bother showing it
- Current playoff seed (visible all season but should display at 50% gray if you can until week 10)

In the playoffs, though, we would not like to show this. We will also show those next games as we do in-game, with the catch that if it's the playoffs, we instead want to show ALL playoff games, finished or not, in the same conference as the next game. It'd either show `Final` or show a date the game will be played, depending on if it's been played or not.

### Layer 3: Between Seasons

After the Super Bowl, we'll want to show a few things:

- The Super Bowl result, in bigger formatting - full team names, etc.
- The final division-by-division standings, except with another column, playoff result:
	- ` `: Not in playoffs
	- `W`: Lost in Wild Card Round
	- `D`: Lost in Divisional Round
	- `C`: Lost in Conference Championship
	- `S`: Lost in Super Bowl
	- Either a Lombardi icon if we can show images, or `🏆` if we can't: Won Super Bowl
- The final league standings, all 40 teams in order. We have three columns for win-loss-tie: preseason, regular season, and postseason
- The Game Hall of Fame: Single non-preseason games that:
	- Has the highest total score
	- Has the lowest total score
	- Has the largest margin of victory
	- Has the most team drives
	- (maybe more stuff based on the fuller event log?)

## The Architecture

My main way of thinking is an "event sink" - the state machines do a thing, tell this opaque box what just happened, without caring exactly what the box does with it. We do need something more than "dump a string", though - an event object that has a lot of optional parameters is helpful:

- An event type, which we will expand as we go
- The current play context, game context, and system context - rather than add a boatload of properties like `NewHomeScore`.
- A message string, optional, but comes up a lot during play evaluation. This would probably be rendered as an unordered list of play evaluation events with the result in bold, but the output formatters can figure out how they want to do it.
- Message flush flag, a flag that says basically "this is the end of the play, if you're buffering play evaluation updates, this one is complete and you can display it now", which feels... ugh.
- ...

Honestly... this is kind of making me want an event log database table. We do save stuff per-drive so that we don't have to worry about rebuilding a game from literally any point if the app crashes and restarts (especially in play evaluation... ugh), so we can commit a pile of events we can then filter on later. They can be associated with a game record but don't have to be, and don't have to be super structured, but some stuff we can save:

- First downs
- Third down conversions
- Fourth down conversions
- Long conversions (>= 15 yards to go)
- Scoring plays
- Injuries
- Injury recoveries
- Unlikely safeties (one-point offensive safety, safety from Hail Mary attempt or wind blowing a kicked ball backward)
- Onside kick attempts and recoveries

Or, maybe all of these, but maybe non-player aware records for every play? Even if it's just "IND 12 yard rush to CIN 33, first down.". Querying this will be a pain, but maybe we can have some sort of list of tags per event? `["Play", "RushingPlay", "FirstDownConversion", "ConversionOnThirdDown"]`? Ideally this would be a unified, single thing that would feed everything - Serilog, the output event sink, the database, but we really kind of want it to be unstructured as that lets us add lots of stats, and since the stats would only need to be queried, that helps cut down on complexity. Or we could write events to a JSON file per game or something just to avoid ballooning the SQLite database.

## Output

I envision two output sinks: console-hosted plain text updates, where the console is cleared and rewritten when an update takes place, and rich HTML. Not too fancy, but fancy enough, for rendering in browsers but mainly HTML rendering controls on desktop apps. The HTML can come with images, even a football field so we can represent current field position, line to gain, and where the drive started.

Output displays shouldn't need logic or need to ask questions of the data, they can just display what they get and update it when new data comes in. The downside is in these intermediate representations - using strongly typed display objects for everything would be pretty safe and not too terribly difficult to do, but will mean having to change multiple places to add new items.