# Football Simulator: Rules and Logic for a Single Game

A single football game is played between a home team and an away team. It is played at the stadium of the home team (except in the Super Bowl; more on that later) on a provided date and time.

## Team Strengths

A football team has a set of strengths, all represented as double-precision floats. The strengths are running offense, running defense, passing offense, passing defense, offensive line strength, defensive line strength, kicking strength, field goal strength, kick return strength, kick defense strength, and clock management strength.

A value of 1000 is considered to be average; strengths higher than this are above average, and strengths lower are below average. The gameplay logic uses each strength differently, but it provides a nice ELO-score-like quality because the result of each play will slightly change the strength.

For example, if a team has a really good rushing play, that might add 2 or 3 points to the running offense strength. At the same time, the defense's rushing defense strength might lose a few points, too.

## Standard Asymptotic Formula (Done!)

There's a lot of probabilities that go into Football Simulator, and we want the probability of an event to go up with the strength of the team trying that event while also never quite reaching 100% odds. For this, we'll use a standard asymptotic formula: f(x) = x/(x+n) where n can be changed to change how quickly the function approaches 1. We can then multiply, invert, or add to this function's output to generate asymptotic curves that we want.

The standard asymptotic formula will be referred to as SAF(x, n) for the rest of this document.

### Changing Team Strengths after Plays

## Determining Game Weather

## The Clock, Quarters, and Overtime

A football game consists of 4 quarters, each 15 minutes long. At the beginning of a game, the home team chooses the side for a coin flip; the winner of the coin flip then elects to kick or to receive to open the first quarter. Each play takes a certain amount of time off the clock, counting down to 0 seconds. When the clock reaches 0, the next quarter begins at 15 minutes. At the start of the third quarter, the team that kicked in the first quarter will now receive. Otherwise, play continues into the next quarter with the same team having possession at the same location on the field.

If one team has a higher score at the end of the 4th quarter, they are the winner and the game is over. If the score is tied, the game goes into overtime (except in the preseason; the game ends in a tie).

Overtime starts with a coin toss as it does in the first quarter, except the away team decides which side to call. Play begins with a kickoff and kick recovery, just as in the first and third quarters. The overtime period progresses like a normal quarter, except it is 10 minutes long. Both teams get 2 timeouts for overtime.

If, on the first possession, one team scores a touchdown, that team scores 6 points and the game is immediately over. On each subsequent possession, any score immediately ends the game.

If the score is still tied at the end of overtime, the game is declared a tie and is over, except during the postseason, where the game will continue with a modified set of rules.

First, to open another overtime period, the team that lost the coin toss now gets to choose whether to kick or receive. Both teams now get 3 timeouts. Play continues until both teams have had one possession; once both teams have had a possession, the next score wins.

If the game is still tied at the end of second overtime, a third overtime period will start and has the same rules as second overtime. A fourth overtime is also permitted with the same rules.

If the game is still tied at the end of fourth overtime, a fifth and final overtime period will begin with the same coin toss that opened overtime. This period is untimed and play will continue until someone scores. Both teams get unlimited timeouts, though they still cannot call two in a row between plays.

### Timeouts and Clock Management

In each half, both teams receive 3 timeouts. Because, in Football Simulator, time comes off the clock in discrete chunks, managing the clock works a bit differently. The basics are:

- For a rushing or passing play that ends inbounds, the clock continues to run until the next play.
- For an incomplete pass or a rushing or passing play that ends out of bounds, the clock stops until the next play.
- Calling a timeout stops the clock immediately.

A team's clock management strength determines...

## Kickoffs and Onside Kicks

A kickoff occurs to open a half and each overtime period, and the kicking team is determined by coin toss as discussed in The Clock, Quarters, and Overtime. It also occurs after any scoring play except a safety, with the scoring team being the one to kickoff. Kickoffs occur from the kicking team's 35-yard line (unless a penalty has been assessed on the kickoff) facing the opposite endzone.

A kicked ball is live; the receiving team can recover it on any point on the field, and the kicking team can recover it after it has travelled at least 10 yards. Anyone who legally recovers a kick can then advance it forward or take a knee to down the ball; for the receiving team, downing a ball in the endzone makes the result a touchback and the ball comes out to the team's 25-yard line. (If the kicking team recovers, is driven back into their own endzone, and then takes a knee, that's a safety.) Wherever the ball goes down makes first down for the team that recovered the ball. Returning a kicked ball works exactly like a rushing play; if the returner fumbles, it's a live ball; you cannot throw forward passes but can throw laterals; etc.

The receiving team may signal fair catch at any time while the ball is in the air, even if no one recovers it. Signalling fair catch results in a touchback and the ball comes out to the receiving team's 25-yard line.

