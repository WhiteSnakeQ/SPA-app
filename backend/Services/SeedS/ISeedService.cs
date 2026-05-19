
using SPA_приложение.Models;

namespace SPA_app.Services.SeedS
{
    public interface ISeedService
    {
        Task<List<Comment>?> SeedComments(int number = 35);
    }
}
