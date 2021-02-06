#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Svh7WEp3fHNQ/DL8jXd7e3t/enloYDHZ4phe0cpnHSpbuLD3mWB2a8v2sPhY6jwu4tQ3uoMHbUbr8clxNFmlWtwX4EUcih4vuSFlTa3UrYH4e3V6Svh7cHj4e3t63Z/uD/RHsyvz32xjROdRH36eHYf5PBl0MKzJ+XCNx9QaKaWEuhiW6N2d0HXvEjfsK/KYVzKnR9Iy0KGR+xE0mGqRH3i5FVoHe/YE0wE23ILbMk5FHeBKeRX9T04qqZHdSZanJaX3nQLuAPrw9x8xoPLIjLWUTsmRc6GVyJqgQ8cn1YXSSgO/VKz0TJWVutV7TidUWthVyjYAkOg2lp8MH/Hdnn6DZgubwIJxw0gYfPjSwojBiIIsYGwQcPaZGthvaa3By3h5e3p7");
        private static int[] order = new int[] { 0,13,3,9,13,11,13,9,12,13,10,11,12,13,14 };
        private static int key = 122;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
