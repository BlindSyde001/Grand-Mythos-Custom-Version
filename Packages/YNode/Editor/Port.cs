using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace YNode.Editor
{
    public delegate INodeValue? GetConnected();

    public delegate bool CanConnectTo(Type type);

    public delegate void SetConnection(INodeValue? value);

    public class Port
    {
        private readonly CanConnectTo _canConnectTo;

        private readonly GetConnected _getConnected;

        private readonly SetConnection _setConnection;

        public IO Direction { get; }
        public string FieldName { get; }
        public NodeEditor NodeEditor { get; }
        public Type ValueType { get; }
        public string Tooltip { get; set; }
        public NoodleStroke Stroke { get; set; }
        public Rect CachedRect { get; set; }
        public float CachedHeight { get; set; }
        public int Drawn { get; set; }

        /// <summary> Return the first non-null connection </summary>
        public NodeEditor? Connection
        {
            get
            {
                INodeValue? value = _getConnected();
                return value != null ? NodeEditor.Window.NodesToEditor[value] : null;
            }
        }

        /// <summary> Construct a dynamic port. Dynamic ports are not forgotten on reimport, and is ideal for runtime-created ports. </summary>
        public Port(string fieldName, NodeEditor nodeEditorParam, Type type, IO direction, GetConnected getConnected, CanConnectTo canConnectTo, SetConnection setConnection, NoodleStroke stroke, string? tooltip = null)
        {
            FieldName = fieldName;
            ValueType = type;
            Direction = direction;
            NodeEditor = nodeEditorParam;
            _getConnected = getConnected;
            _canConnectTo = canConnectTo;
            _setConnection = setConnection;
            Stroke = stroke;
            Tooltip = tooltip ?? ValueType.Name;
        }

        /// <summary> Connect this <see cref="Port" /> to another </summary>
        public void Connect(NodeEditor newConnection)
        {
            NodeEditor? connected = Connection;
            if (connected == newConnection)
            {
                Debug.LogWarning("Port already connected. ");
                return;
            }
#if UNITY_EDITOR
            Undo.RecordObjects(new[] { NodeEditor, newConnection }, "Connect Port");
#endif
            _setConnection(newConnection.Value);
        }

        public bool CanConnectTo(Type type) => _canConnectTo(type);

        /// <summary> Disconnect this port from another port </summary>
        public void Disconnect() => _setConnection(null);

        /// <summary> Get reroute points for a given connection. This is used for organization </summary>
        public List<Vector2> GetReroutePoints()
        {
            if (NodeEditor.ReroutePoints.TryGetValue(FieldName, out var points) == false)
                NodeEditor.ReroutePoints[FieldName] = points = new List<Vector2>();
            return points;
        }

        public bool TryGetReroutePoints([MaybeNullWhen(false)] out List<Vector2> points)
        {
            return NodeEditor.ReroutePoints.TryGetValue(FieldName, out points);
        }

        public void ClearReroute()
        {
            NodeEditor.ReroutePoints.Remove(FieldName);
        }
    }
}
