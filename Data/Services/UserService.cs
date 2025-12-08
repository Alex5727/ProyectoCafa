using Npgsql;

public class UserService
{
    private readonly string _connectionString;

    public UserService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void CreateUser(string username, string password)
    {
        using var con = new NpgsqlConnection(_connectionString);
        con.Open();

        using var cmd = new NpgsqlCommand("SELECT fun_create_user(@u, @p)", con);
        cmd.Parameters.AddWithValue("u", username);
        cmd.Parameters.AddWithValue("p", password);
        cmd.ExecuteNonQuery();
    }
}