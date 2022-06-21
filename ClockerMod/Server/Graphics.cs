using System;
using System.Threading;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;

using Celeste;
using Celeste.Mod;
using Monocle;

using Clocker.Server;
using Clocker.Utils;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Clocker.Mod
{
	public partial class Server
	{
		public GraphicsModule Graphics;
		
		public void InitGraphics() {
			Graphics = new GraphicsModule(this);
		}
		
		public void UnloadGraphics() {
			Graphics.ClearCache();
			Graphics = null;
		}
		
		public class GraphicsModule {
			public static void GraphicToPNG(Atlas atlas, string name, Stream output, int scale = 1) {
				var sprite = atlas[name];
				var textureIn = sprite.Texture.Texture_Safe;
				
				if (sprite.ClipRect.Start() == Point.Zero && sprite.DrawOffset == Vector2.Zero
				    && sprite.Width == textureIn.Width && sprite.Height == textureIn.Height) {
					textureIn.SaveAsPng(output, textureIn.Width, textureIn.Height);
				}
				
				var clip = sprite.ClipRect;
				var len = clip.Width * clip.Height;
				var off = new Point((int)sprite.DrawOffset.X, (int)sprite.DrawOffset.Y);
				Color[] data = new Color[len];
				
 				var textureOut = new Texture2D(
					Engine.Instance.GraphicsDevice,
					clip.Width + off.X * 2,
					clip.Height + off.Y * 2,
					false, textureIn.Format
				);
				
				var dest = new Rectangle(
					off.X,
					off.Y,
					clip.Width,
					clip.Height
				);
				
				textureIn.GetData(0, clip, data, 0, len);
				textureOut.SetData(0, dest, data, 0, len);
				
				// TODO: Figure out how to scale output
				textureOut.SaveAsPng(output, textureOut.Width, textureOut.Height);
			}
			
			public Dictionary<Atlas, Dictionary<string, byte[]>> GraphicsCache = new Dictionary<Atlas, Dictionary<string, byte[]>>();
			public Server Owner;
			
			internal GraphicsModule(Server owner) {
				Owner = owner;
				
				owner.Http.Add("/gfx/game/").SetBackup(HandleGameGraphic).Add("cache", CacheGameGraphics);
				GraphicsCache.Add(GFX.Game, new Dictionary<string, byte[]>());
				
				owner.Http.Add("/gfx/ui/").SetBackup(HandleUIGraphic).Add("cache", CacheUIGraphics);
				GraphicsCache.Add(GFX.Gui, new Dictionary<string, byte[]>());
				
				owner.Http.Add("/gfx/port/").SetBackup(HandlePortGraphic).Add("cache", CachePortGraphics);
				GraphicsCache.Add(GFX.Portraits, new Dictionary<string, byte[]>());
				
				owner.Http.Add("/gfx/misc/").SetBackup(HandleMiscGraphic).Add("cache", CacheMiscGraphics);
				GraphicsCache.Add(GFX.Misc, new Dictionary<string, byte[]>());
			}
			
			public void ClearCache() {
				foreach (var atlas in GraphicsCache) {
					atlas.Value.Clear();
				}
			}
			
			public void HandleGameGraphic(HttpListenerContext ctx, string name) {
				HandleGraphic(ctx, GFX.Game, name);
			}
			
			public void HandleUIGraphic(HttpListenerContext ctx, string name) {
				HandleGraphic(ctx, GFX.Gui, name);
			}
			
			public void HandlePortGraphic(HttpListenerContext ctx, string name) {
				HandleGraphic(ctx, GFX.Portraits, name);
			}
			
			public void HandleMiscGraphic(HttpListenerContext ctx, string name) {
				HandleGraphic(ctx, GFX.Misc, name);
			}
			
			public void HandleGraphic(HttpListenerContext ctx, Atlas atlas, string name) {
				var scale = 1;
				var query = ctx.Query("scale");
				if (query != null) int.TryParse(query, out scale);
				if (scale < 1) scale = 1;
				var id = name + " " + scale;
				
				if (GraphicsCache[atlas].ContainsKey(id)) {
					var buffer = GraphicsCache[atlas][id];
					ctx.Response.ContentType = "png".MimeOf();
					ctx.Response.ContentLength64 = buffer.Length;
					try {
						ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
					} catch {}
					ctx.Response.Close();
				} else {
					if (!atlas.Has(name))
						ctx.Ratio("Nonexistent graphic path.");
					else {
						try {
							CacheGraphic(atlas, name, scale).CopyTo(ctx.Response.OutputStream);
						} catch {}
						ctx.Response.Close();
					}
				}
			}
			
			public void CacheGameGraphics(HttpListenerContext ctx) {
				CacheGraphics(GFX.Game, ctx);
			}
			
			public void CacheUIGraphics(HttpListenerContext ctx) {
				CacheGraphics(GFX.Gui, ctx);
			}
			
			public void CachePortGraphics(HttpListenerContext ctx) {
				CacheGraphics(GFX.Portraits, ctx);
			}
			
			public void CacheMiscGraphics(HttpListenerContext ctx) {
				CacheGraphics(GFX.Misc, ctx);
			}
			
			public void CacheGraphics(Atlas atlas, HttpListenerContext ctx) {
				var names = ctx.ReadAsStr().NormalizeLines().Split('\n');
				
				foreach (var name in names) {
					var args = name.Split(' ');
					int scale = 1;
					if (args.Length > 1) int.TryParse(args[1], out scale);
					CacheGraphic(atlas, args[0], scale);
				}
				
				ctx.ServeText("Cached.");
			}
			
			public MemoryStream CacheGraphic(Atlas atlas, string name, int scale = 1) {
				var stream = new MemoryStream();
				GraphicToPNG(atlas, name, stream, scale);
				var buffer = new byte[(int)stream.Length];
				stream.Seek(0, SeekOrigin.Begin);
				stream.Read(buffer, 0, (int)stream.Length);
				GraphicsCache[atlas].Add(name + " " + scale.ToString(), buffer);
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
		}
	}
}
