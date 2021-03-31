using UnityEngine;
using Aya.UNES;

namespace Aya.Example
{
    public class UNESTest : MonoBehaviour
    {
        public string RomFile;

        public UNESBehaviour UNES { get; set; }

        public void Awake()
        {
            UNES = GetComponent<UNESBehaviour>();
            var data = Resources.Load<TextAsset>(RomFile).bytes;
            UNES.Boot(data);
        }
    }
}
