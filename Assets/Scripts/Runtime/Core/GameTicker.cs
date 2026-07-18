using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mojipet.Systems;
using Mojipet.Utilities;

namespace Mojipet.Core
{
    public sealed class GameTicker
    {
        private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

        private readonly PetSystem _petSystem;
        private readonly ResearchSystem _researchSystem;
        private readonly CurrencySystem _currencySystem;

        private CancellationTokenSource _cts;
        private DateTime _lastTickUtc;

        public GameTicker(PetSystem petSystem, ResearchSystem researchSystem, CurrencySystem currencySystem)
        {
            _petSystem = petSystem;
            _researchSystem = researchSystem;
            _currencySystem = currencySystem;
        }

        public void Start()
        {
            if (_cts != null)
            {
                return;
            }

            _cts = new CancellationTokenSource();
            _lastTickUtc = TimeUtility.CurrentUtc;
            RunAsync(_cts.Token).Forget();
        }

        public void Stop()
        {
            if (_cts == null)
            {
                return;
            }

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        private async UniTaskVoid RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TickInterval, cancellationToken: token).SuppressCancellationThrow();
                if (token.IsCancellationRequested)
                {
                    return;
                }

                Tick();
            }
        }

        private void Tick()
        {
            var now = TimeUtility.CurrentUtc;
            var elapsed = now - _lastTickUtc;
            _lastTickUtc = now;

            if (elapsed <= TimeSpan.Zero)
            {
                return;
            }

            _petSystem.UpdateHunger(elapsed);
            _researchSystem.UpdateResearch();

            var production = _petSystem.CalculateProduction(elapsed);
            if (production > 0)
            {
                _currencySystem.AddMoney(production);
            }
        }
    }
}
