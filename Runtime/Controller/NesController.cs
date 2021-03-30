using System.Collections.Generic;
using UnityEngine;
using Aya.UNES.Input;

namespace Aya.UNES.Controller
{
    public class NesController : IController
    {
        private readonly UNESBehaviour _nes;
        private int _data;
        private int _serialData;
        private bool _strobing;

        public bool Debug;

        public KeyConfig Config => _nes.KeyConfig;

        // bit:   	 7     6     5     4     3     2     1     0
        // button:	 A B  Select Start  Up Down  Left 

        private readonly Dictionary<KeyCode, int> _keyMapping;

        public NesController(UNESBehaviour nse)
        {
            _nes = nse;

            _keyMapping = new Dictionary<KeyCode, int>
            {
                {Config.A, 7},
                {Config.B, 6},
                {Config.Select, 5},
                {Config.Start, 4},
                {Config.Up, 3},
                {Config.Down, 2},
                {Config.Left, 1},
                {Config.Right, 0},
            };
        }

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
            if (keyCode == Config.Debug)
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
