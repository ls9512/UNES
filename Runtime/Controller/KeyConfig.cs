using System;
using UnityEngine;

namespace Aya.UNES.Controller
{
    [Serializable]
    public class KeyConfig
    {
        public KeyCode Up = KeyCode.UpArrow;
        public KeyCode Down = KeyCode.DownArrow;
        public KeyCode Left = KeyCode.LeftArrow;
        public KeyCode Right = KeyCode.RightArrow;

        public KeyCode A = KeyCode.A;
        public KeyCode B = KeyCode.S;

        public KeyCode Start = KeyCode.Alpha1;
        public KeyCode Select = KeyCode.Alpha2;

        public KeyCode Debug = KeyCode.P;
    }
}
