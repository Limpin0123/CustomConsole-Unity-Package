using System;
using UnityEngine;

namespace CustomConsole.Runtime.Console
{
    /// <summary>
    /// Define a function callable with the custom console.
    /// The <c> functionName</c> must not contain spaces.
    ///
    /// ⚠ only parameters with those type of parameters are supported :
    /// <c>string</c>, <c>int</c>, <c>float</c>, <c>bool</c>, <c>Vector2</c>, <c>Vector3</c>, <c>Color</c>, <c>Enum</c>
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method)]
    public class CallableFunctionAttribute : Attribute
    {
        public string functionName { get; }

        /// <param name="name">the function's name can't include spaces,
        /// they'll be automatically replaced by "_".
        /// The name must be at least 4 digits long</param>
        public CallableFunctionAttribute(string name)
        {
            functionName = AddingMissingDigits(name).Replace(" ", "_");
        }

        private string AddingMissingDigits(string str)
        {
            int missingDigitsAmount = Mathf.Max(4 - str.Length, 0);
            return str + new string('_', missingDigitsAmount);
        }
    }
}