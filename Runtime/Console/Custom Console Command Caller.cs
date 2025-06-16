using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CustomConsole.Runtime.Console
{
    [RequireComponent(typeof(CustomConsoleCommandHelper))]
    public class CustomConsoleCommandCaller : MonoBehaviour
    {
        private CustomConsoleCommandHelper helper;
        [SerializeField] private Toggle isPersistentCommandToggle;

        private void Awake()
        {
            helper = GetComponent<CustomConsoleCommandHelper>();
            helper.inputField.onSubmit.AddListener(TryCallingFunction);
            isPersistentCommandToggle.onValueChanged.AddListener(
                (bool value) => helper.SelectInputFieldAtLastPosition());
        }

        private void TryCallingFunction(string userInput)
        {
            //string[] splitInput = userInput.Split(' ');
            if (!TrySplittingCommandInput(userInput, out string[] splitInput)) return;

            string inputCommandName = splitInput[0];
            string[] inputParameters = splitInput.Skip(1).ToArray();

            if (helper.commands.ContainsKey(inputCommandName))
            {
                CustomConsoleCommandHelper.CallableCommand commandInfo = helper.commands[inputCommandName];
                ParameterInfo[] methodParameters = commandInfo.method.GetParameters();
                if (methodParameters.Length == inputParameters.Length)
                {
                    if (TryCastStringToParameters(methodParameters, inputParameters, out object[] convertedParameters))
                    {
                        commandInfo.method.Invoke(commandInfo.target, convertedParameters);
                        if (!isPersistentCommandToggle.isOn) helper.inputField.text = "";
                        else //select the input field at last position
                        {
                            helper.SelectInputFieldAtLastPosition();
                        }
                    }
                    else
                    {
                        Debug.Log($"Parameter conversion failed.");
                    }
                }
                else
                {
                    Debug.Log(
                        $"The amount of provided parameters ({inputParameters.Length}) doesn't correspond to the amount of needed parameters {methodParameters.Length}");
                }
            }
            else
            {
                Debug.Log($"The name of the function wasn't recognized : '{inputCommandName}'");
            }
        }

        bool TrySplittingCommandInput(string commandInput, out string[] splitInput)
        {
            //get index of all spaces in the string
            List<int> spacesIndex = commandInput
                .Select((c, i) => new { Char = c, Index = i })
                .Where(x => x.Char == ' ')
                .Select(x => x.Index)
                .ToList();
            spacesIndex.Add(commandInput.Length); //to check between last spaces and the end of the prompt

            //get index of all quotation mark in the string
            List<int> quoteIndex = commandInput
                .Select((c, i) => new { Char = c, Index = i })
                .Where(x => x.Char == '"')
                .Select(x => x.Index)
                .ToList();

            if (quoteIndex.Count % 2 != 0)
            {
                Debug.Log("quotation mark missing in command");
                splitInput = new string[0];
                return false;
            }

            //if the command doesn't contain string parameter
            if (!commandInput.Contains('"'))
            {
                splitInput = commandInput.Split(' ');
                splitInput[0] = splitInput[0].Substring(1); //getting rid of the '/' character
                return true;
            }

            List<string> splitedBlocks = new List<string>();

            List<Tuple<int, int>> quotePairPosition = new List<Tuple<int, int>>();
            for (int i = 0; i < quoteIndex.Count; i += 2)
            {
                quotePairPosition.Add(new Tuple<int, int>(quoteIndex[i], quoteIndex[i + 1]));
            }

            int lastSpaceIndex = 0; //start at 0 so the '/' is automatically removed
            int currentQuoteMarkPair = 0;

            for (int i = 0; i < spacesIndex.Count; i++)
            {
                if (spacesIndex[i] < quotePairPosition[currentQuoteMarkPair].Item1)
                {
                    string block = ExtractStringBetweenGivenIndex(commandInput, lastSpaceIndex + 1, spacesIndex[i]);
                    splitedBlocks.Add(block);

                    lastSpaceIndex = spacesIndex[i];
                }
                else if (spacesIndex[i] > quotePairPosition[currentQuoteMarkPair].Item2)
                {
                    if (quotePairPosition.Count > currentQuoteMarkPair + 1)
                    {
                        currentQuoteMarkPair++;
                        i -= 1; //redo the loop but with the new quote pair position
                    }
                    else
                    {
                        string block = ExtractStringBetweenGivenIndex(commandInput, lastSpaceIndex + 1, spacesIndex[i]);
                        splitedBlocks.Add(block);

                        lastSpaceIndex = spacesIndex[i];
                    }
                }
            }

            splitInput = splitedBlocks.ToArray();
            return true;
        }

        string ExtractStringBetweenGivenIndex(string stringInput, int firstIndex, int lastIndex)
        {
            return stringInput.Substring(firstIndex, lastIndex - firstIndex);
        }

        private bool TryCastStringToParameters(ParameterInfo[] parametersInfo, string[] userParameterValues,
            out object[] parameters)
        {
            parameters = new object[parametersInfo.Length];
            for (int i = 0; i < parametersInfo.Length; i++)
            {
                try
                {
                    if (parametersInfo[i].ParameterType == typeof(string))
                    {
                        parameters[i] = userParameterValues[i].Trim('"');
                    }
                    else if (parametersInfo[i].ParameterType == typeof(int))
                    {
                        parameters[i] = int.Parse(userParameterValues[i]);
                    }
                    else if (parametersInfo[i].ParameterType == typeof(float))
                    {
                        parameters[i] = float.Parse(userParameterValues[i], CultureInfo.InvariantCulture);
                    }
                    else if (parametersInfo[i].ParameterType == typeof(bool))
                    {
                        parameters[i] = bool.Parse(userParameterValues[i]);
                    }
                    else if (TryStringToVectorConversion(userParameterValues[i], out object vector))
                    {
                        parameters[i] = vector;
                    }
                    else if (TryStringToColorConversion(userParameterValues[i], out object color))
                    {
                        parameters[i] = color;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Conversion from string to {parametersInfo[i].ParameterType.Name} failed:\n{e.Message}");
                    return false;
                }
            }

            return true;
        }

        private bool TryStringToVectorConversion(string userInput, out object vector)
        {
            try
            {
                string[] values = userInput.Trim('(', ')', '[', ']', '{', '}').Split(',');

                if (values.Length == 2)
                {
                    vector = new Vector2(
                        float.Parse(values[0], CultureInfo.InvariantCulture),
                        float.Parse(values[1], CultureInfo.InvariantCulture)
                    );
                }
                else
                {
                    vector = new Vector3(
                        float.Parse(values[0], CultureInfo.InvariantCulture),
                        float.Parse(values[1], CultureInfo.InvariantCulture),
                        float.Parse(values[2], CultureInfo.InvariantCulture)
                    );
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.Log($"Conversion to vector failed :\n{e}");
                vector = null;
                return false;
            }
        }

        private bool TryStringToColorConversion(string userInput, out object color)
        {
            try
            {
                string[] values = userInput.Trim('(', ')', '[', ']', '{', '}').Split(',');

                if (values.Length == 3)
                {
                    color = new Color(
                        float.Parse(values[0], CultureInfo.InvariantCulture),
                        float.Parse(values[1], CultureInfo.InvariantCulture),
                        float.Parse(values[2], CultureInfo.InvariantCulture)
                    );
                }
                else
                {
                    color = new Color(
                        float.Parse(values[0], CultureInfo.InvariantCulture),
                        float.Parse(values[1], CultureInfo.InvariantCulture),
                        float.Parse(values[2], CultureInfo.InvariantCulture),
                        float.Parse(values[3], CultureInfo.InvariantCulture)
                    );
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.Log($"Conversion to color failed :\n{e}");
                color = null;
                return false;
            }
        }
    }
}
