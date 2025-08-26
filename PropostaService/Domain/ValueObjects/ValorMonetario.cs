namespace PropostaService.Domain.ValueObjects;

public record ValorMonetario
{
    public decimal Valor { get; }

    public ValorMonetario(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor monetário deve ser maior que zero", nameof(valor));

        if (valor > 999_999_999.99m)
            throw new ArgumentException("Valor monetário não pode exceder 999.999.999,99", nameof(valor));

        Valor = Math.Round(valor, 2);
    }

    public static implicit operator decimal(ValorMonetario valorMonetario) => valorMonetario.Valor;
    public static implicit operator ValorMonetario(decimal valor) => new(valor);

    public ValorMonetario Somar(ValorMonetario outro) => new(Valor + outro.Valor);
    public ValorMonetario Subtrair(ValorMonetario outro) => new(Valor - outro.Valor);
    public ValorMonetario Multiplicar(decimal fator) => new(Valor * fator);
    public ValorMonetario Dividir(decimal divisor)
    {
        if (divisor == 0)
            throw new ArgumentException("Não é possível dividir por zero", nameof(divisor));
        return new(Valor / divisor);
    }

    public bool EhMaiorQue(ValorMonetario outro) => Valor > outro.Valor;
    public bool EhMenorQue(ValorMonetario outro) => Valor < outro.Valor;
    public bool EhIgualA(ValorMonetario outro) => Valor == outro.Valor;

    public override string ToString() => Valor.ToString("C2");
}
