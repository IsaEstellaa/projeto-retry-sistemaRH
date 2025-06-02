using System;

namespace SistemaRH.Domain
{
    public class InscricaoProcessoSeletivo
    {
        public Guid InscricaoId { get; set; }
        public int ProcessoSeletivoId { get; set; }
        public DateTime DataInscricao { get; set; }
        public string? Status { get; set; }  // Status da inscrição (exemplo: "Em Análise", "Entrevista Agendada", etc.)

        // Propriedades de navegação para Inscricao e ProcessoSeletivo
        public Inscricao Inscricao { get; set; }
        public ProcessoSeletivo ProcessoSeletivo { get; set; }
    }
}
