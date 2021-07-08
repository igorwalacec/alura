using Bogus;
using ByteBank.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteBank.Core.Repository
{
    public class ContaClienteRepository
    {
        public IEnumerable<ContaCliente> ObterContaClientes()
        {
            return new Faker<ContaCliente>()
                .RuleFor(p => p.Nome, f => f.Name.FirstName())
                .RuleFor(p => p.Movimentacoes,
                    new Faker<Movimento>()
                    .RuleFor(p => p.Valor, f => f.Random.Decimal())
                    .RuleFor(p => p.Tipo, f => f.PickRandom<TipoMovimento>())
                    .RuleFor(p => p.Data, f => f.Date.Recent())
                .Generate(1500))
                .Generate(10);                    
        }
    }
}
