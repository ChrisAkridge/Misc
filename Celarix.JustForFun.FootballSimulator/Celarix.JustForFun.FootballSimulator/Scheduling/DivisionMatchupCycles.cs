using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    internal static class DivisionMatchupCycles
    {
        public static Division GetIntraconferenceOpponentDivision(int yearCycleNumber, Division division)
        {
            const Division N = Division.North;
            const Division S = Division.South;
            const Division E = Division.East;
            const Division W = Division.West;
            const Division X = Division.Extra;

            return yearCycleNumber switch
            {
                0 => division switch
                {
                    E => W,
                    W => E,
                    N => S,
                    S => N,
                    X => X,
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                1 => division switch
                {
                    E => S,
                    S => E,
                    N => X,
                    X => N,
                    W => W,
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                2 => division switch
                {
                    E => N,
                    N => E,
                    W => X,
                    X => W,
                    S => S,
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                3 => division switch
                {
                    E => X,
                    X => E,
                    W => S,
                    S => W,
                    N => N,
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                4 => division switch
                {
                    X => S,
                    N => W,
                    W => N,
                    S => X,
                    E => E,
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
            };
        }

        public static Division GetInterconferenceOpponentDivision(int yearCycleNumber, Conference conference,
            Division division)
        {
            const Conference AFC = Conference.AFC;
            const Conference NFC = Conference.NFC;

            const Division N = Division.North;
            const Division S = Division.South;
            const Division E = Division.East;
            const Division W = Division.West;
            const Division X = Division.Extra;

            return conference switch
            {
                AFC => division switch
                {
                    E => yearCycleNumber switch
                    {
                        0 => N,
                        1 => E,
                        2 => W,
                        3 => S,
                        4 => X,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    N => yearCycleNumber switch
                    {
                        0 => S,
                        1 => W,
                        2 => E,
                        3 => X,
                        4 => N,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    S => yearCycleNumber switch
                    {
                        0 => E,
                        1 => S,
                        2 => X,
                        3 => N,
                        4 => W,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    W => yearCycleNumber switch
                    {
                        0 => W,
                        1 => X,
                        2 => N,
                        3 => E,
                        4 => S,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    X => yearCycleNumber switch
                    {
                        0 => X,
                        1 => N,
                        2 => S,
                        3 => W,
                        4 => E,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                NFC => division switch
                {
                    E => yearCycleNumber switch
                    {
                        0 => S,
                        1 => E,
                        2 => N,
                        3 => W,
                        4 => X,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    N => yearCycleNumber switch
                    {
                        0 => E,
                        1 => X,
                        2 => W,
                        3 => S,
                        4 => N,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    S => yearCycleNumber switch
                    {
                        0 => N,
                        1 => S,
                        2 => X,
                        3 => E,
                        4 => W,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    W => yearCycleNumber switch
                    {
                        0 => W,
                        1 => N,
                        2 => E,
                        3 => X,
                        4 => S,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    X => yearCycleNumber switch
                    {
                        0 => X,
                        1 => W,
                        2 => S,
                        3 => N,
                        4 => E,
                        _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                _ => throw new ArgumentOutOfRangeException(nameof(conference))
            };
        }

        public static (Division, Division) GetRemainingIntraconferenceOpponentDivisions(int yearCycleNumber,
            Division division)
        {
            const Division N = Division.North;
            const Division S = Division.South;
            const Division E = Division.East;
            const Division W = Division.West;
            const Division X = Division.Extra;

            return yearCycleNumber switch
            {
                0 => division switch
                {
                    N => (W, E),
                    S => (S, S),
                    E => (X, N),
                    W => (N, X),
                    X => (E, W),
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                1 => division switch
                {
                    N => (E, S),
                    S => (W, N),
                    E => (N, W),
                    W => (S, E),
                    X => (X, X),
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                2 => division switch
                {
                    N => (W, X),
                    S => (X, W),
                    E => (E, E),
                    W => (N, S),
                    X => (S, N),
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                3 => division switch
                {
                    N => (S, E),
                    S => (N, X),
                    E => (X, N),
                    W => (W, W),
                    X => (E, S),
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                4 => division switch
                {
                    N => (N, N),
                    S => (E, W),
                    E => (S, X),
                    W => (X, S),
                    X => (W, E),
                    _ => throw new ArgumentOutOfRangeException(nameof(division))
                },
                _ => throw new ArgumentOutOfRangeException(nameof(yearCycleNumber))
            };
        }
    }
}
