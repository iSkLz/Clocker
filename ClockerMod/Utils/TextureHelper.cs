using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Clocker.Mod
{
	public static class TextureHelper
	{
		public static int PosToIndex(this Point pos, int width) {
			return pos.Y * width + pos.X;
		}
		
		public static Point IndexToPos(this int index, int width) {
			return new Point(index / width, index % width);
		}
		
		public static Point OffsetPos(this Point pos, Point offset, int width) {
			pos += offset;
			var extra = pos.X % width;
			pos.Y += (pos.X - extra) / width;
			pos.X = extra;
			return pos;
		}
		
		public static Point OffsetPos(this Point pos, int offset, int width) {
			return pos.OffsetPos(new Point(offset, 0), width);
		}
		
		public static int OffsetIndex(this int index, Point offset, int width) {
			return OffsetPos(index.IndexToPos(width), offset, width).PosToIndex(width);
		}
		
		public static int OffsetIndex(this int index, int offset, int width) {
			return OffsetPos(index.IndexToPos(width), offset, width).PosToIndex(width);
		}
		
		public static Point Start(this Rectangle rect) {
			return new Point(rect.X, rect.Y);
		}
		
		public static Point End(this Rectangle rect) {
			return rect.Start() + new Point(rect.Width, rect.Height);
		}
	}
}
