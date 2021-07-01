using System;
using System.Collections.Generic;

namespace PactNet.Native
{
    /// <summary>
    /// Defines the scenarios static list for messaging support
    /// </summary>
    public class Scenarios
    {
        /// <summary>
        /// The available scenarios
        /// </summary>
        internal static Dictionary<string, Func<dynamic>> All;

        /// <summary>
        /// Invokes the scenarios
        /// </summary>
        /// <param name="description">the name of the scenario</param>
        /// <returns>a dynamic message object</returns>
        public static dynamic InvokeScenario(string description)
        {
            return All.TryGetValue(description, out _) ? All[description].Invoke() : null;
        }
    }
}
