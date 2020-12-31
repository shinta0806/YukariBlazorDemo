// ============================================================================
// 
// 検索条件
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Web;

namespace YukariBlazorDemo.Shared
{
	public class SearchWord : ISongProperty
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター（デフォルト）
		// --------------------------------------------------------------------
		public SearchWord()
		{
			InitDetailValues();
		}

		// --------------------------------------------------------------------
		// コンストラクター（URL パラメーター）
		// --------------------------------------------------------------------
		public SearchWord(String? query)
		{
			InitDetailValues();

			if (String.IsNullOrEmpty(query))
			{
				return;
			}

			Dictionary<String, String> parameters = YbdCommon.AnalyzeQuery(query);
			Page = YbdCommon.GetPageFromQueryParameters(parameters);
			foreach (KeyValuePair<String, String> param in parameters)
			{
				String paramName = param.Key;
				String paramValue = param.Value;
				if (paramName == YbdConstants.SEARCH_PARAM_NAME_ANY_WORD)
				{
					AnyWord = paramValue;
				}
				else if (paramName == YbdConstants.SEARCH_PARAM_NAME_SORT)
				{
					Int32.TryParse(paramValue, out Int32 paramValueNum);
					Sort = (SearchResultSort)Math.Clamp(paramValueNum, 0, (Int32)(SearchResultSort.__End__) - 1);
				}
				else
				{
					Int32 paramIndex = YbdConstants.SEARCH_DETAIL_PARAM_NAMES.ToList().IndexOf(paramName);
					if (paramIndex >= 0)
					{
						DetailValues[paramIndex] = paramValue;
					}
				}
			}
			Type = String.IsNullOrEmpty(AnyWord) ? SearchWordType.Detail : SearchWordType.AnyWord;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 検索方法
		public SearchWordType Type { get; set; } = SearchWordType.AnyWord;

		// キーワード
		[SearchWord()]
		public String AnyWord { get; set; } = String.Empty;

		// 詳細条件：曲名
		public String SongName
		{
			get => DetailValues[(Int32)SearchDetailCondition.SongName];
			set => DetailValues[(Int32)SearchDetailCondition.SongName] = value;
		}

		// 詳細条件：タイアップ名
		public String TieUpName
		{
			get => DetailValues[(Int32)SearchDetailCondition.TieUpName];
			set => DetailValues[(Int32)SearchDetailCondition.TieUpName] = value;
		}

		// 詳細条件：歌手名
		public String ArtistName
		{
			get => DetailValues[(Int32)SearchDetailCondition.ArtistName];
			set => DetailValues[(Int32)SearchDetailCondition.ArtistName] = value;
		}

		// 詳細条件：制作会社
		public String MakerName
		{
			get => DetailValues[(Int32)SearchDetailCondition.MakerName];
			set => DetailValues[(Int32)SearchDetailCondition.MakerName] = value;
		}

		// 詳細条件：動画制作者
		public String Worker
		{
			get => DetailValues[(Int32)SearchDetailCondition.Worker];
			set => DetailValues[(Int32)SearchDetailCondition.Worker] = value;
		}

		// 詳細条件：動画ファイル名
		public String Path
		{
			get => DetailValues[(Int32)SearchDetailCondition.Path];
			set => DetailValues[(Int32)SearchDetailCondition.Path] = value;
		}

		// 詳細条件：配列アクセス用
		public String[] DetailValues { get; set; } = new String[(Int32)SearchDetailCondition.__End__];

		// 検索結果ソート方法
		public SearchResultSort Sort { get; set; }

		// 表示ページ
		// 0 ベースだが、URL パラメーターにする際は 1 ベースに変換する
		public Int32 Page { get; set; }

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 検索条件の並び替えを変更したインスタンスを返す
		// --------------------------------------------------------------------
		public SearchWord ChangeSort(SearchResultSort sort)
		{
			SearchWord change = this.DeepCopy();
			change.Sort = sort;
			return change;
		}

		// --------------------------------------------------------------------
		// 検索方法を変更したインスタンスを返す
		// --------------------------------------------------------------------
		public SearchWord ChangeType(SearchWordType type, SearchDetailCondition detailCondition, out String activeCondition)
		{
			activeCondition = YbdConstants.SEARCH_ANY_WORD_CONDITION_NAME;

			// 既存の検索キーワードを確認
			String activeKeyword;
			if (Type == SearchWordType.AnyWord)
			{
				activeKeyword = AnyWord;
			}
			else
			{
				Int32 numConditions = DetailValues.Where(x => x != String.Empty).Count();
				if (numConditions == 1)
				{
					if (!String.IsNullOrEmpty(SongName))
					{
						activeCondition = YbdConstants.SEARCH_DETAIL_CONDITION_NAMES[(Int32)SearchDetailCondition.SongName];
						activeKeyword = SongName;
					}
					else if (!String.IsNullOrEmpty(TieUpName))
					{
						activeCondition = YbdConstants.SEARCH_DETAIL_CONDITION_NAMES[(Int32)SearchDetailCondition.TieUpName];
						activeKeyword = TieUpName;
					}
					else if (!String.IsNullOrEmpty(ArtistName))
					{
						activeCondition = YbdConstants.SEARCH_DETAIL_CONDITION_NAMES[(Int32)SearchDetailCondition.ArtistName];
						activeKeyword = ArtistName;
					}
					else
					{
						activeKeyword = DetailValues.Where(x => x != String.Empty).First();
					}
				}
				else
				{
					activeKeyword = DetailValues.Where(x => x != String.Empty).First();
				}
			}

			SearchWord change = this.DeepCopy();
			change.Type = type;
			change.InitDetailValues();
			if (change.Type == SearchWordType.AnyWord)
			{
				change.AnyWord = activeKeyword;
			}
			else
			{
				change.DetailValues[(Int32)detailCondition] = activeKeyword;
			}
			return change;
		}

		// --------------------------------------------------------------------
		// 詳細コピー
		// --------------------------------------------------------------------
		public SearchWord DeepCopy()
		{
			// 簡易コピー
			SearchWord copy = (SearchWord)MemberwiseClone();

			// 詳細コピー
			copy.DetailValues = new String[(Int32)SearchDetailCondition.__End__];
			for (Int32 i = 0; i < DetailValues.Length; i++)
			{
				copy.DetailValues[i] = DetailValues[i];
			}

			return copy;
		}

		// --------------------------------------------------------------------
		// 必須項目が格納されているか
		// --------------------------------------------------------------------
		public Boolean IsValid([NotNullWhen(false)] out String? errorMessage)
		{
			Boolean valid = true;
			errorMessage = null;

			switch (Type)
			{
				case SearchWordType.AnyWord:
					if (String.IsNullOrEmpty(AnyWord))
					{
						valid = false;
						errorMessage = "なんでも検索のキーワードを入力してください。";
					}
					break;
				case SearchWordType.Detail:
					if (String.IsNullOrEmpty(Path) && String.IsNullOrEmpty(SongName) && String.IsNullOrEmpty(TieUpName) && String.IsNullOrEmpty(ArtistName)
							&& String.IsNullOrEmpty(MakerName) && String.IsNullOrEmpty(Worker))
					{
						valid = false;
						errorMessage = "詳細検索の条件を入力してください。";
					}
					break;
				default:
					valid = false;
					errorMessage = "検索方法を指定してください。";
					break;
			}

			return valid;
		}

		// --------------------------------------------------------------------
		// ページを 0 にしたインスタンスを返す
		// --------------------------------------------------------------------
		public SearchWord ResetPage()
		{
			SearchWord change = this.DeepCopy();
			change.Page = 0;
			return change;
		}

		// --------------------------------------------------------------------
		// URL パラメーターとして出力
		// --------------------------------------------------------------------
		public override String? ToString()
		{
			String str = String.Empty;
			if (Type == SearchWordType.AnyWord)
			{
				AddString(ref str, YbdConstants.SEARCH_PARAM_NAME_ANY_WORD, AnyWord);
			}
			else
			{
				for (Int32 i = 0; i < (Int32)SearchDetailCondition.__End__; i++)
				{
					AddString(ref str, YbdConstants.SEARCH_DETAIL_PARAM_NAMES[i], DetailValues[i]);
				}
			}
			if (Sort != 0)
			{
				AddString(ref str, YbdConstants.SEARCH_PARAM_NAME_SORT, ((Int32)Sort).ToString());
			}
			if (Page != 0)
			{
				AddString(ref str, YbdConstants.SEARCH_PARAM_NAME_PAGE, (Page + 1).ToString());
			}
			return str;
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// URL パラメーター追加
		// --------------------------------------------------------------------
		private void AddString(ref String str, String paramName, String? paramValue)
		{
			if (String.IsNullOrEmpty(paramValue))
			{
				return;
			}

			if (!String.IsNullOrEmpty(str))
			{
				str += "&";
			}
			str += paramName + "=" + Uri.EscapeDataString(paramValue);
		}

		// --------------------------------------------------------------------
		// 詳細条件初期化
		// --------------------------------------------------------------------
		private void InitDetailValues()
		{
			for (Int32 i = 0; i < (Int32)SearchDetailCondition.__End__; i++)
			{
				DetailValues[i] = String.Empty;
			}
		}

		// ====================================================================
		// SearchWord 用の入力チェッカー
		// ====================================================================

		private class SearchWordAttribute : ValidationAttribute
		{
			protected override ValidationResult? IsValid(Object? value, ValidationContext validationContext)
			{
				if (validationContext.ObjectInstance is SearchWord searchWord)
				{
					Boolean valid = searchWord.IsValid(out String? errorMessage);
					if (valid)
					{
						return ValidationResult.Success;
					}
					return new ValidationResult(errorMessage);
				}
				return new ValidationResult("内部エラー：型が不適切です：" + validationContext.ObjectInstance?.GetType().Name);
			}
		}

	}


}
