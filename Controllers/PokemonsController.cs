using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace pokedex_analytics_up.Controllers;

[ApiController]
[Route("pokemons")]
public class PokemonsController : ControllerBase
{
    private readonly ILogger<PokemonsController> _logger;

    public PokemonsController(ILogger<PokemonsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetPokemons")]
    public async Task<Result<Pokemon, Error>> GetPokemons()
    {
        HttpClient client = new HttpClient();

        string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        AuthenticationHeaderValue bearer = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Authorization = bearer;
        
        try {
            HttpResponseMessage response = await client.GetAsync("http://localhost:5000/users/pokemons");
            string? content = await response.Content.ReadAsStringAsync();
            PokemonResponse? pokemons = JsonSerializer.Deserialize<PokemonResponse>(content);

            Pokemon? pokemon = pokemons!.data.OrderByDescending(p => p.life).FirstOrDefault();

            return Result<Pokemon, Error>.Ok(pokemon!);
        } catch (Exception e) {
            return Result<Pokemon, Error>.Error(new Error { Message = e.Message });
        }
    }
}

public class PokemonResponse
{
    public IEnumerable<Pokemon> data { get; set; } = null!;
}

public class Error
{
    public string Message { get; set; } 
}