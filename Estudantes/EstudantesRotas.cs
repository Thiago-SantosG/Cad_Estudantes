using ApiCrud.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes
{
    public static class EstudantesRotas
    {
        public static void AddRotasEstudantes(this WebApplication app)
        {
            var rotasEstudantes = app.MapGroup("estudantes");

            // Para criar se usa o Post
            rotasEstudantes.MapPost(pattern:"", 
                handler: async (AddEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
                { 

                var JaExiste = await context.Estudantes.AnyAsync(estudante => estudante.Nome == request.Nome, ct);

                if (JaExiste)
                    return Results.Conflict(error: "Ja Existe!");

                var novoestudante = new Estudante(request.Nome);
                await context.Estudantes.AddAsync(novoestudante, ct);
                await context.SaveChangesAsync(ct);

                var estudanteRetorno = new EstudanteDTO(novoestudante.Id, novoestudante.Nome);

                return Results.Ok(novoestudante);

                });

            // retorno dos user cadastrados
            rotasEstudantes.MapGet("", async (AppDbContext context, CancellationToken ct) =>
            {

                var estudantes = await context
                .Estudantes
                .Where(estudante => estudante.Ativo)
                .Select(estudante => new EstudanteDTO(estudante.Id, estudante.Nome))
                .ToListAsync(ct);

                return estudantes;

            });

            //atualizar Nome estudante
            rotasEstudantes.MapPut("{id:guid}", async(Guid id, UpdateEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if (estudante == null)
                    return Results.NotFound();

                estudante.AtualizarNome(request.Nome);

                await context.SaveChangesAsync(ct);
                return Results.Ok(new EstudanteDTO(estudante.Id, estudante.Nome));
            });

            //Deletar
            rotasEstudantes.MapDelete("{id}", async (Guid id, AppDbContext context, CancellationToken ct) =>
            {

                var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if (estudante == null)
                    return Results.NotFound();

                estudante.Desativar();

                await context.SaveChangesAsync(ct);
                return Results.Ok();
            });
        } 
    }
}
