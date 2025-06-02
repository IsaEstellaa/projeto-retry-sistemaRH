using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace SistemaRH.Domain;
public class Inscricao
{
    public Guid Id { get; set; }

    [Required]
    public string? NomeCandidato { get; set; }
    public string? EmailCandidato { get; set; }
    public DateTime? DataNasc { get; set; }

    public List<Vaga> VagasParticipando { get; set; } = new();
    public List<ProcessoSeletivo> ProcessosParticipando { get; set; } = new();

}
