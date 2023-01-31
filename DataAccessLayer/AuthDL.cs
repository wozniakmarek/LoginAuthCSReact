using System.Data;
using System.Data.Common;
using LoginAuthCSReact.Model;
using MySqlConnector;

namespace LoginAuthCSReact.DataAccessLayer;

public class AuthDL : IAuthDL
{
    public readonly IConfiguration _configuration;
    public readonly MySqlConnection _mySqlConnection;

    private const string SqlQuerySelect = @"SELECT * 
                                            FROM users.userdetails
                                            WHERE UserName=@UserName AND PassWord=@PassWord AND Role=@Role;";

    private const string SqlQueryInsert = @"INSERT INTO users.userdetails
                                            (UserName, PassWord, Role) VALUES 
                                            (@UserName, @PassWord, @Role)";


    public AuthDL(IConfiguration configuration)
    {
        _configuration = configuration;
        _mySqlConnection = new MySqlConnection(_configuration["ConnectionStrings:MySqlDBConnection"]);
    }

    public async Task<SignInResponse> SignIn(SignInRequest request)
    {
        var response = new SignInResponse();
        response.IsSuccess = true;
        response.Message = "Successful";
        try
        {
            if (_mySqlConnection.State != ConnectionState.Open) await _mySqlConnection.OpenAsync();

            

            using (var sqlCommand = new MySqlCommand(SqlQuerySelect, _mySqlConnection))
            {
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandTimeout = 180;
                sqlCommand.Parameters.AddWithValue("@UserName", request.UserName);
                sqlCommand.Parameters.AddWithValue("@PassWord", request.Password);
                sqlCommand.Parameters.AddWithValue("@Role", request.Role);
                using (DbDataReader dataReader = await sqlCommand.ExecuteReaderAsync())
                {
                    if (dataReader.HasRows)
                    {
                        response.Message = "Login Successfully";
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Login Unsuccessfully";
                        return response;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<SignUpResponse> SignUp(SignUpRequest request)
    {
        var response = new SignUpResponse();
        response.IsSuccess = true;
        response.Message = "Successful";
        try
        {
            if (_mySqlConnection.State != ConnectionState.Open) await _mySqlConnection.OpenAsync();

            if (!request.Password.Equals(request.ConfigPassword))
            {
                response.IsSuccess = false;
                response.Message = "Password & Confirm Password not Match";
                return response;
            }


            using (var sqlCommand = new MySqlCommand(SqlQueryInsert, _mySqlConnection))
            {
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandTimeout = 180;
                sqlCommand.Parameters.AddWithValue("@UserName", request.UserName);
                sqlCommand.Parameters.AddWithValue("@PassWord", request.Password);
                sqlCommand.Parameters.AddWithValue("@Role", request.Role);
                var Status = await sqlCommand.ExecuteNonQueryAsync();
                if (Status <= 0)
                {
                    response.IsSuccess = false;
                    response.Message = "Something Went Wrong";
                    return response;
                }
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<bool> CreateDummyUser()
    {
        try
        {
            if (_mySqlConnection.State != ConnectionState.Open) await _mySqlConnection.OpenAsync();

            var SqlQuery = @"INSERT INTO users.userdetails
                                (UserName, PassWord, Role) VALUES 
                                ('dummyuser', 'password', 'User');";

            using (var sqlCommand = new MySqlCommand(SqlQuery, _mySqlConnection))
            {
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandTimeout = 180;
                var Status = await sqlCommand.ExecuteNonQueryAsync();
                if (Status <= 0)
                {
                    return false;
                }
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

}