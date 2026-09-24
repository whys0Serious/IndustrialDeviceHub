using DeviceHub.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Core + Infrastructure（与 WPF 共用）
builder.Services.AddCoreServices();
builder.Services.AddInfrastructure(builder.Configuration);

//Api
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();