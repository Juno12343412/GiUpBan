#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class AppleTangle
    {
        private static byte[] data = System.Convert.FromBase64String("YPJiNRKghx7sQMnb6QPz1RO74jifg4SZgp+S2v3b/+3ovu/o+Oaqm8Fto20c5urq7u7r24na4Nvi7ei+5HbWGMCiw/EjFSVeUuUytfc9INayTO7il/yrvfr1nzhcYMjQrEg+hIWPy4iEhY+Cn4KEhZjLhI3LnpiOrpX0p4C7fapiL5+J4PtoqmzYYWqfgo2CiIqfjsuJksuKhZLLm4qZn6IznXTY/45KnH8ixuno6uvqSGnqm4eOy7mEhJ/LqKrb9fzm293b39nt2+Tt6L72+OrqFO/u2+jq6hTb9tbNjMth2IEc5mkkNQBIxBK4gbCPmYqIn4KIjsuYn4qfjoaOhZ+YxducnMWKm5uHjsWIhIbEipubh46IiuO122nq+u3ovvbL72nq49tp6u/b9How9ay7AO4GtZJvxgDdSbynvgdDN5XJ3iHOPjLkPYA/Sc/I+hxKR88JADpcmzTkrgrMIRqGkwYMXvz822nvUNtp6EhL6Onq6enq6dvm7eKCjYKIip+ChIXLqp6fg4SZgp+S2tjdsduJ2uDb4u3ovu/t+Om+uNr4x8uIjpmfgo2CiIqfjsubhIeCiJL0bmhu8HLWrNwZQnCrZcc/Wnv5M4/eyP6g/rL2WH8cHXd1JLtRKrO7K4jYnBzR7Me9ADHkyuUxUZjypF6bh47LqI6Zn4KNgoiKn4KEhcuqnv3b/+3ovu/o+Oaqm5uHjsu5hISf7uvoaerk69tp6uHpaerq6w96QuLjwO3q7u7s6er99YOfn5uY0cTEnMuoqttp6snb5u3iwW2jbRzm6urq3XKnxpNcBmdwNxiccBmdOZzbpCrLioWPy4iOmZ+CjYKIip+ChIXLmyLymR625T6UtHAZzuhRvmSmtuYaksuKmJieho6Yy4qIiI6bn4qFiI7N28/t6L7v4Pj2qpubh47LqI6Zn97Z2t/b2N2x/ObY3tvZ29LZ2t/bWtuzB7Hv2WeDWGT2NY6YFIy1jlfv7fjpvrja+Nv67ei+7+H44aqbm+bt4sFto20c5urq7u7r6Gnq6uu3ZJhqiy3wsOLEeVkTr6Mbi9N1/h6UqkNzEjohjXfPgPo7SFAP8MEo9EBImnmsuL4qRMSqWBMQCJsmDUinh47LooWIxdrN28/t6L7v4Pj2qpu5joeCioWIjsuEhcufg4KYy4iOmWnq6+3iwW2jbRyIj+7q22oZ28HtfnWR50+sYLA//dzYIC/kpiX/gjrFq00crKaU47Xb9O3ovvbI7/Pb/e3ovvbl7/3v/8A7gqx/neIVH4Bmkdtp6p3b5e3ovvbk6uoU7+/o6erLhI3Ln4OOy5+DjoXLipubh4KIioxk41/LHCBHx8uEm13U6ttnXKgkiYeOy5ifioWPipmPy5+OmYaYy4pe0UYf5OXreeBayv3Fnz7X5jCJ/Wv/wDuCrH+d4hUfgGbFq00crKaUMt2UKmy+MkxyUtmpEDM+mnWVSrlVH5hwBTmP5CCSpN8zSdUSkxSAI+wHltJoYLjLONMvWlRxpOGAFMAXXPBWeKnP+cEs5PZdpne1iCOga/zE22oo7ePA7eru7uzp6dtqXfFqWNv67ei+7+H44aqbm4eOy6KFiMXau0FhPjEPFzvi7Nxbnp7K");
        private static int[] order = new int[] { 16,5,5,22,52,43,25,39,27,25,29,56,46,36,33,15,54,53,54,56,27,44,49,27,57,52,46,33,35,36,48,45,56,40,36,39,46,43,47,55,53,57,49,47,46,54,54,48,55,57,54,57,53,56,58,58,57,59,58,59,60 };
        private static int key = 235;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
