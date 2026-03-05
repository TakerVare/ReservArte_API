using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReservArte_API.Controllers;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services;
using ReservArte_API.Repositories;
using ReservArte_API.Services.Interfaces;
using ReservArte_API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using ReservArte_API.Converters;




var builder = WebApplication.CreateBuilder(args);

// Configuración de CORS
builder.Services.AddCors(options =>
{
    // options.AddPolicy("AllowFrontend", policy =>
    // {
    //     policy.WithOrigins("http://localhost:5173")
    //           .AllowAnyHeader()
    //           .AllowAnyMethod()
    //           .AllowCredentials();
    // });
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5297")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
        };
    });

var connectionString = builder.Configuration.GetConnectionString("ReservArteDB");

// Configuración de Redsys
builder.Services.Configure<RedsysSettings>(builder.Configuration.GetSection(RedsysSettings.SectionName));

// HttpClient para Redsys
builder.Services.AddHttpClient<IRedsysService, RedsysService>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IWaitingListService, WaitingListService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IWaitingListRepository, WaitingListRepository>();
builder.Services.AddScoped<ICancellationPolicyRepository, CancellationPolicyRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<ICustomerPaymentMethodRepository, CustomerPaymentMethodRepository>();

// === Reminder System ===
// Repositories
builder.Services.AddScoped<IReminderConfigurationRepository, ReminderConfigurationRepository>();
builder.Services.AddScoped<IMessageTemplateRepository, MessageTemplateRepository>();
builder.Services.AddScoped<IReminderLogRepository, ReminderLogRepository>();
builder.Services.AddScoped<IConfirmationTokenRepository, ConfirmationTokenRepository>();

// Services
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IEmailSenderService, MockEmailSenderService>();
builder.Services.AddScoped<IWhatsAppSenderService, MockWhatsAppSenderService>();

// Background job para procesar recordatorios (desactivado - requiere DB)
// builder.Services.AddHostedService<ReminderBackgroundService>();

// === Service Photo System ===
// Configuration
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection(S3Settings.SectionName));

// Repositories
builder.Services.AddScoped<IServicePhotoRepository, ServicePhotoRepository>();

// Services
builder.Services.AddScoped<IServicePhotoService, ServicePhotoService>();

// Background job para limpieza RGPD de fotos expiradas (desactivado - requiere DB)
// builder.Services.AddHostedService<PhotoCleanupBackgroundService>();

// === Product Management System ===
// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
builder.Services.AddScoped<IProductSaleRepository, ProductSaleRepository>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();

// Add Authorization Policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ClientOwnAppointment", policy =>
        policy.RequireRole(Roles.Client)
              .RequireAssertion(context =>
              {
                  var userIdClaim = context.User.FindFirst("id")?.Value;
                  var routeData = context.Resource as Microsoft.AspNetCore.Routing.RouteData;
                  
                  if (routeData == null || !routeData.Values.TryGetValue("userId", out var userIdObj))
                      return false;
                  
                  return userIdObj?.ToString() == userIdClaim;
              })
    )
    .AddPolicy("ClientOwnCustomer", policy =>
        policy.RequireRole(Roles.Client)
              .RequireAssertion(context =>
              {
                  var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? context.User.FindFirst("nameid")?.Value;
                  if (string.IsNullOrEmpty(userIdClaim)) return false;

                  var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
                  var routeId = httpContext?.GetRouteData()?.Values["id"]?.ToString();
                  if (string.IsNullOrEmpty(routeId)) return false;

                  return routeId == userIdClaim;
              })
    )
    // Admin/Employee pueden siempre; Client solo si id de ruta = su nameid (un solo [Authorize] con OR)
    .AddPolicy("AdminOrEmployeeOrClientOwnCustomer", policy =>
        policy.RequireAssertion(context =>
        {
            if (context.User.IsInRole(Roles.Admin) || context.User.IsInRole(Roles.Employee))
                return true;

            if (!context.User.IsInRole(Roles.Client))
                return false;

            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return false;

            var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
            var routeId = httpContext?.GetRouteData()?.Values["id"]?.ToString();
            if (string.IsNullOrEmpty(routeId)) return false;

            return routeId == userIdClaim;
        })
    )
    // Admin/Employee pueden siempre; Client solo si es propietario de la cita (appointment.CustomerId == currentUserId)
    .AddPolicy("AdminOrEmployeeOrAppointmentOwner", policy =>
        policy.RequireAssertion(context =>
        {
            if (context.User.IsInRole(Roles.Admin) || context.User.IsInRole(Roles.Employee))
                return true;

            if (!context.User.IsInRole(Roles.Client))
                return false;

            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("nameid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                return false;

            int resourceCustomerId = context.Resource switch
            {
                Models.DTOs.AppointmentDtoOut dto => dto.CustomerId,
                Models.Appointment apt => apt.CustomerId,
                _ => 0
            };
            return resourceCustomerId != 0 && resourceCustomerId == currentUserId;
        })
    );

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Insert(0, new TimeOnlyJsonConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
