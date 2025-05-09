using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Application;

[ApiController]
[Route("api/quizzes")]
public class QuizController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public QuizController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllQuizzesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT Id, Name FROM Quiz", connection);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        var quizzes = new List<object>();
        while (await reader.ReadAsync())
        {
            quizzes.Add(new { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        }
        return Ok(quizzes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetQuizByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(@"
            SELECT q.Id, q.Name, q.PathFile, t.Name AS TeacherName
            FROM Quiz q
            JOIN PotatoTeacher t ON q.PotatoTeacherId = t.Id
            WHERE q.Id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return NotFound();
        var quiz = new 
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            PathFile = reader.GetString(2),
            TeacherName = reader.GetString(3)
        };
        return Ok(quiz);
    }

    [HttpPost]
    public async Task<IActionResult> AddQuizAsync([FromBody] CreateQuizDto quizDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            using var checkTeacherCmd = new SqlCommand(
                "SELECT Id FROM PotatoTeacher WHERE Name = @name", connection, transaction);
            checkTeacherCmd.Parameters.AddWithValue("@name", quizDto.PotatoTeacherName);
            var teacherIdObj = await checkTeacherCmd.ExecuteScalarAsync();
            int teacherId;
            if (teacherIdObj != null)
            {
                teacherId = (int)teacherIdObj;
            }
            else
            {
                using var insertTeacherCmd = new SqlCommand(
                    "INSERT INTO PotatoTeacher (Name) OUTPUT INSERTED.Id VALUES (@name)", connection, transaction);
                insertTeacherCmd.Parameters.AddWithValue("@name", quizDto.PotatoTeacherName);
                teacherId = (int)await insertTeacherCmd.ExecuteScalarAsync();
            }

            using var insertQuizCmd = new SqlCommand(
                "INSERT INTO Quiz (Name, PotatoTeacherId, PathFile) OUTPUT INSERTED.Id " +
                "VALUES (@name, @teacherId, @pathFile)", connection, transaction);
            insertQuizCmd.Parameters.AddWithValue("@name", quizDto.Name);
            insertQuizCmd.Parameters.AddWithValue("@teacherId", teacherId);
            insertQuizCmd.Parameters.AddWithValue("@pathFile", quizDto.Path);
            var quizId = (int)await insertQuizCmd.ExecuteScalarAsync();

            transaction.Commit();
            return CreatedAtAction(nameof(GetQuizByIdAsync), new { id = quizId }, null);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return StatusCode(500, ex.Message);
        }
    }
}