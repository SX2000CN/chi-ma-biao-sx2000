using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using SizeChartGenerator.Models;

namespace SizeChartGenerator.Services
{
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private readonly string _connectionString;
        private readonly string _dbPath;

        private DatabaseService()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                   "SizeChartGenerator", "categories.db");
            var directory = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            _connectionString = $"Data Source={_dbPath};Version=3;";
        }

        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseService();
                }
                return _instance;
            }
        }

        public void Initialize()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        BaseIncrement INTEGER NOT NULL,
                        IsCustom INTEGER NOT NULL DEFAULT 1
                    )";
                createTableCmd.ExecuteNonQuery();
            }
        }

        public List<SizeCategory> GetCustomCategories()
        {
            var categories = new List<SizeCategory>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Categories WHERE IsCustom = 1";
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new SizeCategory
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            BaseIncrement = reader.GetInt32(2),
                            IsCustom = true
                        });
                    }
                }
            }
            
            return categories;
        }

        public void SaveCategory(SizeCategory category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Categories (Name, BaseIncrement, IsCustom)
                    VALUES (@name, @increment, 1)";
                
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@increment", category.BaseIncrement);
                
                command.ExecuteNonQuery();
                
                // 获取插入的ID
                command.CommandText = "SELECT last_insert_rowid()";
                category.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void UpdateCategory(SizeCategory category)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Categories 
                    SET Name = @name, BaseIncrement = @increment
                    WHERE Id = @id";
                
                command.Parameters.AddWithValue("@id", category.Id);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@increment", category.BaseIncrement);
                
                command.ExecuteNonQuery();
            }
        }

        public void DeleteCategory(int categoryId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Categories WHERE Id = @id";
                command.Parameters.AddWithValue("@id", categoryId);
                
                command.ExecuteNonQuery();
            }
        }
    }
}