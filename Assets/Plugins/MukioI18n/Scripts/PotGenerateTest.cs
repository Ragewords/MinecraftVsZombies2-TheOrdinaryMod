using UnityEngine;

namespace MukioI18n
{
    public class PotGenerateTest : MonoBehaviour
    {
        private void Start()
        {
            var pot = new MukioPotGenerator("TestProject", "TranslateTeam");
            pot.AddString("新游戏", "Start/Canvas/Menu");
            pot.AddString("载入游戏", "Start/Canvas/Menu");
            pot.AddString("选项", "Start/Canvas/Menu");
            pot.AddString("退出", "Start/Canvas/Menu");

            pot.WriteOut("test.pot");
        }
    }
}

