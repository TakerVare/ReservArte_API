using Moq;
using ReservArte_API.Controllers;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Xunit;
using System.Security.Claims;

namespace ReservArte_API.Tests;

public static class ValidationTestHelper
{
    public static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }
}

public class AppointmentControllerTests
{
    private AppointmentDtoOut SampleOut() => new()
    {
        Id = 1,
        CustomerId = 5,
        CustomerName = "Cliente",
        EmployeeId = 2,
        EmployeeName = "Empleado",
        AppointmentDate = "2026-03-08",
        StartTime = TimeOnly.FromDateTime(DateTime.Now),
        EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
        Status = "Booked",
        TotalPrice = 100m,
        DepositAmount = 25m,
        CreatedAt = DateTime.UtcNow,
        Services = new List<AppointmentServiceItemDtoOut>()
    };

    private AppointmentDtoToList SampleListItem() => new()
    {
        Id = 1,
        CustomerId = "5",
        EmployeeId = "2",
        AppointmentDate = "2026-03-08",
        StartTime = TimeOnly.FromDateTime(DateTime.Now),
        EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
        Status = "Booked"
    };

    [Fact]
    public async Task GetAll_ReturnsOk_WithData()
    {
        var mockService = new Mock<IAppointmentService>();
        mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new[] { SampleListItem() });
        var mockAuth = new Mock<IAuthorizationService>();
        var controller = new AppointmentController(mockService.Object, mockAuth.Object);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<AppointmentDtoToList>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetById_ReturnsForbid_WhenNotAuthorized()
    {
        var dto = SampleOut();
        var mockService = new Mock<IAppointmentService>();
        mockService.Setup(s => s.GetByIdDetailedAsync(It.IsAny<int>()))
            .ReturnsAsync(dto);
        var mockAuth = new Mock<IAuthorizationService>();
        mockAuth.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), dto, It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());
        var controller = new AppointmentController(mockService.Object, mockAuth.Object);

        var result = await controller.GetById(1);

        var actionResult = Assert.IsType<ActionResult<AppointmentDtoOut>>(result);
        Assert.IsType<ForbidResult>(actionResult.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValidDto()
    {
        var dtoIn = new AppointmentDtoIn
        {
            CustomerId = 5,
            EmployeeId = 2,
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            Services = new List<AppointmentServiceItemDtoIn>
            {
                new() { ServiceId = 10 }
            }
        };
        var expected = SampleOut();
        var mockService = new Mock<IAppointmentService>();
        mockService.Setup(s => s.CreateAsync(dtoIn))
            .ReturnsAsync(expected);
        var mockAuth = new Mock<IAuthorizationService>();
        var controller = new AppointmentController(mockService.Object, mockAuth.Object);

        var result = await controller.Create(dtoIn);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(AppointmentController.GetById), created.ActionName);
        Assert.Equal(expected, created.Value);
    }

    [Fact]
    public async Task Update_ReturnsForbid_WhenNotAuthorized()
    {
        var existing = SampleOut();
        var mockService = new Mock<IAppointmentService>();
        mockService.Setup(s => s.GetByIdDetailedAsync(It.IsAny<int>()))
            .ReturnsAsync(existing);
        mockService.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Appointment>()))
            .ReturnsAsync((Appointment?)null);
        var mockAuth = new Mock<IAuthorizationService>();
        mockAuth.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), existing, It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());
        var controller = new AppointmentController(mockService.Object, mockAuth.Object);

        var result = await controller.Update(1, new Appointment());

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var mockService = new Mock<IAppointmentService>();
        mockService.Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
        var mockAuth = new Mock<IAuthorizationService>();
        var controller = new AppointmentController(mockService.Object, mockAuth.Object);

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenNotDeleted()
    {
        var mockService = new Mock<IAppointmentService>();
        mockService.Setup(s => s.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(false);
        var mockAuth = new Mock<IAuthorizationService>();
        var controller = new AppointmentController(mockService.Object, mockAuth.Object);

        var result = await controller.Delete(1);

        var nf = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("no encontrada", nf.Value!.ToString()!);
    }
}

public class ModelValidationTests
{
    [Fact]
    public void AppointmentDtoIn_InvalidWhenNoServices()
    {
        var dto = new AppointmentDtoIn
        {
            CustomerId = 1,
            EmployeeId = 2,
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            Services = new List<AppointmentServiceItemDtoIn>() // empty
        };
        var results = ValidationTestHelper.Validate(dto);
        Assert.Contains(results, r => r.ErrorMessage?.Contains("Debe incluir al menos un servicio") == true);
    }

    [Fact]
    public void AppointmentDtoIn_ValidWhenComplete()
    {
        var dto = new AppointmentDtoIn
        {
            CustomerId = 1,
            EmployeeId = 2,
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            Services = new List<AppointmentServiceItemDtoIn> { new() { ServiceId = 5 } }
        };
        var results = ValidationTestHelper.Validate(dto);
        Assert.Empty(results);
    }

    [Fact]
    public void AppointmentDtoToList_RequiredFieldsEnforced()
    {
        var dto = new AppointmentDtoToList(); // all fields default
        var results = ValidationTestHelper.Validate(dto);
        Assert.True(results.Count >= 1);
        Assert.All(results, r => Assert.Contains("required", r.ErrorMessage?.ToLower() ?? string.Empty));
    }
}