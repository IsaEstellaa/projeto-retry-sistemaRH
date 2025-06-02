using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SistemaRH.Domain;
using SistemaRH.Infra;
using SistemaRH.Infra.Interfaces;
using SistemaRH.Infra.Messaging;
using SistemaRH.Infra.Repositories;
using System.Diagnostics;
using System.Threading.Tasks;

[ApiController]
[Route("api/inscricoes")]
public class InscricoesController : ControllerBase
{
    private readonly IInscricaoRepository _inscricaoRepository;
    private readonly IVagaRepository _vagaRepository;
    private readonly IProcessoRepository _processoRepository;
    private readonly InscricaoPublisher _inscricaoPublisher;


    public InscricoesController(IInscricaoRepository inscricaoRepository, IVagaRepository vagaRepository, IProcessoRepository processoRepository, InscricaoPublisher inscricaoPublisher)
    {
        _inscricaoRepository = inscricaoRepository;
        _vagaRepository = vagaRepository;
        _processoRepository = processoRepository;
        _inscricaoPublisher = inscricaoPublisher;
    }

    // Registrar uma nova inscrição
    [HttpPost(Name = "Registrar_inscricao_de_candidato")]
    public async Task<IActionResult> RegistrarInscricao([FromBody] Inscricao inscricao)
    {

        if (inscricao == null)
        {
            return BadRequest("Dados da inscrição são inválidos.");
        }

        _inscricaoPublisher.Publicar(inscricao);
        return Accepted("Processo Seletivo enviado para processamento.");

        // Simula o erro no registro de uma inscricoa
        /*throw new Exception("Erro simulado ao salvar no banco de dados :/ ");*/
    }

    // Obter uma inscrição por ID
    [HttpGet("{id}", Name = "Obter_inscricao_de_candidato_por_ID")]
    public async Task<IActionResult> ObterInscricaoPorId(Guid id)
    {
        var inscricao = await _inscricaoRepository.ObterInscricaoPorId(id); // Supondo que você tenha esse método no repositório

        if (inscricao == null)
        {
            return NotFound($"Inscrição com ID {id} não encontrada."); // 404 Not Found
        }

        return Ok(inscricao); // 200 OK
    }

    // Obter todas as inscrições
    [HttpGet(Name = "Obter_todas_as_inscricoes_de_candidato")]
    public async Task<IActionResult> ObterTodasInscricoes()
    {
        var inscricoes = await _inscricaoRepository.ObterTodasInscricoes(); // Supondo que você tenha esse método no repositório
        return Ok(inscricoes); // 200 OK
    }

    // Atualizar uma inscrição existente
    [HttpPut("{id}", Name = "Atualizar_inscricao_de_candidato")]
    public async Task<IActionResult> AtualizarInscricao(Guid id, [FromBody] Inscricao inscricao)
    {
        var inscricaoExistente = await _inscricaoRepository.ObterInscricaoPorId(id);
        if (inscricaoExistente == null)
        {
            return NotFound("Inscrição não encontrada.");
        }

        // Atualiza os campos
        inscricaoExistente.NomeCandidato = inscricao.NomeCandidato;
        if (!string.IsNullOrEmpty(inscricao.EmailCandidato))
            inscricaoExistente.EmailCandidato = inscricao.EmailCandidato;
        if (inscricao.DataNasc.HasValue)
            inscricaoExistente.DataNasc = inscricao.DataNasc.Value;

        // Salva as alterações no banco de dados (passa a entidade já existente)
        await _inscricaoRepository.AtualizarInscricao(inscricaoExistente);

        return Ok(inscricaoExistente);
    }

    // Excluir uma inscrição
    [HttpDelete("{id}", Name = "Excluir_inscricao_de_candidato")]
    public async Task<IActionResult> ExcluirInscricao(Guid id)
    {
        try
        {
            var inscricao = await _inscricaoRepository.ObterInscricaoPorId(id);
            if (inscricao == null)
            {
                return NotFound($"Inscrição com ID {id} não encontrada.");
            }

            await _inscricaoRepository.ExcluirInscricao(id); // Supondo que você tenha esse método no repositório
            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao excluir a inscrição: {ex.Message}");
        }
    }

    // Simulando um erro para testar o Status 500 (erro interno)
    [HttpGet("simulate-500", Name = "Simular_erro_na_inscricao_de_candidato")]
    public IActionResult SimularErroInscricao()
    {
        throw new Exception("Um erro aconteceu durante a inscrição! :(");
    }


    // VINCULAR UMA VAGA
    [HttpPost("{inscricaoId}/vagas/{vagaId}", Name = "Cadastrar_candidato_a_uma_vaga")]
    public async Task<IActionResult> VincularVaga(Guid inscricaoId, Guid vagaId)
    {
        var inscricao = await _inscricaoRepository.ObterInscricaoPorId(inscricaoId);
        if (inscricao == null)
            return NotFound($"Inscrição com ID {inscricaoId} não encontrada.");

        var vaga = await _vagaRepository.ObterVagaPorId(vagaId);
        if (vaga == null)
            return NotFound($"Vaga com ID {vagaId} não encontrada.");

        try
        {
            await _inscricaoRepository.VincularVaga(inscricaoId, vaga);
            return Ok("Vaga vinculada com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);  // Já sabemos que é vaga já vinculada
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao vincular vaga: {ex.Message}");
        }
    }


    // VINCULAR PROCESSOS

    [HttpPost("{inscricaoId}/processos/{processoId}", Name = "Cadastrar_candidato_a_um_processo_seletivo")]
    public async Task<IActionResult> VincularProcesso(Guid inscricaoId, int processoId)
    {
        var inscricao = await _inscricaoRepository.ObterInscricaoPorId(inscricaoId);
        if (inscricao == null)
            return NotFound($"Inscrição com ID {inscricaoId} não encontrada.");

        var processo = await _processoRepository.ObterProcessoSeletivoPorId(processoId);
        if (processo == null)
            return NotFound($"Processo com ID {processoId} não encontrado.");

        try
        {
            await _inscricaoRepository.VincularProcesso(inscricaoId, processo);
            return Ok("Processo seletivo vinculado com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);  // Processo já vinculado
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao vincular processo: {ex.Message}");
        }
    }

}
