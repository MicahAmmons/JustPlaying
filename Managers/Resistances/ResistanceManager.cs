using PlayingAround.Entities.Player;
using PlayingAround.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PlayingAround.Managers.Resistances
{
    public static class ResistanceManager
    {
        private static Dictionary<ElementType, Dictionary<ElementType, float>> _resistAndImmunityData = new Dictionary<ElementType, Dictionary<ElementType, float>>();

        // Set these before calling LoadContent
        public static float SuperResistant = -0.4f;
        public static float MildlyResistant = -0.2f;
        public static float SuperVulnerable = 0.4f;
        public static float MildlyVulnerable = 0.2f;

        public static void LoadContent()
        {
            var rawData = JsonLoader.LoadResistanceData();
            // rawData: Dictionary<string, Dictionary<string, string>>

            foreach (var element in rawData)
            {
                var elementName = element.Key;
               
                var relations = element.Value; // Dictionary<string, Elementtype>
              

                var resistanceValues = new Dictionary<ElementType, float>();

               // Add helper method locally
                void Add(string key, float value)
                {
                    if (relations.TryGetValue(key, out var relatedElement) && !string.IsNullOrEmpty(key))
                    {
                        resistanceValues[relatedElement] = value;
                    }
                    else
                    {
                        resistanceValues[relatedElement] = 0f; // fallback key with 0 value if missing
                    }
                }

                Add("superEffective", SuperResistant);
                Add("mildlyEffective", MildlyResistant);
                Add("superVulnerable", SuperVulnerable);
                Add("mildlyVulnerable", MildlyVulnerable);

                _resistAndImmunityData[elementName] = resistanceValues;
            }
        }
        

        public static void GetPlayerResistances(Player player)
        {
            Dictionary<ElementType, float> resistances = new Dictionary<ElementType, float>
    {
        { ElementType.Fire, 1.0f },
        { ElementType.Water, 1.0f },
        { ElementType.Acid, 1.0f },
        { ElementType.Wind, 1.0f },
        { ElementType.Earth, 1.0f },
        { ElementType.Metal, 1.0f },
        { ElementType.Electricity, 1.0f },
        { ElementType.Ice, 1.0f },
        {ElementType.Dark, 1.0f },
        {ElementType.Light, 1.0f }
    };

            player.Resistances = resistances;
        }

        public static Dictionary<ElementType, float> GetResistances(ElementType element)
        {

            // Make a full dictionary of all elements, assigning 0.0f if not explicitly listed
            var fullResistances = new Dictionary<ElementType, float>();

            foreach (var otherElement in _resistAndImmunityData.Keys)
            {
                if (_resistAndImmunityData[element].ContainsKey(otherElement))
                    fullResistances[otherElement] = _resistAndImmunityData[element][otherElement];
                else
                    fullResistances[otherElement] = 0.0f; // Neutral if no special relationship
            }

            return fullResistances;
        }

        internal static Dictionary<ElementType, float> GetPlayerBaseResistance()
        {
            throw new NotImplementedException();
        }
    }
}

public enum ElementType
{
    Fire,
    Water,
    Earth,
    Metal,
    Acid,
    Wind,
    Electricity,
    Ice,
    Dark,
    Light,
    Normal
}