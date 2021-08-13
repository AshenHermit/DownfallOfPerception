using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class Turret : Spatial
    {
        [Export]
        public NodePath RayGeometryPath;
        MeshInstance _rayGeometry;
        [Export]
        public NodePath VisualNodePath;

        float _maxDistance = 40.0f;

        float _attackTimer = 0.0f;
        float _fireTimer = 0.0f;
        float _lookTimer = 0.0f;
        Vector3 _lookTargetPosition;
        Vector3 _originalNormal;
        Vector3 _targetPos;
        Vector3 _softTargetPos;

        Spatial _muzzle;
        int _gunBoneId = 0;
        Skeleton _skeleton;
        Spatial _gunBone;

        AnimationPlayer _animationPlayer;

        bool _seeingPlayer;

        static float _fireSpeed = 0.2f;
        static float _damage = 4.0f;

        float _difficulty = 0.1f;
        public void SetDifficulty(float difficulty)
        {
            _difficulty = difficulty;
        }

        public override void _Ready()
        {
            _muzzle = GetNode(VisualNodePath).GetNode<Spatial>("Armature/Skeleton/BoneAttachment/Muzzle");
            _skeleton = GetNode(VisualNodePath).GetNode<Skeleton>("Armature/Skeleton");
            _gunBoneId = _skeleton.FindBone("gun");
            _rayGeometry = GetNode<MeshInstance>(RayGeometryPath);
            _rayGeometry.Reparent(_muzzle);
            _rayGeometry.Transform = Transform.Identity;


            SetupRay();
            CallDeferred("DefferedSetup");
        }
        public void DefferedSetup()
        {
            _originalNormal = GlobalTransform.basis.z;
            _targetPos = GlobalTransform.origin + _originalNormal;
        }
        public override void _Process(float delta)
        {
            base._Process(delta);

            if (GlobalTransform.origin.DistanceTo(Global.Instance.GetPlayer().GlobalTransform.origin) < _maxDistance)
            {
                CastRay();
            }
            UpdateFiring(delta);
            if (_seeingPlayer) _softTargetPos += (_targetPos - _softTargetPos) / 1.5f;
            else _softTargetPos += (_targetPos - _softTargetPos) / 5.0f;

            Transform bonePose = _skeleton.GetBoneGlobalPose(_gunBoneId);
            Vector3 origin = -ToLocal(_softTargetPos);
            origin.y = -origin.y;
            _skeleton.SetBoneGlobalPoseOverride(_gunBoneId, bonePose.LookingAt(origin, Vector3.Up), 1.0f, true);
            //_muzzle.LookAt(_softTargetPos, Vector3.Up);
            //_muzzle.RotateObjectLocal(Vector3.Up, Mathf.Pi);
        }

        public void SetupRay()
        {
            SurfaceTool st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.LineStrip);
            st.AddUv(new Vector2(0, 0));
            st.AddVertex(new Vector3(0, 0, 0));
            st.AddUv(new Vector2(1, 0));
            st.AddVertex(new Vector3(0, 0, 1));
            _rayGeometry.Mesh = st.Commit();
        }

        public void CastRay()
        {
            float dot = (Global.Instance.GetPlayer().GlobalTransform.origin - GlobalTransform.origin).Normalized().Dot(_originalNormal);

            bool canSeePlayer = true;
            if (dot < 0.2f)
            {
                if (_attackTimer > 1.0f)
                {
                    _attackTimer = 1.0f;
                    canSeePlayer = false;
                }
            }

            Godot.Collections.Dictionary collision = Global.Instance.GetDirectSpaceState().IntersectRay(
                _rayGeometry.GlobalTransform.origin + _muzzle.GlobalTransform.basis.z * 0.3f,
                _rayGeometry.GlobalTransform.origin + _muzzle.GlobalTransform.basis.z * _maxDistance
            );
            if (collision.Count > 0)
            {
                float dist = collision.Get<Vector3>("position").DistanceTo(_rayGeometry.GlobalTransform.origin);
                _rayGeometry.Scale = new Vector3(0.0f, 0.0f, dist);

                Node collider = collision.Get<Node>("collider");
                if (collider == Global.Instance.GetPlayer() && canSeePlayer)
                {
                    if (Global.Instance.GetPlayer().IsAlive())
                    {
                        _attackTimer = 2.0f;
                        _seeingPlayer = true;
                    }
                    else
                    {
                        _seeingPlayer = false;
                    }
                    return;
                }
            }
            _seeingPlayer = false;
        }

        void UpdateFiring(float delta)
        {
            if (_attackTimer > 0.0f)
            {
                if (_attackTimer > 1.0f)
                {
                    _targetPos = Global.Instance.GetPlayer().GlobalTransform.origin + Vector3.Up * 2.0f;
                    if (_seeingPlayer)
                    {
                        _fireTimer -= delta;
                        if (_fireTimer <= 0.0f)
                        {
                            if (_animationPlayer != null)
                            {
                                _animationPlayer.Stop(true);
                                _animationPlayer.Play("fire");
                            }
                            Global.Instance.GetPlayer().TakeDamage(_damage);
                            _fireTimer = _fireSpeed;

                            AudioManager.AudioUnit sound = 
                                Global.Instance.GetAudioManager().PlaySoundFromBankAtPosition("turret_shot", _muzzle.GlobalTransform.origin, _muzzle);
                            sound.MaxDb = 10.0f;
                            sound.UnitSize = 50.0f;
                        }
                    }
                }
                _attackTimer -= delta;
            }
            else
            {
                _lookTimer -= delta;
                if (_lookTimer <= 0.0f)
                {
                    _lookTimer = 0.3f + GD.Randf() * (8.0f - _difficulty*7.0f);
                    _lookTargetPosition = GlobalTransform.origin + _originalNormal*2.6f + MathUtils.Randv()*(_difficulty*2.0f);
                }
                _targetPos = _lookTargetPosition;
            }
        }
    }
}
