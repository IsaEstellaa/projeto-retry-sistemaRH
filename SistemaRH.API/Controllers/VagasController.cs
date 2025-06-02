using Microsoft.AspNetCore.Mvc;
using SistemaRH.Domain;
using SistemaRH.Infra;
using System.Diagnostics;
using SistemaRH.Infra.Interfaces;
using SistemaRH.Infra.Repositories;
using SistemaRH.Infra.Messaging;


[ApiController]
[Route("api/vagas")]
public class VagasController : ControllerBase
{
    private readonly IVagaRepository _vagaRepository;
    private readonly VagaPublisher _vagaPublisher;

    public VagasController(IVagaRepository vagaRepository, VagaPublisher vagaPublisher)
    {
        _vagaRepository = vagaRepository;
        _vagaPublisher = vagaPublisher;
    }

    // Registrar uma nova vaga
    [HttpPost(Name = "Registrar_vaga_de_emprego")]
    public async Task<IActionResult> RegistrarVaga([FromBody] Vaga vaga)
    {
        _vagaPublisher.Publicar(vaga);

        return Accepted("Vaga enviada para processamento.");

        // Simula o erro no registro de uma inscricoa
        /*throw new Exception("Erro simulado ao salvar no banco de dados :/ ");*/
    }


    // Obter uma vaga por ID
    [HttpGet("{id}", Name = "Obter_vaga_de_emprego_por_ID")]
    public async Task<IActionResult> ObterVagaPorId(Guid id)
    {
        var vaga = await _vagaRepository.ObterVagaPorId(id);
        if (vaga == null)
            return NotFound("Vaga não encontrada.");

        return Ok(vaga);
    }


    // Obter todas as vagas
    [HttpGet(Name = "Obter_todas_as_vagas" )]
    public async Task<IActionResult> ObterTodasVagas()
    {
        var vagas = await _vagaRepository.ObterTodasVagas();
        return Ok(vagas);  // Retorna 200 com a lista de vagas
    }


    // Atualizar uma vaga
    [HttpPut("{id}", Name = "Atualiza_a_vaga")]
    public async Task<IActionResult> AtualizarVaga(Guid id, [FromBody] Vaga vaga)
    {
 
        var vagaExistente = await _vagaRepository.ObterVagaPorId(id);  // esperando verificar se ja nao existe
        if (vagaExistente == null)
        {
            return NotFound("Vaga não encontrada.");
        }

        // atualiza os campos
        vagaExistente.Titulo = vaga.Titulo; // obrigatório
        if (!string.IsNullOrEmpty(vaga.Descricao))
            vagaExistente.Descricao = vaga.Descricao;
        if (!string.IsNullOrEmpty(vaga.Localizacao))
            vagaExistente.Localizacao = vaga.Localizacao;
        
        vagaExistente.DataPublicacao = vaga.DataPublicacao; // obrigatório
        vagaExistente.Salario = vaga.Salario;

        // Manda as alterações para o repository
        await _vagaRepository.AtualizarVaga(vagaExistente);  // Método que atualiza a vaga
        return Ok(vagaExistente);  // Retorna a vaga atualizada
    }

    // Excluir uma vaga
    [HttpDelete("{id}", Name = "Exclui_uma_vaga")]
    public async Task<IActionResult> ExcluirVaga(Guid id)
    {
        var vaga = await _vagaRepository.ObterVagaPorId(id);
        if (vaga == null)
        {
            return NotFound("Vaga não encontrada.");  // Retorna 404 se a vaga não for encontrada
        }

        await _vagaRepository.ExcluirVaga(id);
        return NoContent();  // Retorna 204 se a exclusão for bem-sucedida
    }


    // Simular um erro 500
    [HttpGet("simulate-500", Name = "Simular_erro_na_vaga")]
    public IActionResult SimulateError500()
    {
        // Simulando erro no processo
        throw new Exception("Um erro aconteceu durante a inclusão da Vaga! :(");
    }
}