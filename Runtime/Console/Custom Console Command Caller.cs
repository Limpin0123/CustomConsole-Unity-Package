using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CustomConsole.Runtime.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace CustomConsole.Runtime.Console
{
    [RequireComponent(typeof(CustomConsoleCommandHelper))]
    public class CustomConsoleCommandCaller : MonoBehaviour
    {
        private CustomConsoleCommandHelper helper;
        [SerializeField] private Toggle isPersistentCommandToggle;

        private readonly Dictionary<Type, Func<string, object>> TypeParser = new Dictionary<Type, Func<string, object>>
        {
            {typeof(string), s => s.Trim('"')},
            {typeof(int), s => int.Parse(s)},
            {typeof(float), s=> float.Parse(s, CultureInfo.InvariantCulture)},
            {typeof(bool), s => bool.Parse(s)}
        };

        private void Awake()
        {
            helper = GetComponent<CustomConsoleCommandHelper>();
            helper.inputField.onSubmit.AddListener(TryCallingFunction);
            isPersistentCommandToggle.onValueChanged.AddListener(
                (bool value) => helper.SelectInputFieldAtLastPosition());
        }

        private void TryCallingFunction(string userInput)
        {
            if(helper._selectedCommandIndex >=0)
            {
                helper.UnselectEntry();
                return;
            }
            if (!TrySplittingCommandInput(userInput, out string[] splitInput)) return;

            string inputCommandName = splitInput[0];
            string[] inputParameters = splitInput.Skip(1).ToArray();

            if (helper.commands.ContainsKey(inputCommandName))
            {
                CustomConsoleCommandHelper.CallableCommand commandInfo = helper.commands[inputCommandName];
                ParameterInfo[] methodParameters = commandInfo.method.GetParameters();
                int obligatoryParameterCount = 0;
                foreach (ParameterInfo parameter in methodParameters)
                {
                    if(!parameter.IsOptional) obligatoryParameterCount++;
                }
                
                if (obligatoryParameterCount <= inputParameters.Length)
                {
                    int missingParameterCount = methodParameters.Length - inputParameters.Length;
                    if(missingParameterCount > 0)
                    {
                        int missingParameterIndex = inputParameters.Length;
                        Array.Resize(ref inputParameters, inputParameters.Length + missingParameterCount);
                        for (int i = missingParameterIndex; i < methodParameters.Length; i++)
                        {
                            object defaultValue = methodParameters[i].DefaultValue;
                            inputParameters[i] = defaultValue != null ? defaultValue.ToString() : "null";
                        }
                    }
                    
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
                        helper.SelectInputFieldAtLastPosition();
                    }
                }
                else
                {
                    Debug.Log($"The amount of provided parameters ({inputParameters.Length}) doesn't correspond to the amount of obligatory parameters {methodParameters.Length}");
                    helper.SelectInputFieldAtLastPosition();
                }
            }
            else
            {
                Debug.Log($"The name of the function wasn't recognized : '{inputCommandName}'");
                helper.SelectInputFieldAtLastPosition();
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
                    Type targetType = parametersInfo[i].ParameterType;
                    //manage string, int, float and bool type
                    if (TypeParser.TryGetValue(targetType, out var parser))
                    {
                        parameters[i] = parser(userParameterValues[i]);
                    }
                    else if ((parametersInfo[i].ParameterType == typeof(Vector2)
                              || parametersInfo[i].ParameterType == typeof(Vector3))
                             && TryStringToVectorConversion(userParameterValues[i], out object vector))
                    {
                        parameters[i] = vector;
                    }
                    else if (parametersInfo[i].ParameterType == typeof(Color) && TryStringToColorConversion(userParameterValues[i], out object color))
                    {
                        parameters[i] = color;
                    }
                    else if(parametersInfo[i].ParameterType.FullName == "Unity.Netcode.ServerRpcParams"
                            || parametersInfo[i].ParameterType.FullName == "Unity.Netcode.ClientRpcParams")
                    {
                        try
                        {
                            parameters[i] = Activator.CreateInstance(parametersInfo[i].ParameterType);
                        }
                        catch (Exception e)
                        {
                            CustomLogger.CCErrorLog($"Cannot create an instance of type {parametersInfo[i].ParameterType.FullName} : {e.Message}");
                            return false;
                        }
                    }
                    else
                    {
                        CustomLogger.CCErrorLog($"Parameters conversion failed");
                        return false;
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
                //conversion to vector failed
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
