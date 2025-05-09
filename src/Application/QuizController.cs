using Microsoft.AspNetCore.Mvc;
using Repositories;
namespace Application;

[ApiController]
[Route("api/quizzes")]
public class QuizController : ControllerBase
{
    private readonly IQuizRepository _quizRepository;

    public QuizController(IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllQuizzesAsync()
    {
        var quizzes = await _quizRepository.GetAllAsync();
        return Ok(quizzes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetQuizByIdAsync(int id)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz == null)
            return NotFound();
        return Ok(quiz);
    }

    [HttpPost]
    public async Task<IActionResult> AddQuizAsync([FromBody] CreateQuizDto quizDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var quizId = await _quizRepository.AddAsync(quizDto);
        return CreatedAtAction(nameof(GetQuizByIdAsync), new { id = quizId }, null);
    }
}