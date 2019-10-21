using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using System.Reflection;
using System.IO;


namespace Dreamteck.Splines
{

    public static class SplineEditorGUI
    {
        public static readonly GUIStyle defaultButton = null;
        public static readonly GUIStyle defaultEditorButton = null;
        public static readonly GUIStyle defaultEditorButtonSelected = null;
        public static readonly GUIStyle dropdownItem = null;
        public static readonly GUIStyle bigButton = null;
        public static readonly GUIStyle bigButtonSelected = null;
        public static readonly GUIStyle labelText = null;
        public static GUIStyle whiteBox
        {
            get
            {
                if (_whiteBox.normal.background == null) _whiteBox.normal.background = white;
                return _whiteBox;
            }
        }
        private static readonly GUIStyle _whiteBox = null;
        public static GUIStyle defaultField
        {
            get
            {
                if (_defaultField.normal.background == null) _defaultField.normal.background = white;
                return _defaultField;
            }
        }
        private static GUIStyle _defaultField = null;
        public static GUIStyle smallField
        {
            get
            {
                if (_smallField.normal.background == null) _smallField.normal.background = white;
                return _smallField;
            }
        }
        private static GUIStyle _smallField = null;
        public static readonly Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 0.3f);
        public static readonly Color textColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public static readonly Color activeColor = new Color(1f, 1f, 1f, 1f);
        public static readonly Color blackColor = new Color(0, 0, 0, 0.7f);
        public static readonly Color buttonContentColor = Color.black;
        private static bool[] controlStates = new bool[0];
        private static string[] floatFieldContents = new string[0];
        private static int controlIndex = 0;
        public static float scale = 1f;
        private static Trigger.Type addTriggerType = Trigger.Type.Double;
        public static Texture2D white
        {
            get
            {
                if (_white == null)
                {
                    _white = new Texture2D(1, 1);
                    _white.SetPixel(0, 0, Color.white);
                    _white.Apply();
                }
                return _white;
            }
        }
        private static Texture2D _white = null;

        public static void Update()
        {
            controlStates = new bool[0];
            floatFieldContents = new string[0];
        }

        public static void Reset()
        {
            controlIndex = 0;
        }

        static SplineEditorGUI()
        {
            defaultButton = new GUIStyle(GUI.skin.GetStyle("button"));
            _whiteBox = new GUIStyle(GUI.skin.GetStyle("box"));
            _whiteBox.normal.background = white;
            _defaultField = new GUIStyle(GUI.skin.GetStyle("textfield"));
            _defaultField.normal.background = white;
            _defaultField.normal.textColor = Color.white;
            defaultField.alignment = TextAnchor.MiddleLeft;
            _smallField = new GUIStyle(GUI.skin.GetStyle("textfield"));
            _smallField.normal.background = white;
            _smallField.normal.textColor = Color.white;
            _smallField.alignment = TextAnchor.MiddleLeft;
            _smallField.clipping = TextClipping.Clip;
            labelText = new GUIStyle(GUI.skin.GetStyle("label"));
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleRight;
            labelText.normal.textColor = Color.white;
            dropdownItem = new GUIStyle(GUI.skin.GetStyle("button"));
            dropdownItem.normal.background = white;
            dropdownItem.normal.textColor = Color.white;
            dropdownItem.alignment = TextAnchor.MiddleLeft;
            bigButton = new GUIStyle(GUI.skin.GetStyle("button"));
            bigButton.fontStyle = FontStyle.Bold;
            bigButton.normal.textColor = buttonContentColor;
            bigButtonSelected = new GUIStyle(GUI.skin.GetStyle("button"));
            bigButtonSelected.fontStyle = FontStyle.Bold;
            buttonContentColor = defaultButton.normal.textColor;
            //If the button text color is too dark, generate a brightened version
            float avg = (buttonContentColor.r + buttonContentColor.g + buttonContentColor.b) / 3f;
            if (avg <= 0.2f) buttonContentColor = new Color(0.2f, 0.2f, 0.2f);
            Rescale();
        }



