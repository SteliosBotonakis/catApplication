using catApplication.Configuration;
using catApplication.Dtos;

public interface ICatService
{
    Task FetchAndSaveCatsAsync(int count);
    Task<CatDto> GetCatByIdAsync(string id);
    Task<List<CatDto>> GetCatsAsync(int page, int pageSize);
    Task<List<CatDto>> GetCatsByTagAsync(string tag, int page, int pageSize);
}