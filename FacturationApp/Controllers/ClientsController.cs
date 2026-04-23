// FacturationApp/Controllers/ClientsController.cs

using Microsoft.AspNetCore.Mvc;
using FacturationApp.Data.Entities;
using FacturationApp.Services.Interfaces;

namespace FacturationApp.Controllers;

[ApiController]
[Route("api/[controller]")]  // → /api/clients
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    // DI : le service est injecté automatiquement
    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    // GET /api/clients — Liste tous les clients actifs
    [HttpGet]
    public async Task<ActionResult<List<Client>>> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        return Ok(clients);
    }

    // GET /api/clients/5 — Retourne un client par Id
    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetById(int id)
    {
        var client = await _clientService.GetByIdAsync(id);
        if (client == null) return NotFound();
        return Ok(client);
    }

    // POST /api/clients — Crée un nouveau client
    [HttpPost]
    public async Task<ActionResult<Client>> Create([FromBody] Client client)
    {
        var created = await _clientService.CreateAsync(client);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/clients/5 — Modifie un client
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Client client)
    {
        if (id != client.Id) return BadRequest();
        await _clientService.UpdateAsync(client);
        return NoContent();
    }

    // DELETE /api/clients/5 — Suppression logique
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _clientService.DeleteAsync(id);
        return NoContent();
    }
}