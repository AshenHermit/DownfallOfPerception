using Godot;
using System;

namespace Game.UI
{
    public enum OverlayType
    {
        NONE, GAME_OVERLAY, MENU_OVERLAY
    }
    public delegate void OverlayChangeHandler(OverlayType overlay);

    public interface IUIManager
    {
        void AddOverlayChangeListener(OverlayChangeHandler listener);

        void SetIconUnderCursorPosition(Vector2 newPosition);
        void SetIconUnderCursor(Texture iconUnderCursor);
        void ClearIconUnderCursor();

        void HideOverlays();
        void ShowOverlay(OverlayType overlay);
        OverlayType GetCurrentOverlay();
    }
}
