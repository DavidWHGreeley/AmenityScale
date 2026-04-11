namespace AmenityScaleCore.Models.Battle
{
    public class BattleParticipantDTO
    {
        public string DisplayName { get; set; }
        public string LocationName { get; set; }
        public double Score { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public int UserID { get; set; }

    }
}
