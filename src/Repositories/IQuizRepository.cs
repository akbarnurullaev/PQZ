using Application;

namespace Repositories;

public interface IQuizRepository
{
    Task<IEnumerable<object>> GetAllAsync();
    Task<object?> GetByIdAsync(int id);
    Task<int> AddAsync(CreateQuizDto quizDto);
}