using System;

namespace AmenityScaleCore.Models.Battle
{
    class BattleDTO
    {
        public int BattleID { get; set; }
        public Guid BattleCode { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; }
    }
}
