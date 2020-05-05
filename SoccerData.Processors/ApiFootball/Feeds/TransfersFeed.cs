using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SoccerData.Processors.ApiFootball.Feeds
{
	public class TransfersFeed
	{
		public static string GetUrlFromTeamId(int apiFootballTeamId)
		{
			return $"https://api-football-v1.p.rapidapi.com/v2/transfers/team/{apiFootballTeamId}";
		}

		[JsonProperty("api")]
		public ApiResult Result { get; set; }

		public class ApiResult
		{
			[JsonProperty("results")]
			public int Count { get; set; }

			[JsonProperty("transfers")]
			public List<ApiTransfer> Transfers { get; set; }
		}

		public class ApiTransfer
		{
			[JsonProperty("player_id")]
			public int PlayerId { get; set; }

			[JsonProperty("player_name")]
			public string PlayerName { get; set; }

			[JsonIgnore]
			public DateTime? TransferDate
			{
				get
				{
					if (string.IsNullOrEmpty(this._transferDate) || !DateTime.TryParse(this._transferDate, out DateTime dtp))
					{
						return null;
					}
					return dtp;
				}
			}

			[JsonProperty("transfer_date")]
			public string _transferDate { get; set; }

			[JsonProperty("type")]
			public string TransferType { get; set; }

			[JsonProperty("team_in")]
			public ApiTeam TeamIn { get; set; }

			[JsonProperty("team_out")]
			public ApiTeam TeamOut { get; set; }

			[JsonProperty("lastUpdate")]
			public int LastUpdate { get; set; }

			public override string ToString()
			{
				return $"{this.PlayerName}: {this.TeamOut?.TeamName ?? "FA"}->{this.TeamIn?.TeamName ?? "FA"} on {this.TransferDate?.ToShortDateString()}";
			}
		}

		public class ApiTransferEqualityComparer : IEqualityComparer<ApiTransfer>
		{
			public bool Equals(ApiTransfer x, ApiTransfer y)
			{
				return x.PlayerId == y.PlayerId
						&& ((x.TeamIn?.TeamId == null && y.TeamIn?.TeamId == null) || (x.TeamIn?.TeamId != null && y.TeamIn?.TeamId != null && x.TeamIn.TeamId == y.TeamIn.TeamId))
						&& ((x.TeamOut?.TeamId == null && y.TeamOut?.TeamId == null) || (x.TeamOut?.TeamId != null && y.TeamOut?.TeamId != null && x.TeamOut.TeamId == y.TeamOut.TeamId))
						&& ((!x.TransferDate.HasValue && !y.TransferDate.HasValue) || (x.TransferDate.HasValue && y.TransferDate.HasValue && x.TransferDate.Value == y.TransferDate.Value))
						&& ((string.IsNullOrEmpty(x.TransferType) && string.IsNullOrEmpty(y.TransferType)) || x.TransferType == y.TransferType);
			}

			public int GetHashCode(ApiTransfer obj)
			{
				return base.GetHashCode();
			}
		}

		public class ApiTeam
		{
			[JsonProperty("team_id")]
			public int? TeamId { get; set; }

			[JsonProperty("team_name")]
			public string TeamName { get; set; }
		}

		public class ApiTeamEqualityComparer : IEqualityComparer<ApiTeam>
		{
			public bool Equals(ApiTeam x, ApiTeam y)
			{
				return x.TeamId == y.TeamId;
			}

			public int GetHashCode(ApiTeam obj)
			{
				return base.GetHashCode();
			}
		}

		public static TransfersFeed FromJson(string json) => JsonConvert.DeserializeObject<TransfersFeed>(json, Converter.Settings);
	}

	public static partial class Serialize
	{
		public static string ToJson(this TransfersFeed self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}
}
