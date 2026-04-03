/// <summary>
/// Version         Date        Coder           Remarks
/// 0.1             2026-01-04  Greeley         Init
/// 
using System;

namespace AmenityScaleCore.Models.Battle
{
    public class BattleDTO
    {
        public int BattleID { get; set; }
        public Guid BattleCode { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; }
    }
}
