// ============================================================================
// 
// YukariBlazorDemo 共通で使用する関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace YukariBlazorDemo.Shared.Misc
{
	public class YbdCommon
	{
		// --------------------------------------------------------------------
		// URL のクエリ部分をパラメーター名と値に分離
		// --------------------------------------------------------------------
		public static Dictionary<String, String> AnalyzeQuery(String? query)
		{
			Dictionary<String, String> result = new();
			if (!String.IsNullOrEmpty(query))
			{
				String[] parameters = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
				foreach (String param in parameters)
				{
					String[] elements = param.Split('=', StringSplitOptions.RemoveEmptyEntries);
					if (elements.Length < 2)
					{
						continue;
					}
					String paramName = elements[0];
					String paramValue = Uri.UnescapeDataString(elements[1]);
					result[paramName] = paramValue;
				}
			}
			return result;
		}

		// --------------------------------------------------------------------
		// クエリパラメーターからページを取得する
		// クエリパラメーターでは 1 ベースだが、取得時は 0 ベースとなる
		// 例）page=2 からは 1 が取得される
		// --------------------------------------------------------------------
		public static Int32 GetPageFromQueryParameters(Dictionary<String, String> parameters)
		{
			parameters.TryGetValue(YbdConstants.SEARCH_PARAM_NAME_PAGE, out String? paramValue);
			Int32.TryParse(paramValue, out Int32 paramValueNum);
			return Math.Max(paramValueNum - 1, 0);
		}

		// --------------------------------------------------------------------
		// フリガナのみの文字列かどうか
		// 返値が true の場合、ruby には正規化されたルビが格納される
		// --------------------------------------------------------------------
		public static Boolean IsRuby(String source, out String ruby)
		{
			if (String.IsNullOrEmpty(source))
			{
				ruby = String.Empty;
				return false;
			}

			ruby = NormalizeRuby(source);
			return source.Length == ruby.Length;
		}

		// --------------------------------------------------------------------
		// フリガナの表記揺れを減らす
		// --------------------------------------------------------------------
		public static String NormalizeRuby(String source)
		{
			Debug.Assert(NORMALIZE_DB_RUBY_FROM.Length == NORMALIZE_DB_RUBY_TO.Length, "NormalizeRuby() different NORMALIZE_DB_FURIGANA_FROM NORMALIZE_DB_FURIGANA_TO length");

			if (String.IsNullOrEmpty(source))
			{
				return String.Empty;
			}

			StringBuilder katakana = new StringBuilder();

			for (Int32 i = 0; i < source.Length; i++)
			{
				Char chara = source[i];

				// 小文字・半角カタカナ等を全角カタカナに変換
				Int32 pos = NORMALIZE_DB_RUBY_FROM.IndexOf(chara);
				if (pos >= 0)
				{
					katakana.Append(NORMALIZE_DB_RUBY_TO[pos]);
					continue;
				}

				// 上記以外の全角カタカナ・音引きはそのまま
				if ('ア' <= chara && chara <= 'ン' || chara == 'ー')
				{
					katakana.Append(chara);
					continue;
				}

				// 上記以外のひらがなをカタカナに変換
				if ('あ' <= chara && chara <= 'ん')
				{
					katakana.Append((Char)(chara + 0x60));
					continue;
				}

				// その他の文字は無視する
			}

			return katakana.ToString();
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

		// --------------------------------------------------------------------
		// 文字正規化
		// --------------------------------------------------------------------

		// NormalizeRuby() 用：フリガナ正規化対象文字（小文字・濁点のカナ等）
		private const String NORMALIZE_DB_RUBY_FROM = "ァィゥェォッャュョヮヵヶガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポヰヱヴヷヸヹヺｧｨｩｪｫｯｬｭｮ"
				+ "ぁぃぅぇぉっゃゅょゎゕゖがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽゐゑゔ" + NORMALIZE_DB_FORBIDDEN_FROM;
		private const String NORMALIZE_DB_RUBY_TO = "アイウエオツヤユヨワカケカキクケコサシスセソタチツテトハヒフヘホハヒフヘホイエウワイエヲアイウエオツヤユヨ"
				+ "アイウエオツヤユヨワカケカキクケコサシスセソタチツテトハヒフヘホハヒフヘホイエウ" + NORMALIZE_DB_FORBIDDEN_TO;

		// NormalizeString() 用：禁則文字（全角スペース、一部の半角文字等）
		private const String NORMALIZE_DB_STRING_FROM = "　\u2019ｧｨｩｪｫｯｬｭｮﾞﾟ｡｢｣､･~\u301C" + NORMALIZE_DB_FORBIDDEN_FROM;
		private const String NORMALIZE_DB_STRING_TO = " 'ァィゥェォッャュョ゛゜。「」、・～～" + NORMALIZE_DB_FORBIDDEN_TO;

		// NormalizeXXX() 用：変換後がフリガナ対象の禁則文字（半角カタカナ）
		private const String NORMALIZE_DB_FORBIDDEN_FROM = "ｦｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝ";
		private const String NORMALIZE_DB_FORBIDDEN_TO = "ヲーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン";

	}
}
