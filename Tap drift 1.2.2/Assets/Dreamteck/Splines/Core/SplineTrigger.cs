using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Reflection;

namespace Dreamteck.Splines
{

    [System.Serializable]
    public class SplineAction
    {
        [SerializeField]
        public Object target = null;

        public int intValue;
        public float floatValue;
        public double doubleValue;
        public string stringValue;
        public bool boolValue;
        public GameObject goValue;
        public Transform transformValue;


        private UnityAction action;
        private UnityAction<int> intAction;
        private UnityAction<float> floatAction;
        private UnityAction<double> doubleAction;
        private UnityAction<string> stringAction;
        private UnityAction<bool> boolAction;
        private UnityAction<GameObject> goAction;
        private UnityAction<Transform> transformAction;

        private MethodInfo methodInfo = null;

        [SerializeField]
        private string methodName = "";

        [SerializeField]
        private int paramType = 0;

        public SplineAction(){

        }

        public SplineAction(UnityAction call){
            action = call;
            paramType = 0;
        }
        public SplineAction(UnityAction<int> call, int value)
        {
            intAction = call;
            intValue = value;
            paramType = 1;
        }
        public SplineAction(UnityAction<float> call, float value)
        {
            floatAction = call;
            floatValue = value;
            paramType = 2;
        }
        public SplineAction(UnityAction<double> call, double value)
        {
            doubleAction = call;
            doubleValue = value;
            paramType = 3;
        }
        public SplineAction(UnityAction<string> call, string value)
        {
            stringAction = call;
            stringValue = value;
            paramType = 4;
        }
        public SplineAction(UnityAction<bool> call, bool value)
        {
            boolAction = call;
            boolValue = value;
            paramType = 5;
        }
        public SplineAction(UnityAction<GameObject> call, GameObject value)
        {
            goAction = call;
            goValue = value;
            paramType = 6;
        }
        public SplineAction(UnityAction<Transform> call, Transform value)
        {
            transformAction = call;
            transformValue = value;
            paramType = 7;
        }

        public void SetMethod(MethodInfo newMethod)
        {

            ParameterInfo[] parameters = newMethod.GetParameters();
            if(parameters.Length > 1)
            {
                Debug.LogError("Cannot add method with more than one argument");
                return;
            }
            methodInfo = newMethod;
            methodName = methodInfo.Name;
            if (parameters.Length == 0) paramType = 0;
            else
            {
                if (parameters[0].ParameterType == typeof(int)) paramType = 1;
                else if (parameters[0].ParameterType == typeof(float)) paramType = 2;
                else if (parameters[0].ParameterType == typeof(double)) paramType = 3;
                else if (parameters[0].ParameterType == typeof(string)) paramType = 4;
                else if (parameters[0].ParameterType == typeof(bool)) paramType = 5;
                else if (parameters[0].ParameterType == typeof(GameObject)) paramType = 6;
                else if (parameters[0].ParameterType == typeof(Transform)) paramType = 7;
            }
            ConstructUnityAction();
        }

        private System.Type GetParamType()
        {
            switch (paramType)
            {
                case 0: return null;
                case 1: return typeof(int);
                case 2: return typeof(float);
                case 3: return typeof(double);
                case 4: return typeof(string);
                case 5: return typeof(bool);
                case 6: return typeof(GameObject);
                case 7: return typeof(Transform);
            }
            return null;
        }

        public MethodInfo GetMethod()
        {
            if(methodInfo == null && target != null && methodName != "")
            {
                System.Type t = GetParamType();
                if (t == null) methodInfo = target.GetType().GetMethod(methodName, new System.Type[] { });
                else methodInfo = target.GetType().GetMethod(methodName, new System.Type[] { t });
            }
            return methodInfo;
        }

