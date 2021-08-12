using System;

namespace Game
{
    public interface IPlayerCamera
    {
        void SetActiveState(bool activeState);
        bool IsActive();
        void RotateByDelta(float deltaX, float deltaY);
    }
}