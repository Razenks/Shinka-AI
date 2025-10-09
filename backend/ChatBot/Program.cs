using ChatBot.Services; // Não esqueça de adicionar a referência ao seu namespace de serviços

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Adicionar serviços ao contêiner

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- NOSSAS ADIÇÕES COMEÇAM AQUI ---

// 1. Registra o HttpClient para que possamos usál-lo em nossos serviços
builder.Services.AddHttpClient();

// 2. Registra nosso GeminiService. "AddScoped significa que um novo serviço será criado para cada requisição HTTP.
builder.Services.AddScoped<GeminiService>();

// --- NOSSAS ADIÇÕES TERMINAM AQUI ---

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();