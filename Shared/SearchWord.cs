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
	public class SearchWord
	{
		public SearchWord()
		{

		}

		public SearchWord(String? query)
		{
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
				switch (paramName)
				{
					case PARAM_NAME_ANY_WORD:
						AnyWord = paramValue;
						break;
					case PARAM_NAME_FILE_NAME:
						FileName = paramValue;
						break;
					case PARAM_NAME_SONG_NAME:
						SongName = paramValue;
						break;
					case PARAM_NAME_TIE_UP_NAME:
						TieUpName = paramValue;
						break;
				}
			}
			Type = String.IsNullOrEmpty(AnyWord) ? SearchWordType.Detail : SearchWordType.AnyWord;
		}


		public SearchWordType Type { get; set; } = SearchWordType.AnyWord;

		//public Boolean ByAnyWord { get; set; }

		//[Required(ErrorMessage = "キーワードを入力してください。")]
		[SearchWord()]
		public String? AnyWord { get; set; }

		//public Boolean ByDetail { get; set; }

		public String? FileName { get; set; }

		//[Required(ErrorMessage = "曲名を入力してください。")]
		public String? SongName { get; set; }

		public String? TieUpName { get; set; }

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
					if (String.IsNullOrEmpty(FileName) && String.IsNullOrEmpty(SongName) && String.IsNullOrEmpty(TieUpName))
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

#if false
		[Compare(nameof(True), ErrorMessage = "キーワードまたは、詳細検索条件を入力してください。")]
		public Boolean IsValid
		{
			get
			{
				return !String.IsNullOrEmpty(AnyWord)
						|| !String.IsNullOrEmpty(FileName) || !String.IsNullOrEmpty(SongName) || !String.IsNullOrEmpty(TieUpName);
			}
		}

		public Boolean True
		{
			get => true;
		}
#endif

		public override String? ToString()
		{
			if (Type == SearchWordType.AnyWord)
			{
				return PARAM_NAME_ANY_WORD + "=" + HttpUtility.UrlEncode(AnyWord, Encoding.UTF8);
			}

			String str = String.Empty;
			AddString(ref str, PARAM_NAME_FILE_NAME, FileName);
			AddString(ref str, PARAM_NAME_SONG_NAME, SongName);
			AddString(ref str, PARAM_NAME_TIE_UP_NAME, TieUpName);
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

		private const String PARAM_NAME_ANY_WORD = "anyword";
		private const String PARAM_NAME_FILE_NAME = "filename";
		private const String PARAM_NAME_SONG_NAME = "songname";
		private const String PARAM_NAME_TIE_UP_NAME = "tieupname";


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
