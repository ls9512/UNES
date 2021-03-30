using System;
using UnityEngine;

namespace Aya.UNES.Input
{
    public abstract class BaseInput
    {
        public abstract void HandlerKeyDown(Action<KeyCode> onKeyDown);
        public abstract void HandlerKeyUp(Action<KeyCode> onKeyUp);
    }
}
