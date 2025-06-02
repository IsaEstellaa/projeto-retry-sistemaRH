using System.ComponentModel.DataAnnotations;

namespace SistemaRH.Domain;

public class ProcessoSeletivo
{
    public int Id { get; set; }

    [Required]
    public string Nome { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    // Relacionamento com inscrições
    public List<Inscricao> InscricoesParticipando { get; set; } = new();
}