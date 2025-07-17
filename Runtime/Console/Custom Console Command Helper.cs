using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection;
using TMPro;
using System.Linq;
using CustomConsole.Runtime.Logger;
using UnityEngine.UI;

namespace CustomConsole.Runtime.Console
{
    public class CustomConsoleCommandHelper : MonoBehaviour
    {
        public TMP_InputField inputField;
        [SerializeField] private RectTransform FunctionAreaRectTransform;
        private RectTransform CanvasRectTransform;
        [SerializeField] private Transform contentTransform;
        [SerializeField] private Transform commandTooltipPrefab;

        public Dictionary<string, CallableCommand> commands = new Dictionary<string, CallableCommand>();
        private Dictionary<string, EntryUpdater> _instancedCommandsHelper = new Dictionary<string, EntryUpdater>();

        [SerializeField] private RectTransform commandHighLighter;
        private List<string> _instancedCommandList = new List<string>();
        [HideInInspector]public int _selectedCommandIndex = -1;
        private InputAction tabAction;
        private InputAction enterAction;
        public class CallableCommand
        {
            public MethodInfo method;
            public MonoBehaviour target;
        }

        void Awake()
        {
            FindCallableFunctions();
            UpdateScrollArea();
            inputField.onValueChanged.AddListener(UpdateFunctionHelper);
            inputField.onDeselect.AddListener(str => UnselectEntry());
            inputField.onEndEdit.AddListener(str => UnselectEntry());
            CanvasRectTransform = GetComponentInParent<RectTransform>();
        }

        private void OnEnable()
        {
            tabAction = new InputAction(
                name: "TabPress",
                type: InputActionType.Button,
                binding: "<Keyboard>/tab"
            );

            tabAction.performed += ctx => OnTabPressed();
            tabAction.Enable();

            enterAction = new InputAction(
                name: "EnterPress",
                type: InputActionType.Button,
                binding: "<Keyboard>/enter"
            );
            enterAction.performed += ctx => OnEnterPressed();
            enterAction.Enable();
        }

        private void OnDisable()
        {
            tabAction.Disable();
            tabAction.Dispose();
            
            enterAction.Disable();
            enterAction.Dispose();
        }

        private void OnTabPressed()
        {
            if(!inputField.isFocused) return;
            
            if(_selectedCommandIndex + 1 >= _instancedCommandList.Count
               || Keyboard.current.shiftKey.isPressed)
            {
                UnselectEntry();
                return;
            }
            else
            {
                _selectedCommandIndex += 1;
                if(!commandHighLighter.gameObject.activeSelf) commandHighLighter.gameObject.SetActive(true);
            }
            EntryUpdater correspondingCommand = _instancedCommandsHelper[_instancedCommandList[_selectedCommandIndex]];
            //Updating Highlight position
            Vector3 pos = commandHighLighter.position;
            pos.y = correspondingCommand.selfRectTransform.position.y;
            commandHighLighter.position = pos;
            //Updating Highlight height
            Vector2 size = commandHighLighter.sizeDelta;
            size.y = correspondingCommand.selfRectTransform.rect.height;
            commandHighLighter.sizeDelta = size;
        }

        private void OnEnterPressed()
        {
            if (_selectedCommandIndex >= 0
                && inputField.isFocused)
            {
                EntryUpdater correspondingCommand = _instancedCommandsHelper[_instancedCommandList[_selectedCommandIndex]];
                correspondingCommand.ForceAction();
                //Unselection is made in Custom Console Command Caller, after preventing the function to be called
            }
        }

        public void UnselectEntry()
        {
            _selectedCommandIndex = -1;
            commandHighLighter.gameObject.SetActive(false);
        }

