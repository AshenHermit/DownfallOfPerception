using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public class GenerationManager : Node
    {
        [Export]
        public NodePath VoxelLODTerrainPath;
        VoxelLodTerrain _terrain;
        public VoxelLodTerrain GetVoxelTerrain() => _terrain;
        [Export]
        public NodePath VoxelViewerPath;
        VoxelViewer _voxelViewer;
        [Export]
        public Vector3 VoxelViewerStartPosition = Vector3.Zero;
        public VoxelViewer GetVoxelViewer() => _voxelViewer;

        [Export]
        public GenerationProfile CurrentGenerationProfile;

        //TODO: this is not supposed to be here
        [Export]
        public AudioStream AmbienceStream;
        AudioStreamPlayer _ambiencePlayer;

        float timer = 2.0f;

        public bool Generated = false;
        int _previousBlockCount = 0;
        int _checkPos = 0;
        int _stepsCount = 0;

        public event Action OnStartAreaGenerated;

        int _stateRepeatWaitCount = 0;

        HashSet<Vector3> _spawnPoints = new HashSet<Vector3>();
        List<Transform> _spawnTransformsQueue = new List<Transform>();

        public string GetSoundIdFromBank(string soundId)
        {
            //TODO: hard code
            if (CurrentGenerationProfile == null) return "withering_" + soundId;
            return CurrentGenerationProfile.GetSoundIdFromBank(soundId);
        }

        public GenerationManager()
        {
            
        }

        public override void _Ready()
        {
            Global.Instance.RegisterGenerationManager(this);
            
            if (!VoxelViewerPath.IsEmpty()) _voxelViewer = GetNode<VoxelViewer>(VoxelViewerPath);
            if (!VoxelLODTerrainPath.IsEmpty()) _terrain = GetNode<VoxelLodTerrain>(VoxelLODTerrainPath);
            _ambiencePlayer = GetNode<AudioStreamPlayer>("AmbiencePlayer");
            _ambiencePlayer.Stream = AmbienceStream;

            if (CurrentGenerationProfile != null)
            {
                CurrentGenerationProfile.Clear();
            }
            CallDeferred("DefferedSetup");
        }
        public void StopAmbience()
        {
            _ambiencePlayer.Stop();
        }
        public void DefferedSetup()
        {
            if (CurrentGenerationProfile != null)
            {
                if(_terrain != null)
                {
                    CurrentGenerationProfile.GetGeneratorScript()?.Call("setup");
                    _terrain.Generator = CurrentGenerationProfile.GetGeneratorScript();
                    _terrain.Visible = false;
                }
                //TODO: why is this here
                Global.Instance.GetGameStorage().LoadGenerationProfileUserData();
                CurrentGenerationProfile.CallDeferred("_Ready");
            }
        }

        public void RegisterNode(string key, Node node, bool isArray)
        {
            if (CurrentGenerationProfile == null) return;

            if (isArray)
            {
                ((Godot.Collections.Array)CurrentGenerationProfile.Get(key)).Add(node);
            }
            else
            {
                CurrentGenerationProfile.Set(key, node);
            }
        }

        public T GetCurrentGenerationProfile<T>() where T : GenerationProfile
        {
            return (T)CurrentGenerationProfile;
        }

        /// <summary>
        /// When voxel terrain item generator finds place where to spawn item, it gives spawn point transform and calls this method
        /// </summary>
        /// <param name="transform"></param>
        public async void ProcessSurfacePoint(Transform transform)
        {
            if (CurrentGenerationProfile != null)
            {
                if (!_spawnPoints.Contains(transform.origin))
                {
                    _spawnPoints.Add(transform.origin);
                    if (!Generated)
                    {
                        _spawnTransformsQueue.Add(transform);
                    }
                    else
                    {
                        await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
                        CurrentGenerationProfile.ProcessSurfacePoint(transform);
                    }
                }
            }
        }

        public void GiveSpawnTransformsFromQueue()
        {
            for (int i=0; i< _spawnTransformsQueue.Count; ++i)
            {
                //await ToSignal(GetTree().CreateTimer(0.0f), "timeout");
                CurrentGenerationProfile.ProcessSurfacePoint(_spawnTransformsQueue[0]);
                _spawnTransformsQueue.RemoveAt(0);
            }
        }

        public void ActionHappened(string actionId)
        {
            GDE.Print("action happened: " + actionId);
            if (CurrentGenerationProfile == null) return;
            CurrentGenerationProfile.ActionHappened(actionId);
        }

        //TODO: this is not in use, but maybe someday i will make voxel generation callback in a c#, now it is possible only in a gdscript
        public float GetVoxelValue(Vector3 pos)
        {
            return CurrentGenerationProfile.GetVoxelValue(pos);
        }

        async void EndStartAreaGeneration()
        {
            if (AmbienceStream != null) _ambiencePlayer.Play();
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            GiveSpawnTransformsFromQueue();
            Global.Instance.GetPlayer()?.Enable();
            if (_terrain != null)
            {
                _terrain.Visible = true;
                _terrain.LodDistance = 32;
            }
            if (OnStartAreaGenerated!=null) OnStartAreaGenerated.Invoke();
        }

        //TODO: method is big as fuck and half of it is not in use
        public override void _Process(float delta)
        {
            base._Process(delta);
            if (CurrentGenerationProfile != null) CurrentGenerationProfile._Process(delta);

            if (_terrain != null)
            {
                if (!Generated)
                {
                    timer -= delta;
                    if (timer <= 0.0f)
                    {
                        Transform trans = _voxelViewer.GlobalTransform;
                        trans.origin = VoxelViewerStartPosition;
                        _voxelViewer.GlobalTransform = trans;

                        timer = 1.0f;
                        int blockCount = _terrain.DebugGetDataBlockCount();
                        _stepsCount += blockCount;
                        GDE.Print(_stepsCount);
                        //64465
                        //67825
                        //67574
                        //?

                        if (blockCount == _previousBlockCount)
                        {
                            if (CurrentGenerationProfile != null)
                            {
                                _stateRepeatWaitCount += 1;
                                GDE.Print(_stateRepeatWaitCount);
                                if (_stateRepeatWaitCount >= CurrentGenerationProfile.WaitAfterGenerated)
                                {
                                    Generated = true;
                                    EndStartAreaGeneration();
                                    GDE.Print("Generated");
                                }
                            }
                        }
                        _previousBlockCount = blockCount;
                    }
                }
                else
                {
                    Transform trans = _voxelViewer.GlobalTransform;
                    trans.origin = Global.Instance.GetPlayer().GetCamera().GlobalTransform.origin;
                    _voxelViewer.GlobalTransform = trans;
                }
                #region DEAD_CODE
                //else
                //{
                //    if (!Generated)
                //    {
                //        timer -= delta;
                //        if (timer <= 0.0f)
                //        {
                //            timer = 1.0f;

                //            Transform trans = _voxelViewer.GlobalTransform;
                //            //trans.origin = (new Vector3(GD.Randf(), GD.Randf(), GD.Randf())*2.0f-Vector3.One) * 50.0f;
                //            int step = 5;
                //            trans.origin = (new Vector3(
                //                checkPos % step,
                //                Mathf.Floor(checkPos / ((float)step * (float)step)),
                //                Mathf.Floor(checkPos / (float)step) % step) - Vector3.One * (step / 2)) * 50.0f;
                //            //GDE.Print(trans.origin);
                //            _voxelViewer.GlobalTransform = trans;

                //            if (checkPos >= step * step * step)
                //            {
                //                Generated = true;
                //                //GDE.Print("Generated");
                //            }
                //            checkPos += 1;
                //        }
                //    }
                //    else
                //    {
                //        Transform trans = _voxelViewer.GlobalTransform;
                //        trans.origin = Global.Instance.GetPlayer().GetCamera().GlobalTransform.origin;
                //        _voxelViewer.GlobalTransform = trans;
                //    }
                //}
                #endregion
            }
            else
            {
                if (!Generated)
                {
                    Generated = true;
                    EndStartAreaGeneration();
                }
            }
        }
    }
}