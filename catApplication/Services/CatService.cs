using System.Text.Json;
using catApplication.Configuration;
using catApplication.Dtos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace catApplication.Services;

public class CatService : ICatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly DatabaseContext _context;
    
    public CatService(HttpClient httpClient, IConfiguration configuration, DatabaseContext context)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _context = context;
    }
    
    public async Task FetchAndSaveCatsAsync(int count)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://api.thecatapi.com/v1/images/search?size=med&mime_types=jpg&format=json&has_breeds=true&order=RANDOM&page=0&limit={count}"),
        };
        request.Headers.Add("x-api-key", _configuration["CatApi:ApiKey"]);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to fetch cats from API.");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var catDtos = JsonConvert.DeserializeObject<List<CatDto>>(responseContent);

        foreach (var catDto in catDtos)
        {
            // Check if the cat is already in the database by its CatId
            var exists = await _context.Cats.AnyAsync(c => c.CatId == catDto.Id);
            if (!exists)
            {
                // Create the new CatEntity
                var newCat = new CatEntity
                {
                    CatId = catDto.Id,
                    Width = catDto.Width,
                    Height = catDto.Height,
                    Image = catDto.Url,
                    Created = DateTime.UtcNow,
                    Tags = ParseBreedsToTags(catDto.Breeds)
                };

                _context.Cats.Add(newCat);
            }
        }

        await _context.SaveChangesAsync();
    }
    
    public async Task<CatEntity> GetCatByIdAsync(int id)
    {
        return await _context.Cats
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<CatEntity>> GetCatsAsync(int page, int pageSize)
    {
        return await _context.Cats
            .Include(c => c.Tags)
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    
    public async Task<List<CatEntity>> GetCatsByTagAsync(string tag, int page, int pageSize)
    {
        return await _context.Cats
            .Include(c => c.Tags)
            .Where(c => c.Tags.Any(t => t.Name == tag))
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }


    private List<TagEntity> ParseBreedsToTags(List<BreedDto> breeds)
    {
        var tags = new List<TagEntity>();

        foreach (var breed in breeds)
        {
            var temperaments = breed.Temperament.Split(','); // Splitting temperaments by comma
            foreach (var temperament in temperaments)
            {
                var tagName = temperament.Trim(); // Remove spaces
                var existingTag = _context.Tags.FirstOrDefault(t => t.Name == tagName);
                if (existingTag != null)
                {
                    tags.Add(existingTag);
                }
                else
                {
                    var newTag = new TagEntity
                    {
                        Name = tagName,
                        Created = DateTime.UtcNow
                    };
                    _context.Tags.Add(newTag);
                    tags.Add(newTag);
                }
            }
        }

        return tags;
    }

}