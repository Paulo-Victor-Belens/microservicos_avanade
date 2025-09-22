using Duende.IdentityServer.Models;

public static class Config
{
    private static readonly string[] RoleClaimType = { "role" };
    
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "Seus papéis", RoleClaimType)
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("stock.api", "Acesso total ao Stock Service"),
            new ApiScope("order.api", "Acesso total ao Order Service")
        ];

    public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("SeuEcommerce.Api", "E-Commerce API")
            {
                Scopes = { "stock.api", "order.api" },
                UserClaims = { "role", "name" }
            }
        ];

    public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "order.service",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "stock.api" }
            },
            new Client
            {
                ClientId = "ecommerce.client",
                ClientSecrets = { new Secret("super_secret_client_password".Sha256()) },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = { "stock.api", "order.api", "openid", "profile", "roles" }
            }
        ];
}