using catApplication.Configuration;

public interface ICatService
{
    Task FetchAndSaveCatsAsync(int count);
    Task<CatEntity> GetCatByIdAsync(int id);
    Task<List<CatEntity>> GetCatsAsync(int page, int pageSize);
    Task<List<CatEntity>> GetCatsByTagAsync(string tag, int page, int pageSize);
}