using Celarix.JustForFun.FootballSimulator.Data.Models;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.SummaryWriting
{
#pragma warning disable OPENAI001
    internal sealed class OpenAISummaryWriter : ISummaryWriter
    {
        private const string GameRecordInstruction = """
            You are a sports broadcast writer.
            Write a concise but vivid summary of the football game described below.
            Do not invent plays, players, or scores; all information needed is present.
            These games are simulated by a quasi-random process and may contain unexpected or atypical elements.
            The simulation is intentionally a little underspecified to avoid complexity runaway; you will be told the complete roster of both teams,
            but not which players played on each play, so feel free to choose reasonable options on key plays. Have fun with the environment - pick
            weather conditions, crowd reactions, and other atmospheric details that fit the scenario.
            """;
        private const string SeasonRecordInstruction = """
            You are a sports broadcast writer.
            Write a concise summary of the football season described below. You will be given each team in the league, their final records,
            division standings and playoff seeds, as well as condensed game results. For example, Indianapolis's game list might be
            "W 27-14 vs. CHI, L 21-24 @ NE, W 31-28 vs. MIA...", and they'll be in week order. You will also receive playoff game results
            in a similar format. Do not invent plays, players, or scores; all information needed is present.
            """;

        public string WriteGameSummary(GameRecord gameRecord)
        {
            string apiKey;
            try
            {
                var apiKeyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Football Simulator OpenAI API Key.txt");
                apiKey = File.ReadAllText(apiKeyPath).Trim();
                var client = new OpenAIClient(apiKey);
                var responses = client.GetResponsesClient("gpt-5-mini");
                return WriteGameSummaryImpl(gameRecord, responses);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OpenAISummaryWriter: Failed to generate game summary using OpenAI, using fallback. See the {Type} for more information.",
                    ex.GetType().Name);
                return new FallbackSummaryWriter().WriteGameSummary(gameRecord);
            }
        }

        public string WriteSeasonSummary(SeasonRecord seasonRecord, IReadOnlyList<Team> teams)
        {
            throw new NotImplementedException();
        }

        private string WriteGameSummaryImpl(GameRecord gameRecord, ResponsesClient chatClient)
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore OPENAI001
}
