#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Dreamteck.Splines
{


    public class PointSelector : GUIObject
    {
        public delegate void SelectionHandler(List<int> selection);
        public event SelectionHandler onSelect;

        public List<int> selection = new List<int>();
        public SplineComputer computer = null;
        public bool allowMultipleSelection = false;
        private int lastSelection = -1;
        private int singleSelection = -1;

        public PointSelector(string t, SplineComputer comp, bool mutlple, SelectionHandler selectHandler) : base(t)
        {
            computer = comp;
            allowMultipleSelection = mutlple;
            if (selectHandler != null) onSelect += selectHandler;
        }

        public override void Draw()
        {
            base.Draw();
            string[] options = new string[(computer.isClosed ? computer.pointCount - 1 : computer.pointCount) + 1];
            for (int i = 0; i < options.Length - 1; i++)
            {
                options[i + 1] = "Point " + (i+1);
                if (computer.type == Spline.Type.Bezier) options[i + 1] = "Point " + i + " Bezier " + (computer.GetPoint(i, SplineComputer.Space.Local).type == SplinePoint.Type.SmoothMirrored ? "(smooth)" : "(broken)");
            }
            options[0] = "- Select -";
            BeginPanel();
            if(selection.Count == 1) lastSelection = singleSelection = selection[0];
            else if(selection.Count > 1) lastSelection = singleSelection = -1;
            singleSelection = EditorGUILayout.Popup(GetTitle(), singleSelection +1, options) - 1;
            if (lastSelection != singleSelection) Select();

            switch(ButtonGroup(new string[] { "Select All", "Deselect All", "Select Inverse"}, 30, true))
            {
                case 0:
                    selection = new List<int>();
                    singleSelection = lastSelection = -1;
                    for (int i = 0; i < computer.pointCount; i++)
                    {
                        if (i == computer.pointCount - 1 && computer.isClosed) break;
                        selection.Add(i);
                    }
                    Select();
                    break;
                case 1:
                    selection = new List<int>();
                    singleSelection = lastSelection = -1;
                    Select();
                    break;
                case 2:
                    singleSelection = lastSelection = -1;
                    List<int> inverse = new List<int>();
                    for (int i = 0; i < (computer.isClosed ? computer.pointCount - 1 : computer.pointCount); i++)
                    {
                        bool found = false;
                        for(int j = 0; j < selection.Count; j++)
                        {
                            if(selection[j] == i)
                            {
                                found = true;
                                break;
                            }
                        }
                        if(!found) inverse.Add(i);
                    }
                    selection = new List<int>(inverse);
                    Select();
                    break;
            }

            EndPanel();
        }

        private void Select()
        {
            lastSelection = singleSelection;
            if(singleSelection >= 0) selection = new List<int>(new int[]{singleSelection});
            if (onSelect != null) onSelect(selection);
        }
    }
}

#endif