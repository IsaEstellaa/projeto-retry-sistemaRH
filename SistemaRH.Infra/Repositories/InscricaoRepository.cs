using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaRH.Domain;
using SistemaRH.Infra.Interfaces;

namespace SistemaRH.Infra.Repositories
{
    public class InscricaoRepository : IInscricaoRepository
    {
        private readonly SqlContext _context;

        public InscricaoRepository(SqlContext context)
        {
            _context = context;
        }

        // Registrar a inscrição do candidato
        public async Task RegistrarInscricao(Inscricao inscricao)
        {
            try
            {
                if (inscricao == null) throw new ArgumentNullException(nameof(inscricao));

                _context.Inscricoes.Add(inscricao);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao registrar a inscrição", ex);
            }
        }

        // Obter uma inscrição por ID
        public async Task<Inscricao> ObterInscricaoPorId(Guid id)
        {
            return await _context.Inscricoes
                .Include(i => i.VagasParticipando)
                .Include(i => i.ProcessosParticipando)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        // Obter todas as inscrições
        public async Task<List<Inscricao>> ObterTodasInscricoes()
        {
            return await _context.Inscricoes
                .Include(i => i.VagasParticipando)       // Carrega as vagas relacionadas
                .Include(i => i.ProcessosParticipando)   // Carrega os processos seletivos relacionados
                .ToListAsync();
        }

        // Atualizar uma inscrição
        public async Task AtualizarInscricao(Inscricao inscricao)
        {
            try
            {
                if (inscricao == null) throw new ArgumentNullException(nameof(inscricao));

                _context.Inscricoes.Update(inscricao);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao atualizar inscrição", ex);
            }
        }

        // Excluir uma inscrição
        public async Task ExcluirInscricao(Guid id)
        {
            try
            {
                var inscricao = await ObterInscricaoPorId(id);
                if (inscricao == null)
                {
                    throw new KeyNotFoundException("Inscrição não encontrada.");
                }

                _context.Inscricoes.Remove(inscricao);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao excluir inscrição", ex);
            }
        }


        // Implementação para vincular vaga à inscrição
        public async Task VincularVaga(Guid inscricaoId, Vaga vaga)
        {
            var inscricao = await ObterInscricaoPorId(inscricaoId);
            if (inscricao == null)
                throw new KeyNotFoundException("Inscrição de candidato não encontrada.");

            if (inscricao.VagasParticipando.Any(v => v.Id == vaga.Id))
                throw new InvalidOperationException("Essa Vaga já foi vinculada à inscrição.");

            inscricao.VagasParticipando.Add(vaga);
            _context.Inscricoes.Update(inscricao);
            await _context.SaveChangesAsync();
        }


        // Implementação para vincular processo seletivo à inscrição
        public async Task VincularProcesso(Guid inscricaoId, ProcessoSeletivo processo)
        {
            var inscricao = await ObterInscricaoPorId(inscricaoId);
            if (inscricao == null)
                throw new KeyNotFoundException("Inscrição não encontrada.");

            if (inscricao.ProcessosParticipando.Any(p => p.Id == processo.Id))
                throw new InvalidOperationException("Processo seletivo já foi vinculado à inscrição.");

            inscricao.ProcessosParticipando.Add(processo);
            _context.Inscricoes.Update(inscricao);
            await _context.SaveChangesAsync();
        }

    }
}
