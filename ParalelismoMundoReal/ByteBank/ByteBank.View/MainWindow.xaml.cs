using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {            
            BtnProcessar.IsEnabled = false;
            _cancellationTokenSource = new CancellationTokenSource();
            var contas = r_Repositorio.ObterContaClientes();

            PgsProgresso.Maximum = contas.Count();

            LimparView();

            var inicio = DateTime.Now;
            BtnCancelar.IsEnabled = true;

            //Task.WaitAll(contasTarefas);
            var progress = new Progress<string>(str => PgsProgresso.Value++);
            //var byteBankProgress = new ByteBankProgress<string>(str => PgsProgresso.Value++);
            try
            {
                var resultado = await ConsolidarContas(contas, progress, _cancellationTokenSource.Token).ConfigureAwait(true);
                var fim = DateTime.Now;
                AtualizarView(resultado, fim - inicio);
            }
            catch (OperationCanceledException ex)
            {
                TxtTempo.Text = "Operação cancelada pelo usuário";
            }
            finally
            {
                BtnProcessar.IsEnabled = true;
                BtnCancelar.IsEnabled = false;
            }

        }
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
            _cancellationTokenSource.Cancel();

        }
        private async Task<List<string>> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> reportadorDeProgresso, CancellationToken ct)
        {
            var taskSchedulerGui = TaskScheduler.FromCurrentSynchronizationContext();
            var tasks = contas.Select(conta =>
            {
                return Task.Factory.StartNew(() =>
                {
                    //if (ct.IsCancellationRequested)
                    //    throw new OperationCanceledException(ct);
                    ct.ThrowIfCancellationRequested();
                    var resultadoConsolidadcao = r_Servico.ConsolidarMovimentacao(conta, ct);

                    reportadorDeProgresso.Report(resultadoConsolidadcao);
                    ct.ThrowIfCancellationRequested();
                    return resultadoConsolidadcao;
                }, ct);
            });

            var result = await Task.WhenAll(tasks);

            return result.ToList();
        }
        private void LimparView()
        {
            PgsProgresso.Value = 0;
            LstResultados.ItemsSource = null;
            TxtTempo.Text = null;
        }
        private void AtualizarView(List<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;            
        }
    }
}