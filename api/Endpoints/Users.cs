using api.Domaine.Data;
using api.Dto;
using Microsoft.EntityFrameworkCore;

namespace api.Endpoints
{
    public static class Users
    {
        public static void RegisterUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var users = routes.MapGroup("/api/users");

            users.MapGet("", GetAll).WithName("getAll")
                .WithTags("User")
                .WithSummary("Get all the users.")
                .Produces<List<UserDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .WithOpenApi();

            users.MapGet("/{id}", GetOne).WithName("getOne")
                .WithTags("User")
                .WithSummary("Get one user by is Id.")
                .Produces<UserDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .WithOpenApi();
        }

        private static async Task<IResult> GetAll(SqlContext context)
        {
            var output = await context.Users.ToListAsync();

            var users = output.Select(x => new UserDto
            {
                Id = x.Id.ToString(),
                Username = x.Username,
                Email = x.Email,
            }).ToList();

            return Results.Ok(users);
        }

        private static async Task<IResult> GetOne(SqlContext context, string id)
        {
            var output = await context.Users.Where(x => x.Id == Guid.Parse(id)).FirstOrDefaultAsync();

            return output switch
            {
                null => Results.NotFound(),
                _ => Results.Ok(new UserDto
                {
                    Id = output.Id.ToString(),
                    Username = output.Username,
                    Email = output.Email,
                })
            };
        }
    }
}
