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
        }

        public static Dictionary<ElementType, float> GetResistances(ElementType element)
        {
            // Return a shallow copy of the specific element's resistance dictionary
            return new Dictionary<ElementType, float>(_resistAndImmunityData[element]);
        }

        internal static Dictionary<ElementType, float> GetPlayerBaseResistance()
        {
            throw new NotImplementedException();
        }
    }
}

public enum ElementType
{
    None,
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