using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // access duration for particular cookie
        options.LoginPath = "/api/login"; // Specify your login path
        options.AccessDeniedPath = "/api/access-denied"; // Specify your access denied path
    });

// Configure CORS to allow requests from the React app
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(build =>
    {
        build.WithOrigins("http://localhost:3000") // Replace with your frontend app's URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // If you need to include credentials (e.g., cookies)
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors();
app.UseAuthentication();  // Enable authentication
app.UseAuthorization();

app.MapControllers();

app.Run();
