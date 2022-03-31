using System.Text;
using AutoMapper;
using Tantalus.Data;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IFoodService {
    Task<FoodResponse> AddFood(FoodRequest foodRequest, Guid userId);
}

public class FoodService : IFoodService {
    
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public FoodService(DataContext dataContext, IMapper mapper) {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public async Task<FoodResponse> AddFood(FoodRequest foodRequest, Guid userId) {
        
        // add missing data when necessary
        var food = _mapper.Map<Food>(foodRequest);
        food.Id = Guid.NewGuid();
        food.ShortUrl = ShortenUrl(food.FullName);
        food.UserId = userId;
        
        // insert into "Foods" ("Id", "FullName", "ShortUrl", "UserId") values (gen_random_uuid (), 'Banana', 'banana', 'e1bcbc54-52bd-4618-9f27-deef344f9f57')
        await _dataContext.Foods.AddAsync(food);
        await _dataContext.SaveChangesAsync();
        return _mapper.Map<FoodResponse>(food);
    }
    
    private static string ShortenUrl(string url) {

        const int maxLength = 50;           // tk export
        var previousDash = false;
        var stringBuilder = new StringBuilder(url.Length);

        for (var i = 0; i < url.Length; i++) {
            var character = url[i];
            switch (character) {
                case >= 'a' and <= 'z' or >= '0' and <= '9':
                    stringBuilder.Append(character);
                    previousDash = false;
                    break;
                case >= 'A' and <= 'Z':
                    stringBuilder.Append((char)(character | 32));       // convert to lowercase
                    previousDash = false;
                    break;
                case ' ':
                case ',':
                case '.':
                case '/':
                case '\\':
                case '-':
                case '_':
                case '=': {
                    if (!previousDash && stringBuilder.Length > 0) {
                        stringBuilder.Append('-');
                        previousDash = true;
                    }

                    break;
                }
                default: {
                    if (character >= 128) {
                        var previousLength = stringBuilder.Length;
                        stringBuilder.Append(AsciiSubstitute(character));
                        if (previousLength != stringBuilder.Length) previousDash = false;
                    }

                    break;
                }
            }
            if (i == maxLength) break;
        }

        return previousDash ? stringBuilder.ToString()[..(stringBuilder.Length - 1)] : stringBuilder.ToString();
    }
    
    // not the fastest implementation, but easy to maintain and update
    // tk look for builtin methods
    private static string AsciiSubstitute(char character) {
        var s = character.ToString().ToLowerInvariant();
        
        if ("àåáâäãåą".Contains(s)) return "a";
        if ("èéêëę".Contains(s)) return "e";
        if ("ìíîïı".Contains(s)) return "i";
        if ("òóôõöøőð".Contains(s)) return "o";
        if ("ùúûüŭů".Contains(s)) return "u";
        if ("çćčĉ".Contains(s)) return "c";
        if ("żźž".Contains(s)) return "z";
        if ("śşšŝ".Contains(s)) return "s";
        if ("ñń".Contains(s)) return "n";
        if ("ýÿ".Contains(s)) return "y";
        if ("ğĝ".Contains(s)) return "g";
        
        return character switch {
            'ß' => "ss",
            'Þ' => "th",
            'ĥ' => "h",
            'ĵ' => "j",
            _ => ""
        };
    }
    
}