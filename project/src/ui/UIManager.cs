using Godot;
using System;

namespace Game.UI
{
    //TODO: why am i inheriting interface in a class that acts like a singleton?
    // really bad, im just havent been experienced in this
    public class UIManager : Container, IUIManager
    {
        [Export]
        public Godot.Collections.Array<NodePath> OverlayPaths;
        private Godot.Collections.Array<Container> _overlays = new Godot.Collections.Array<Container>();
        [Export]
        public NodePath TabContainerNodePath;
        private TabContainer _tabContainer;
        [Export]
        public NodePath CursorImageRectNodePath;
        public TextureRect CursorImageRect;
        [Export]
        public NodePath ScreenFadePanelPath;
        public Control _screenFadePanel;

        OverlayType _currentOverlay = OverlayType.NONE;

        public event OverlayChangeHandler OnOverlayChanged;

        float screenFadeAmount = 1.0f;
        float screenFadeSpeed = 1.0f;
        bool autoFading = true;
        bool fading = false;

        // private fields
        private Texture _iconUnderCursor;

        public UIManager() : base()
        {
            if(Global.Instance!=null) Global.Instance.RegisterUIManager(this);
        }

        async public override void _Ready()
        {
            foreach (NodePath path in OverlayPaths)
            {
                _overlays.Add(GetNode<Container>(path));
            }
            _tabContainer = GetNode<TabContainer>(TabContainerNodePath);
            _screenFadePanel = GetNode<Control>(ScreenFadePanelPath);
            _screenFadePanel.Visible = true;
            CursorImageRect = GetNode<TextureRect>(CursorImageRectNodePath);

            ClearIconUnderCursor();

            ShowOverlay(_currentOverlay);
        }
        public override void _Input(InputEvent e)
        {
            if(e is InputEventMouseMotion)
            {
                InputEventMouseMotion motionEvent = (InputEventMouseMotion)e;
                SetIconUnderCursorPosition(motionEvent.Position);
            }
        }
        public override void _Process(float delta)
        {
            if (Input.IsActionJustPressed("show_game_overlay") && Global.Instance.GetPlayer().IsAlive())
            {
                if (_currentOverlay != OverlayType.GAME_OVERLAY && _currentOverlay != OverlayType.MENU_OVERLAY) { ShowOverlay(OverlayType.GAME_OVERLAY); }
                else if (_currentOverlay == OverlayType.GAME_OVERLAY)
                {
                    ShowOverlay(OverlayType.NONE);
                }
            }
            else if (Input.IsActionJustPressed("show_game_menu"))
            {
                if (_currentOverlay != OverlayType.MENU_OVERLAY) { ShowOverlay(OverlayType.MENU_OVERLAY); }
                else { ShowOverlay(OverlayType.NONE); }
            }

            UpdateCutsceneFading(delta);
            UpdateScreenFading(delta);
        }
        void UpdateCutsceneFading(float delta)
        {
            if (Global.Instance.GetPlayerCamera() != null)
            {
                if (Global.Instance.GetPlayerCamera().IsCutscenePlaying())
                {
                    foreach(Control control in GetChildren())
                    {
                        if (control.IsInGroup("DontHide")) continue;
                        control.Modulate = new Color(1.0f, 1.0f, 1.0f, control.Modulate.a + (0.0f - control.Modulate.a) / 6.0f);
                    }
                }
                else
                {
                    foreach (Control control in GetChildren())
                    {
                        if (control.IsInGroup("DontHide")) continue;
                        control.Modulate = new Color(1.0f, 1.0f, 1.0f, control.Modulate.a + (1.0f - control.Modulate.a) / 6.0f);
                    }
                }
            }
        }

        void UpdateScreenFading(float delta)
        {
            if (fading)
            {
                if (autoFading)
                {
                    if (screenFadeAmount > 0.0f)
                    {
                        screenFadeAmount = Math.Min(screenFadeAmount + delta * screenFadeSpeed, 1.0f);
                    }
                }
            }
            else
            {
                if (autoFading)
                {
                    screenFadeAmount = Math.Max(0.0f, screenFadeAmount - delta * screenFadeSpeed);
                }
            }
            _screenFadePanel.Modulate = new Color(1.0f, 1.0f, 1.0f, screenFadeAmount);
        }
        public void StartScreenFade(float fadeSpeed=1.0f)
        {
            screenFadeSpeed = fadeSpeed;
            fading = true;
            autoFading = true;
            screenFadeAmount = 0.001f;
        }
        public void StartScreenShowing(float showSpeed = 1.0f)
        {
            screenFadeSpeed = showSpeed;
            fading = false;
            autoFading = true;
            screenFadeAmount = 1.0f;
        }
        public void SetScreenFadeAmount(float amount)
        {
            autoFading = false;
            screenFadeAmount = amount;
        }

        public void HideOverlays()
        {
            for (int i = 0; i < _overlays.Count; ++i)
            {
                _overlays[i].Visible = false;
            }
        }
        public void ShowOverlay(OverlayType overlay)
        {
            HideOverlays();
            _currentOverlay = overlay;
            if (overlay != OverlayType.NONE)
            {
                _overlays[(int)overlay-1].Visible = true;
                _tabContainer.RectMinSize = new Vector2(630, _tabContainer.RectSize.y);
                Global.Instance.GetPlayerCamera().SetActiveState(false);

                if (overlay == OverlayType.MENU_OVERLAY)
                {
                    Global.Instance.SetGamePauseState(true);
                }
            }
            else
            {
                Global.Instance.SetGamePauseState(false);
                Global.Instance.GetPlayerCamera().SetActiveState(true);
            }
            if (OnOverlayChanged != null) OnOverlayChanged.Invoke(_currentOverlay);
        }
        public OverlayType GetCurrentOverlay() => _currentOverlay;

        public void SetIconUnderCursorPosition(Vector2 newPosition)
        {
            if(CursorImageRect!=null) CursorImageRect.RectGlobalPosition = newPosition-CursorImageRect.RectSize/2.0f;
        }
        public void SetIconUnderCursor(Texture iconUnderCursor)
        {
            _iconUnderCursor = iconUnderCursor;
            if(CursorImageRect!=null) CursorImageRect.Texture = iconUnderCursor;
        }
        public void ClearIconUnderCursor()
        {
            SetIconUnderCursor(null);
        }

        public void AddOverlayChangeListener(OverlayChangeHandler listener)
        {
            OnOverlayChanged += listener;
        }
    }
}
