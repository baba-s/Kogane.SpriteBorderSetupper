using UnityEngine;

namespace OnionRing
{
    internal class TextureSlicer
    {
        public static SlicedTexture Slice( Texture2D texture )
        {
            var pixels = texture.GetPixels();
            var slicer = new TextureSlicer( texture, pixels );
            return slicer.Slice( pixels );
        }

        private readonly int   width;
        private readonly int   height;
        private readonly int[] pixels;
        private readonly int   safetyMargin = 2;
        private readonly int   margin       = 2;

        private TextureSlicer( Texture2D refTexture, Color[] getPixels )
        {
            width  = refTexture.width;
            height = refTexture.height;

            pixels = new int[ getPixels.Length ];
            for ( var i = 0; i < getPixels.Length; ++i )
            {
                pixels[ i ] = getPixels[ i ].a > 0 ? getPixels[ i ].GetHashCode() : 0;
            }
        }

        private void CalcLine( ulong[] list, out int start, out int end )
        {
            start = 0;
            end   = 0;
            var tmpStart = 0;
            var tmpEnd   = 0;
            var tmpHash  = list[ 0 ];
            for ( var i = 0; i < list.Length; ++i )
            {
                if ( tmpHash == list[ i ] )
                {
                    tmpEnd = i;
                }
                else
                {
                    if ( end - start < tmpEnd - tmpStart )
                    {
                        start = tmpStart;
                        end   = tmpEnd;
                    }

                    tmpStart = i;
                    tmpEnd   = i;
                    tmpHash  = list[ i ];
                }
            }

            if ( end - start < tmpEnd - tmpStart )
            {
                start = tmpStart;
                end   = tmpEnd;
            }

            end -= ( safetyMargin * 2 + margin );
            if ( end < start )
            {
                start = 0;
                end   = 0;
            }
        }

        private ulong[] CreateHashListX( int aMax, int bMax )
        {
            var hashList = new ulong[ aMax ];
            for ( var a = 0; a < aMax; ++a )
            {
                ulong line = 0;
                for ( var b = 0; b < bMax; ++b )
                {
                    line = ( ulong )( line + ( ulong )( pixels[ b * width + a ] * b ) ).GetHashCode();
                }

                hashList[ a ] = line;
            }

            return hashList;
        }

        private ulong[] CreateHashListY( int aMax, int bMax )
        {
            var hashList = new ulong[ aMax ];
            for ( var a = 0; a < aMax; ++a )
            {
                ulong line = 0;
                for ( var b = 0; b < bMax; ++b )
                {
                    line = ( ulong )( line + ( ulong )( pixels[ a * width + b ] * b ) ).GetHashCode();
                }

                hashList[ a ] = line;
            }

            return hashList;
        }

        private SlicedTexture Slice( Color[] originalPixels )
        {
            int xStart, xEnd;
            {
                var hashList = CreateHashListX( width, height );
                CalcLine( hashList, out xStart, out xEnd );
            }

            int yStart, yEnd;
            {
                var hashList = CreateHashListY( height, width );
                CalcLine( hashList, out yStart, out yEnd );
            }

            var skipX = false;
            if ( xEnd - xStart < 2 )
            {
                skipX  = true;
                xStart = 0;
                xEnd   = 0;
            }

            var skipY = false;
            if ( yEnd - yStart < 2 )
            {
                skipY  = true;
                yStart = 0;
                yEnd   = 0;
            }

            var left   = xStart + safetyMargin;
            var bottom = yStart + safetyMargin;
            var right  = width - xEnd - safetyMargin - margin;
            var top    = height - yEnd - safetyMargin - margin;
            if ( skipX )
            {
                left  = 0;
                right = 0;
            }

            if ( skipY )
            {
                top    = 0;
                bottom = 0;
            }

            return new( new( left, bottom, right, top ) );
        }

        private int Get( int x, int y )
        {
            return pixels[ y * width + x ];
        }
    }

    internal class SlicedTexture
    {
        public SlicedTexture( Boarder boarder )
        {
            Boarder = boarder;
        }

        public Boarder Boarder { get; }
    }

    internal class Boarder
    {
        public Boarder( int left, int bottom, int right, int top )
        {
            Left   = left;
            Bottom = bottom;
            Right  = right;
            Top    = top;
        }

        public Vector4 ToVector4() { return new( Left, Bottom, Right, Top ); }

        public int Left   { get; }
        public int Bottom { get; }
        public int Right  { get; }
        public int Top    { get; }
    }
}