using Godot;
using Game.Utils;
using System;

namespace Game{
    public class Player : CharacterController, IEntity
    {
        // components node paths
        [Export]
        public NodePath CameraNodePath;
        [Export]
        public NodePath AreaNodePath;
        [Export]
        public NodePath HandPointNodePath;
        [Export]
        public NodePath LightPath;

        [Export]
        public float UseDistance = 8.0f;

        [Export]
        public PackedScene DeadBodyScene;

        // components instances
        private PlayerCamera _camera;
        public PlayerCamera GetCamera() { return _camera; }
        private PlayerArea _area;
        public PlayerArea GetArea() { return _area; }

        // fields
        private Spatial _handPoint;
        private Weapon _currentWeapon;
        public Weapon GetCurrentWeapon() { return _currentWeapon; }

        private IUsable _objectToUse;
        [Signal]
        public delegate void ObjectToUseChangedHandler(Spatial objectToUse);
        public event ObjectToUseChangedHandler OnObjectToUseChanged;

        [Signal]
        public delegate void OnHealthChangeHandler(float healthChange);
        public event OnHealthChangeHandler OnHealthChangeEvent;

        [Export]
        public float MaxHealth { get; set; }
        public float Health { get; set; }
        public bool Damageable { get; set; }

        public float Steps = 0.0f;
        public float StepsTimer = 1.0f;

        public float JumpStrength = 20.0f;

        FastNoiseLite noise = new FastNoiseLite();
        float _crazyTimer;
        bool _isCrazy = false;

        Vector3 defaultHandPos;

        bool _controlling;
        public bool IsControlling() => _controlling;

        public Player()
        {
            Damageable = true;
            SetMaxHealth(100.0f);

            SetupNoise();
        }
        void SetupNoise()
        {
            noise.Period = 0.6f;
        }
        public void Disable()
        {
            Disabled = true;
            _camera.SetActiveState(false);
            Input.SetMouseMode(Input.MouseMode.Captured);
            SetWeapon(null, null);
        }
        public void Immobilize()
        {
            Disabled = true;
            _camera.SetActiveState(false);
            Input.SetMouseMode(Input.MouseMode.Captured);
            SetWeapon(null, null);
        }
        public void Stun(Spatial lookAtObject)
        {
            Disable();
            _camera.StunLookAtObject = lookAtObject;
        }
        public void Enable()
        {
            if (Disabled && !_camera.IsCutscenePlaying())
            {
                Disabled = false;
                _camera.SetActiveState(true);
            }
        }
        public void MakeCrazy()
        {
            _isCrazy = true;
        }
        public void MakeNormal()
        {
            _isCrazy = false;
        }


        // methods
        public override void _Ready()
        {
            

            _camera = GetNode<PlayerCamera>(CameraNodePath);
            _area = GetNode<PlayerArea>(AreaNodePath);
            _handPoint = GetNode<Spatial>(HandPointNodePath);
            _handPoint.Transform = _handPoint.Transform.Scaled(Vector3.One+_handPoint.GetParent<Spatial>().Transform.basis.Scale);
            defaultHandPos = _handPoint.GetParent<Spatial>().Transform.origin;

            Global.Instance.GetInventory().OnActiveItemChange += OnActiveInventoryItemChange;
            OnGroundedEvent += OnGrounded;

            SetHealth(MaxHealth);
            Global.Instance.RegisterPlayer(this);

            if (Global.Instance.DEBUG)
            {
                MaxSpeed *= 1.6f;
                GetNode<OmniLight>("Light").OmniRange = 100.0f;
                CanFly = true;
                JumpStrength *= 1.5f;
            }
        }
        public override void _Process(float delta)
        {
            _controlling = false;
            if (this.IsAlive() && !Disabled)
            {
                UpdateObjectToUse();
                ProcessControls(delta);
                FitWeaponCameraDirection();
                UpdateSteps(delta);
                if(_isCrazy) UpdateCrazyState(delta);
            }
            base._Process(delta);
        }
        void UpdateCrazyState(float delta)
        {
            _crazyTimer += delta;
            if (!IsControlling())
            {
                Move(new Vector3(noise.GetNoise2d(_crazyTimer, -100.0f), 0.0f, noise.GetNoise2d(_crazyTimer, 100.0f)).Normalized());
            }
        }
        void UpdateSteps(float delta)
        {
            if (IsGrounded())
            {
                float step = new Vector2(Velocity.x, Velocity.z).Length() * 1.6f * delta;
                Steps += step;
                StepsTimer -= step/Mathf.Pi/2.0f;
                if (StepsTimer <= 0.0f)
                {
                    PlayStepSound();
                    StepsTimer = 1.0f;
                }
            }
        }
        void PlayStepSound()
        {
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition(
                Global.Instance.GetGenerationManager().GetSoundIdFromBank("player_footsteps"),
                GlobalTransform.origin,
                this
            );
        }
        void PlayLandingSound()
        {
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition(
                Global.Instance.GetGenerationManager().GetSoundIdFromBank("player_landing"),
                GlobalTransform.origin,
                this
            );
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
        }