If the ball is kicked out the back of the receiving team's endzone, that's a touchback and the ball comes out to the receiving team's 25-yard line. If the ball is kicked out of bounds to the left or the right (even in the receiving team's endzone), that's a kickoff out of bounds penalty and the ball comes out to the receiving team's 30-yard line. If the ball stops inside the receiving team's endzone and no one had signaled fair catch, the ball is live and can be recovered by either team, and it's too late to signal fair catch. Recovery by the receiving team results in either a kick return (if they run it out of the endzone), a touchback (if they down the ball), or a safety (if they try to return it and get forced back into their own endzone). Recovery by the kicking team is an immediate touchdown.

Due to the ball becoming recoverable by either team after it travels 10 yards, the kicking team can try to kick the ball with less force to maximize their chances of recovery. This is called an onside kick.

Relevant strengths are kicking strength, kick return strength, and kick defense strength. The kicking team has the following options:

1. Attempt a normal kickoff.
2. Attempt an onside kick.

The kicking team will choose option 2 if:

- Their disposition is Insane, or
- It is the 4th quarter or overtime, and they're down by more than 8 points, and there's less than 10 minutes on the clock in the 4th quarter or 2:30 on the clock in any overtime period.

Let's go over each option in turn. First, we must compute the kick differential, which is the kicking team's kick strength minus the receiving team's kick defense strength.

### Option 1: Normal Kickoff

To calculate if the kickoff goes out of bounds, the odds are 2% + (kick differential / 50).

To calculate the distance the kick travels, let K = kicking team's kick strength minus 1000. Use a normal distribution with a mean of 65 yards and a standard deviation of (3 yards \* (K / 50)).

To calculate if the receiving team calls for a fair catch, compute (kicking team's rush defense strength - receiving team's kick return strength). If it's -500 or less, they always signal for fair catch.

To calculate if anyone from the defense catches the ball if a fair catch is not signaled, the odds start at 99.99% and go down by 0.01% for every 50 points beneath 1000 that the receiving team's kick return strength is.

The kick return, if it happens, acts as a rushing play and has the outcomes calculated by the same rules. See Rushing Plays below.

### Option 2: Onside Kickoff Attempt

To calculate the odds that the kicking team recovers the ball: 10% + (kick differential / 50) - that is, for every 50 points in differential, the recovery chance goes up by 1 percentage point.

To calculate how far the ball travels on the onside kick, use a normal distribution with a mean of 10 yards and a standard deviation of 1 yard for every 50 points of onside kick differential. If and only if the kicking team recovers, any values less than 10 yards are rounded up to 10 yards, and values that go beyond the receiving team's endzone are clamped to their 1-yard line. Effectively, this means that a differential of 450 points would make the standard deviation average 10 yards with a standard deviation of 9 yards.

Onside kicks can theoretically be returned by the kicking team if recovered, but Football Simulator will assume this never happens.

Either way, someone gets possession of the football, and if the play doesn't end in a touchdown or safety, it's first down for the team that recovered it. Touchdowns result in a conversion attempt, safeties result in a free kick.

## Progression of a Drive

An offensive drive consists of a series of plays, beginning with first down with 10 yards to go for another first down (unless penalties are assessed on the team, which moves the line of scrimmage).

A single football play always has an outcome called a result. There are several results to a play and what happens next in the drive is determined by them. They are as follows:

- **Ball Dead**: The usual outcome - the offense (or the defense in case of a turnover) has started a play and has either downed the ball or became down somewhere on the field (except the endzones). This also includes the ball going out of bounds - the ball is dead where it went out - sacks, and kick returns. If the ball becomes dead past the line-to-gain, it is first down, otherwise, the down advances to the next one (i.e. first becomes second, second becomes third, etc.). If the ball is dead on fourth down, it is a turnover on downs to the other team, who get it where it became dead.
- **Incomplete Pass**: The offense has tried to throw a forward pass and was not able to complete it. This also includes spiking the ball to stop the clock. The line of scrimmage doesn't change and the down counter advances as usual.
- **Made Field Goal**: The offense kicks a successful field goal. The next play is kickoff by the offense.
- **Missed Field Goal**: The offense tries and fails to kick a field goal. The next play is first down for the defense from the line of scrimmage on the field goal attempt.
- **Touchdown**: The ball is carried into the opponent's endzone. The next play is a kickoff by the scoring team.
- **Conversion Attempt**: Either an extra-point try or a two-point-conversion try, successful or not. The next play is kickoff for the offense.
- **Safety**: The ball is downed inside the offense's own endzone. The next play is a free kick.
- **Punt Downed by Punting Team**: The ball is punted by the offense and touched by the offense. The next play is first down for the defense from wherever the ball was touched.
- **Fair Catch**: The receiving team signals for fair catch outside their own endzone on either a punt, kickoff, or free kick. The next play is first down from where the ball first hits the ground.
- **Touchback**: A fair catch is signalled by the receiving team and the ball first lands in their endzone, or the ball goes out the back of their endzone, on a punt, kickoff, or free kick. The next play is first down from the 25-yard line.

