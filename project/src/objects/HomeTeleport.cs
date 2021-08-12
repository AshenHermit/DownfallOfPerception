using Game;
using System;

namespace Game
{
    public class HomeTeleport : Teleporter
    {


        public override string GetUseText()
        {
            return Global.Translate("use_text.GO_HOME");
        }
    }
}
