namespace LightsCameraAction
{
    /// <summary>
    /// This class is used to provide information about your mod to BepInEx.
    /// </summary>
    internal class PluginInfo
    {
        public const string GUID = "com.kylethescientist.gorillatag.lightscameraaction";
        public const string Name = "LightsCameraAction";
        public const string Version = "0.0.11";

        // Moved over here jjust in case AA decides to pull a fast one and vert it back 5 times like they did with rig caching.
        public const string localRigPath = "Player Objects/Local VRRig/Local Gorilla Player";
        public const string palmPath = "/rig/body/shoulder.{0}/upper_arm.{0}/forearm.{0}/hand.{0}/palm.01.{0}"; 
    }
}
