using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using JwtWebApi.Models;
using JwtWebApi.Tools;

namespace JwtWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Context _context;
        static private int idUser = 1;
        static private int idSession = 1;

        public AuthController(Context context, IConfiguration configuration)
        {
            _context = context;
           _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(UserReg request)
        {
            if (request.Password.Length < 8)
            {
                return BadRequest("Unreliable password.");
            }

            User? foundUser = await _context.Users.FirstOrDefaultAsync(user => user.Email == request.Email);
            if (foundUser != null)
            {
                return BadRequest("User with this email already exists.");
            }

            string emailPattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(request.Email, emailPattern, RegexOptions.IgnoreCase);
            if (!isMatch.Success)
            {
                return BadRequest("Invalid Email.");
            }

            User? user = new User();
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.Id = idUser;
            user.Username = request.Username;
            user.Email = request.Email;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Role = "Customer";
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            _context.SaveChanges();

            idUser++;

            return Ok("User has been signed up successfully.");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserAuth request)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(user => user.Email == request.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }
            var tokenInfo = CreateToken(user);

            Session session = new Session();
            session.Id = idSession;
            session.UserId = user.Id;
            session.SessionToken = tokenInfo.Item1;
            session.ExpiresAt = tokenInfo.Item2;

            _context.Sessions.Add(session);
            _context.SaveChanges();

            return Ok("You were successfully signed in.");
        }

        private Tuple<string, DateTime> CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            DateTime expiresAt = DateTime.Now.AddHours(2);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return new Tuple<string, DateTime>(jwt, expiresAt);
        }

        [HttpGet("findByToken")]
        public async Task<ActionResult<User>> GetUsers(string token)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(session => session.SessionToken == token);
            if (session == null)
            {
                return BadRequest("Invalid token.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == session.UserId);
            if (user == null)
            {
                return BadRequest("Session has ended for this token.");
            }
            return Ok(user);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
