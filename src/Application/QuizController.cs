using API;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Application;

[Microsoft.AspNetCore.Components.Route("api/quizzes")]
public class QuizController : IQuizController
{
    [HttpGet]
    public Task<IEnumerable<Quiz>> GetAllQuizesAsync()
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}")]
    public Task<Quiz> GetQuizByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public Task<bool> AddQuizAsync([FromBody] CreateQuizzDto quiz)
    {
        throw new NotImplementedException();
    }
}