using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaRH.Domain;

namespace SistemaRH.Infra.Interfaces
{
    public interface IProcessoRepository
    {
        Task RegistrarProcessoSeletivo(ProcessoSeletivo processo); // DEU CERTO
        Task<ProcessoSeletivo?> ObterProcessoSeletivoPorId(int id); // DEU CERTO
        Task<List<ProcessoSeletivo>> ObterTodosProcessos(); // DEU CERTO
        Task AtualizarProcessoSeletivo(ProcessoSeletivo processo); // DEU CERTO
        Task ExcluirProcessoSeletivo(int id); // DEU CERTO
    }
}