## Picking a Rush or a Pass

The offense can choose between a rush and a pass based on their rushing and passing differentials, which are (their rushing offense strength - the defense's rushing defense strength) and (their passing offense strength - the defense's passing defense strength), respectively.



## Rushing Plays

A rushing play (including kick returns) has the offense try to advance the ball on the ground. A rush has a small chance of resulting in a fumble, which might be recovered by the offense or the defense. In extreme circumstances, the offense might attempt laterals to prevent going down, but this is a very risky strategy.

A team has a rushing offense strength and their opponent has a rushing defense strength (kick return strength and kick defense strength are used on kick returns, respectively). The rushing differential is the offense's strength minus the defense's strength. Negative differentials indicate the defense is better at stopping runs, and positive differentials indicate that the offense is better at running.

If the offense decides to run a rushing play, we first compute the chance of a fumble. Fumbles are based on the offense's strength, not on the differential, and starts at 2% per play at 1000 points of strength. For every 50 points above or below 1000, the fumble rate is changed by 5%. Note that this isn't 5 percentage points, but 5% - a strength of 1100 would make the fumble chance (2% \* 0.95 \* 0.95) = 1.805%. A strength of -3009.03 would make fumbles guaranteed.

WEATHER EFFECT

If a fumble occurs, we must calculate who recovers the fumble. At a rushing differential of 0, both teams recover the ball at a 50% chance. If the rushing differential is positive, the offense is favored to recover, and we compute their odds as 0.5 + (SAF(x, 1) / 2) where x = the differential / 50. This basically means that their odds start at 50% and approach 100% asymptotically. If the rushing differential is negative, the defense is favored, and we compute their odds with the same formula.

The team that recovers the fumble has a chance to have not downed the ball and is eligible to return it. This is computed based on the rushig strength of the fumbling team - the worse they are, the more likely the ball is returnable. A rushing strength of 0 or below means the ball is always returnable, and a rushing strength of 2000 or higher means the ball is never returnable. The probability is linear with respect to the strength (meaning a strength of 1000 makes a 50% chance). Thus, the percentage odds for a rushing strength x are x/2000.

But let's assume a fumble doesn't happen. To determine the yards gained by the offense, we take a normal distribution with mean = 4.2 yards and standard deviation = 3 yards at a rushing differential of 0. Every 50 points corresponds to a 0.2 yard change in the mean and an 0.3 yard change in the standard deviation. A point is then sampled to produce the rushing distance.

WEATHER EFFECT

Rushing plays might go out of bounds. Teams with good clock management are much more likely to try to go out of bounds when it's the fourth quarter or overtime and there's less than 5 minutes in the period, and the team is losing. When this isn't the case, the odds of a play going out of bounds are a straight 25%. When it is, the odds depend on the team's clock management strength:

- For teams with a clock management strength of 1000 or more, the odds become 0.9 + (SAF(s, 1) / 10), where s = the team's clock management strength divided by 50.
- For teams with a clock management strength of less than 1000, the odds become 0.9 \* (1 - SAF(s, 1)), where s = the team's clock management strength divided by 50.

## Passing Plays

Passing plays have several steps. The passing differential, which is the offense's passing strength minus the defense's pass defense strength, is a key value to use here. First, we need to determine if the quarterback is sacked. The odds of this are 3.6% per play at a passing differential of 0. For positive differentials, the odds are 0.036 + (SAF(d, 1) \* 0.9964), where d = the differential divided by 50. For negative differentials, the odds are 0.036 \* (1 - SAF(d, 1)).

If the quarterback is sacked, we need to calculate how many yards were lost. At a passing differential of 0, a normal distribution with mean 6.5 yards and a standard deviation of 2 yards, with a minimum of 3 yards and a proper cap on maximum distance based on field position. Each 50 points of differential moves the mean by 0.3 yards and increases the standard deviation by 0.2 yards.

Additionally, the same fumble calculation that occurs during running plays occurs during a sack. See Rushing Plays above for more information.

If the quarterback is not sacked, we next figure out if the pass was a spike to stop the clock, thrown with no receiver in the vicinity, or thrown beyond the line of scrimmage.

