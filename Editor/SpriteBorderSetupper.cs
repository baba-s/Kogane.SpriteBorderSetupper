using OnionRing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
	/// <summary>
	/// スプライトの border を設定するクラス
	/// </summary>
	internal static class SpriteBorderSetupper
	{
		//================================================================================
		// 定数
		//================================================================================
		private const string NAME           = "UniSpriteBorderSetupper";
		private const string MENU_ITEM_NAME = "Assets/" + NAME + "/選択中のスプライトの Border を自動で設定";

		//================================================================================
		// デリゲート
		//================================================================================
		private delegate void DisplayProgressBarCallback
		(
			int    number,
			int    count,
			string path
		);

		private delegate void ClearProgressBarCallback();

		//================================================================================
		// クラス
		//================================================================================
		/// <summary>
		/// テクスチャ情報を管理するクラス
		/// </summary>
		private sealed class TextureData
		{
			public string          Path     { get; }
			public Texture2D       Texture  { get; }
			public TextureImporter Importer { get; }

			public TextureData( Texture2D texture )
			{
				Path     = AssetDatabase.GetAssetPath( texture );
				Texture  = texture;
				Importer = AssetImporter.GetAtPath( Path ) as TextureImporter;
			}
		}
		
		//================================================================================
		// 関数（static）
		//================================================================================
		/// <summary>
		/// スプライトの border を設定します
		/// </summary>
		[MenuItem( MENU_ITEM_NAME )]
		private static void DoSetup()
		{
			var isOk = EditorUtility.DisplayDialog
			(
				title: NAME,
				message: "選択中のスプライトの Border を自動で設定しますか？",
				ok: "OK",
				cancel: "Cancel"
			);

			if ( !isOk ) return;

			var list = Selection.objects
					.OfType<Texture2D>()
					.ToArray()
				;

			if ( !list.Any() ) return;

			void OnDisplayProgressBarPreprocess( int number, int count, string path )
			{
				EditorUtility.DisplayProgressBar
				(
					title: $"{NAME} Preprocess",
					info: $"{number}/{count} {path}",
					progress: ( float ) number / count
				);
			}

			void OnDisplayProgressBarProcessing( int number, int count, string path )
			{
				EditorUtility.DisplayProgressBar
				(
					title: $"{NAME} Processing",
					info: $"{number}/{count} {path}",
					progress: ( float ) number / count
				);
			}

			void OnComplete()
			{
				EditorUtility.DisplayDialog
				(
					title: NAME,
					message: "選択中のスプライトの Border を自動で設定しました",
					ok: "OK"
				);
			}

			Setup
			(
				textureList: list,
				onDisplayProgressBarPreprocess: OnDisplayProgressBarPreprocess,
				onDisplayProgressBarProcessing: OnDisplayProgressBarProcessing,
				onClearProgressBar: EditorUtility.ClearProgressBar,
				onComplete: OnComplete
			);
		}

		private static void Setup
		(
			IEnumerable<Texture2D>     textureList,
			DisplayProgressBarCallback onDisplayProgressBarPreprocess = default,
			DisplayProgressBarCallback onDisplayProgressBarProcessing = default,
			ClearProgressBarCallback   onClearProgressBar             = default,
			Action                     onComplete                     = default
		)
		{
			var list = textureList
					.Select( c => new TextureData( c ) )
					.ToArray()
				;

			var count = list.Length;

			try
			{
				foreach ( var ( index, val ) in list.Select( ( val, index ) => ( index, val ) ) )
				{
					onDisplayProgressBarPreprocess?.Invoke( index + 1, count, val.Path );

					var importer = val.Importer;
					importer.isReadable = true;
					importer.SaveAndReimport();
				}
			}
			finally
			{
				onClearProgressBar?.Invoke();
			}

			// isReadable を true にしてからじゃないと TextureSlicer.Slice が使用できないため
			// isReadable を true にして 1 フレーム待機してから
			// TextureSlicer.Slice を使用しています
			EditorApplication.delayCall += () =>
			{
				try
				{
					foreach ( var ( index, val ) in list.Select( ( val, index ) => ( index, val ) ) )
					{
						onDisplayProgressBarProcessing?.Invoke( index + 1, count, val.Path );

						var slicedTexture = TextureSlicer.Slice( val.Texture );
						var importer      = val.Importer;
						importer.spriteBorder = slicedTexture.Boarder.ToVector4();
						importer.isReadable   = false;
						importer.SaveAndReimport();
					}
				}
				finally
				{
					onClearProgressBar?.Invoke();
					onComplete?.Invoke();
				}
			};
		}
	}
}