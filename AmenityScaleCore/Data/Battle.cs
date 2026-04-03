/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2026-01-04  Greeley         Created Battle Data access layer

using AmenityScaleCore.Models.Battle;
using AmenityScaleCore.Models.User;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace AmenityScaleCore.Data
{
    public class BattleDataAccess
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public UserDTO CreateUser(string displayName)
        {
            SqlDataReader r = PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_User_Create",
                new SqlParameter("@DisplayName", displayName)
            );

            r.Read();
            return new UserDTO
            {
                UserID = Convert.ToInt32(r["UserID"]),
                DisplayName = r["DisplayName"].ToString()
            };
        }

        public BattleDTO CreateBattle(int userID, DateTime expiresAt)
        {
            SqlDataReader r = PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_Battle_Create",
                new SqlParameter("@UserID", userID),
                new SqlParameter("@ExpiresAt", expiresAt)
            );

            r.Read();
            return new BattleDTO
            {
                BattleID = Convert.ToInt32(r["BattleID"]),
                BattleCode = (Guid)r["BattleCode"],
                ExpiresAt = Convert.ToDateTime(r["ExpiresAt"]),
                Status = r["Status"].ToString()
            };
        }

        public BattleDTO GetByCode(Guid battleCode)
        {
            SqlDataReader r = PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_Battle_GetByCode",
                new SqlParameter("@BattleCode", battleCode)
            );

            if (!r.Read()) return null;

            return new BattleDTO
            {
                BattleID = Convert.ToInt32(r["BattleID"]),
                BattleCode = (Guid)r["BattleCode"],
                ExpiresAt = Convert.ToDateTime(r["ExpiresAt"]),
                Status = r["Status"].ToString()
            };
        }

        public List<BattleParticipantDTO> JoinBattle(Guid battleCode, int userID, int locationID)
        {
            SqlDataReader r = PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_Battle_Join",
                new SqlParameter("@BattleCode", battleCode),
                new SqlParameter("@UserID", userID),
                new SqlParameter("@LocationID", locationID)
            );

            return MapLeaderboard(r);
        }

        public List<BattleParticipantDTO> GetLeaderboard(Guid battleCode)
        {
            SqlDataReader r = PDM.Data.SqlHelper.ExecuteReader(
                GetConnectionString(),
                "sp_Battle_Leaderboard",
                new SqlParameter("@BattleCode", battleCode)
            );

            return MapLeaderboard(r);
        }

        private List<BattleParticipantDTO> MapLeaderboard(SqlDataReader r)
        {
            var list = new List<BattleParticipantDTO>();

            while (r.Read())
            {
                list.Add(new BattleParticipantDTO
                {
                    DisplayName = r["DisplayName"].ToString(),
                    LocationName = r["LocationName"]?.ToString() ?? "",
                    Score = Convert.ToDouble(r["Score"]),
                    Latitude = r["Latitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Latitude"]),
                    Longitude = r["Longitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Longitude"])
                });
            }

            return list;
        }
    }
}
