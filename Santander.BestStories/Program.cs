using Santander.BestStories.Api.Middlewares;
using Santander.BestStories.Application.DependencyInjection;
using Santander.BestStories.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}


#region  Dependency Injection  

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

#endregion


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
