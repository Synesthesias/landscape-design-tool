using UnityEngine.UIElements;

namespace Landscape2.Runtime.GisDataLoader
{
    public class GisPointPin
    {
        // ピンのIndex
        public int PointID { get; private set; }

        // 属性のindex
        public int AttributeIndex { get; private set; }
        
        // ピンの要素
        public VisualElement Element { get; private set; }
        
        public GisPointPin(int pointID, int attributeIndex, VisualElement element)
        {
            PointID = pointID;
            AttributeIndex = attributeIndex;
            Element = element;
        }
    }
}