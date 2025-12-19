// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("KkubYV+QVbq31Fl4+Xz5yrrbrZIXgBCUGnZXLE9GjkxNh25HZhsmdNMl5e+FnCFONw0k/a95x1oSq8EwvdlIJE5I/xa1++nQ1H+sZ/ndJYYGl4zmdKcv/jn58vRDveBig8GLqvKSLu+04aezq5T9GBzDsv9kaP5TOYsIKzkEDwAjj0GP/gQICAgMCQoija9cp2tn6LSPiPkq1lDUADIaIBMUqJ/Ffmk3n6Ao2Jo6r5dHlU0HMn7r+B4vhCgehCtPDhALr3mNeduLCAYJOYsIAwuLCAgJrDXlIvD2nTHdxa80kHugkyPs2X3sZKP3UB6QH7gri+sQXu3V6SLv/XEgQK2UQMkKsv+EcJ8FCg6LBS6naCjryXFJVa58D1O/IVjPxAsKCAkI");
        private static int[] order = new int[] { 12,4,9,3,9,5,12,11,8,11,11,11,13,13,14 };
        private static int key = 9;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
