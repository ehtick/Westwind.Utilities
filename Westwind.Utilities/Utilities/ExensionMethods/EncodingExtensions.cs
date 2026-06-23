
#if NET10_0_OR_GREATER

namespace System.Text
{
    public static class EncodingExtensions
    {
        private static UTF8Encoding _utf8NoBom = new UTF8Encoding(false);

        extension(Encoding )
        {
            public static UTF8Encoding UTF8NoBom => _utf8NoBom;
        }
    }
}

#endif