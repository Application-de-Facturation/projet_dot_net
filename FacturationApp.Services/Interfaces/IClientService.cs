// FacturationApp.Services/Interfaces/IClientService.cs

using FacturationApp.Data.Entities;

namespace FacturationApp.Services.Interfaces;

/// <summary>
/// Contrat du service Client.
/// Définit CE QUE le service peut réaliser , pas " COMMENT " il le fait.
/// Toutes les méthodes sont async (opérations BDD = I/O asynchrone).
/// </summary>
/// 

public interface IClientService

{

    // READ — Lecture
    Task<List<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(int id);

    // READ avec filtrage (pour la SearchBar de M3)
    Task<List<Client>> SearchAsync(string terme);

    // WRITE — Écriture
    Task<Client> CreateAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(int id);  // Suppression LOGIQUE (IsDeleted = true)


}