        private void ConstructUnityAction()
        {
#if !UNITY_WSA
            action = null;
            intAction = null;
            floatAction = null;
            doubleAction = null;
            stringAction = null;
            boolAction = null;
            goAction = null;
            transformAction = null;
            methodInfo = GetMethod();
            
            switch (paramType)
            {
                case 0: action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), target, methodInfo); break;
                case 1: intAction = (UnityAction<int>)System.Delegate.CreateDelegate(typeof(UnityAction<int>), target, methodInfo); break;
                case 2: floatAction = (UnityAction<float>)System.Delegate.CreateDelegate(typeof(UnityAction<float>), target, methodInfo); break;
                case 3: doubleAction = (UnityAction<double>)System.Delegate.CreateDelegate(typeof(UnityAction<double>), target, methodInfo); break;
                case 4: stringAction = (UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, methodInfo); break;
                case 5: boolAction = (UnityAction<bool>)System.Delegate.CreateDelegate(typeof(UnityAction<bool>), target, methodInfo); break;
                case 6: goAction = (UnityAction<GameObject>)System.Delegate.CreateDelegate(typeof(UnityAction<GameObject>), target, methodInfo); break;
                case 7: transformAction = (UnityAction<Transform>)System.Delegate.CreateDelegate(typeof(UnityAction<Transform>), target, methodInfo); break;
            }
#else
            throw new System.Exception("ConstructUnityAction is not available for WindowsStoreApplications. Call could not be invoked.");
#endif
        }

        public void Invoke()
        {
            switch (paramType)
            {
                case 0: if (action == null) ConstructUnityAction(); action.Invoke(); break;
                case 1: if (intAction == null) ConstructUnityAction();  intAction.Invoke(intValue); break;
                case 2: if (floatAction == null) ConstructUnityAction(); floatAction.Invoke(floatValue); break;
                case 3: if (doubleAction == null) ConstructUnityAction(); doubleAction.Invoke(doubleValue); break;
                case 4: if (stringAction == null) ConstructUnityAction(); stringAction.Invoke(stringValue); break;
                case 5: if (boolAction == null) ConstructUnityAction(); boolAction.Invoke(boolValue); break;
                case 6: if (goAction == null) ConstructUnityAction(); goAction.Invoke(goValue); break;
                case 7: if (transformAction == null) ConstructUnityAction(); transformAction.Invoke(transformValue); break;
            }
        }

    }

    [System.Serializable]
    public class SplineTrigger : ScriptableObject
    {
        public enum Type { Double, Forward, Backward }
        [SerializeField]
        public Type type = Type.Double;
        [Range(0f, 1f)]
        public double position = 0.5;
        [SerializeField]
        public bool enabled = true;
        [SerializeField]
        public Color color = Color.white;
        [SerializeField]
        [HideInInspector]
        public SplineAction[] actions = new SplineAction[0];
        public GameObject[] gameObjects;
    }

    [System.Serializable]
    public class Trigger
    {
        public string name = "Trigger";
        public enum Type { Double, Forward, Backward}
        [SerializeField]
        public Type type = Type.Double;
        public bool workOnce = false;
        private bool worked = false;
        [Range(0f, 1f)]
        public double position = 0.5;
        [SerializeField]
        public bool enabled = true;
        [SerializeField]
        public Color color = Color.white;
        [SerializeField]
        [HideInInspector]
        public SplineAction[] actions = new SplineAction[0];
        public GameObject[] gameObjects;

        public void Create(Type t, UnityAction call)
        {
            Create(t);
            AddAction(new SplineAction(call));
        }
        public void Create(Type t, UnityAction<int> call, int value)
        {
            AddAction(new SplineAction(call, value));
        }

        public void Create(Type t, UnityAction<float> call, float value)
        {
            AddAction(new SplineAction(call, value));
        }

        public void Create(Type t, UnityAction<double> call, double value)
        {
            AddAction(new SplineAction(call, value));
        }

        public void Create(Type t, UnityAction<string> call, string value)
        {
            AddAction(new SplineAction(call, value));
        }

        public void Create(Type t, UnityAction<bool> call, bool value)
        {
            AddAction(new SplineAction(call, value));
        }

        public void Create(Type t, UnityAction<Transform> call, Transform value)
        {
            AddAction(new SplineAction(call, value));
        }

        public void Create(Type t, UnityAction<GameObject> call, GameObject value)
        {
            AddAction(new SplineAction(call, value));
        }

        public void Create(Type t)
        {
            type = t;
            switch (t)
            {
                case Type.Double: color = Color.yellow; break;
                case Type.Forward: color = Color.green; break;
                case Type.Backward: color = Color.red; break;
            }
            enabled = true;
        }

        public void ResetWorkOnce()
        {
            worked = false;
        }

        public bool Check(double previousPercent, double currentPercent)
        {
            if (!enabled) return false;
            if (workOnce && worked) return false;
            bool passed = false;
            switch (type)
            {
                case Type.Double: passed = (previousPercent <= position && currentPercent >= position) || (currentPercent <= position && previousPercent >= position); break;
                case Type.Forward: passed = previousPercent <= position && currentPercent >= position; break;
                case Type.Backward: passed = currentPercent <= position && previousPercent >= position; break;
            }
            if (passed) worked = true;
            return passed;
        }

        public void Invoke()
        {
            for (int i = 0; i < actions.Length; i++) actions[i].Invoke();
        }

        private void AddAction()
        {
            SplineAction[] newActions = new SplineAction[actions.Length + 1];
            actions.CopyTo(newActions, 0);
            newActions[newActions.Length - 1] = new SplineAction();
            actions = newActions;
        }

        
        public void AddListener(MonoBehaviour behavior, string method, object arg)
        {
            System.Type t = null;
            if (arg.GetType() == typeof(int)) t = typeof(int);
            else if (arg.GetType() == typeof(float)) t = typeof(float);
            else if (arg.GetType() == typeof(double)) t = typeof(double);
            else if (arg.GetType() == typeof(string)) t = typeof(string);
            else if (arg.GetType() == typeof(bool)) t = typeof(bool);
            else if (arg.GetType() == typeof(GameObject)) t = typeof(GameObject);
            else if (arg.GetType() == typeof(Transform)) t = typeof(Transform);
            MethodInfo mi;
            if(t == null) mi = behavior.GetType().GetMethod(method, new System.Type[0]);
            else mi = behavior.GetType().GetMethod(method, new System.Type[]{t});
            if (mi == null)
            {
                Debug.LogError("There is no overload of the method " + method + " that uses a " + t + " parameter");
                return;
            }
            AddAction();
            actions[actions.Length - 1].target = behavior;
            actions[actions.Length - 1].SetMethod(mi);
            ParameterInfo[] parameters = mi.GetParameters();
            if(parameters.Length == 1)
            {
                if (parameters[0].ParameterType == typeof(int)) actions[actions.Length - 1].intValue = (int)arg;
                else if (parameters[0].ParameterType == typeof(float)) actions[actions.Length - 1].floatValue = (float)arg;
                else if (parameters[0].ParameterType == typeof(double)) actions[actions.Length - 1].doubleValue = (double)arg;
                else if (parameters[0].ParameterType == typeof(string)) actions[actions.Length - 1].stringValue = (string)arg;
                else if (parameters[0].ParameterType == typeof(bool)) actions[actions.Length - 1].boolValue = (bool)arg;
                else if (parameters[0].ParameterType == typeof(GameObject)) actions[actions.Length - 1].goValue = (GameObject)arg;
                else if (parameters[0].ParameterType == typeof(Transform)) actions[actions.Length - 1].transformValue = (Transform)arg;
            }
        }

        public void AddAction(SplineAction action)
        {
            AddAction();
            actions[actions.Length - 1] = action;
        }
    }
}
