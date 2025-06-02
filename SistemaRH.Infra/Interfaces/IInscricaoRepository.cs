using Microsoft.Extensions.Logging;
using SistemaRH.Domain;

namespace SistemaRH.Infra.Interfaces
{
    public interface IInscricaoRepository
    {
        Task RegistrarInscricao(Inscricao inscricao); // DEU CERTO
        Task<Inscricao?> ObterInscricaoPorId(Guid id); // DEU CERTO
        Task<List<Inscricao>> ObterTodasInscricoes(); // DEU CERTO
        Task AtualizarInscricao(Inscricao inscricao); // DEU CERTO
        Task ExcluirInscricao(Guid id); // DEU CERTO
        Task VincularVaga(Guid inscricaoId, Vaga vaga);
        Task VincularProcesso(Guid inscricaoId, ProcessoSeletivo processo);
    }
}
