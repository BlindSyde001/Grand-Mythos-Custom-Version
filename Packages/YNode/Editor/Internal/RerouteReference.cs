using UnityEngine;

namespace YNode.Editor.Internal
{
    public struct ReroutePoint
    {
        public Port? Port;
        public int PointIndex;

        public ReroutePoint(Port? port, int pointIndex)
        {
            this.Port = port;
            this.PointIndex = pointIndex;
        }

        public void InsertPoint(Vector2 pos)
        {
            Port!.GetReroutePoints().Insert(PointIndex, pos);
        }

        public void SetPoint(Vector2 pos)
        {
            Port!.GetReroutePoints()[PointIndex] = pos;
        }

        public void RemovePoint()
        {
            Port!.GetReroutePoints().RemoveAt(PointIndex);
        }

        public Vector2 GetPoint()
        {
            return Port!.GetReroutePoints()[PointIndex];
        }
    }
}