        private static void Rescale()
        {
            defaultField.padding = new RectOffset(Mathf.RoundToInt(5 * scale), Mathf.RoundToInt(5 * scale), Mathf.RoundToInt(5 * scale), Mathf.RoundToInt(5 * scale));
            smallField.padding = new RectOffset(Mathf.RoundToInt(2 * scale), Mathf.RoundToInt(2 * scale), Mathf.RoundToInt(2 * scale), Mathf.RoundToInt(2 * scale));
            dropdownItem.padding = new RectOffset(Mathf.RoundToInt(10 * scale), 0, 0, 0);
            bigButton.padding = new RectOffset(Mathf.RoundToInt(3*scale), Mathf.RoundToInt(3 * scale), Mathf.RoundToInt(3 * scale), Mathf.RoundToInt(3 * scale));
            bigButtonSelected.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
            bigButton.padding = new RectOffset(Mathf.RoundToInt(4 * scale), Mathf.RoundToInt(4 * scale), Mathf.RoundToInt(4 * scale), Mathf.RoundToInt(4 * scale));
            bigButton.fontSize = Mathf.RoundToInt(30 * scale);
            bigButtonSelected.fontSize = Mathf.RoundToInt(30 * scale);
            defaultButton.fontSize = Mathf.RoundToInt(14 * scale);
            dropdownItem.fontSize = Mathf.RoundToInt(12 * scale);
            labelText.fontSize = Mathf.RoundToInt(12 * scale);
            defaultField.fontSize = Mathf.RoundToInt(14 * scale);
            smallField.fontSize = Mathf.RoundToInt(11 * scale);
        }

        public static void SetScale(float s)
        {
            if(s != scale)
            {
                scale = s;
                Rescale();
            } scale = s;
        }

        public static bool EditorLayoutSelectableButton(GUIContent content, bool active = true, bool selected = false, params GUILayoutOption[] options)
        {
            Color prevColor = GUI.color;
            Color prevContentColor = GUI.contentColor;
            Color prevBackgroundColor = GUI.backgroundColor;
            GUIStyle selectedStyle = GUI.skin.button;
            if (!active) GUI.color = inactiveColor;
            else
            {
                GUI.color = activeColor;
                if (selected)
                {
                    GUI.backgroundColor = SplinePrefs.highlightColor;
                    GUI.contentColor = SplinePrefs.highlightContentColor;
                    selectedStyle = new GUIStyle(selectedStyle);
                    selectedStyle.normal.textColor = Color.white;
                    selectedStyle.hover.textColor = Color.white;
                    selectedStyle.active.textColor = Color.white;
                } else GUI.contentColor = buttonContentColor;
            }
            bool clicked = GUILayout.Button(content, selectedStyle, options);
            GUI.color = prevColor;
            GUI.contentColor = prevContentColor;
            GUI.backgroundColor = prevBackgroundColor;
            return clicked && active;
        }

        public static bool BigButton(Rect position, GUIContent content, bool active = true, bool selected = false)
        {
            bool result = false;
            if (position.width < 30*scale) position.width = 30 * scale;
            if (position.height < 30 * scale) position.height = 30 * scale;
            Color previousContentColor = GUI.contentColor;
            Color previousBackgroundCOlor = GUI.backgroundColor;
            GUI.contentColor = buttonContentColor;
            if (!active) GUI.color = inactiveColor;
            else
            {
                GUI.color = activeColor;
                if (selected)
                {
                    GUI.backgroundColor = SplinePrefs.highlightColor;
                    GUI.contentColor = SplinePrefs.highlightContentColor;
                }
            }
            if (GUI.Button(position, content, selected ? bigButtonSelected : bigButton))
            {
                Event.current.Use();
                if (active) result = true;
            }
            GUI.backgroundColor = previousBackgroundCOlor;
            GUI.contentColor = previousContentColor;
            return result;
        }

