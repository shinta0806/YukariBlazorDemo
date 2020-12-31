// ============================================================================
// 
// サーバー側で共通して使われる関数
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.Net.Http.Headers;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace YukariBlazorDemo.Server.Misc
{
	public class ServerCommon
	{
		// ====================================================================
		// public static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// DateTime→修正ユリウス日
		// --------------------------------------------------------------------
		public static Double DateTimeToModifiedJulianDate(DateTime dateTime)
		{
			return DateTimeToJulianDay(dateTime) - MJD_DELTA;
		}

		// --------------------------------------------------------------------
		// ETag 生成
		// --------------------------------------------------------------------
		public static EntityTagHeaderValue GenerateEntityTag(Double lastModified)
		{
			return new EntityTagHeaderValue("\"" + lastModified.ToString() + "\"");
		}

		// --------------------------------------------------------------------
		// ETag 生成（パラメーター 1 つ）
		// --------------------------------------------------------------------
		public static EntityTagHeaderValue GenerateEntityTag(Double lastModified, String paramName, String paramValue)
		{
			return new EntityTagHeaderValue("\"" + lastModified.ToString() + "&" + paramName + "=" + paramValue + "\"");
		}

		// --------------------------------------------------------------------
		// ファイルの更新日時（UTC）
		// --------------------------------------------------------------------
		public static DateTime LastModified(String path)
		{
			FileInfo fileInfo = new FileInfo(path);
			return fileInfo.LastWriteTimeUtc;
		}

		// --------------------------------------------------------------------
		// 修正ユリウス日→DateTime
		// --------------------------------------------------------------------
		public static DateTime ModifiedJulianDateToDateTime(Double mjd)
		{
			return JulianDayToDateTime(mjd + MJD_DELTA);
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
			return new DateTime((Int64)((julianDay - JULIAN_BASE) * TICKS_PER_DAY));
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================

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
