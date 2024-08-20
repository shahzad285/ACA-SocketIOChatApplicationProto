using ACA_SocketIOChatApplicationProto.Server.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ACA_SocketIOChatApplicationProto.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        public ChatController(DatabaseContext databaseContext)
        {

            _databaseContext = databaseContext;

        }
        [HttpPost("Login")]
        public async Task<string> Login(SigninDTO signinDTO)
        {
            var query = $"Select * from Users where username='{signinDTO.UserName}' and password='{signinDTO.Password}'";
            var resDT=_databaseContext.ExecuteQuery(query);
            if (resDT.Rows.Count==0) {
                return "Username or password is incorrect";
            }
            return "Loggedin";
        }
        [HttpPost("Signup")]
        public async Task<string> SignUp(SignupDTO signupDto)
        {
            var query = $"Select * from Users where username='{signupDto.UserName}'";
            var resDT = _databaseContext.ExecuteQuery(query);
            if (resDT.Rows.Count != 0)
            {
                return "Username already exist";
            }

            query = $"Insert into Users(Username,Name,Password) values('{signupDto.UserName}','{signupDto.Name}','{signupDto.Password}')";
            int res=_databaseContext.ExecuteNonQuery(query);
            if (res == 0)
            {
                return "Something went wrong";
            }
            return "Signup successfull";
        }
        [HttpPost("ImAlive")]
        public async Task<string> ImAlive(int userId)
        {
            userId = 1;
            var query = $@"IF EXISTS (SELECT 1 FROM UserOnlineStatus WHERE userId ={userId})
                            BEGIN
                             UPDATE UserOnlineStatus SET IsOnline=1,ExpiryTimestamp= (DATEDIFF(SECOND, '1970-01-01', GETUTCDATE())+600)  where userId={userId}
                            END
                           ELSE
                            BEGIN
                             INSERT INTO UserOnlineStatus (UserId, IsOnline, ExpiryTimestamp)
                                    VALUES ({userId}, {1},(DATEDIFF(SECOND, '1970-01-01', GETUTCDATE())+600));
                           END";
            var res = _databaseContext.ExecuteNonQuery(query);
            if (res>0)
            {
                return "Alive status updated";
            }

            return "Something went wrong";
        }
        [HttpGet("GetOnlineUsers")]
        public async Task<List<GetOnlineUsersDTO>> GetOnlineUsers([FromQuery]List<int> ids)
        {
            List<GetOnlineUsersDTO> onlineUsers = new List<GetOnlineUsersDTO>();
            GetOnlineUsersDTO getOnlineUsersDTO = null;
            var query = $@"select u.Id,username, name,IsOnline from Users u Join UserOnlineStatus uos on u.Id=uos.userId where u.Id in ({string.Join(',',ids)}) and expiryTimeStamp>DATEDIFF(SECOND, '1970-01-01', GETUTCDATE())";
            var res= _databaseContext.ExecuteQuery(query);
            for (int i=0;i< res.Rows.Count;i++)
            {
                getOnlineUsersDTO = new GetOnlineUsersDTO();
                getOnlineUsersDTO.IsOnline = true;
                getOnlineUsersDTO.Name = Convert.ToString(res.Rows[i]["Name"]);
                getOnlineUsersDTO.Id= Convert.ToInt32(res.Rows[i]["Id"]);
                onlineUsers.Add(getOnlineUsersDTO);
            }
            return onlineUsers;
        }
    }
}