        public static bool Button(Rect position, string text, bool active = true, bool selected = false)
        {
            bool result = false;
            if (!active) GUI.color = inactiveColor;
            else
            {
                if (selected) GUI.backgroundColor = SplinePrefs.highlightColor;
                GUI.color = activeColor;
            }
            if (GUI.Button(position, text, defaultButton))
            {
                Event.current.Use();
                if (active) result = true;
            }
            GUI.backgroundColor = Color.white;
            return result;
        }

        public static bool DropDown(Rect position, GUIStyle style, string[] options, bool active, ref int currentOption)
        {
            if (!active) GUI.color = inactiveColor;
            else GUI.color = activeColor;
            TextAnchor anchor = style.alignment;
            style.alignment = TextAnchor.MiddleLeft;
            bool mouseHovers = false;
            HandleControlsCount();
            if (GUI.Button(position, options[currentOption], style)) if (active) controlStates[controlIndex] = !controlStates[controlIndex];
            style.alignment = anchor;
            GUI.Label(new Rect(position.x + position.width - 20, position.y, 20, position.height), "▼", style);
            if (controlStates[controlIndex] && active)
            {
                for (int i = 0; i < controlStates.Length; i++)
                {
                    if (i == controlIndex) continue;
                    controlStates[i] = false;
                }
                SceneView.RepaintAll();
                GUI.BeginGroup(new Rect(position.x, position.y + position.height, position.width, position.height * options.Length));
                GUI.backgroundColor = blackColor;
                GUI.Box(new Rect(0, 0, position.width, position.height * options.Length), "", whiteBox); 
                if (new Rect(0, 0, position.width, position.height * options.Length).Contains(Event.current.mousePosition)) mouseHovers = true;
                for (int i = 0; i < options.Length; i++)
                {
                    Rect current = new Rect(0, position.height * i, position.width, position.height);
                    if (current.Contains(Event.current.mousePosition)) GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
                    else GUI.backgroundColor = Color.clear;
                    if (GUI.Button(current, options[i], dropdownItem))
                    {
                        currentOption = i;
                        controlStates[controlIndex] = false;
                    }
                }
                if (controlStates[controlIndex])
                {
                    if (Event.current.type == EventType.MouseDown) controlStates[controlIndex] = false;
                }
                GUI.backgroundColor = Color.white;
                GUI.EndGroup();
            }
            controlIndex++;
            return mouseHovers;
        }

        public static float FloatField(Rect position, float value, bool active = true, GUIStyle style = null)
        {
            HandleControlsCount();
            if (style == null) style = smallField;
            if (!active) GUI.color = inactiveColor;
            else GUI.color = activeColor;
            GUI.backgroundColor = blackColor;

            string floatStr = value.ToString();
            float result = 0f;
            if (floatFieldContents[controlIndex] != null)
            {
                floatFieldContents[controlIndex] = GUI.TextField(position, floatFieldContents[controlIndex], style);
                if (float.TryParse(floatFieldContents[controlIndex], out result))
                {
                    floatStr = floatFieldContents[controlIndex];
                    floatFieldContents[controlIndex] = null;
                }
            } else floatStr = GUI.TextField(position, floatStr, style);
            if (!float.TryParse(floatStr, out result)) floatFieldContents[controlIndex] = floatStr;
            
            GUI.backgroundColor = Color.white;
            controlIndex++;
            if (float.TryParse(floatStr, out result)) return result;
            else return value;
        }

        private static string CleanStringForFloat(string input)
        {
            if (Regex.Match(input, @"^-?[0-9]*(?:\.[0-9]*)?$").Success)
                return input;
            else
            {
                return "0";
            }
        }

