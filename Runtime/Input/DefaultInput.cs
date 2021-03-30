using System;
using UnityEngine;

namespace Aya.UNES.Input
{
    public class DefaultInput : BaseInput
    {
        private KeyCode[] _keyCodes
        {
            get
            {
                if (_keyCodeCache == null)
                {
                    var array = Enum.GetValues(typeof(KeyCode));
                    _keyCodeCache = new KeyCode[array.Length];
                    for (var i = 0; i < array.Length; i++)
                    {
                        _keyCodeCache[i] = (KeyCode)array.GetValue(i);
                    }
                }

                return _keyCodeCache;
            }
        }
        private KeyCode[] _keyCodeCache;

        public override void HandlerKeyDown(Action<KeyCode> onKeyDown)
        {
            foreach (var keyCode in _keyCodes)
            {
                if (UnityEngine.Input.GetKeyDown(keyCode))
                {
                    onKeyDown(keyCode);
                }
            }
        }

        public override void HandlerKeyUp(Action<KeyCode> onKeyUp)
        {
            foreach (var keyCode in _keyCodes)
            {
                if (UnityEngine.Input.GetKeyUp(keyCode))
                {
                    onKeyUp(keyCode);
                }
            }
        }
    }
}
