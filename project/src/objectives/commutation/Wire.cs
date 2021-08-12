using Godot;
using System;

namespace Game
{
    public class Wire : Spatial
    {
        [Export]
        public NodePath MultiMeshInstancePath;
        MultiMeshInstance _multiMeshInstance;
        MultiMesh _multiMesh;

        [Export]
        public NodePath TransSpatialPath;
        Spatial _trasnSpatial;

        [Export]
        public Mesh WireMesh;
        [Export]
        public Material WireMaterial;

        public Spatial ConnectedInput;


        Vector3 _startPoint;
        Vector3 _nextPosition;
        public void SetStartPoint(Vector3 startPoint) { _startPoint = startPoint; }
        Vector3 _endPoint;
        public void SetEndPoint(Vector3 endPoint) { _endPoint = endPoint; }

        int _partsCount = 0;

        public bool IsFolded = true;
        public bool IsFolding = true;

        public string Id = "";
        public string GetId() => Id;

        public delegate void ConnectionHandler(Spatial wireInputNode);
        public event ConnectionHandler OnConnected;

        public override void _Ready()
        {
            _trasnSpatial = GetNode<Spatial>(TransSpatialPath);
            _multiMeshInstance = GetNode<MultiMeshInstance>(MultiMeshInstancePath);
            _multiMeshInstance.MaterialOverride = WireMaterial;
            UpdateGlobalTransform();

            _multiMesh = new MultiMesh();
            _multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
            _multiMesh.Mesh = WireMesh;
            _multiMesh.InstanceCount = 512;
            _multiMesh.VisibleInstanceCount = 0;
            _multiMeshInstance.Multimesh = _multiMesh;

            AddPart();
        }

        void UpdateGlobalTransform()
        {
            _startPoint = GlobalTransform.origin;
            _endPoint = GlobalTransform.origin + Vector3.Forward;
            _multiMeshInstance.GlobalTransform = new Transform(Basis.Identity, Vector3.Zero);
        }

        void UpdateUnfoldedState(float delta)
        {
            if (_partsCount > 0 && _partsCount <= _multiMesh.InstanceCount)
            {
                Vector3 partPosition;
                if (_partsCount == 1)
                {
                    partPosition = _startPoint;
                }
                else
                {
                    IsFolded = false;
                    partPosition = _multiMesh.GetInstanceTransform(_partsCount - 2).origin - _multiMesh.GetInstanceTransform(_partsCount - 2).basis.z;
                }

                Vector3 nextPartPosition = _endPoint;
                Godot.Collections.Dictionary collision = GetWorld().DirectSpaceState.IntersectRay(partPosition, nextPartPosition, null, 2);
                bool bending = false;
                if (collision.Count > 0)
                {
                    Vector3 pos = (Vector3)collision["position"] + (Vector3)collision["normal"] * 0.6f;
                    if (partPosition.DistanceTo(pos) > 1.0f)
                    {
                        bending = true;
                        nextPartPosition = pos;
                    }
                }

                _trasnSpatial.GetChild<Spatial>(0).Transform = new Transform(Basis.Identity, Vector3.Zero).Scaled(new Vector3(1.0f, 1.0f, partPosition.DistanceTo(nextPartPosition)));
                _trasnSpatial.GlobalTransform = new Transform(Basis.Identity, partPosition).LookingAt(nextPartPosition, Vector3.Up);
                _nextPosition = nextPartPosition;
                Transform partTransform = _trasnSpatial.GetChild<Spatial>(0).GlobalTransform;
                _multiMesh.SetInstanceTransform(_partsCount - 1, partTransform);
                if (bending)
                {
                    AddPart();
                }
            }
        }
        void UpdateFoldedState(float delta)
        {
            if (_partsCount >= 1)
            {
                Transform partTransform = _multiMesh.GetInstanceTransform(_partsCount - 1);
                _trasnSpatial.GlobalTransform = new Transform(Basis.Identity, partTransform.origin).LookingAt(partTransform.origin - partTransform.basis.z, Vector3.Up);
                float length = partTransform.basis.z.Length();
                length = Mathf.Max(0.0f, length - 0.5f);
                _trasnSpatial.GetChild<Spatial>(0).Transform = new Transform(Basis.Identity, Vector3.Zero).Scaled(new Vector3(1.0f, 1.0f, length));
                partTransform = _trasnSpatial.GetChild<Spatial>(0).GlobalTransform;
                _multiMesh.SetInstanceTransform(_partsCount - 1, partTransform);
                if (_partsCount > 1 && length == 0.0f)
                {
                    PopPart();
                }
                if (_partsCount == 1 && length == 0.0f)
                {
                    IsFolded = true;
                }
            }
        }

        public override void _Process(float delta)
        {
            if(IsFolding)
            {
                UpdateFoldedState(delta);
            }
            else
            {
                UpdateUnfoldedState(delta);
            }
        }

        public void Fold()
        {
            IsFolding = true;
        }
        public void Unfold()
        {
            if (IsFolded)
            {
                UpdateGlobalTransform();
                IsFolding = false;
            }
        }

        public void Connect(Spatial input)
        {
            ConnectedInput = input;
            if (OnConnected != null) OnConnected.Invoke(input);
        }
        public void Disconnect()
        {
            Fold();
            ConnectedInput = null;
        }


        void AddPart() {
            _partsCount += 1;
            _multiMesh.VisibleInstanceCount += 1;
        }
        void PopPart()
        {
            _partsCount -= 1;
            _multiMesh.VisibleInstanceCount -= 1;
        }
    }

}
