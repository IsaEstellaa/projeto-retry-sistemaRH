using System.ComponentModel.DataAnnotations;

namespace SistemaRH.Domain;

public class Vaga
{
    public Guid Id { get; set; }  // Id da vaga

    [Required]
    public string Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? Localizacao { get; set; }
    [Required]
    public DateTime DataPublicacao { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Salário deve ser um valor positivo.")]
    public double? Salario { get; set; }

    // Relacionamento com inscrições
    public List<Inscricao> InscricoesParticipando { get; set; } = new();

}
