using UnityEngine;

namespace Aya.UNes.Controller
{
    public interface IController
    {
        void Strobe(bool on);

        int ReadState();

        void PressKey(KeyCode keyCode);

        void ReleaseKey(KeyCode keyCode);
    }
}
