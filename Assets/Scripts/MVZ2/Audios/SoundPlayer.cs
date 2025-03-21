﻿using MVZ2.Managers;
using PVZEngine;
using UnityEngine;

namespace MVZ2.Audios
{
    public class SoundPlayer : MonoBehaviour
    {
        public void Play2D()
        {
            Play2D(soundID.Get());
        }
        public void PlaySound2D(string idString)
        {
            Play2D(NamespaceID.Parse(idString, MainManager.Instance.BuiltinNamespace));
        }
        public void Play2D(NamespaceID id)
        {
            MainManager.Instance.SoundManager.Play(id, Vector3.zero, pitch, 0);
        }
        [SerializeField]
        private NamespaceIDReference soundID;
        [SerializeField]
        private float pitch = 1;
    }
}
