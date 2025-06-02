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
    public class VagaRepository: IVagaRepository
    {
        private readonly SqlContext _context;

        public VagaRepository(SqlContext context)
        {
            _context = context;
        }

        // Registrar a vaga de emprego
        public async Task RegistrarVaga(Vaga vaga)
        {
            try
            {
                if (vaga == null) throw new ArgumentNullException(nameof(vaga));

                await _context.Vagas.AddAsync(vaga);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log ou trate a exceção conforme necessário
                throw new Exception("Erro ao registrar vaga", ex);
            }
        }

        // Obter uma vaga por ID
        public async Task<Vaga?> ObterVagaPorId(Guid id)
        {
            try
            {
                return await _context.Vagas
                                     .Where(v => v.Id == id)
                                     .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Log ou trate a exceção conforme necessário
                throw new Exception("Erro ao obter vaga por ID", ex);
            }
        }

        // Obter todas as vagas
        public async Task<List<Vaga>> ObterTodasVagas()
        {
            try
            {
                return await _context.Vagas
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log ou trate a exceção conforme necessário
                throw new Exception("Erro ao obter todas as vagas", ex);
            }
        }

        // Atualizar uma vaga
        public async Task AtualizarVaga(Vaga vaga)
        {
            try
            {
                if (vaga == null) throw new ArgumentNullException(nameof(vaga));

                _context.Vagas.Update(vaga);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log ou trate a exceção conforme necessário
                throw new Exception("Erro ao atualizar vaga", ex);
            }
        }

        // Excluir uma vaga
        public async Task ExcluirVaga(Guid id)
        {
            try
            {
                var vaga = await ObterVagaPorId(id);
                if (vaga == null)
                {
                    throw new KeyNotFoundException("Vaga não encontrada.");
                }

                _context.Vagas.Remove(vaga);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log ou trate a exceção conforme necessário
                throw new Exception("Erro ao excluir vaga", ex);
            }
        }
    }
}