        /// <summary>
        /// Find all functions marked as [CallableFunction] and reference them in a dictionary for latter use
        /// </summary>
        void FindCallableFunctions()
        {
            MonoBehaviour[] allScriptInScene = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (MonoBehaviour script in allScriptInScene)
            {
                //get all functions with all type of flags
                MethodInfo[] methodsInScript = script.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                                           BindingFlags.Public |
                                                                           BindingFlags.NonPublic);
                foreach (MethodInfo method in methodsInScript)
                {
                    //try to get the custom [CallableFunction] attribute
                    CallableFunctionAttribute attribute = method.GetCustomAttribute<CallableFunctionAttribute>();
                    if (attribute != null)
                    {
                        //To Prevent ambiguity, the method's name is written followed by the script it was found in
                        //Format looks something like : MyFunctionName<GameObjectName>
                        //All spaces in the gameObject's name are removed
                        string functionFullName =
                            $"{attribute.functionName}<{script.gameObject.name.Replace(" ", "")}>";

                        if (!commands.ContainsKey(functionFullName) &&
                            AreFunctionParametersSupported(method.GetParameters(), method, script))
                        {
                            commands.Add(functionFullName, new CallableCommand { method = method, target = script });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if each parameter is a supported type
        /// </summary>
        /// <param name="parameters">An array containing all parameters to check</param>
        /// <returns></returns>
        bool AreFunctionParametersSupported(ParameterInfo[] parameters, MethodInfo method, MonoBehaviour target)
        {
            Type[] supportedTypes = new Type[]
            {
                typeof(string), typeof(int), typeof(float), typeof(bool), typeof(Vector2), typeof(Vector3),
                typeof(Color)
            };
            foreach (ParameterInfo parameter in parameters)
            {
                if (!supportedTypes.Contains(parameter.ParameterType) && !parameter.ParameterType.IsEnum)
                {
                    CustomLogger.CCErrorLog(
                        $"\nThe function : <u>{method.Name}</u> in script : <u>{target.name.ToUpper()}</u> is not eligible for CallableFunction attribute.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Update the function display area's size depending on the amount of shown functions and canvas' height
        /// </summary>
        void UpdateScrollArea()
        {
            //if there is no function shown
            if (_instancedCommandsHelper.Count == 0)
            {
                FunctionAreaRectTransform.gameObject.SetActive(false);
                return;
            }

            FunctionAreaRectTransform.gameObject.SetActive(true);
            //displayed height is clamped to the canvas' height
            LayoutRebuilder.ForceRebuildLayoutImmediate(FunctionAreaRectTransform);
            float desiratedHeight = 0f;
            foreach (EntryUpdater command in _instancedCommandsHelper.Values)
            {
                desiratedHeight += command.selfRectTransform.rect.height;
            }
            float yOffset =
                Mathf.Clamp(desiratedHeight, 0,
                    CanvasRectTransform.rect.height);
            FunctionAreaRectTransform.offsetMax = new Vector2(FunctionAreaRectTransform.offsetMax.x, yOffset);
        }

        /// <summary>
        /// Update the displayed functions in the tooltip above the input field
        /// </summary>
        /// <param name="userText">the text written by the user</param>
        void UpdateFunctionHelper(string userText)
        {
            //if the user prompt is too short or does not start with "/"
            if (userText.Length == 0 || userText[0] != '/' || userText.Length < 4)
            {
                foreach (Transform child in contentTransform)
                {
                    Destroy(child.gameObject);
                }

                _instancedCommandsHelper.Clear();
                UpdateScrollArea();
                return;
            }

            //removing the '/' at start and cutting spaces and following parameters
            string commandNameUserInput = userText.Substring(1);
            List<string> correspondingCommands = new List<string>();
            //different logic, if the user is starting to write parameters check if the name is exact
            //else check if one of the function has a similar name.
            if (commandNameUserInput.Contains(" "))
            {
                commandNameUserInput = commandNameUserInput.Split(" ")[0];
                //getting all functions exactly named like the prompt
                correspondingCommands = commands
                    .Where(keyValue =>
                        keyValue.Value.target != null &&
                        keyValue.Key.Equals(commandNameUserInput, StringComparison.OrdinalIgnoreCase))
                    .Select(keyValue => keyValue.Key)
                    .ToList();
            }
            else
            {
                commandNameUserInput = commandNameUserInput.Split(" ")[0];
                //getting all functions containing the user input ignoring uppercase
                correspondingCommands = commands
                    .Where(keyValue =>
                        keyValue.Value.target != null &&
                        keyValue.Key.IndexOf(commandNameUserInput, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(keyValue => keyValue.Key)
                    .ToList();
            }


            //destroying commandTooltip that aren't correct anymore
            for (int i = _instancedCommandsHelper.Count - 1; i >= 0; i--)
            {
                KeyValuePair<string, EntryUpdater> commandTooltip = _instancedCommandsHelper.ElementAt(i);
                if (!correspondingCommands.Contains(commandTooltip.Key))
                {
                    _instancedCommandsHelper.Remove(commandTooltip.Key);
                    Destroy(commandTooltip.Value.gameObject);
                }
            }

            //instantiate command tooltip that are missing
            foreach (string commandTooltipName in correspondingCommands)
            {
                if (!_instancedCommandsHelper.ContainsKey(commandTooltipName))
                {
                    Transform commandTooltip = Instantiate(commandTooltipPrefab, contentTransform).transform;
                    EntryUpdater entry = commandTooltip.gameObject.GetComponent<EntryUpdater>();
                    _instancedCommandsHelper.Add(commandTooltipName, entry);

                    SplitFunctionNameAndBehaviour(commandTooltipName, out string functionName, out string objectName);
                    string commandDisplayName =
                        $"<color=#ae4cd9>{functionName}</color> <i><size=70%>{objectName}</size></i>";
                    entry.UpdateEntryText(
                        $"{commandDisplayName} {SerializeParameters(commands[commandTooltipName].method)}");


                    entry.UpdateClickableArea(() =>
                    {
                        GUIUtility.systemCopyBuffer =
                            inputField.text; //copy the previous prompt to the clipboard to prevent miss clicks
                        inputField.text = $"/{commandTooltipName}"; //replace the text with the correct string
                        UpdateFunctionHelper($"/{commandTooltipName}");
                        //select back the input field at last position
                        SelectInputFieldAtLastPosition();
                    }, true);
                }
            }

            UpdateScrollArea();
            RefreshCommandList();
        }

        /// <summary>
        /// refresh the list of all shown commands
        /// </summary>
        void RefreshCommandList()
        {
            List<string> newList = _instancedCommandsHelper.Keys.ToList();
            if (_selectedCommandIndex >= newList.Count)
            {
                _selectedCommandIndex = -1;
            }
            _instancedCommandList = newList;
        }

        /// <summary>
        /// Split the full function's name into the real function name and the MonoBehaviour's gameObject name
        /// </summary>
        /// <param name="functionNameReference">The full function's name unaltered</param>
        /// <param name="functionName">The real function name as specified in the Attribute</param>
        /// <param name="objectName">The gameObject's name on which the MonoBehaviour is attached</param>
        void SplitFunctionNameAndBehaviour(string functionNameReference,
            out string functionName, out string objectName)
        {
            string[] splitInfos = functionNameReference.Split("<");
            functionName = splitInfos[0];

            int arrowIndex = splitInfos[1].IndexOf('>');
            objectName = splitInfos[1].Substring(0, arrowIndex);
        }

        #region Parameter Serialization

        /// <summary>
        /// Serializing the parameters of a method to make it easier to read
        /// </summary>
        /// <param name="method">The method we are serializing parameters of</param>
        /// <returns></returns>
        string SerializeParameters(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return String.Join(" ", parameters.Select(SerializeParameter));
        }

        string SerializeParameter(ParameterInfo parameter)
        {
            return $"<color=#5f82ed>{GetTypeAlias(parameter.ParameterType)}</color> {parameter.Name}";
        }

        //some type won't be supported anymore for now
        string GetTypeAlias(Type type)
        {
            Dictionary<Type, string> typeToString = new Dictionary<Type, string>
            {
                { typeof(string), "string" },
                { typeof(int), "int" },
                { typeof(float), "float" },
                { typeof(bool), "bool" },
                { typeof(Vector2), "Vector2" },
                { typeof(Vector3), "Vector3" },
                { typeof(Color), "Color" }
            };
            if (type.IsEnum)
                return type.Name;
            else
            {
                return typeToString[type];
            }
        }

        #endregion

        public void SelectInputFieldAtLastPosition()
        {
            inputField.Select();
            inputField.ActivateInputField();
            inputField.caretPosition = inputField.text.Length;
            inputField.selectionAnchorPosition = inputField.text.Length;
            inputField.selectionFocusPosition = inputField.text.Length;
        }
    }
}