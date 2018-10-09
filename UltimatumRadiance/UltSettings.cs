using Modding;

namespace UltimatumRadiance
{
    public class UltSettings : IModSettings
    {
        public bool DefeatedLord { get => GetBool(); set => SetBool(value); } 
    }
}