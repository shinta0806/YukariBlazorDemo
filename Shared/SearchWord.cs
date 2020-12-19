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
						Path = paramValue;
						break;
					case PARAM_NAME_SONG_NAME:
						SongName = paramValue;
						break;
					case PARAM_NAME_TIE_UP_NAME:
						TieUpName = paramValue;
						break;
					case PARAM_NAME_ARTIST_NAME:
						ArtistName = paramValue;
						break;
					case PARAM_NAME_MAKER:
						Maker = paramValue;
						break;
					case PARAM_NAME_WORKER:
						Worker = paramValue;
						break;
				}
			}
			Type = String.IsNullOrEmpty(AnyWord) ? SearchWordType.Detail : SearchWordType.AnyWord;
		}


		public SearchWordType Type { get; set; } = SearchWordType.AnyWord;

		[SearchWord()]
		public String? AnyWord { get; set; }

		public String Path { get; set; } = String.Empty;

		public String SongName { get; set; } = String.Empty;

		public String TieUpName { get; set; } = String.Empty;

		public String ArtistName { get; set; } = String.Empty;

		public String Maker { get; set; } = String.Empty;

		public String Worker { get; set; } = String.Empty;

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
			if (Type == SearchWordType.AnyWord)
			{
				return PARAM_NAME_ANY_WORD + "=" + HttpUtility.UrlEncode(AnyWord, Encoding.UTF8);
			}

			String str = String.Empty;
			AddString(ref str, PARAM_NAME_FILE_NAME, Path);
			AddString(ref str, PARAM_NAME_SONG_NAME, SongName);
			AddString(ref str, PARAM_NAME_TIE_UP_NAME, TieUpName);
			AddString(ref str, PARAM_NAME_ARTIST_NAME, ArtistName);
			AddString(ref str, PARAM_NAME_MAKER, Maker);
			AddString(ref str, PARAM_NAME_WORKER, Worker);
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
		private const String PARAM_NAME_ARTIST_NAME = "artistname";
		private const String PARAM_NAME_MAKER = "maker";
		private const String PARAM_NAME_WORKER = "worker";


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
