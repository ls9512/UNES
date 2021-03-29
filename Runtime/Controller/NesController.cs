using System.Collections.Generic;
using UnityEngine;

namespace Aya.UNes.Controller
{
    public class NesController : IController
    {
        private int _data;
        private int _serialData;
        private bool _strobing;

        public bool Debug;
        // bit:   	 7     6     5     4     3     2     1     0
        // button:	 A B  Select Start  Up Down  Left 

        private readonly Dictionary<KeyCode, int> _keyMapping = new Dictionary<KeyCode, int>
        {
            {KeyCode.A, 7},
            {KeyCode.S, 6},
            {KeyCode.RightShift, 5},
            {KeyCode.Return, 4},
            {KeyCode.UpArrow, 3},
            {KeyCode.DownArrow, 2},
            {KeyCode.LeftArrow, 1},
            {KeyCode.RightArrow, 0},
        };

        public void Strobe(bool on)
        {
            _serialData = _data;
            _strobing = on;
        }

        public int ReadState()
        {
            int ret = ((_serialData & 0x80) > 0).AsByte();
            if (!_strobing)
            {
                _serialData <<= 1;
                _serialData &= 0xFF;
            }

            return ret;
        }

        public void PressKey(KeyCode keyCode)
        {
            if (keyCode == KeyCode.P)
            {
                Debug ^= true;
            }

            if (!_keyMapping.ContainsKey(keyCode))
            {
                return;
            }

            _data |= 1 << _keyMapping[keyCode];
        }

        public void ReleaseKey(KeyCode keyCode)
        {
            if (!_keyMapping.ContainsKey(keyCode))
            {
                return;
            }

            _data &= ~(1 << _keyMapping[keyCode]);
        }
    }
}
