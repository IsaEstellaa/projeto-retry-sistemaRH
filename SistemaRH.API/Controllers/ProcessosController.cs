using Microsoft.AspNetCore.Mvc;
using SistemaRH.Infra;
using SistemaRH.Domain;
using SistemaRH.Infra.Interfaces;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using SistemaRH.Infra.Messaging;

[ApiController]
[Route("api/processos")]
public class ProcessosController : ControllerBase
{
    // chamada de todos os repositórios utilizados
    private readonly IProcessoRepository _processoRepository;
    private readonly ProcessoPublisher _processoPublisher;

    public ProcessosController(IProcessoRepository processoRepository, ProcessoPublisher processoPublisher)
    {
        _processoRepository = processoRepository;
        _processoPublisher = processoPublisher;
    }

    // Registrar o Processo Seletivo
    [HttpPost(Name = "Registrar_processo_seletivo")]
    public async Task<IActionResult> RegistrarProcessoSeletivo([FromBody] ProcessoSeletivo processo)
    {
        if (processo == null)
        {
            return BadRequest("Os dados do processo seletivo são inválidos.");
        }

        // chamada do publisher de processo
        _processoPublisher.Publicar(processo);
        return Accepted("Processo Seletivo enviado para processamento.");

        // Simula o erro no registro de uma inscricoa
        /*throw new Exception("Erro simulado ao salvar no banco de dados :/ ");*/
    }


    // Obter um processo seletivo por ID
    [HttpGet("{id}", Name = "Obter_processo_seletivo_por_ID")]
    public async Task<IActionResult> ObterProcessoSeletivoPorId(int id)
    {
        var processo = await _processoRepository.ObterProcessoSeletivoPorId(id);

        if (processo == null)
        {
            return NotFound($"Processo seletivo com ID {id} não encontrado.");
        }

        return Ok(processo);
    }


    // Obter todos os processos seletivos
    [HttpGet(Name = "Obter_todos_os_processos_seletivos")]
    public async Task<IActionResult> ObterTodosProcessos()
    {
        var processos = await _processoRepository.ObterTodosProcessos();
        return Ok(processos);
    }


    // Atualizar um processo seletivo existente
    [HttpPut("{id}", Name = "Atualizar_um_processo_seletivo")]
    public async Task<IActionResult> AtualizarProcessoSeletivo(int id, [FromBody] ProcessoSeletivo processo)
    {
        
        if (processo == null)
        {
            return BadRequest("Os dados enviados são inválidos.");
        }

        // existe no banco de dados?
        var processoExistente = await _processoRepository.ObterProcessoSeletivoPorId(id);
        if (processoExistente == null)
        {
            return NotFound($"O processo seletivo com o ID {id} não foi encontrado.");
        }

        // atualizando os campos existentes
        processoExistente.Nome = processo.Nome;
        if (processo.DataInicio.HasValue)
            processoExistente.DataInicio = processo.DataInicio;
        if (processo.DataFim.HasValue)
            processoExistente.DataFim = processo.DataFim;

        // salva as alterações no banco de dados
        await _processoRepository.AtualizarProcessoSeletivo(processoExistente);
        return NoContent();
    }


    // Excluir um processo seletivo
    [HttpDelete("{id}", Name = "Excluir_um_processo")]
    public async Task<IActionResult> ExcluirProcessoSeletivo(int id)
    {
        var processo = await _processoRepository.ObterProcessoSeletivoPorId(id);
        if (processo == null)
        {
            return NotFound($"Processo seletivo com ID {id} não encontrado.");
        }

        await _processoRepository.ExcluirProcessoSeletivo(id);
        return NoContent();
    }


    // Simulando um erro para testar o Status 500 (erro interno)
    [HttpGet("simulate-500", Name = "Simular_erro_no_processo_seletivo")]
    public IActionResult SimulateError500()
    {
        throw new Exception("Um erro aconteceu durante o processo seletivo! :(");
    }
}
