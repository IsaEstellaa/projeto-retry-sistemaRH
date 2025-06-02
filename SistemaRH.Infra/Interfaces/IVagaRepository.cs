using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaRH.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaRH.Infra.Interfaces
{
    public interface IVagaRepository
    {
        Task RegistrarVaga(Vaga vaga); // DEU CERTO
        Task<Vaga?> ObterVagaPorId(Guid id); // DEU CERTO
        Task<List<Vaga>> ObterTodasVagas(); // DEU CERTO
        Task AtualizarVaga(Vaga vaga); // DEU CERTO
        Task ExcluirVaga(Guid id); // DEU CERTO

    }
}
