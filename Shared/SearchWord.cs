using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace YukariBlazorDemo.Shared
{
	public class SearchWord : ISongProperty
	{
		public SearchWord()
		{
			InitDetailValues();
		}

		public SearchWord(String? query)
		{
			InitDetailValues();

			if (String.IsNullOrEmpty(query))
			{
				return;
			}

			String[] parameters = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
			foreach (String param in parameters)
			{
				String[] elements = param.Split('=', StringSplitOptions.RemoveEmptyEntries);
				if (elements.Length < 2)
				{
					continue;
				}
				String paramName = elements[0];
				String paramValue = HttpUtility.UrlDecode(elements[1], Encoding.UTF8);
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


		public SearchWordType Type { get; set; } = SearchWordType.AnyWord;

		[SearchWord()]
		public String AnyWord { get; set; } = String.Empty;

		public String SongName
		{
			get => DetailValues[(Int32)SearchDetailCondition.SongName];
			set => DetailValues[(Int32)SearchDetailCondition.SongName] = value;
		}

		public String TieUpName
		{
			get => DetailValues[(Int32)SearchDetailCondition.TieUpName];
			set => DetailValues[(Int32)SearchDetailCondition.TieUpName] = value;
		}

		public String ArtistName
		{
			get => DetailValues[(Int32)SearchDetailCondition.ArtistName];
			set => DetailValues[(Int32)SearchDetailCondition.ArtistName] = value;
		}

		public String Maker
		{
			get => DetailValues[(Int32)SearchDetailCondition.Maker];
			set => DetailValues[(Int32)SearchDetailCondition.Maker] = value;
		}

		public String Worker
		{
			get => DetailValues[(Int32)SearchDetailCondition.Worker];
			set => DetailValues[(Int32)SearchDetailCondition.Worker] = value;
		}

		public String Path
		{
			get => DetailValues[(Int32)SearchDetailCondition.Path];
			set => DetailValues[(Int32)SearchDetailCondition.Path] = value;
		}

		public String[] DetailValues { get; set; } = new String[(Int32)SearchDetailCondition.__End__];

		public SearchResultSort Sort { get; set; }

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
							&& String.IsNullOrEmpty(Maker) && String.IsNullOrEmpty(Worker))
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
			return str;
		}

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
			str += paramName + "=" + HttpUtility.UrlEncode(paramValue, Encoding.UTF8);
		}

		private void InitDetailValues()
		{
			for (Int32 i = 0; i < (Int32)SearchDetailCondition.__End__; i++)
			{
				DetailValues[i] = String.Empty;
			}
		}




		class SearchWordAttribute : ValidationAttribute
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
				return new ValidationResult("入力が不適切です：" + validationContext.ObjectInstance?.GetType().Name);
			}
		}

	}


}
