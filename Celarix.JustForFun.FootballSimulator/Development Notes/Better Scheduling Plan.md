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
- If the North division plays the South in a season, necessarily the South must also play the North

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

Finally, remaining intraconference divisions are kinda annoying. Actually, the previous two tables were annoying to assign, too. But now that we have `SymmetricTable<TKey>`, we can make the determination of a table quite easy! Here it is, generated with symmetry!

- East:  Extra/South, West/Extra, South/West, West/North, South/West
- West:  North/Extra, East/South, North/East, North/Extra, North/East
- North: West/Extra, South/West, West/South, Extra/South, West/South
- South: Extra/East, Extra/West, East/Extra, East/North, East/Extra
- Extra: South/North, South/East, East/South, North/West, East/South