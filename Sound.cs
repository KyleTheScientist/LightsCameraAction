using GorillaScience.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightsCameraAction
{
    public static class Sound
    {
        public static void Play(int sound=67, float volume=.05f)
        {
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(sound, false, volume);
        }
    }
}
