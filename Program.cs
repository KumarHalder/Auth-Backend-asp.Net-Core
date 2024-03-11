using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Add services to the container.
builder.Services.AddControllers();

var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
  // our authentication process will used signed cookies
  options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  // our authentication challenge is openid
  options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
});

authenticationBuilder
 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
 {
     options.Cookie.Name = "oidc";
     options.Cookie.SameSite = SameSiteMode.None;
     options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
     options.Cookie.IsEssential = true;
 })
 .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
 { 
     options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always; 
     options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    // How middleware persists the user identity? (Cookie)
    options.SignInScheme = 
    CookieAuthenticationDefaults.AuthenticationScheme;
    options.GetClaimsFromUserInfoEndpoint = true;
    // How Browser redirects user to authentication provider?
    // (direct get)
    options.AuthenticationMethod = 
    OpenIdConnectRedirectBehavior.RedirectGet;
    
    // How response should be sent back from authentication provider?
    //(form_post)
    options.ResponseMode = OpenIdConnectResponseMode.FormPost;
    
    // Who is the authentication provider? (IDP)
    options.Authority = "https://accounts.google.com";
    
    // Who are we? (client id)
    options.ClientId = configuration["Google:ClientId"];
    
    // How does authentication provider know, we are ligit? (secret key)
    options.ClientSecret = configuration["Google:Secret"];
    
    // What do we intend to receive back?
    // (code to make for consequent requests)
    options.ResponseType = OpenIdConnectResponseType.Code;
    
    // Should there be extra layer of security?
    // (false: as we are using hybrid)
    options.UsePkce = true;
    
    // Where we would like to get the response after authentication?
    options.CallbackPath = configuration["Google:CallbackPath"];
    
    // Should we persist tokens?
    options.SaveTokens = true;
    
    // Should we request user profile details for user end point?
    options.GetClaimsFromUserInfoEndpoint = true;

    // What scopes do we need?
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("phone");
    options.Scope.Add("profile");

    
    // How to handle OIDC events?
    options.Events = new OpenIdConnectEvents
    {
      OnRedirectToIdentityProviderForSignOut = context =>
      {
          context.Response.Redirect(configuration["Google:RedirectOnSignOut"]);
          context.HandleResponse();

          return Task.CompletedTask;
      },
      
      // Where to redirect when we get authentication errors?
      OnRemoteFailure = context =>
      {
         context.Response.Redirect("/error");
         context.HandleResponse();
         return Task.FromResult(0);
      },
   };
 });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();  // Enable authentication
app.UseAuthorization();

app.MapControllers();

app.Run();