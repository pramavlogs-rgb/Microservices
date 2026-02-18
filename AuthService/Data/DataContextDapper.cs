using System.Data;
using Dapper;
using Npgsql;

namespace AuthService.Data
{
    public class DataContextDapper : IDataContextDapper
    {
        private readonly IConfiguration _config;
        public DataContextDapper(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }

        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql) > 0;
        }

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }

        public bool ExecuteSqlWithParameters(string sql, List<NpgsqlParameter> parameters)
        {
            using (NpgsqlConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                using (NpgsqlCommand commandWithParams = new NpgsqlCommand(sql, dbConnection))
                {
                    foreach(NpgsqlParameter parameter in parameters)
                    {
                        commandWithParams.Parameters.Add(parameter);
                    }

                    dbConnection.Open();
                    int rowsAffected = commandWithParams.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        public T LoadDataSingle<T>(string sql, List<NpgsqlParameter> parameters)
        {
            using (NpgsqlConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                using (NpgsqlCommand command = new NpgsqlCommand(sql, dbConnection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    dbConnection.Open();
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.Parse<T>().FirstOrDefault();
                        }
                        return default(T);
                    }
                }
            }
        }

        public IEnumerable<T> LoadData<T>(string sql, List<NpgsqlParameter> parameters)
        {
            using (NpgsqlConnection dbConnection = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                using (NpgsqlCommand command = new NpgsqlCommand(sql, dbConnection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    dbConnection.Open();
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        return reader.Parse<T>().ToList();
                    }
                }
            }
        }
    }
}
