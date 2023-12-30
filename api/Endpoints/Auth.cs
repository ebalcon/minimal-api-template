using api.Domaine.Data;
using api.Domaine.Models;
using api.Dto;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api.Endpoints
{
    public static class Auth
    {
        public static void RegisterAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var auth = routes.MapGroup("/api/auth");

            auth.MapPost("", Register).WithName("register")
                .WithTags("Auth")
                .WithSummary("Allow user to register.")
                .Produces(StatusCodes.Status204NoContent)
                .AllowAnonymous()
                .WithOpenApi();

            auth.MapPost("/login", Login).WithName("login")
                .WithTags("Auth")
                .WithSummary("To get authenticate.")
                .Produces<string>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .AllowAnonymous()
                .WithOpenApi();

            auth.MapPost("/current", GetCurrentUser).WithName("current")
                .WithTags("Auth")
                .WithSummary("Get the current user.")
                .Produces<UserDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization()
                .WithOpenApi();
        }

        private static async Task<IResult> Register(SqlContext context, IValidator<RegisterDto> validator, RegisterDto input)
        {
            if (context.Users.Any(x => x.Email == input.Email))
                return Results.BadRequest();

            var validation = await validator.ValidateAsync(input);

            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = input.Username,
                Email = input.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password)
            };

            context.Add(user);
            await context.SaveChangesAsync();

            return Results.NoContent();
        }

        private static async Task<IResult> Login(SqlContext context, IConfiguration configuration, LoginDto input)
        {
            var user = await context.Users.Where(x => x.Email == input.Email).FirstOrDefaultAsync();

            if (user == null)
                return Results.NotFound();

            if (!BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
                return Results.BadRequest();

            var token = CreateToken(configuration, user);

            return Results.Ok(token);
        }

        private static async Task<IResult> GetCurrentUser(SqlContext context, ClaimsPrincipal current)
        {
            var output = await context.Users.Where(x => x.Id == Guid.Parse(current.FindFirstValue(ClaimTypes.NameIdentifier))).FirstOrDefaultAsync();

            return output switch
            {
                null => Results.NotFound(),
                _ => Results.Ok(new UserDto
                {
                    Id = output.Id.ToString(),
                    Username = output.Username,
                    Email = output.Email,
                }),
            };
        }

        private static string CreateToken(IConfiguration configuration, User user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:Token"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
