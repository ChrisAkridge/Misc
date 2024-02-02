# Football Simulator: Teams and Scheduling

Football Simulator is a program that simulates plays, games, and seasons in a hypothetical NFL. This document covers the 40 teams in this hypothetical league plus the method of generating a preseason and regular season schedule, plus how the playoffs work.

## Teams

The NFL in Football Simulator brings all 32 teams across 2 conferences and 8 divisions from the real NFL. However, some key differences are present:

- The Rams remain in St. Louis.
- The Chargers remain in San Diego.
- The Raiders remain in Oakland.
- The Washingtom team is known as the Presidents.

Additionally, we add 8 additional teams, 4 in the AFC and 4 in the NFC, to be part of a new "Extra" division. The AFC Extra consists of:

- Louisville Thunder, playing in Cardinal Stadium
- Vostok Station Penguins, playing in Bellingshausen Stadium in Antarctica
- Toledo Mud Hens, playing in the Glass Bowl
- Portales Gladiators, playing in Roosevelt General Stadium

The NFC Extra consists of:

- Furnace Creek Reapers, playing in The Crucible
- Dover Wasps, playing in Alumni Stadium in Delaware
- Grand Forks Hawks, playing in Alerus Center in North Dakota
- Wainwright Glaciers, playing in Chukchi Stadium in Alaska

## Symmetric Tables

Symmetry is a concept that comes up a lot in scheduling an NFL season:

- If Team A plays Team B, necessarily Team B must play Team A.
- If the North division plays the South in a season, necessarily the South must also play the North.

And so forth. It'd be nice to have a data structure resembling a table that, when a certain cell is assigned, its symmetric cell is also assigned. So let's build one!

The `SymmetricTable<TKey> where TKey : class` type appears as a 2D array of rows and columns. Rows have keys, like a dictionary, and these keys must be unique and implement `IEquatable<TKey>`. Using the `FromRowKeys(IEnumerable<TKey> keys, int columnCount)` method, a `keys.Count()` by `columnCount` table can be constructed, and cells can be accessed by the `TKey this[TKey key, int columnIndex]` indexer.

Symmetric assignment is the main show here. When assigning a value to a cell, that value is used as the key to find its symmetric partner. If we set the cell `["Colts", 4]` to `"Bengals"`, the table will automatically set the `["Bengals", 4]` cell to `"Colts"`.

For looping that allows us to avoid collection-modified exceptions, the type also exposes an `ElementAt(int)` method, where indices 0 through `keys.Count() - 1` represent the first key's cells and `keys.Count()` through `(keys.Count() * 2) - 1` represent the second keys... this allows us to loop through and assign stuff after checking if the value isn't already assigned.

## Game Groups

The Football Simulator regular season consists of 17 weeks, where each of the 40 teams plays 16 games. Since each game has two teams playing in it, each week has up to 20 games. Teams get a week off, called a bye, assigned randomly between Week 4 and Week 12, inclusive, but we'll get to that.

The opponents a team faces each season are made of four groups:

1. Intradivision: Each team plays its 3 division rivals, once at home and once on the road (six games).
2. Intraconference: Each team plays against all 4 teams in another division in the same conference, two on the road and two at home (four games). However, since there are now 5 divisions per conference, each season has a single division play further games against itself. Since a team only has 3 divisonal opponents but we must fill 4 games, each team must face a randomly chosen division rival twice in this group.
3. Interconference: Each team plays against all the teams in a division from another conference, two at home and two on the road (four games).
4. Remaining intraconference: Each team plays two games against two opponents from the other 3 divisions not faced in the intraconference group, once at home and once on the road (two games). Teams face teams that finished in the same position as they did last year (that is, if you finished 3rd in your division last year, you play the 3rd place team in your opponent division).

The latter 3 groups run on a five-year cycle. Intraconference opponents are determined as follows:

- First year:  East/West,   North/South, Extra plays self
- Second year: East/South,  North/Extra, West plays self
- Third year:  East/North,  West/Extra,  South plays self
- Fourth year: East/Extra,  West/South,  North plays self
- Fifth year:  Extra/North, West/South,  East plays self

Interconference opponents each year are determined as follows:

- AFC East:  North, East,  West,  South, Extra
- AFC North: South, West,  East,  Extra, North
- AFC South: East,  South, Extra, North, West
- AFC West:  West,  Extra, North, East,  South
- AFC Extra: Extra, North, South, West,  East
- NFC North: East,  Extra, West,   South, North
- NFC East:  South, East,  North,  West,  Extra
- NFC West:  West,  North, East,   Extra, South
- NFC South: North, South, Extra,  East,  West
- NFC Extra: Extra, West,  South,  North, East

Finally, remaining intraconference divisions are kinda annoying. In fact, they're not possible without modification. I'm having trouble understanding it, so I'll attempt to explain.

