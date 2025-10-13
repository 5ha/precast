using PrecastTracker.Business;
using PrecastTracker.Data;
using PrecastTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC controller infrastructure (routing, model binding, JSON formatters, [ApiController] behaviors).
builder.Services.AddControllers();

// Enable RFC 7807/9457 ProblemDetails formatting; wires up IProblemDetailsService used by error handlers.
builder.Services.AddProblemDetails();

// Add layer dependencies
builder.Services.AddBusiness();
builder.Services.AddServices();
builder.Services.AddData(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler: catches unhandled exceptions and (with AddProblemDetails) returns JSON ProblemDetails.
app.UseExceptionHandler();

// Adds bodies for bare 4xx/5xx responses (e.g., 404); with ProblemDetails registered, emits JSON ProblemDetails.
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseCors();

// Serve static files from wwwroot (SvelteKit output)
app.UseStaticFiles();

// Map attribute-routed controller endpoints into the pipeline (must come after middleware is set up).
app.MapControllers();

// SPA fallback for client-side routing
app.MapFallbackToFile("index.html");

app.Run();