        public void OnGrounded(Godot.Collections.Dictionary collision)
        {
            if (collision["collider"] is IUsable)
            {
                IUsable usable = (IUsable)collision["collider"];
                _objectToUse = usable;
                if(!(_objectToUse is Npc) && !(_objectToUse is EnemyCorpse))
                {
                    TryToUseObject();
                }
            }
            if (PreventVelocityY <= -40.0f)
            {
                float damage = Mathf.Max(0.0f, (-PreventVelocityY - 30.0f) / 1.1f);
                TakeDamage(damage);
            }
            PlayLandingSound();
        }
        /// <summary>
        /// Check user input and control movement, use, firing weapon
        /// </summary>
        void ProcessControls(float delta)
        {
            if (Input.IsActionPressed("move_forward"))
            {
                Move(-GlobalTransform.basis.z);
                _controlling = true;
            }
            if (Input.IsActionPressed("move_backward"))
            {
                Move(GlobalTransform.basis.z);
                _controlling = true;
            }
            if (Input.IsActionPressed("move_left"))
            {
                Move(-GlobalTransform.basis.x);
                _controlling = true;
            }
            if (Input.IsActionPressed("move_right"))
            {
                Move(GlobalTransform.basis.x);
                _controlling = true;
            }

            if (Input.IsActionPressed("slow_walk"))
            {
                SpeedFactor = 0.5f;
            }
            else if (Input.IsActionPressed("run"))
            {
                SpeedFactor = 1.4f;
            }
            else
            {
                SpeedFactor = 1.0f;
            }

            if (Input.IsActionJustPressed("jump") && ((!CanFly && (IsOnFloor() || IsGrounded())) || CanFly))
            {
                Jump(JumpStrength);
            }


            // for not being able to use this controls when some overlay is active
            if (Global.Instance.GetUIManager().GetCurrentOverlay() != UI.OverlayType.NONE) return;
            

            if (Input.IsActionJustPressed("use"))
            {
                TryToUseObject();
            }

            if (Input.IsActionJustPressed("drop_item"))
            {
                DropItem(Global.Instance.GetInventory().GetActiveItem());
            }

            if (Input.IsActionJustPressed("fire"))
            {
                TryUseActiveItem();
                if (_currentWeapon != null) _currentWeapon.Fire();
            }
            if (Input.IsActionJustReleased("fire"))
            {
                if (_currentWeapon != null) _currentWeapon.StopFiring();
            }

            UpdateAiming();

        }

        void UpdateAiming()
        {
            Transform trans = _handPoint.GetParent<Spatial>().Transform;
            if (Input.IsActionPressed("aim"))
            {
                trans.origin += (new Vector3 (0.0f, -0.5f, 0.0f) - trans.origin) / 2.0f;
            }
            else
            {
                trans.origin += (defaultHandPos - trans.origin) / 2.0f;
            }
            _handPoint.GetParent<Spatial>().Transform = trans;
        }

