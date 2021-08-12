using Godot;
using System;


namespace Game
{
    public class GameSaverObject : Usable
    {
        public override void Use(Node invoker)
        {
            if (invoker == Global.Instance.GetPlayer())
            {
                Global.Instance.GetGameStorage().SaveGame();
            }
        }

        public override string GetUseText()
        {
            return Global.Translate("use_text.SAVE_GAME");
        }
    }
}
