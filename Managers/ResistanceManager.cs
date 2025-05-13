using PlayingAround.Entities.Player;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PlayingAround.Managers
{
    public static class ResistanceManager
    {
        private static Dictionary<string, Dictionary<string, float>> _resistAndImmunityData;

        // Set these before calling LoadContent
        public static float SuperResistant = -0.4f;
        public static float MildlyResistant = -0.2f;
        public static float SuperVulnerable = 0.4f;
        public static float MildlyVulnerable = 0.2f;

        public static void LoadContent()
        {
            var rawData = JsonLoader.LoadResistanceData();
            // rawData: Dictionary<string, Dictionary<string, string>>

            _resistAndImmunityData = new Dictionary<string, Dictionary<string, float>>();

            foreach (var element in rawData)
            {
                var elementName = element.Key;
                var relations = element.Value; // Dictionary<string, string>

                var resistanceValues = new Dictionary<string, float>();

                // ✅ Direct access, assuming they exist
                var superEffective = relations["superEffective"];
                resistanceValues[superEffective] = SuperResistant;

                var mildlyEffective = relations["mildlyEffective"];
                resistanceValues[mildlyEffective] = MildlyResistant;

                var superVulnerable = relations["superVulnerable"];
                resistanceValues[superVulnerable] = SuperVulnerable;

                var mildlyVulnerable = relations["mildlyVulnerable"];
                resistanceValues[mildlyVulnerable] = MildlyVulnerable;

                _resistAndImmunityData[elementName] = resistanceValues;
            }
        }

        public static void GetPlayerResistances(Player player)
        {
            Dictionary<string, float> resistances = new Dictionary<string, float>
    {
        { "fire", 1.0f },
        { "ice", 1.0f },
        { "earth", 1.0f },
        { "wind", 1.0f },
        { "acid", 1.0f },
        { "metal", 1.0f },
        { "electric", 1.0f },
        { "water", 1.0f }
    };

            player.PlayerResistances = resistances;
        }

        public static Dictionary<string, float> GetResistances(string element)
        {
            if (!_resistAndImmunityData.ContainsKey(element))
                throw new Exception($"Element '{element}' not found in resistance data!");

            // Make a full dictionary of all elements, assigning 0.0f if not explicitly listed
            var fullResistances = new Dictionary<string, float>();

            foreach (var otherElement in _resistAndImmunityData.Keys)
            {
                if (_resistAndImmunityData[element].ContainsKey(otherElement))
                    fullResistances[otherElement] = _resistAndImmunityData[element][otherElement];
                else
                    fullResistances[otherElement] = 0.0f; // Neutral if no special relationship
            }

            return fullResistances;
        }
    }
}