        //item use
        void TryUseActiveItem()
        {
            Inventory.Item item = Global.Instance.GetInventory().GetActiveItem();
            if (item != null) {
                //TODO: check other places where GetPropertyValue can be used, I forgot about it
                if (item.GetPropertyValue<bool>("usable"))
                {
                    //TODO: this is temporary. when you return from your trip, immediatly make effects and make heal effect instead of this
                    //UPD: now its not necessary, game has no complex effects. later, in another project
                    float heal = item.GetPropertyValue<float>("heal");
                    if (heal>0.0f)
                    {
                        Global.Instance.GetAudioManager().PlayNonSpatialSoundFromBank("human_eating");
                        SetHealth(GetHealth()+heal);
                        Global.Instance.GetInventory().TakeItem(item.ID, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Finds new object to use and invokes OnObjectToUseChanged event.
        /// </summary>
        void UpdateObjectToUse()
        {
            IUsable newObjectToUse = FindObjectToUse();
            _objectToUse = newObjectToUse;
            OnObjectToUseChanged.Invoke((Spatial)_objectToUse);
        }
        /// <summary>
        /// Finds object under camera ray or nearest object that can be used
        /// </summary>
        /// <returns></returns>
        IUsable FindObjectToUse()
        {
            Godot.Collections.Dictionary collision = _camera.CastRay(UseDistance, 1 + 8);
            if (collision.Count > 0)
            {
                if (collision["collider"] is IUsable)
                {
                    return (IUsable)collision["collider"];
                }
            }
            return _area.GetNearestUsableObject();
        }
        void TryToUseObject()
        {
            if (_objectToUse != null)
            {
                if ((_objectToUse).IsUsable())
                {
                    if ((_objectToUse).IsUsable()) (_objectToUse).Use(this);
                }
                OnObjectToUseChanged.Invoke((Spatial)_objectToUse);
            }
        }

        void Die()
        {
            GDE.Print("DIE");
            if (Global.Instance.DEBUG) {
                SetHealth(MaxHealth);
                return;
            }

            GetNode<Spatial>(LightPath).Visible = false;
            DropItem(Global.Instance.GetInventory().GetActiveItem());
            RigidBody deadBody = DeadBodyScene.Instance<RigidBody>();
            GetParent<Spatial>().AddChild(deadBody);
            Transform trans = deadBody.GlobalTransform;
            trans.origin = _camera.GlobalTransform.origin;
            deadBody.GlobalTransform = trans;
            _camera.Reparent(deadBody);
            _camera.SetActiveState(false);
            deadBody.LinearVelocity = Velocity;
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition("human_death",
                _camera.GlobalTransform.origin,
                _camera, "master");
        }

        /// <summary>
        /// Calls Die method if health <= 0
        /// </summary>
        void DieIfNotAlive()
        {
            if (Health <= 0.0f)
            {
                Die();
            }
        }
        // IEntity implementations
        /// <summary>
        /// call to take damage and call SetHealth
        /// </summary>
        public void TakeDamage(float damage)
        {
            _camera.Shake(damage / 4.0f);
            Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition(
                "human_damage",
                _camera.GlobalTransform.origin,
                _camera, "master"
            );

            if (Health > 0.0f)
            {
                SetHealth(Mathf.Max(0.0f, Health - damage));
                DieIfNotAlive();
            }
        }
        /// <summary>
        /// set health and invoke health change event
        /// </summary>
        public void SetHealth(float hp)
        {
            float healthChange = hp - Health;
            Health = Mathf.Min(hp, MaxHealth);
            if (OnHealthChangeEvent != null) OnHealthChangeEvent.Invoke(healthChange);
            DieIfNotAlive();
        }
        /// <summary>
        /// get health
        /// </summary>
        public float GetHealth()
        {
            return Health;
        }
        public void SetMaxHealth(float maxHealth)
        {
            MaxHealth = maxHealth;
        }
        public float GetMaxHealth()
        {
            return MaxHealth;
        }

        // item manipulation
        public void DropItem(Inventory.Item item, Vector2 viewportPosition)
        {
            if (item != null)
            {
                Vector3 worldPos = _camera.ProjectPosition(viewportPosition, 2.0f);
                Godot.Collections.Dictionary collision = GetWorld().DirectSpaceState.IntersectRay(
                    _camera.GlobalTransform.origin, worldPos, new Godot.Collections.Array { this });
                if (collision.Count > 0)
                {
                    worldPos = ((Vector3)collision["position"]) + ((Vector3)collision["normal"]) * 0.1f;
                }
                PickableItem itemInstance = item.PickableItem.Instance<PickableItem>();
                GetParent().AddChild(itemInstance);
                Transform trans = itemInstance.GlobalTransform;
                trans.origin = worldPos;
                trans = trans.LookingAt(_camera.GlobalTransform.origin, Vector3.Up);
                itemInstance.GlobalTransform = trans;
                ((IPickableItem)itemInstance).FitInventoryItem(item);
                itemInstance.Set("mode", 0);
                itemInstance.Call("apply_impulse", Vector3.Zero, (-_camera.GlobalTransform.basis.z) * 10.0f);

                Global.Instance.GetAudioManager().PlaySoundAtPosition("res://resources/sounds/layers/withering/impact/drop.ogg", 
                    itemInstance.GlobalTransform.origin, itemInstance);

                Global.Instance.GetInventory().TakeWholeAmountOfItem(item.ID);
            }
        }
        public void DropItem(Inventory.Item item)
        {
            DropItem(item, GetViewport().Size / 2.0f);
        }

        // weapon
        /// <summary>
        /// Updates item in hand, if possible.
        /// </summary>
        public void OnActiveInventoryItemChange()
        {
            SetWeapon(null, null);

            Inventory.Item item = Global.Instance.GetInventory().GetActiveItem();
            if (item != null && this.IsAlive())
            {
                if (item.GetPropertyValue<bool>("weapon"))
                {
                    PackedScene weaponScene = item.GetPropertyValue<PackedScene>("weapon_scene");
                    if (weaponScene != null)
                    {
                        SetWeapon(item, weaponScene);
                        return;
                    }
                }
            }

            OnObjectToUseChanged.Invoke((Spatial)_objectToUse);
        }
        /// <summary>
        /// Rotates weapon in hand to point of camera ray collision
        /// </summary>
        void FitWeaponCameraDirection()
        {
            if (_currentWeapon != null)
            {
                if (_currentWeapon.CanBeRotated)
                {
                    Godot.Collections.Dictionary collision = _camera.CastRay(1000.0f);
                    if (collision.Count > 0)
                    {
                        _currentWeapon.LookAt((Vector3)collision["position"], _handPoint.GlobalTransform.basis.y);
                    }
                    else
                    {
                        _currentWeapon.LookAt(_camera.GlobalTransform.origin - _camera.GlobalTransform.basis.z * 1000.0f, _handPoint.GlobalTransform.basis.y);
                    }
                }
            }
        }
        /// <summary>
        /// Removes old weapon and instantiates new weapon in hand.
        /// Or just removes weapon from hands if item and weaponScene are null.
        /// </summary>
        /// <param name="item">weapon item</param>
        /// <param name="weaponScene">weapon scene, from items properties</param>
        public void SetWeapon(Inventory.Item item, PackedScene weaponScene)
        {
            if (_currentWeapon != null && (item == null && weaponScene==null))
            {
                _currentWeapon.StopFiring();
                _handPoint.RemoveChild(_currentWeapon);
                _currentWeapon.Free();
                _currentWeapon = null;
            }
            if (weaponScene!=null)
            {
                _currentWeapon = weaponScene.Instance<Weapon>();
                _currentWeapon.Item = item;
                _handPoint.AddChild(_currentWeapon);
            }
        }
    }
}