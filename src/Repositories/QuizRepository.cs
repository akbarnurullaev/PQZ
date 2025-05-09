using Application;

namespace Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

public class QuizRepository : IQuizRepository
    {
        private readonly string _connectionString;

        public QuizRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            var quizzes = new List<object>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT Id, Name FROM Quiz", connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                quizzes.Add(new { Id = reader.GetInt32(0), Name = reader.GetString(1) });
            }
            return quizzes;
        }

        public async Task<object?> GetByIdAsync(int id)
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
            if (!await reader.ReadAsync()) return null;

            return new
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                PathFile = reader.GetString(2),
                TeacherName = reader.GetString(3)
            };
        }

        public async Task<int> AddAsync(CreateQuizDto quizDto)
        {
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
                        "INSERT INTO PotatoTeacher (Name) OUTPUT INSERTED.Id VALUES (@name)",
                        connection, transaction);
                    insertTeacherCmd.Parameters.AddWithValue("@name", quizDto.PotatoTeacherName);
                    teacherId = (int)await insertTeacherCmd.ExecuteScalarAsync();
                }

                // Insert quiz
                using var insertQuizCmd = new SqlCommand(
                    "INSERT INTO Quiz (Name, PotatoTeacherId, PathFile) OUTPUT INSERTED.Id " +
                    "VALUES (@name, @teacherId, @pathFile)", connection, transaction);
                insertQuizCmd.Parameters.AddWithValue("@name", quizDto.Name);
                insertQuizCmd.Parameters.AddWithValue("@teacherId", teacherId);
                insertQuizCmd.Parameters.AddWithValue("@pathFile", quizDto.Path);
                var quizId = (int)await insertQuizCmd.ExecuteScalarAsync();

                transaction.Commit();
                return quizId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }