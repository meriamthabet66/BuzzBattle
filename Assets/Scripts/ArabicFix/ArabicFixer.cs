using RTLTMPro;

namespace UI {
    public static class ArabicFixer
    {
        public static string Fix(string input)
        {
            var builder = new FastStringBuilder(128);
            RTLSupport.FixRTL(input, builder, false, true, true);
            return builder.ToString();
        }
    }
}