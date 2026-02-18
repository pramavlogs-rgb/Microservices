namespace AuthService.Data
{
    public interface IDataContextDapper
    {
        IEnumerable<T> LoadData<T>(string sql);
        IEnumerable<T> LoadData<T>(string sql, List<Npgsql.NpgsqlParameter> parameters);
        T LoadDataSingle<T>(string sql);
        T LoadDataSingle<T>(string sql, List<Npgsql.NpgsqlParameter> parameters);
        bool ExecuteSql(string sql);
        int ExecuteSqlWithRowCount(string sql);
        bool ExecuteSqlWithParameters(string sql, List<Npgsql.NpgsqlParameter> parameters);
    }
}
