using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class ObjectPlacer : Spatial
    {
        public class ObjectToPlaceAgent
        {
            public Spatial obj;
            public uint collisionLayer;
            public bool isPersist;
            public ObjectToPlaceAgent(Spatial objectToPlace, Material objectPreviewMaterial) {
                obj = objectToPlace;

                foreach (MeshInstance mesh in obj.GetAllChildren<MeshInstance>())
                {
                    mesh.MaterialOverride = objectPreviewMaterial;
                }
                foreach (CollisionShape collisionShape in obj.GetAllChildren<CollisionShape>())
                {
                    collisionShape.Disabled = true;
                }


                //if (obj.Get("collision_layer")!=null)
                //{
                //    collisionLayer = Convert.ToUInt32(obj.Get("collision_layer"));
                //    obj.Set("collision_layer", (uint)0);
                //    obj.Set("collision_mask", (uint)0);
                //}


                obj.Set("frozen", true);
            }
            public void Disable()
            {
                obj.Set("frozen", true);

                Godot.Collections.Array<Light> lights = obj.GetAllChildren<Light>();
                foreach (Light light in lights) { light.Visible = false; }

                if (obj.IsInGroup("Persist"))
                {
                    isPersist = true; obj.RemoveFromGroup("Persist");
                }

            }
            public void Enable()
            {
                obj.Set("frozen", false);
                foreach (MeshInstance mesh in obj.GetAllChildren<MeshInstance>())
                {
                    mesh.MaterialOverride = null;
                }
                //if (obj.Get("collision_layer") != null)
                //{
                //    obj.Set("collision_layer", (uint)collisionLayer);
                //    obj.Set("collision_mask", (uint)collisionLayer);
                //}
                foreach (CollisionShape collisionShape in obj.GetAllChildren<CollisionShape>())
                {
                    collisionShape.Disabled = false;
                }
                Godot.Collections.Array<Light> lights = obj.GetAllChildren<Light>();
                foreach (Light light in lights) { light.Visible = true; }

                if (isPersist)
                {
                    obj.AddToGroup("Persist");
                }
            }
        }

        [Export]
        public NodePath PlayerNodePath;
        private Player _player;
        [Export]
        public Material ObjectPreviewMaterial;

        private Spatial _objectToPlace = null;
        private ObjectToPlaceAgent _objectAgent;
        private Inventory.Item _item;
        private bool _canPlace = true;
        private Spatial _rayCollidingBody;

        PlaceRegion _placeRegion;

        public override void _Ready()
        {
            _player = GetNode<Player>(PlayerNodePath);
            Global.Instance.GetInventory().OnActiveItemChange += OnActiveItemChange;
            Global.Instance.GetInventory().OnItemsArrayChanged+= OnActiveItemChange;
        }
        public override void _Process(float delta)
        {
            if (_player.Disabled) return;
            if (_player.GetCamera().IsCutscenePlaying()) return;
            UpdateObjectPosition();

            if (Input.IsActionJustPressed("fire") && _objectToPlace != null && Global.Instance.GetUIManager().GetCurrentOverlay() == UI.OverlayType.NONE)
            {
                PlaceObject();
            }
        }

        public void OnActiveItemChange()
        {
            Inventory.Item item = Global.Instance.GetInventory().GetActiveItem();
            if (item != null)
            {
                if (item.GetPropertyValue<bool>("placeable"))
                {
                    PackedScene sceneToPlace = item.GetPropertyValue<PackedScene>("scene_to_place");
                    _item = item;
                    if (sceneToPlace!=null)
                    {
                        SetPreviewObject(sceneToPlace);
                    }
                    else
                    {
                        SetPreviewObject(item.PickableItem);
                    }
                    return;
                }
            }
            RemovePreviewObject();
        }

        public Godot.Collections.Dictionary CastRay(uint collisionMask = 2147483647, bool areas=false)
        {
            return GetWorld().DirectSpaceState.IntersectRay(
                    _player.GetCamera().GlobalTransform.origin,
                    _player.GetCamera().GlobalTransform.origin - _player.GetCamera().GlobalTransform.basis.z * _player.UseDistance - _player.GetCamera().GlobalTransform.basis.y * 2.0f,
                new Godot.Collections.Array { _player, _objectToPlace }, collisionMask, !areas, areas);
        }

        bool FindAndFitPlaceRegion()
        {
            //TODO: fucking logic
            if (_item.Properties.ContainsKey("placeable_group"))
            {
                Godot.Collections.Dictionary placeRegionCollision = CastRay(4, true);
                if (placeRegionCollision.Count > 0)
                {
                    if (placeRegionCollision["collider"] is PlaceRegion)
                    {
                        _placeRegion = (PlaceRegion)placeRegionCollision["collider"];
                        bool canPlace = false;
                        if((string)_item.Properties["placeable_group"] == _placeRegion.PlaceableGroup)
                        {
                            canPlace = _placeRegion.UpdateObjectPlacement(_objectToPlace);
                        }
                        if (canPlace)
                        {
                            SetSpawnAbility(true);
                            return true;
                        }
                        else
                        {
                            _placeRegion = null;
                        }
                    }
                }
            }
            return false;
        }
        bool FindAndFitSurface()
        {
            Godot.Collections.Dictionary collision = CastRay(1);
            if (collision.Count > 0)
            {
                if (!(collision["collider"] is Spatial)) return false;
                _rayCollidingBody = ((Spatial)collision["collider"]);
                Vector3 normal = ((Vector3)collision["normal"]);

                _objectToPlace.SetRotationAlongNormal(
                    collision.Get<Vector3>("position"), 
                    normal, 
                    _player.GetCamera().GlobalTransform.basis.z);

                SetSpawnAbility(true);
                return true;
            }
            return false;
        }
        void UpdateObjectPosition()
        {
            if (_objectToPlace != null)
            {
                if (FindAndFitPlaceRegion()) return;
                if (FindAndFitSurface()) return;
                SetSpawnAbility(false);
            }
        }
        public override void _Input(InputEvent e)
        {
            
        }
        public void SetSpawnAbility(bool canSpawn)
        {
            _canPlace = canSpawn;
            if (_objectToPlace != null)
            {
                _objectToPlace.Visible = canSpawn;
            }
        }
        public void SetPreviewObject(PackedScene objScene)
        {
            if (_objectToPlace != null)
            {
                RemovePreviewObject();
            }
            _objectToPlace = (Spatial)objScene.Instance();
            AddChild(_objectToPlace);
            _objectAgent = new ObjectToPlaceAgent(_objectToPlace, ObjectPreviewMaterial);
            _objectAgent.Disable();
        }
        public void RemovePreviewObject()
        {
            if (_objectToPlace != null)
            {
                if (_objectAgent != null) _objectAgent.Disable();
                _objectToPlace.QueueFree();
            }
            _objectToPlace = null;
            _objectAgent = null;
            _placeRegion = null;
        }
        public void PlaceObject()
        {
            if (_objectToPlace != null && _canPlace)
            {
                //TODO: lol. validTransaction
                bool validTransaction = Global.Instance.GetInventory().HasAmountOfItem(_item.ID, 1);
                if (validTransaction)
                {
                    _objectAgent.Enable();

                    if (_placeRegion != null) _placeRegion.CompleteObjectPlacement(_objectToPlace);
                    else _objectToPlace.Reparent(_rayCollidingBody);
                    if (_objectToPlace.HasMethod("_on_placed")) _objectToPlace.Call("_on_placed");

                    if(_objectToPlace is IContainingItem)
                    {
                        ((IContainingItem)_objectToPlace).SetItem(_item);
                    }

                    if (Global.Instance.GetGenerationManager() != null)
                        Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition(
                        Global.Instance.GetGenerationManager().GetSoundIdFromBank("place_object"), _objectToPlace.GlobalTransform.origin, _objectToPlace);

                    _objectToPlace = null;
                    _objectAgent = null;
                    _placeRegion = null;
                    Global.Instance.GetInventory().TakeItem(_item.ID, 1);
                }
            }
        }
    }
}
