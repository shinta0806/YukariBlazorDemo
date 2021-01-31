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
		// ====================================================================
		// public static メンバー関数
		// ====================================================================

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
		// DateTime→修正ユリウス日
		// --------------------------------------------------------------------
		public static Double DateTimeToModifiedJulianDate(DateTime dateTime)
		{
			return DateTimeToJulianDay(dateTime) - MJD_DELTA;
		}

		// --------------------------------------------------------------------
		// クエリパラメーターからページを取得する
		// クエリパラメーターでは 1 ベースだが、取得時は 0 ベースとなる
		// 例）page=2 からは 1 が取得される
		// --------------------------------------------------------------------
		public static Int32 GetPageFromQueryParameters(Dictionary<String, String> parameters)
		{
			parameters.TryGetValue(YbdConstants.SEARCH_PARAM_NAME_PAGE, out String? paramValue);
			_ = Int32.TryParse(paramValue, out Int32 paramValueNum);
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
		// 修正ユリウス日→DateTime
		// --------------------------------------------------------------------
		public static DateTime ModifiedJulianDateToDateTime(Double mjd)
		{
			return JulianDayToDateTime(mjd + MJD_DELTA);
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

		// --------------------------------------------------------------------
		// 現在時刻（UTC）の修正ユリウス日
		// --------------------------------------------------------------------
		public static Double UtcNowModifiedJulianDate()
		{
			return DateTimeToModifiedJulianDate(DateTime.UtcNow);
		}

		// ====================================================================
		// private static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// DateTime→（修正じゃない）ユリウス日
		// --------------------------------------------------------------------
		private static Double DateTimeToJulianDay(DateTime dateTime)
		{
			return (dateTime.Ticks + (JULIAN_BASE * TICKS_PER_DAY)) / TICKS_PER_DAY;
		}

		// --------------------------------------------------------------------
		// （修正じゃない）ユリウス日→DateTime
		// --------------------------------------------------------------------
		private static DateTime JulianDayToDateTime(Double julianDay)
		{
			return new DateTime((Int64)((julianDay - JULIAN_BASE) * TICKS_PER_DAY), DateTimeKind.Utc);
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
		//private const String NORMALIZE_DB_STRING_FROM = "　\u2019ｧｨｩｪｫｯｬｭｮﾞﾟ｡｢｣､･~\u301C" + NORMALIZE_DB_FORBIDDEN_FROM;
		//private const String NORMALIZE_DB_STRING_TO = " 'ァィゥェォッャュョ゛゜。「」、・～～" + NORMALIZE_DB_FORBIDDEN_TO;

		// NormalizeXXX() 用：変換後がフリガナ対象の禁則文字（半角カタカナ）
		private const String NORMALIZE_DB_FORBIDDEN_FROM = "ｦｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝ";
		private const String NORMALIZE_DB_FORBIDDEN_TO = "ヲーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン";

		// --------------------------------------------------------------------
		// ユリウス日
		// --------------------------------------------------------------------

		// 1 日を Ticks（100 ナノ秒）で表した数値
		private const Int64 TICKS_PER_DAY = 24L * 60 * 60 * 1000 * 1000 * 10;

		// 0001/01/01 00:00 をユリウス日で表した数値
		private const Double JULIAN_BASE = 1721425.5;

		// 修正ユリウス日 = ユリウス通日 - MJD_DELTA
		private const Double MJD_DELTA = 2400000.5;

	}
}