        public static float FloatDrag(Rect position, float value)
        {
            HandleControlsCount();
            if (position.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    controlStates[controlIndex] = true;
                    Event.current.Use();
                }
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    value = 0f;
                    Event.current.Use();
                }
            }
            if (Event.current.type == EventType.MouseUp) controlStates[controlIndex] = false;
            if (controlStates[controlIndex])
            {
                float delta = Event.current.delta.x;
                float moveStep = Mathf.Clamp(Mathf.Floor(Mathf.Log10(Mathf.Abs(value)) + 1), 1f, 5f);
                float movePerPixel = delta * moveStep * 0.1f;
                value += movePerPixel;
                SceneView.RepaintAll();
            }
            controlIndex++;
            return value;
        }

       private static void HandleControlsCount() {
            if (controlIndex >= controlStates.Length)
            {
                bool[] newStates = new bool[controlStates.Length + 1];
                controlStates.CopyTo(newStates, 0);
                controlStates = newStates;

                string[] newContents = new string[controlStates.Length + 1];
                floatFieldContents.CopyTo(newContents, 0);
                floatFieldContents = newContents;
            }
        }

       public static double ScreenPointToSplinePercent(SplineComputer computer, Vector2 screenPoint)
        {
            SplinePoint[] points = computer.GetPoints();
            float closestDistance = (screenPoint - HandleUtility.WorldToGUIPoint(points[0].position)).sqrMagnitude;
            double closestPercent = 0.0;
            double add = computer.moveStep;
            if (computer.type == Spline.Type.Linear) add /= 2f;
            int count = 0;
            for (double i = add; i < 1.0; i += add)
            {
                SplineResult result = computer.Evaluate(i);
                Vector2 point = HandleUtility.WorldToGUIPoint(result.position);
                float dist = (point - screenPoint).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent = i;
                }
                count++;
            }
            return closestPercent;
        }

        private static List<MethodInfo> GetVoidMethods(Object behavior)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.OptionalParamBinding;
            List<MethodInfo> methods = new List<MethodInfo>();
            methods.AddRange(behavior.GetType().GetMethods(flags));
            for (int i = methods.Count - 1; i >= 0; i--)
            {
                if (methods[i].ReturnType != typeof(void))
                {
                    methods.RemoveAt(i);
                } else{
                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if (parameters.Length == 0) continue;
                    if (parameters.Length > 1) methods.RemoveAt(i);
                    else
                    {
                        System.Type paramType = parameters[0].ParameterType;
                        if (paramType != typeof(int) && paramType != typeof(float) && paramType != typeof(double) && paramType != typeof(string) && paramType != typeof(bool) && paramType != typeof(MonoBehaviour) && paramType != typeof(GameObject) && paramType != typeof(Transform))
                        {
                            methods.RemoveAt(i);
                        }
                    }
                }
            }
            return methods;
        }

        public static void ActionField(SplineAction action)
        {
            EditorGUILayout.BeginHorizontal();
            action.target = (Object)EditorGUILayout.ObjectField(action.target, typeof(Object), true, GUILayout.MinWidth(120));
            if (action.target == null)
            {
                EditorGUILayout.EndHorizontal();
                return;
            }
            GameObject gameObject = null;
            Transform transform = null;
            Component component = null;
            try
            {
                gameObject = (GameObject)action.target;
                transform = gameObject.transform;
            }
            catch
            {
                try
                {
                    transform = (Transform)action.target;
                    gameObject = transform.gameObject;
                }
                catch
                {
                    try
                    {
                        component = (Component)action.target;
                        transform = component.transform;
                        gameObject = component.gameObject;
                    }
                    catch (System.InvalidCastException ex3)
                    {
                        Debug.LogError(ex3.Message);
                        Debug.LogError("Supplied object is not a GameObject and is not a component");
                    }
                }
            }

            List<MethodInfo> methods = new List<MethodInfo>();
            List<string> names = new List<string>();
            int selected = 0;
            MethodInfo method = action.GetMethod();
            List<Object> targets = new List<Object>();
            if (gameObject != null)
            {
                List<MethodInfo> addRange = GetVoidMethods(gameObject);
                for (int i = 0; i < addRange.Count; i++)
                {
                    names.Add("GameObject/" + addRange[i].Name);
                    targets.Add(gameObject);
                    if (method == addRange[i]) selected = names.Count - 1;
                }
                methods.AddRange(addRange);
                Component[] components = gameObject.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    string typeName = components[i].GetType().ToString();
                    addRange = GetVoidMethods(components[i]);
                    methods.AddRange(addRange);
                    for (int n = 0; n < addRange.Count; n++)
                    {
                        names.Add(typeName + "/" + addRange[n].Name);
                        targets.Add(components[i]);
                        if (method == addRange[n]) selected = names.Count - 1;
                    }
                }
            }

            selected = EditorGUILayout.Popup(selected, names.ToArray(), GUILayout.MinWidth(120));
            if (selected >= 0)
            {
                action.target = targets[selected];
                action.SetMethod(methods[selected]);
            }
            if (method != null)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    System.Type paramType = parameters[0].ParameterType;

                    if (paramType == typeof(int)) action.intValue = EditorGUILayout.IntField(action.intValue, GUILayout.MaxWidth(120));
                    else if (paramType == typeof(float)) action.floatValue = EditorGUILayout.FloatField(action.floatValue);
                    else if (paramType == typeof(double)) action.doubleValue = EditorGUILayout.FloatField((float)action.doubleValue);
                    else if (paramType == typeof(bool)) action.boolValue = EditorGUILayout.Toggle(action.boolValue);
                    else if (paramType == typeof(string)) action.stringValue = EditorGUILayout.TextField(action.stringValue);
                    else if (paramType == typeof(GameObject)) action.goValue = (GameObject)EditorGUILayout.ObjectField(action.goValue, typeof(GameObject), true);
                    else if (paramType == typeof(Transform)) action.transformValue = (Transform)EditorGUILayout.ObjectField(action.transformValue, typeof(Transform), true);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void TriggerArray(ref Trigger[] triggers, ref int open) {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            for(int i = 0; i < triggers.Length; i++)
            {
                if (triggers[i] == null)
                {
                    GUILayout.Box("", GUILayout.Width(EditorGUIUtility.currentViewWidth - 30), GUILayout.Height(20));
                    Rect rect = GUILayoutUtility.GetLastRect();
                    GUI.BeginGroup(rect);
                    GUI.Label(new Rect(25, 2, rect.width - 90, 16), "NULL");
                    if (GUI.Button(new Rect(rect.width - 62, 2, 45, 16), "x"))
                    {
                        Trigger[] newTriggers = new Trigger[triggers.Length - 1];
                        for (int n = 0; n < triggers.Length; n++)
                        {
                            if (n < i) newTriggers[n] = triggers[n];
                            else if (n == i) continue;
                            else newTriggers[n - 1] = triggers[n];
                        }
                        triggers = newTriggers;
                    }
                    GUI.EndGroup();
                    continue;
                }
                Color col = new Color(triggers[i].color.r, triggers[i].color.g, triggers[i].color.b);
                if (open == i) col.a = 1f;
                else col.a = 0.6f;
                GUI.color = col;
                GUILayout.Box("", GUILayout.Width(EditorGUIUtility.currentViewWidth - 30), GUILayout.Height(20));
                GUI.color = Color.white;
                Rect boxRect = GUILayoutUtility.GetLastRect();
                GUI.BeginGroup(boxRect);
                Rect nameRect = new Rect(25, 2, boxRect.width - 90, 16);
                if (open == i) triggers[i].name = GUI.TextField(nameRect, triggers[i].name);
                else
                {
                    GUI.Label(nameRect, triggers[i].name);
                    if(nameRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        open = i;
                        GUI.EndGroup();
                        break;
                    }
                }
                triggers[i].enabled = GUI.Toggle(new Rect(2, 2, 21, 16), triggers[i].enabled, "");
                triggers[i].color = EditorGUI.ColorField(new Rect(boxRect.width - 62, 2, 45, 16), triggers[i].color);
                GUI.EndGroup();
                if (i != open) continue;
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                        triggers[i].position = EditorGUILayout.Slider("Position", (float)triggers[i].position, 0f, 1f);
                        triggers[i].type = (Trigger.Type)EditorGUILayout.EnumPopup("Type", triggers[i].type);
                        triggers[i].workOnce = EditorGUILayout.Toggle("Work Once", triggers[i].workOnce);
                GUILayout.Label("Actions");
                for (int n = 0; n < triggers[i].actions.Length; n++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        SplineAction[] newActions = new SplineAction[triggers[i].actions.Length - 1];
                        for (int x = 0; x < triggers[i].actions.Length; x++)
                        {
                            if (x < n) newActions[x] = triggers[i].actions[x];
                            else if (x == n) continue;
                            else newActions[x - 1] = triggers[i].actions[x];
                        }
                        triggers[i].actions = newActions;
                        break;
                    }
                    ActionField(triggers[i].actions[n]);
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("New Action"))
                { 
                    SplineAction[] newActions = new SplineAction[triggers[i].actions.Length + 1];
                    triggers[i].actions.CopyTo(newActions, 0);
                    newActions[newActions.Length - 1] = new SplineAction();
                    triggers[i].actions = newActions;
                }


                EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical();
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    open = -1;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    Trigger[] newTriggers = new Trigger[triggers.Length - 1];
                    for (int n = 0; n < triggers.Length; n++)
                    {
                        if (n < i) newTriggers[n] = triggers[n];
                        else if (n == i) continue;
                        else newTriggers[n - 1] = triggers[n];
                    }
                    triggers = newTriggers;
                }
                if (GUILayout.Button("d", GUILayout.Width(20)))
                {
                    //SplineTrigger newTrigger = ScriptableObject.CreateInstance<SplineTrigger>();
                    Trigger newTrigger = new Trigger();
                    //newTrigger = (SplineTrigger)GameObject.Instantiate(triggers[i]);
                    newTrigger.color = triggers[i].color;
                    newTrigger.enabled = triggers[i].enabled;
                    newTrigger.position = triggers[i].position;
                    newTrigger.type = triggers[i].type;
                    newTrigger.name = "Trigger " + (triggers.Length + 1);
                    Trigger[] newTriggers = new Trigger[triggers.Length + 1];
                    triggers.CopyTo(newTriggers, 0);
                    newTriggers[newTriggers.Length - 1] = newTrigger;
                    triggers = newTriggers;
                    open = triggers.Length - 1;
                }
                if (i > 0)
                {
                    if (GUILayout.Button("▲", GUILayout.Width(20)))
                    {
                        Trigger temp = triggers[i - 1];
                        triggers[i - 1] = triggers[i];
                        triggers[i] = temp;
                    }
                }
                if (i < triggers.Length - 1)
                {
                    if (GUILayout.Button("▼", GUILayout.Width(20)))
                    {
                        Trigger temp = triggers[i + 1];
                        triggers[i + 1] = triggers[i];
                        triggers[i] = temp;
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add new"))
            {
                //SplineTrigger newTrigger = ScriptableObject.CreateInstance<SplineTrigger>();
                Trigger newTrigger = new Trigger();
                newTrigger.Create(addTriggerType);
                newTrigger.name = "Trigger " + (triggers.Length+1);
                Trigger[] newTriggers = new Trigger[triggers.Length+1];
                triggers.CopyTo(newTriggers, 0);
                newTriggers[newTriggers.Length - 1] = newTrigger;
                triggers = newTriggers;
            }
            addTriggerType = (Trigger.Type)EditorGUILayout.EnumPopup(addTriggerType);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
        }

    }
}
