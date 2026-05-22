using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ReservArte_API.Models;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SupplierController : ControllerBase
{
    private readonly string _connectionString;

    public SupplierController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ReservArteDB")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Employee}")]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = new List<object>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "SELECT Id, Name, Phone, Address, Type FROM Suppliers WHERE IsActive = 1 ORDER BY Name",
            connection
        );

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            suppliers.Add(new
            {
                id = reader.GetInt32(reader.GetOrdinal("Id")),
                name = reader.GetString(reader.GetOrdinal("Name")),
                phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                type = reader.GetString(reader.GetOrdinal("Type")),
            });
        }

        return Ok(suppliers);
    }
}