using Models;

namespace Application;

public interface IQuizController
{
    public Task<IEnumerable<Quiz>> GetAllQuizesAsync();
    public Task<Quiz> GetQuizByIdAsync(int id);
    public Task<bool> AddQuizAsync(CreateQuizDto quiz);
}