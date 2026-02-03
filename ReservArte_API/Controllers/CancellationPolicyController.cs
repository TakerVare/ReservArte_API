using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservArte_API.Models;
using ReservArte_API.Models.DTOs;
using ReservArte_API.Repositories.Interfaces;

namespace ReservArte_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CancellationPolicyController : ControllerBase
{
    private readonly ICancellationPolicyRepository _policyRepository;

    public CancellationPolicyController(ICancellationPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    /// <summary>
    /// Obtiene la política de cancelación de una organización
    /// </summary>
    [HttpGet("{organizationId}")]
    public async Task<ActionResult<CancellationPolicyDto>> GetByOrganization(int organizationId)
    {
        var policy = await _policyRepository.GetByOrganizationAsync(organizationId);
        if (policy == null)
        {
            // Retornar política por defecto si no existe
            return Ok(new CancellationPolicyDto
            {
                OrganizationId = organizationId,
                MinHoursBeforeCancel = 24,
                PenaltyPercentage = 50,
                MaxNoShowsBeforeBlock = 3,
                IsActive = false
            });
        }

        return Ok(new CancellationPolicyDto
        {
            Id = policy.Id,
            OrganizationId = policy.OrganizationId,
            MinHoursBeforeCancel = policy.MinHoursBeforeCancel,
            PenaltyPercentage = policy.PenaltyPercentage,
            MaxNoShowsBeforeBlock = policy.MaxNoShowsBeforeBlock,
            VipMinHoursBeforeCancel = policy.VipMinHoursBeforeCancel,
            VipPenaltyPercentage = policy.VipPenaltyPercentage,
            IsActive = policy.IsActive
        });
    }

    /// <summary>
    /// Crea o actualiza la política de cancelación de una organización
    /// </summary>
    [HttpPut("{organizationId}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CancellationPolicyDto>> CreateOrUpdate(
        int organizationId, 
        [FromBody] CancellationPolicyDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.OrganizationId != organizationId)
            {
                return BadRequest(new { message = "El ID de organización no coincide" });
            }

            var policy = new CancellationPolicy
            {
                OrganizationId = dto.OrganizationId,
                MinHoursBeforeCancel = dto.MinHoursBeforeCancel,
                PenaltyPercentage = dto.PenaltyPercentage,
                MaxNoShowsBeforeBlock = dto.MaxNoShowsBeforeBlock,
                VipMinHoursBeforeCancel = dto.VipMinHoursBeforeCancel,
                VipPenaltyPercentage = dto.VipPenaltyPercentage,
                IsActive = dto.IsActive
            };

            var result = await _policyRepository.CreateOrUpdateAsync(policy);
            if (result == null)
            {
                return StatusCode(500, new { message = "Error al guardar la política de cancelación" });
            }

            return Ok(new CancellationPolicyDto
            {
                Id = result.Id,
                OrganizationId = result.OrganizationId,
                MinHoursBeforeCancel = result.MinHoursBeforeCancel,
                PenaltyPercentage = result.PenaltyPercentage,
                MaxNoShowsBeforeBlock = result.MaxNoShowsBeforeBlock,
                VipMinHoursBeforeCancel = result.VipMinHoursBeforeCancel,
                VipPenaltyPercentage = result.VipPenaltyPercentage,
                IsActive = result.IsActive
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
        }
    }
}
