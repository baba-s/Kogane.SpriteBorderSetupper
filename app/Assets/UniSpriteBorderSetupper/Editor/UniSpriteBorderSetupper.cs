using OnionRing;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniSpriteBorderSetupperEditor
{
	/// <summary>
	/// スプライトの border を設定するクラス
	/// </summary>
	public static class UniSpriteBorderSetupper
	{
		//==============================================================================
		// 定数(const)
		//==============================================================================
		private const string MENU_ITEM_NAME = "Assets/Setup Sprite Border";

		//==============================================================================
		// クラス
		//==============================================================================
		/// <summary>
		/// テクスチャ情報を管理するクラス
		/// </summary>
		private sealed class TextureData
		{
			public Texture2D		Texture		{ get; private set; }
			public TextureImporter	Importer	{ get; private set; }

			public TextureData( Texture2D texture )
			{
				var path = AssetDatabase.GetAssetPath( texture );

				Texture		= texture;
				Importer	= AssetImporter.GetAtPath( path ) as TextureImporter;
			}
		}

		//==============================================================================
		// 関数(static)
		//==============================================================================
		/// <summary>
		/// スプライトの border を設定します
		/// </summary>
		[MenuItem( MENU_ITEM_NAME )]
		private static void Setup()
		{
			var list = Selection.objects
				.OfType<Texture2D>()
				.Select( c => new TextureData( c ) )
			;

			if ( !list.Any() ) return;

			foreach ( var n in list )
			{
				var importer = n.Importer;
				importer.isReadable = true;
				importer.SaveAndReimport();
			}

			// isReadable を true にしてからじゃないと TextureSlicer.Slice が使用できないため
			// isReadable を true にして 1 フレーム待機してから
			// TextureSlicer.Slice を使用しています
			EditorApplication.delayCall += () =>
			{
				foreach ( var n in list )
				{
					var slicedTexture = TextureSlicer.Slice( n.Texture );
					var importer = n.Importer;
					importer.spriteBorder = slicedTexture.Boarder.ToVector4();
					importer.isReadable = false;
					importer.SaveAndReimport();
				}
			};
		}

		/// <summary>
		/// スライスできる場合 true を返します
		/// </summary>
		[MenuItem( MENU_ITEM_NAME, true )]
		private static bool CanSlice()
		{
			return Selection.objects.OfType<Texture2D>().Any();
		}
	}
}