In the normal NFL, with 32 teams and 8 divisions, we have nice symmetry among all the game types. The conferences have 4 divisions and 2 of them are already ruled out each year by being accounted for in the intradivision and intraconference games. The remaining 2 can then fill 2 game slots with ease. But adding a fifth division makes for 3 possible opponents for remaining intraconference. We can just pick 2 of those 3, right? Well, let's try to build just the first year of a valid table:

```
East:  North/South
West:  Extra/North 
North: East /West
South:      /East
Extra: West /
```

We always end up in the same spot: one division has a free slot #1 and another has a free slot #2. Slot #1 represents the first game and slot #2 represents the second; if a division already has a filled slot #1 (like Extra, who plays West here), we can't use that to fill the empty slot #1 in the other division. No one's free that slot. A similar problem exists for slot #2.

Let's say we move Extra's West game to slot #2. This lets us match Extra and South up in slot #1, but we'd also need to move West playing Extra to slot #2, but West is already playing North in that slot. This causes a cascade of slot swaps that ends up in a looping cycle for which there is no solution. Having 8 slots across the 4 divisions is trivial, as in the 4 matchups (`North<>South, North<>East, South<>North, South<>West, East<>North, East<>West, West<>South, West<>East`), each division is named 4 times. To add Extra means we'd need to find 4 spaces to put "Extra", but we'd only be adding 2 new matchups (`Extra<>???, Extra<>???`). The only possible solve is if Extra plays itself.

More intradivision matchups would allow for a solution. We could try to arrange the schedule so that no division faces itself more than twice a year, but the way that remaining intraconference games work would throw a wrench in that. A team plays the team that finished in the same divisional rank as it did last season. If you're playing your own division again, that could very well be yourself, and you can't play a game against yourself. But I think that playoff seeding might give us an idea that might work.

In the previous season, 4 teams ranked from first to fourth in their division. Typical playoff seeding has the #4 team face the #1 team and the #2 team face the #3 team. We can do this twice per team when a division faces itself, preserving the symmetry and still letting division rankings play a role.

Let's determine a rotation that will only have a division face itself no more than twice per season:

- North: West/East,    East/South,   West/Extra,   South/East,   (plays self)
- South: (plays self), West/North,   Extra/West,   North/Extra,  East/West
- East:  Extra/North,  North/West,   (plays self), Extra/North,  South/Extra
- West:  North/Extra,  South/East,   North/South,  (plays self), Extra/South
- Extra: East/West,    (plays self), South/North,  East/South,   West/East

If a division is playing another division, they face the two teams that finished in the same division rank as they did. If they are playing themselves, #4 from the previous season plays #1 twice, and #3 plays #2 twice.

## Determining Opponents for a Team

Now we get to generate the first part of the schedule: the 16 teams that every team would face. Since this is symmetrical (if the Bengals play the Colts, necessarily the Colts play the Bengals), we can use `SymmetricTable<TKey>` here as well. The total opponents grid is 16 by 40 = 640 cells, of which 320 are symmetrically duplicated.

Given a year modulo 5, starting by arbitrarily assigning 2014 as year #0, we can determine the opponents for a team by just iterating over empty grid cells. We create a table with 40 rows, one per team, and 16 opponent cells, looping over each team and cell, skipping ones that are already assigned. Each cell gets assigned an opponent based on its index in the row:

- Rows 0 through 5: Intradivisional games against the other 3 teams in the division.
- Rows 6 through 9: Intraconference games against the 4 teams in the division chosen by the first table up there.
- Rows 10 through 13: Interconference games against the 4 teams in the division chosen by the second table.
- Rows 14 and 15: Remaining intraconference games against the 2 teams that finished in the same divisional position in the previous season, or twice against the inversely ranked team in your own division based on the third table up there.

The first 14 games are easy, but the remaining intraconference games do require us to know last season's division standings. This is provided as an input to the schedule generator. In the case that there is no previous season, a division standing order is determined at "random" by shuffling each division using an instance of the `Random` class. However, for repeatability, it's not quite random - the RNG will be seeded with the same value each time. That value is -1528635010, chosen randomly.

## Creating Game Records

We then need to generate game records from the table of opponents. Again, we loop over the table over rows and cells. At each cell, we take the two teams (key value and cell value) and create a game record using that. The game is hosted by the key value team if the cell is in indices 3 through 5, 8, 9, 12, 13, or 15. Otherwise, the cell value is the home team. Knowing the home team lets us choose the stadium, too. Also, based on the index, we assign the kind of game (intradivisional, intraconference, interconference, remaining divisional). This lets us distinguish "true" intradivisional games from the extras we sometimes have to play from the intraconference games.

As we iterate through the cells, we overwrite the cell with a null, symmetrically setting the opponent to null as well. We do this so that we only create one game record per matchup. Thus, cells containing null are skipped over.

