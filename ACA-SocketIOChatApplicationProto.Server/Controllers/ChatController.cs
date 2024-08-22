using ACA_SocketIOChatApplicationProto.Server.DTOs;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult> Login(SigninDTO signinDTO)
        {
            var query = $"Select * from Users where username='{signinDTO.UserName}' and password='{signinDTO.Password}'";
            var resDT = _databaseContext.ExecuteQuery(query);
            if (resDT.Rows.Count == 0)
            {
                return BadRequest("Username or password is incorrect");
            }
            else
            {
                string userId = Convert.ToString(resDT.Rows[0]["Id"]);
                return Ok(JWTToken.GenerateJwtToken(userId));
            }

        }
        [AllowAnonymous]
        [HttpPost("Signup")]
        public async Task<ActionResult> SignUp(SignupDTO signupDto)
        {
            var query = $"Select * from Users where username='{signupDto.UserName}'";
            var resDT = _databaseContext.ExecuteQuery(query);
            if (resDT.Rows.Count != 0)
            {
                return BadRequest("Username already exist");
            }

            query = $"Insert into Users(Username,Name,Password) values('{signupDto.UserName}','{signupDto.Name}','{signupDto.Password}')";
            int res = _databaseContext.ExecuteNonQuery(query);
            if (res == 0)
            {
                return BadRequest("Something went wrong");
            }
            return Ok("Signup successfull");
        }
        [Authorize]
        [HttpPost("ImAlive")]
        public async Task<ActionResult> ImAlive()
        {
            var token = GetJwtTokenFromHeader();
            var userId = JWTToken.GetUserIdFromToken(token);
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
            if (res > 0)
            {
                return Ok("Alive status updated");
            }

            return BadRequest("Something went wrong");
        }
        [Authorize]
        [HttpGet("GetOnlineUsers")]
        public async Task<ActionResult> GetOnlineUsers()
        {
            var token = GetJwtTokenFromHeader();
            var userId = JWTToken.GetUserIdFromToken(token);
            List<GetOnlineUsersDTO> onlineUsers = new List<GetOnlineUsersDTO>();
            GetOnlineUsersDTO getOnlineUsersDTO = null;
            var query = $@"select u.Id,username, name,Case When expiryTimeStamp>DATEDIFF(SECOND, '1970-01-01', GETUTCDATE()) then 1 else 0 end as IsOnline  from Users u Join UserOnlineStatus uos on u.Id=uos.userId where userid !={userId} order by IsOnline";
            var res = _databaseContext.ExecuteQuery(query);
            for (int i = 0; i < res.Rows.Count; i++)
            {
                getOnlineUsersDTO = new GetOnlineUsersDTO();
                getOnlineUsersDTO.IsOnline = Convert.ToBoolean(res.Rows[i]["IsOnline"]);
                getOnlineUsersDTO.Name = Convert.ToString(res.Rows[i]["Name"]);
                getOnlineUsersDTO.Id = Convert.ToInt32(res.Rows[i]["Id"]);
                onlineUsers.Add(getOnlineUsersDTO);
            }


            return Ok(onlineUsers);
        }

        private string GetJwtTokenFromHeader()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}