- The ball will be spiked if it's the fourth quarter or overtime, there's less than 2 minutes on the clock, the offense has no timeouts, and it isn't fourth down.
- The ball will be thrown with no receiver in the vicinity at a rate of 0.25% per pass attempt at a passing strength of 1000. At strengths above this, the odds become 0.0025 + (SAF(k, 1) \* 0.9975), where k is the passing strength divided by 50. At strengths below this, the odds are 0.0025 \* (1 - SAF(d, 1)).
- The ball will be thrown beyond the line of scrimmage if it wasn't already determined to be thrown with no receiver if the vicinity with odds of 0.01% at a passing strength of 1000. At strengths above this, the odds become 0.0001 + (SAF(k, 1) \* 0.9999), where k is the passing strength divided by 50. At strengths below this, the odds are 0.0001 \* (1 - SAF(d, 1)).

At this point, the quarterback has thrown a legal pass. There are 3 outcomes that can occur: a completed pass, an interception, or an incomplete pass. At a differential of 0, A complete pass occurs with odds of 65.7%, an interception with odds of 2%, and the remainder is incomplete passing.

To compute the odds with different strengths, we start by calculating the odds of an interception. At positive differntials, the odds become 0.02 + (SAF(d, 1 \* 0.98)), and at negative differentials, the odds become 0.02 \* (1 - SAF(d, 1)). The remaining part of the probability is then split between complete and incomplete passes, which are based entirely on offensive passing strength. With a passing strength of 1000, the non-interception odds of completion are 67.04%. At higher strengths, they become 0.6704 + (SAF(k, 1) \* 0.3296) and at strengths below, they become 0.6704 \* (1 - SAF(k, 1)).

WEATHER EFFECT

An incomplete pass stops the clock, costs the offense a down, and does not move the line of scrimmage. A completed pass, however, is made of two parts: a reception some distance downfield, and the yards-after-catch where the receiver runs forward. First, the yards gained on the catch is sampled from a normal distribution with a mean of 3.5 yards and a standard deviation of 5 yards. Every 50 points of passing differential changes these values by 0.2 yards and 0.8 yards, respectively.

The receiver is not always able to advance after the catch; often, they are tackled as soon as they make the catch. The odds of being able to make yards after catch are based on the passing differential. If the passing differential is positive, the receiver is favored to advance the ball, and we compute their odds as 0.5 + (SAF(x, 1) / 2) where x = the differential / 50. If the passing differential is negative, the tackling of the receiver is favored, and we compute the odds of that with the same formula.

If the receiver is able to make yards-after-catch, the play effectively becomes a rushing play and we use the rushing play odds to compute fumbles and how far the ball advances. However, the normal distribution used to calculate the yards-after-carry now has a mean of 7.5 yards (with the same standard deviation of 3 yards and the same changes with changes in the rushing differential).

Interceptions are, similarly, subject to the yards-after-catch computation, just in the other direction. The same computation is used for whether the intercepting player is eligible for yards-after-catch, but the normal distribution for the computation of how many yards are gained now has a mean of only 1.5 yards.

## Laterals

In truly desperate scenarios, the offense may choose to throw lateral passes to attempt to win. The offense will throw laterals if:

- Their disposition is not Ultra-Conservative,
- It is the 4th quarter or any overtime quarter,
- They are losing by 8 points or less,
- and there are under 10 seconds left in the period.

Laterals are very unlikely to succeed. Each lateral between two players of the offense has a 50% chance of being intercepted, and the intercepting player will always take a knee to end the play (and, usually, the game). Each successful lateral is subject to the standard rushing computation (including fumbles), except that there's an 80% chance that the rush ends in another lateral attempt where it would ordinarily down the ball and end the play (which still happens 20% of the time, assuming no fumbles).

Laterals are always passed either sideways or backward. Assuming no interceptions, the number of yards lost is sampled from a normal distribution with a mean of 5 yards and a standard deviation of 3 yards, capped at 0 yards. Distances that would cause the ball to exit the offense's endzone will be counted as safeties, interceptions in the offense's endzones are automatic touchdowns.

## Field Goal Attempts

A team will consider a field goal under the following conditions:

- Their disposition is not Insane,
- and it is 4th down, or it is the second quarter, fourth quarter, or any overtime period and there's less than 10 seconds left in the period.

The team will then try a field goal if the kicking differential (their kicking strength - their opponent's kick defense strength) is high enough. We compute this by sampling 100 points from a normal distribution that will also be used for the kicking distance. This distribution has a mean of 55 yards and a standard deviation of 5 yards, which channge by 0.5 yards and 0.1 yards, respectively, for every 50 points above or below 1000 of the team's kicking strength. If 50% or more of the sampled points are long enough to try a field goal from this distance, the field goal attempt is tried.

However, distance alone is not the only factor. Kicks can travel wide left or wide right...

WEATHER EFFECT

## Conversion Attempts

## Free Kicks following a Safety

## Punts

## Turnovers: Interceptions and Fumble Recoveries

## Scoring Plays

## Penalties

## Team Injuries

## Game Highlights

## Debug Decision View