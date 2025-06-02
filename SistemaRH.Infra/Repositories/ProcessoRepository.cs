using Microsoft.EntityFrameworkCore;
using SistemaRH.Domain;
using SistemaRH.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaRH.Infra.Repositories
{
    public class ProcessoRepository : IProcessoRepository
    {
        private readonly SqlContext _context;

        public ProcessoRepository(SqlContext context)
        {
            _context = context;
        }

        // Registrar um novo processo seletivo
        public async Task RegistrarProcessoSeletivo(ProcessoSeletivo processo)
        {
            try
            {
                if (processo == null) throw new ArgumentNullException(nameof(processo));

                _context.Processos.Add(processo);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao registrar o processo seletivo", ex);
            }
        }

        // Obter um processo seletivo por ID
        public async Task<ProcessoSeletivo?> ObterProcessoSeletivoPorId(int id)
        {
            try
            {
                return await _context.Processos
                                     .Where(p => p.Id == id)
                                     .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao obter o processo seletivo", ex);
            }
        }

        // Obter todos os processos seletivos
        public async Task<List<ProcessoSeletivo>> ObterTodosProcessos()
        {
            try
            {
                return await _context.Processos
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao obter processos seletivos", ex);
            }
        }

        // Atualizar um processo seletivo existente
        public async Task AtualizarProcessoSeletivo(ProcessoSeletivo processo)
        {
            try
            {
                if (processo == null) throw new ArgumentNullException(nameof(processo));

                _context.Processos.Update(processo);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao atualizar o processo seletivo", ex);
            }
        }

        // Excluir um processo seletivo pelo ID
        public async Task ExcluirProcessoSeletivo(int id)
        {
            try
            {
                var processo = await ObterProcessoSeletivoPorId(id);
                if (processo == null)
                {
                    throw new KeyNotFoundException("Processo seletivo não encontrado.");
                }

                _context.Processos.Remove(processo);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log de erro pode ser adicionado aqui
                throw new Exception("Erro ao excluir o processo seletivo", ex);
            }
        }
    }
}
