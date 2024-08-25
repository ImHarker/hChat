
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using hChatAPI.Services;
using hChatAPI.Services.WebSockets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using static hChatAPI.Services.JWTService;

namespace hChatAPI {
    public class Program {
		public static void Main(string[] args) {
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "hChatAPI", Version = "v1" });

				//Access Token
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
					Description = "JWT Authorization header using the Bearer scheme.",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = "bearer"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new List<string>()
					}
				});

				//Refresh Token
				c.AddSecurityDefinition("RefreshToken", new OpenApiSecurityScheme {
					Description = "Refresh Token header using the Bearer scheme.",
					Name = "RefreshToken",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Scheme = "bearer"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "RefreshToken"
							}
						},
						new List<string>()
					}
				});
				
				//Challenge Token
				c.AddSecurityDefinition("ChallengeToken", new OpenApiSecurityScheme {
					Description = "Challenge Token header using the Bearer scheme.",
					Name = "ChallengeToken",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Scheme = "bearer"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "ChallengeToken"
							}
						},
						new List<string>()
					}
				});
				
			});





			builder.Services.AddDbContext<DataContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			
			builder.Services.AddSingleton<CustomSecurityTokenHandler>();


			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				//Access Token
				.AddJwtBearer(options => {
					options.TokenHandlers.Clear();
					options.TokenHandlers.Add(builder.Services.BuildServiceProvider().GetRequiredService<CustomSecurityTokenHandler>());
					options.TokenValidationParameters = new TokenValidationParameters {
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = builder.Configuration["Jwt:Issuer"],
						ValidAudience = builder.Configuration["Jwt:Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Jwt:Key"]))
					};
				})

				//Refresh Token

				.AddJwtBearer("RefreshTokenScheme", options =>
				{
					options.TokenHandlers.Clear();
					options.TokenHandlers.Add(builder.Services.BuildServiceProvider().GetRequiredService<CustomSecurityTokenHandler>());
					options.TokenValidationParameters = new TokenValidationParameters {
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = builder.Configuration["Jwt:Issuer"],
						ValidAudience = "Refresh",
						IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Jwt:Key"]))
					};

					options.Events = new JwtBearerEvents {
						OnMessageReceived = context => {
							if (context.Request.Headers.ContainsKey("RefreshToken")) {
								context.Token = context.Request.Headers["RefreshToken"];
							}
							else {
								context.NoResult();
							}

							return Task.CompletedTask;
						}
					};

				})
				
				//Challenge Token

				.AddJwtBearer("ChallengeTokenScheme", options =>
				{
					options.TokenHandlers.Clear();
					options.TokenHandlers.Add(builder.Services.BuildServiceProvider().GetRequiredService<CustomSecurityTokenHandler>());
					options.TokenValidationParameters = new TokenValidationParameters {
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = builder.Configuration["Jwt:Issuer"],
						ValidAudience = "Challenge",
						IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Jwt:Key"]))
					};

					options.Events = new JwtBearerEvents {
						OnMessageReceived = context => {
							if (context.Request.Headers.ContainsKey("ChallengeToken")) {
								context.Token = context.Request.Headers["ChallengeToken"];
							}
							else {
								context.NoResult();
							}

							return Task.CompletedTask;
						}
					};

				})
				;

			builder.Services.AddScoped<UserService>();
			builder.Services.AddScoped<JwtService>();



			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment()) {	
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.UseWebSockets();

			app.UseWhen(context => context.Request.Path == "/ws", appBuilder => {
				appBuilder.UseMiddleware<WebSocketMiddleware>();
			});


			app.MapControllers();

			app.Run();
		}
	}
}
