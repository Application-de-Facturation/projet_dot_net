namespace FacturationApp.Services.Interfaces;


public interface IPdfService
{

	Task<byte[]> GenererPdfAsync(int factureId);
}