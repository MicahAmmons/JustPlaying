using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
    public class DayCycleSaveData
    {
        [JsonPropertyName("Day")] public float Day {  get; set; }


    }
}
