using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Processors
{
	public class FixturesProcessorHelper
	{
		public static void SetScoringProperties(Feeds.FixturesFeed.Fixture feedFixture, ref Model.Fixture dbFixture)
		{
			if (feedFixture.GoalsHomeTeam.HasValue && (!dbFixture.HomeScore.HasValue || feedFixture.GoalsHomeTeam.Value != dbFixture.HomeScore))
			{
				dbFixture.HomeScore = feedFixture.GoalsHomeTeam.Value;
			}
			if (feedFixture.GoalsAwayTeam.HasValue && (!dbFixture.AwayScore.HasValue || feedFixture.GoalsAwayTeam.Value != dbFixture.AwayScore))
			{
				dbFixture.AwayScore = feedFixture.GoalsAwayTeam.Value;
			}

			if (feedFixture.Score != null)
			{
				if (feedFixture.Score.Halftime != null)
				{
					if (!dbFixture.HomeHalfTimeScore.HasValue
						|| !dbFixture.AwayHalfTimeScore.HasValue
						|| feedFixture.Score.Halftime != $"{dbFixture.HomeHalfTimeScore}-{dbFixture.AwayHalfTimeScore}")
					{
						var arrScores = feedFixture.Score.Halftime.Split('-').ToList();
						if (arrScores != null && arrScores.Count == 2 && int.TryParse(arrScores[0], out int score1) && int.TryParse(arrScores[1], out int score2))
						{
							dbFixture.HomeHalfTimeScore = score1;
							dbFixture.AwayHalfTimeScore = score2;
						}
					}
				}

				if (feedFixture.GoalsHomeTeam.HasValue && feedFixture.GoalsAwayTeam.HasValue)
				{
					if (!dbFixture.HomeFullTimeScore.HasValue
						|| !dbFixture.AwayFullTimeScore.HasValue
						|| dbFixture.HomeScore != feedFixture.GoalsHomeTeam
						|| dbFixture.AwayScore != feedFixture.GoalsAwayTeam)
					{
						dbFixture.HomeFullTimeScore = feedFixture.GoalsHomeTeam;
						dbFixture.AwayFullTimeScore = feedFixture.GoalsAwayTeam;
					}
				}

				if (feedFixture.Score.ExtraTime != null)
				{
					if (!dbFixture.HomeExtraTimeScore.HasValue
						|| !dbFixture.AwayExtraTimeScore.HasValue
						|| feedFixture.Score.ExtraTime != $"{dbFixture.HomeExtraTimeScore}-{dbFixture.AwayExtraTimeScore}")
					{
						var arrScores = feedFixture.Score.ExtraTime.Split('-').ToList();
						if (arrScores != null && arrScores.Count == 2 && int.TryParse(arrScores[0], out int score1) && int.TryParse(arrScores[1], out int score2))
						{
							dbFixture.HomeExtraTimeScore = score1;
							dbFixture.AwayExtraTimeScore = score2;
						}
					}
				}

				if (feedFixture.Score.Penalty != null)
				{
					if (!dbFixture.HomePenaltiesScore.HasValue
						|| !dbFixture.AwayPenaltiesScore.HasValue
						|| feedFixture.Score.Penalty != $"{dbFixture.HomePenaltiesScore}-{dbFixture.AwayPenaltiesScore}")
					{
						var arrScores = feedFixture.Score.Penalty.Split('-').ToList();
						if (arrScores != null && arrScores.Count == 2 && int.TryParse(arrScores[0], out int score1) && int.TryParse(arrScores[1], out int score2))
						{
							dbFixture.HomePenaltiesScore = score1;
							dbFixture.AwayPenaltiesScore = score2;
						}
					}
				}
			}
		}

		public static void SetFixtureProperties(Feeds.FixturesFeed.Fixture feedFixture, ref Model.Fixture dbFixture)
		{
			if (feedFixture.EventDate.HasValue && dbFixture.GameTimeUtc != feedFixture.EventDate.Value.UtcDateTime)
			{
				dbFixture.GameTimeUtc = feedFixture.EventDate.Value.UtcDateTime;
			}

			if (dbFixture.Status != feedFixture.Status || dbFixture.StatusShort != feedFixture.StatusShort)
			{
				dbFixture.Status = feedFixture.Status;
				dbFixture.StatusShort = feedFixture.StatusShort;
			}

			if (feedFixture.Elapsed.HasValue && dbFixture.TimeElapsed != feedFixture.Elapsed.Value)
			{
				dbFixture.TimeElapsed = feedFixture.Elapsed.Value;
			}

			if (feedFixture.FirstHalfStart.HasValue && dbFixture.FirstHalfStartUtc != feedFixture.FirstHalfStart.Value)
			{
				dbFixture.FirstHalfStartUtc = feedFixture.FirstHalfStart.Value.UtcDateTime;
			}

			if (feedFixture.SecondHalfStart.HasValue && dbFixture.SecondHalfStartUtc != feedFixture.SecondHalfStart.Value)
			{
				dbFixture.SecondHalfStartUtc = feedFixture.SecondHalfStart.Value.UtcDateTime;
			}

			if (feedFixture.Referee != null && dbFixture.Referee != feedFixture.Referee)
			{
				dbFixture.Referee = feedFixture.Referee;
			}

			if (feedFixture.Venue != null && dbFixture.VenueName != feedFixture.Venue)
			{
				dbFixture.VenueName = feedFixture.Venue;
			}
		}
	}
}
