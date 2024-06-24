using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    using FlowchartType = FlowExecute.FlowchartType;

    [CustomPropertyDrawer(typeof(FlowExecute))]
    public class FlowExecuteDrawer : CommandBaseDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += GetHeight(10);

            var flowchartTypeProp = DrawField(ref position, property, "flowchartType");
            var flowchartType = (FlowchartType)flowchartTypeProp.enumValueIndex;

            if (flowchartType == FlowchartType.Executor)
            {
                DrawField(ref position, property, "flowchartExecutor");
            }
            else if(flowchartType == FlowchartType.Data)
            {
                DrawField(ref position, property, "flowchartData");
            }

            DrawField(ref position, property, "commandIndex");
            DrawField(ref position, property, "isAwaitNest");
        }
    }
}