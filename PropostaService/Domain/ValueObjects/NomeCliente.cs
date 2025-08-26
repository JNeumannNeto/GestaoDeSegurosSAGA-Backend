using System.Text.RegularExpressions;

namespace PropostaService.Domain.ValueObjects;

public record NomeCliente
{
    public string Valor { get; }

    public NomeCliente(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Nome do cliente não pode ser vazio", nameof(valor));

        if (valor.Length < 2)
            throw new ArgumentException("Nome do cliente deve ter pelo menos 2 caracteres", nameof(valor));

        if (valor.Length > 100)
            throw new ArgumentException("Nome do cliente não pode ter mais de 100 caracteres", nameof(valor));

        if (!Regex.IsMatch(valor, @"^[a-zA-ZÀ-ÿ\s]+$"))
            throw new ArgumentException("Nome do cliente deve conter apenas letras e espaços", nameof(valor));

        Valor = valor.Trim();
    }

    public static implicit operator string(NomeCliente nomeCliente) => nomeCliente.Valor;
    public static implicit operator NomeCliente(string valor) => new(valor);

    public override string ToString() => Valor;
}
