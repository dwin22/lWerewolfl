//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class GameKill
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int KillerId { get; set; }
        public int VictimId { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public int KillMethodId { get; set; }
        public int Day { get; set; }
    
        public virtual Game Game { get; set; }
        public virtual Player Player { get; set; }
        public virtual KillMethod KillMethod { get; set; }
        public virtual Player Player1 { get; set; }
    }
}
