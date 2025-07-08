using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlayingAround.Data.SaveData
{
        public class SummonedSavedStats
        {
              public int TotalNumberOfKills { get; set; }

        [JsonIgnore] public Texture2D Icon { get; set; }
        }
}
