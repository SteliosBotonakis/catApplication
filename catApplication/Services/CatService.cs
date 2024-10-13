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
            RequestUri = new Uri($"https://api.thecatapi.com/v1/images/search?size=med&mime_types=jpg&format=json&has_breeds=true&order=RANDOM&page=0&limit={count}")
        };
        request.Headers.Add("x-api-key", _configuration["CatApi:ApiKey"]);
    
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to fetch cats from API.");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        var rawCatDtos = JsonConvert.DeserializeObject<List<dynamic>>(responseContent);

        var catDtos = rawCatDtos.Select(cat => new CatDto
        {
            Id = cat.id,
            Width = (int)cat.width,
            Height = (int)cat.height,
            Url = cat.url,
            Tags = cat.breeds != null 
                ? ((IEnumerable<dynamic>)cat.breeds)
                .SelectMany(breed => ((string)breed.temperament).Split(','))
                .Select(tag => tag.Trim())
                .ToList()
                : new List<string>()
        }).ToList();


        foreach (var catDto in catDtos)
        {
            var catExists = await _context.Cats.AnyAsync(c => c.CatId == catDto.Id);
            if (!catExists)
            {
                var tags = await ParseBreedsToTags(catDto.Tags);

                var newCat = new CatEntity
                {
                    CatId = catDto.Id,
                    Width = catDto.Width,
                    Height = catDto.Height,
                    Image = catDto.Url,
                    Created = DateTime.UtcNow,
                    Tags = tags
                };

                _context.Cats.Add(newCat);
            }
        }

        await _context.SaveChangesAsync();
    }


    
    public async Task<CatDto> GetCatByIdAsync(string id)
    {
        var catEntity = await _context.Cats
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.CatId == id);

        if (catEntity == null)
        {
            return null;
        }
        
        return new CatDto
        {
            Id = catEntity.CatId,
            Width = catEntity.Width,
            Height = catEntity.Height,
            Url = catEntity.Image,
            Tags = catEntity.Tags.Select(t => t.Name).ToList()
        };
    }

    public async Task<List<CatDto>> GetCatsAsync(int page, int pageSize)
    {
        return await _context.Cats
            .Include(c => c.Tags)
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CatDto
            {
                Id = c.CatId,
                Width = c.Width,
                Height = c.Height,
                Url = c.Image,
                Tags = c.Tags.Select(t => t.Name).ToList()
            })
            .ToListAsync();
    }


    
    public async Task<List<CatDto>> GetCatsByTagAsync(string tag, int page, int pageSize)
    {
        return await _context.Cats
            .Include(c => c.Tags)
            .Where(c => c.Tags.Any(t => t.Name == tag))
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CatDto
            {
                Id = c.CatId,
                Width = c.Width,
                Height = c.Height,
                Url = c.Image,
                Tags = c.Tags.Select(t => t.Name).ToList()
            })
            .ToListAsync();
    }


    private async Task<List<TagEntity>> ParseBreedsToTags(List<string> tagNames)
    {
        var tags = new List<TagEntity>();

        foreach (var tagName in tagNames)
        {
            var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
        
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

        return tags;
    }


}