(seriously this is so much easier than how it's currently being handled)

## Scheduling Bye Weeks

Each of the 40 teams gets 1 bye week between weeks 4 and week 13. This 10-week span is convenient because `40 / 10 = 4`, so 4 teams a week get a bye. Since 4 teams comprise 2 games a week, we can make byes by shuffling around the 20 games in each week. Let's move 20 games from throughout the season into Week 17 to start. Since all games in Week 17 are the last intradivisional games, we'll need to do a bit of clever shuffling. Let's begin.

1. Create an array of 40 `bool` instances, one for each team. This will keep track of which teams have had their games marked to be moved to Week 17.
2. Filter the list of all 320 games down to the 120 intradivisional games and shuffle it.
3. For each game in this second list:
	1. Check if the home team's entry in the boolean array is `true` and skip this game if so.
	2. Otherwise, set the home team's entry AND the away team's entry to `true` and add this game to a list of games to move to Week 17.
3. Back in the list of all 320 games, set the week number on the games from the moved list to 17.

We now have 20 games marked as being in week 17 and 300 games to distribute over 16 weeks. This works out to 18.75 games per week on average, but we can cleverely lay them out.

## How Many Games in a Week?

Each week of the NFL season has many games:

- Thursday Night Football is 1 game on Thursday night at 8:15pm.
- Sunday Night Football is 1 game on Sunday night at 8:15pm.
- Monday Night Football is 1 game on Monday night at 8:15pm.
- We'll arbitrarily pick 2 more games to be the Sunday "double header" on Sunday at 4:25pm.

That leaves every other game for the main timeslot of 1:00pm on Sunday. As mentioned, Week 17 will have 20 games, and byes don't start until Week 4, meaning that Weeks 1 through 3 also have 20 games. Conveniently, by having 20 games each in Weeks 1 through 3 and 14 through 17, having exactly 18 games for Weeks 4 through 13 adds up to 320 games exactly.

Assigning weeks has proven quite tricky in the past, but I think a semi-elegant solution with `SymmetricTable<TKey>` is in the cards.

1. Make a 40-row, 16-column symmetric table, with one row per team and 1 week per column.
2. Loop through all the week 17 games and symmetrically assign both the home team to be playing the away team and the away team to be playing the home team. HOWEVER, the cell we assign it to is going to be chosen as follows
	1. Choose a random cell from index 3 (Week 4) to index 12 (Week 13).
	2. ~~Are there fewer than 4 cells already assigned in this *column* (that is, have fewer than 2 teams already been assigned to this week)? If so, go back to step 2-1.~~ Past Chris, this step, with a blank table, would loop forever, given that we haven't assigned anything yet, so all columns are empty. I'm just going to assume you meant "are there already 4 cells assigned in this column" because that's the only interpretation that makes sense to present-me.
	3. Otherwise, symmetrically assign the home team to play the away team and the away team to play the home team (i.e. for a Bengals/Colts game, assign `["Bengals"][index] = "Colts"`, which automatically assigns `["Colts"][index] = "Bengals"`).
	
Why bother with this? It lets us assign which four teams get byes in each week by determining where their games would have been "taken" from. We can then assign the rest of the 300 games actually quite easily:

1. Iterate through the 640 cells of our table, down each column first, then across the columns. This is basically looping through in games-in-a-week order rather than weeks-for-a-team order. Skip any non-null cell.
2. If the cell is empty, find the first game in our game records list that has the cell's key (home team) as the home team and does not have an assigned week number. Given the column number plus one, assign that to the game record's week number.
3. Set the opponent, which symmetrically sets the opponent to play the home team.

And there we have it. In 5 easy steps, we've figured out the week numbers for all 320 games with each team playing exactly once per week.

## Actually Scheduling the Games

Easy. Super easy.

1. Group all 320 games by their week number (17 games).
2. Shuffle each group.
3. Assign games 0 through 4 to the TNF slot, the SNF slot, the MNF slot, and both double-header slots.
4. Assign all remaining games to the Sunday 1:00pm slot.

Done. Valid regular season schedule generated.

The first day of the season is the second Thursday in September of each calendar year. Each next week starts with the next Thursday.

## Scheduling The Preseason

Each team plays 4 additional exhibition games before the second Thursday in September, known as the preseason. These games don't count for anything, so I don't feel the need to schedule it quite so carefully as the real NFL does (they try not to have any matchups in the preseason that occur in the regular season).

We'll start by making a 40 by 4 symmetric table. We'll iterate over each cell in row-then-cell order (so games 1 through 4 for each team) and choose a random other team to be the opponent, symmetrically assigning that team to play us in the same week. Skipping over empty cells, we eventually get a list of opponents for every team.

Next, we generate game records using the same emptying strategy on the table as before, making 80 game records. We then shuffle that list, assign week numbers based on which batch of 20 the shuffled games end up in, and schedule them. Every single preseason game is played on Thursday at 7:00pm, meaning 20 games are in play at that time. The preseason begins 4 weeks before the regular season.