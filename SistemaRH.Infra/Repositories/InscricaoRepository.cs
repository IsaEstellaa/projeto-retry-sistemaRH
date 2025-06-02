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

        // registra a inscrição do candidato
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
                throw new Exception("Erro ao registrar a inscrição", ex);
            }
        }

        // obtem uma inscrição por ID
        public async Task<Inscricao> ObterInscricaoPorId(Guid id)
        {
            return await _context.Inscricoes
                .Include(i => i.VagasParticipando)       // vagas relacionadas
                .Include(i => i.ProcessosParticipando)   // processos seletivos relacionados
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        // obtem todas as inscrições
        public async Task<List<Inscricao>> ObterTodasInscricoes()
        {
            return await _context.Inscricoes
                .Include(i => i.VagasParticipando)
                .Include(i => i.ProcessosParticipando)
                .ToListAsync();
        }

        // atualiza uma inscrição existente
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
                throw new Exception("Erro ao atualizar inscrição", ex);
            }
        }

        // exclui uma inscrição
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
                throw new Exception("Erro ao excluir inscrição", ex);
            }
        }


        // vincular vaga a uma inscrição
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


        // vincular processo seletivo à inscrição
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
