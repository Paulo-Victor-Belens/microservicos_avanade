using Identity.Api.DTOs;
using Identity.Api.Services;
using Microsoft.AspNetCore.Mvc;
using IdentityModel.Client;

namespace Identity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            AccountService accountService, 
            IHttpClientFactory httpClientFactory, 
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            var result = await _accountService.RegisterUserAsync(model);

            if (result.Succeeded)
            {
                return Ok(new { message = "Usuário registrado com sucesso!" });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            var user = await _accountService.ValidateCredentialsAsync(model);
            if (user == null)
            {
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }

            var client = _httpClientFactory.CreateClient();
            
            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = "http://localhost:8080",
                Policy =
                {
                    Authority = "http://identity-api:8080",
                    ValidateEndpoints = false 
                }
            });
            
            if (disco.IsError)
            {
                _logger.LogError(disco.Exception, "Erro ao obter o Discovery Document do IdentityServer. Erro: {Error}", disco.Error);
                
                return StatusCode(500, new { message = "Não foi possível conectar ao serviço de identidade." });
            }

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "ecommerce.client",
                ClientSecret = "super_secret_client_password",
                UserName = model.Email,
                Password = model.Password,
                Scope = "stock.api order.api openid profile roles"
            });

            if (tokenResponse.IsError)
            {
                _logger.LogWarning("Falha na requisição do token: {Error} - {ErrorDescription}", 
                    tokenResponse.Error, tokenResponse.ErrorDescription);

                return BadRequest(new { message = "Falha ao obter o token.", error = tokenResponse.Error, error_description = tokenResponse.ErrorDescription });
            }
            
            return Ok(tokenResponse.Json);
        }
    }
}