#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.CustomIndicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
	public class IchimokuStrategyV2 : Strategy
	{
		private Ichimoku ichimoku;
		
		private const int SIGNAL_BUY = 1;
		private const int SIGNAL_SELL = 0;
		
		private const string ORIGEM_SINAL_COMPRA = "Origem Sinal Compra";
        private const string ORIGEM_SINAL_VENDA = "Origem Sinal Venda";
		
        private int CurrentBarForLog;

        private double valorInicialAgregadorSaldo = 0.0;

        double MetaFinanceiraAnterior = 0.0;
        double MetaFinanceiraAtual = 0.0;

        double Alavancagem = 500.0;
        double TamanhoLoteForex = 100000.0;


        private string TradeHistoryLastCalc;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "IchimokuStrategyV2";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				TenkanSen			= 9;
				KijunSen			= 26;
				SenkouSpanBPeriod	= 52;
				CloudOpacity		= 40;
				
				LoteDinamico = false;
                LoteInicial = 0.01;
                MultiplicadorLote = 0.01;
                CapitalEmRisco = 100.00;
                Balance = 100.00;
                DistanciaEntreOrdens = 50.00;
                MaximoDeOrdens = 5;
                TakeProfitFinanceiroPorContrato = 2.00;
                StopLossFinanceiroPorContrato = 4.00;
                AgregadorDeMeta = 36.00;
                Sl = 0.00;
                Tp = 2.00;
                SaldoDaContaBacktest = 100.00;
                IsBackTest = true;
                LoteMinimo = 0.01;
                OperarForex = false;
                CloseFriday = false;

                // Times
                Session1 = false;
                Session1Start = 90500;
                Session1End = 174500;

                Session2 = false;
                Session2Start = 90000;
                Session2End = 110000;

                Session3 = false;
                Session3Start = 120000;
                Session3End = 125900;
				
				
			}
			else if (State == State.Configure)
			{
			}
			else if(State == State.DataLoaded) 
			{
				ichimoku = Ichimoku(TenkanSen, KijunSen, SenkouSpanBPeriod, CloudOpacity);
				AddChartIndicator(ichimoku);
				
			}
		}

		private bool SinalCompra()
        {
			bool filter = false;
 			
			try {
				
				double ten = ichimoku.Tenkan[0];
				double kij = ichimoku.Kijun[0];
				double spanA = ichimoku.SenkouSpanA[0];
				double spanB = ichimoku.SenkouSpanB[0];
				double chinkou = ichimoku.ChikouSpan[0];
				double ten1 = ichimoku.Tenkan[1];
				double kij1 = ichimoku.Kijun[1];
				double spanA1 = ichimoku.SenkouSpanA[1];
				double spanB1 = ichimoku.SenkouSpanB[1];
				double chinkou1 = ichimoku.ChikouSpan[1];
				double ten2 = ichimoku.Tenkan[2];
				double kij2 = ichimoku.Kijun[2];
				double spanA2 = ichimoku.SenkouSpanA[2];
				double spanB2 = ichimoku.SenkouSpanB[2];
				double chinkou2 = ichimoku.ChikouSpan[2];
				

	             filter = ((ten1 <= kij1 && ten > kij && GetCurrentAsk() > spanA1 && GetCurrentAsk() > spanB1 && 
					Open[0] < Close[0]) || 
	      			(chinkou1 <= Close[10] && chinkou > Close[9] && 
	      			GetCurrentAsk() > spanA1 && GetCurrentAsk() > spanB1 && 
	      			Open[0] < Close[0]));
				
			} catch(Exception e) {
				PrintLog("Error on SinalCompra " + e.ToString());
			}

			return filter;
        }
		
		private bool SinalVenda()
        {
			bool filter = false;
 			
			try {
				
				double ten = ichimoku.Tenkan[0];
				double kij = ichimoku.Kijun[0];
				double spanA = ichimoku.SenkouSpanA[0];
				double spanB = ichimoku.SenkouSpanB[0];
				double chinkou = ichimoku.ChikouSpan[0];
				double ten1 = ichimoku.Tenkan[1];
				double kij1 = ichimoku.Kijun[1];
				double spanA1 = ichimoku.SenkouSpanA[1];
				double spanB1 = ichimoku.SenkouSpanB[1];
				double chinkou1 = ichimoku.ChikouSpan[1];
				double ten2 = ichimoku.Tenkan[2];
				double kij2 = ichimoku.Kijun[2];
				double spanA2 = ichimoku.SenkouSpanA[2];
				double spanB2 = ichimoku.SenkouSpanB[2];
				double chinkou2 = ichimoku.ChikouSpan[2];
				

	            filter = ((ten1 >= kij1 && ten < kij && GetCurrentBid() < spanA1 && GetCurrentBid() < spanB1 && 
	      			Open[0] > Close[0]) || 
	      			(chinkou1 >= Open[10] && chinkou < Open[9] && 
	      			GetCurrentBid() < spanA1 && GetCurrentBid() < spanB1 && 
	      			Open[0] > Close[0]));
			
			} catch(Exception e) {
				PrintLog("Error on SinalVenda " + e.ToString());
			}

			return filter;
        }

		
		private bool SinalFechaCompra()
        {
			
			bool filter = false;
 			
			try {
				
				double ten = ichimoku.Tenkan[0];
				double kij = ichimoku.Kijun[0];
				double spanA = ichimoku.SenkouSpanA[0];
				double spanB = ichimoku.SenkouSpanB[0];
				double chinkou = ichimoku.ChikouSpan[0];
				double ten1 = ichimoku.Tenkan[1];
				double kij1 = ichimoku.Kijun[1];
				double spanA1 = ichimoku.SenkouSpanA[1];
				double spanB1 = ichimoku.SenkouSpanB[1];
				double chinkou1 = ichimoku.ChikouSpan[1];
				double ten2 = ichimoku.Tenkan[2];
				double kij2 = ichimoku.Kijun[2];
				double spanA2 = ichimoku.SenkouSpanA[2];
				double spanB2 = ichimoku.SenkouSpanB[2];
				double chinkou2 = ichimoku.ChikouSpan[2];
				

	            filter = ((ten1 >= kij1 && ten < kij && GetCurrentBid() < spanA1 && GetCurrentBid() < spanB1 && 
	      			Open[0] > Close[0]) || 
	      			(chinkou1 >= Open[10] && chinkou < Open[9] && 
	      			GetCurrentBid() < spanA1 && GetCurrentBid() < spanB1 && 
	      			Open[0] > Close[0]));
			
			} catch(Exception e) {
				PrintLog("Error on SinalFechaCompra " + e.ToString());
			}

			return filter;
        }

        private bool SinalFechaVenda()
        {
			bool filter = false;
 			
			try {
				
				double ten = ichimoku.Tenkan[0];
				double kij = ichimoku.Kijun[0];
				double spanA = ichimoku.SenkouSpanA[0];
				double spanB = ichimoku.SenkouSpanB[0];
				double chinkou = ichimoku.ChikouSpan[0];
				double ten1 = ichimoku.Tenkan[1];
				double kij1 = ichimoku.Kijun[1];
				double spanA1 = ichimoku.SenkouSpanA[1];
				double spanB1 = ichimoku.SenkouSpanB[1];
				double chinkou1 = ichimoku.ChikouSpan[1];
				double ten2 = ichimoku.Tenkan[2];
				double kij2 = ichimoku.Kijun[2];
				double spanA2 = ichimoku.SenkouSpanA[2];
				double spanB2 = ichimoku.SenkouSpanB[2];
				double chinkou2 = ichimoku.ChikouSpan[2];
				

	             filter = ((ten1 <= kij1 && ten > kij && GetCurrentAsk() > spanA1 && GetCurrentAsk() > spanB1 && 
					Open[0] < Close[0]) || 
	      			(chinkou1 <= Close[10] && chinkou > Close[9] && 
	      			GetCurrentAsk() > spanA1 && GetCurrentAsk() > spanB1 && 
	      			Open[0] < Close[0]));
				
			} catch(Exception e) {
				PrintLog("Error on SinalFechaVenda " + e.ToString());
			}

			return filter;
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;
			
			if (CurrentBar < TenkanSen || CurrentBar < KijunSen || CurrentBar < SenkouSpanBPeriod || CurrentBar < (TenkanSen + KijunSen) || CurrentBar < (TenkanSen + KijunSen + SenkouSpanBPeriod))
				return;
			
			if (BarsInProgress != 0 || CurrentBars[0] < BarsRequiredToTrade || CurrentBar < BarsRequiredToTrade)
                // Halt further processing of our strategy 
                return;

            this.CurrentBarForLog = CurrentBar;

            int currentTime = ToTime(Time[0]);

            if (!canTradeTime())
            {
                this.CloseAllPositions();
                return;
            }

            if (((CloseFriday == true) && (Time[0].DayOfWeek == DayOfWeek.Friday) && currentTime >= Session1End))
            {
                this.CloseAllPositions();
                return;
            }

            if (this.hasPosition())
            {

                if (Position.MarketPosition == MarketPosition.Long)
                {

                    if (SinalCompra() && SinalVenda())
                    {
                        this.CloseAllPositions();
                        this.MetaFinanceiraAnterior = 0.0;
                    }

                    if (SinalVenda())
                    {
                        this.CloseAllPositions();
                        this.MetaFinanceiraAnterior = 0.0;
                        EnterShort(Convert.ToInt32(this.GetVolume()), ORIGEM_SINAL_VENDA);
                    }
                   if (SinalFechaCompra())
                    {
                        this.CloseAllPositions();
                        this.MetaFinanceiraAnterior = 0.0;
                        //EnterShort(Convert.ToInt32(this.GetVolume()), ORIGEM_SINAL_VENDA); //Dúvida
                    }
                }


                if (Position.MarketPosition == MarketPosition.Short)
                {

                    if (SinalCompra() && SinalVenda())
                    {
                        this.CloseAllPositions();
                        this.MetaFinanceiraAnterior = 0.0;
                    }

                    if (SinalCompra())
                    {
                        this.CloseAllPositions();
                        this.MetaFinanceiraAnterior = 0.0;
                        EnterLong(Convert.ToInt32(this.GetVolume()), ORIGEM_SINAL_COMPRA);
                    }
					if (SinalFechaVenda())
                    {
                        this.CloseAllPositions();
                        this.MetaFinanceiraAnterior = 0.0;
                        //EnterLong(Convert.ToInt32(this.GetVolume()), ORIGEM_SINAL_COMPRA); //Dúvida
                        //return;
                    }
                }

            }
            else
            {
                if (SinalCompra() && SinalVenda())
                {
                    return;
                }
                else
                {
                    if (SinalCompra())
                    {
                        this.MetaFinanceiraAnterior = 0.0;
                        EnterLong(Convert.ToInt32(this.GetVolume()), ORIGEM_SINAL_COMPRA);
                    }

                    if (SinalVenda())
                    {
                        this.MetaFinanceiraAnterior = 0.0;
                        EnterShort(Convert.ToInt32(this.GetVolume()), ORIGEM_SINAL_VENDA);
                    }

                }
            }

            if (IsBackTest)
            {
                this.CloseByProfit();
            }
        }

        public void GetProfitLastTrade()
        {
            if (SystemPerformance.AllTrades.Count > 1)
            {
                Trade lastTrade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - 1];
                //PrintLog("The last trade profit is " + lastTrade.ProfitCurrency);
            }
        }

        public void GetTotalProfit()
        {
            if (SystemPerformance.AllTrades.Count > 1)
            {
                Trade lastTrade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - 1];
                //PrintLog(" ---------------- Trade " + (lastTrade.TradeNumber + 1) + " : The last trade profit is : " + lastTrade.ProfitCurrency);
            }
        }

        private void PrintLog(String mensagem)
        {
            if (this.CurrentBar == 0)
            {
                return;
            }
            Print(string.Format("{0} : {1}", Bars.GetTime(this.CurrentBar), mensagem));
        }

        private void PrintLogChart(String message)
        {
            Draw.Text(this, this.CurrentBar.ToString(), message, 0, High[0] + 18 * TickSize);
        }

        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            if (this.hasPosition())
            {
                this.CloseByProfit();
            }
        }


        private double LoteFixo()
        {
            if (OperarForex)
            {
                return ((CapitalEmRisco * Alavancagem) / (Balance * Alavancagem) * (LoteInicial * TamanhoLoteForex));
            }
            else
            {
                return (CapitalEmRisco / Balance * LoteInicial);
            }
        }

        private double LoteVariavel()
        {
            this.valorInicialAgregadorSaldo = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
            double saldo = SaldoDaContaBacktest + this.valorInicialAgregadorSaldo;

            if (saldo <= 0.0)
            {
                if (OperarForex)
                {
                    return LoteInicial * TamanhoLoteForex;
                }
                else
                {
                    return LoteInicial;
                }

            }

            double capitalPorSaldo = 0.0;

            if (OperarForex)
            {
                capitalPorSaldo = (saldo * Alavancagem) / (CapitalEmRisco * Alavancagem);
            }
            else
            {
                capitalPorSaldo = saldo / CapitalEmRisco;
            }

            double capitalVezesLoteInicial = 0.0;

            if (OperarForex)
            {
                capitalVezesLoteInicial = capitalPorSaldo * (LoteInicial * TamanhoLoteForex);
            }
            else
            {
                capitalVezesLoteInicial = capitalPorSaldo * LoteInicial;
            }

            if (capitalVezesLoteInicial <= 0.0)
            {
                if (OperarForex)
                {
                    return LoteInicial * TamanhoLoteForex;
                }
                else
                {
                    return LoteInicial;
                }
            }

            double result = capitalVezesLoteInicial;

            if (result <= 0.0)
            {
                if (OperarForex)
                {
                    return LoteInicial * TamanhoLoteForex;
                }
                else
                {
                    return LoteInicial;
                }
            }

            if (result < this.LoteMinimo)
            {
                return this.LoteMinimo;
            }

            return result;
        }

		private double GetVolume()
        {
            double result = 0.0;
			double restoDivisao = 0.0;
			
            if (LoteDinamico) {
                result = RoundNumberLotForex(Convert.ToInt32(LoteVariavel()));
            } else {
                result = LoteFixo();
				
				if (OperarForex) 
				{
					restoDivisao = (result % (this.LoteMinimo * TamanhoLoteForex));
				}
				else
				{
					restoDivisao = result % this.LoteMinimo;
				}
				
				//PrintLog("LoteFixo dentro do else modificado - dentro do GetVolume() : " + Convert.ToString(result));
				//PrintLog("restoDivisao : " + Convert.ToString(restoDivisao));
            }

			result = (result - restoDivisao);
			//PrintLog("LoteFixo FINAL  - dentro do GetVolume() : " + Convert.ToString(result));

			

			
            return result;
       }

        private double GetVolumeDerivativas(int volume)
        {
            double value = 0.0;
			
           if (OperarForex)
            {
	            value = Instrument.MasterInstrument.RoundToTickSize(volume * MultiplicadorLote);
	            value = RoundNumberLotForex(Convert.ToInt32(value));
            } else
			{
				value = Instrument.MasterInstrument.RoundToTickSize(volume * MultiplicadorLote);
			}
			
			
            return value;
        }

        private void OpenOrdersDerivativas(MarketPosition marketPosition, string idMotherPosition, double price, int quantity, int totalOrders)
        {
            if ((CountDerivativasOrders(idMotherPosition) < MaximoDeOrdens) && totalOrders < MaximoDeOrdens)
            {
                double currentPrice = 0.0;
                int currentVolume = Convert.ToInt32(GetVolumeDerivativas(quantity));
             
                if (marketPosition == MarketPosition.Long)
                {
                    double distancia = 0.0;
                    if (DistanciaDinamica)
                    {
                        distancia = DistanciaEntreOrdens * (((High[0] - Low[0]) + (High[1] - Low[1])) / 2);
                    }
                    else
                    {
                        if (OperarForex)
                        {
                            distancia = (DistanciaEntreOrdens * TamanhoLoteForex) * TickSize;
                        }
                        else
                        {
                            distancia = DistanciaEntreOrdens;
                        }
                    }

                    currentPrice = price + (distancia * TickSize);

                    Order order = EnterLongStopMarket(0, true, currentVolume, currentPrice, idMotherPosition + ":" + totalOrders);
                    if (order != null)
                    {
                    }
                    else
                    {
                    }
                }
                if (marketPosition == MarketPosition.Short)
                {
                    double distancia = 0.0;
                    if (DistanciaDinamica)
                    {
                        distancia = DistanciaEntreOrdens * (((High[0] - Low[0]) + (High[1] - Low[1])) / 2);
                    }
                    else
                    {
                        if (OperarForex)
                        {
                            distancia = (DistanciaEntreOrdens * TamanhoLoteForex) * TickSize;
                        }
                        else
                        {
                            distancia = DistanciaEntreOrdens;
                        }
                    }

                    currentPrice = price - (distancia * TickSize);

                    Order order = EnterShortStopMarket(0, true, currentVolume, currentPrice, idMotherPosition + ":" + totalOrders);
                    if (order != null)
                    {
                    }
                    else
                    {
                    }
                }
                OpenOrdersDerivativas(marketPosition, idMotherPosition, currentPrice, currentVolume, ++totalOrders);
            }
            return;
        }

        private void SetStopLossOrderOrigin(MarketPosition marketPosition, double price, string fromEntrySignal)
        {
            if (marketPosition == MarketPosition.Long)
            {
                if (Sl > 0)
                {
                    SetStopLoss(fromEntrySignal, CalculationMode.Price, price - (Sl * TickSize), true);
                }
            }
            if (marketPosition == MarketPosition.Short)
            {
                if (Sl > 0)
                {
                    SetStopLoss(fromEntrySignal, CalculationMode.Price, price + (Sl * TickSize), true);
                }
            }
        }

        private void SetStopLossOrderDerivativas(MarketPosition marketPosition, double price, string fromEntrySignal)
        {
            if (marketPosition == MarketPosition.Long)
            {
                if (Sl > 0)
                {
                    SetStopLoss(fromEntrySignal, CalculationMode.Price, price - (Sl * TickSize), true);
                }
            }
            if (marketPosition == MarketPosition.Short)
            {
                if (Sl > 0)
                {
                    SetStopLoss(fromEntrySignal, CalculationMode.Price, price + (Sl * TickSize), true);
                }
            }
        }

        private void SetTargetProfitOrderOrigin(MarketPosition marketPosition, double price, string fromEntrySignal)
        {
            if (marketPosition == MarketPosition.Long)
            {
                if (Sl > 0)
                {
                    SetProfitTarget(fromEntrySignal, CalculationMode.Price, price + (Sl * Tp * TickSize), true);
                }
            }
            if (marketPosition == MarketPosition.Short)
            {
                if (Sl > 0)
                {
                    SetProfitTarget(fromEntrySignal, CalculationMode.Price, price - (Sl * Tp * TickSize), true);
                }
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (execution.IsExit)
            {
                this.Account.CancelAllOrders(execution.Instrument);
                return;
            }

            string orderName = execution.Order.Name;
            bool isOrdemOrigin = (orderName == ORIGEM_SINAL_COMPRA || orderName == ORIGEM_SINAL_VENDA);
            bool isFilled = (execution.Order.OrderState == OrderState.Filled);
            bool isChild = (orderName.Contains(":"));

            if (isOrdemOrigin && isFilled)
            {
                OpenOrdersDerivativas(marketPosition, orderId, price, quantity, 0);
            }

            if (isFilled && (isOrdemOrigin || isChild))
            {
                this.MetaFinanceiraAtual = this.GetMetaFinanceira();
                this.SetStopLossOrderOrigin(marketPosition, price, orderName);
                this.SetTargetProfitOrderOrigin(marketPosition, price, orderName);
            }
        }

        private int CountDerivativasOrders(string idMotherPosition)
        {
            int count = 0;
            foreach (Order order in this.Account.Orders)
            {
                if ((order.OrderType == OrderType.StopMarket) && (order.Name.Split(':')[0] == idMotherPosition))
                {
                    count++;
                }
            }
            return count;
        }

        private double StopLossFinanceiroLoteVariavelPorContrato()
        {
            return (StopLossFinanceiroPorContrato / (LoteInicial)) * GetVolume();
        }

        private double TakeProfitFinanceiroLoteVariavelPorContrato(double volume)
        {
            return ((StopLossFinanceiroPorContrato * TakeProfitFinanceiroPorContrato) / (LoteInicial)) * GetVolume();
        }

        private double GetMetaFinanceira()
        {
            double volume = GetVolume();
            double volumeInPosition = Position.Quantity;
            double passoZero = 0.0;
            double passoUm = 0.0;
            double passoDois = 0.0;

            if (this.MetaFinanceiraAnterior == 0.0)
            {
                this.MetaFinanceiraAnterior = TakeProfitFinanceiroLoteVariavelPorContrato(volume);
            }

            else
            {
                this.MetaFinanceiraAnterior = this.MetaFinanceiraAnterior;
                passoZero = (((StopLossFinanceiroPorContrato * TakeProfitFinanceiroPorContrato) / volume) * volumeInPosition);
                passoUm = (this.MetaFinanceiraAnterior * (AgregadorDeMeta / 100.00));

                this.MetaFinanceiraAnterior = passoZero + passoUm;
            }

            return this.MetaFinanceiraAnterior;
        }

        private double GetUnrealizedProfitLossPositionAccount()
        {
            double baseValue = 0.0;

            if (PositionAccount.MarketPosition == MarketPosition.Long)
            {
                baseValue = GetCurrentAsk();
            }

            if (PositionAccount.MarketPosition == MarketPosition.Short)
            {
                baseValue = GetCurrentBid();
            }
            return Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
        }

        private void CloseByProfit()
        {
            if (Position.MarketPosition != MarketPosition.Flat)
            {
                double metaFinanceira = MetaFinanceiraAtual;
                double metaPerda = StopLossFinanceiroLoteVariavelPorContrato();

                double currentProfit = GetUnrealizedProfitLossPositionAccount();

                bool atingiuMetaLucro = currentProfit >= metaFinanceira;
                bool atingiuMetaPerda = (currentProfit * -1) >= metaPerda;

                if (atingiuMetaLucro || atingiuMetaPerda)
                {
                    if (atingiuMetaLucro)
                    {
                    }
                    if (atingiuMetaPerda)
                    {
                    }

                    this.CloseAllPositions();
                }
            }
        }

        private bool hasPosition()
        {
            bool hasPosition = false;
            if (Position.MarketPosition == MarketPosition.Long || Position.MarketPosition == MarketPosition.Short)
            {
                if (Position.MarketPosition == MarketPosition.Long)
                {
                }
                if (Position.MarketPosition == MarketPosition.Short)
                {
                }
                hasPosition = true;
            }
            return hasPosition;
        }

        private bool hasPositionBuy()
        {
            return this.Account.Positions[this.Account.Positions.Count - 1].MarketPosition == MarketPosition.Long;
        }

        private bool hasPositionSell()
        {
            return this.Account.Positions[this.Account.Positions.Count - 1].MarketPosition == MarketPosition.Short;
        }


        private void CloseAllPositions()
        {
            if (Position.MarketPosition == MarketPosition.Long)
            {
				//PrintLog("ClosePositions Long");
                Order order = ExitLong();
                if (order == null)
                {
                }
                else
                {
                }
            }
            if (Position.MarketPosition == MarketPosition.Short)
            {
                //PrintLog("ClosePositions Short");
				Order order = ExitShort();
                if (order == null)
                {
                }
                else
                {
                }
            }
        }
        
        private double GetCurrentProfit()
        {
            double val = 0.0;
            try
            {
                val = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;

            }
            catch (Exception e)
            {
                this.PrintLog("Erros on GetCurrentProfit : " + e.Message);
            }
            return val;
        }
        private int RoundNumberLotForex(int num)
        {
            int rem = num % 1000;
            return rem >= 500 ? (num - rem + 1000) : (num - rem);
        }

        private bool canTradeTime()
        {
            int currentTime = ToTime(Time[0]);
            bool doTrade = false;

			if (Session1 || Session2 || Session3) 
			{
	            if ((Session1
	                && (currentTime >= Session1Start && currentTime <= Session1End)))
	            {
	                doTrade = true;
	            }
	            else if ((Session2
	                && (currentTime >= Session2Start && currentTime <= Session2End)))
	            {
	                doTrade = true;
	            }
	            else if ((Session3
	                    && (currentTime >= Session3Start && currentTime <= Session3End)))
	            {
	                doTrade = true;
	            }
			}
			else
			{		
				doTrade = true;
			}
            return doTrade;
        }

    
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tenkan - sen", Order=1, GroupName="Parameters")]
		public int TenkanSen
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Kijun - sen", Order=2, GroupName="Parameters")]
		public int KijunSen
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Senkou Span B", Order=3, GroupName="Parameters")]
		public int SenkouSpanBPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name="Cloud Opacity", Order=4, GroupName="Parameters")]
		public int CloudOpacity
		{ get; set; }

		//até aqui ichimoku
			
		[NinjaScriptProperty]
        [Display(Name = "Lote Dinamico?", GroupName = "Configuracoes", Order = 0)]
        public bool LoteDinamico { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "LoteInicial", GroupName = "Configuracoes", Order = 1)]
        public double LoteInicial { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "Multiplicador Lote", GroupName = "Configuracoes", Order = 2)]
        public double MultiplicadorLote { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "Capital em Risco", GroupName = "Configuracoes", Order = 3)]
        public double CapitalEmRisco { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "Balance", GroupName = "Configuracoes", Order = 4)]
        public double Balance { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Distância Ordens Dinamica?", GroupName = "Configuracoes", Order = 5)]
        public bool DistanciaDinamica { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "Distancia entre ordens", GroupName = "Configuracoes", Order = 6)]
        public double DistanciaEntreOrdens { get; set; }

        [Range(0, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Máximo de ordens", GroupName = "Configuracoes", Order = 7)]
        public int MaximoDeOrdens { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "Lucro por Lote - Nr x Prej. Lote", GroupName = "Configuracoes", Order = 8)]
        public double TakeProfitFinanceiroPorContrato { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "Prejuizo por Lote", GroupName = "Configuracoes", Order = 9)]
        public double StopLossFinanceiroPorContrato { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "Agregador de Meta - Pós 2nd Ordem", GroupName = "Configuracoes", Order = 10)]
        public double AgregadorDeMeta { get; set; }

        [Range(0.00, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "SL", GroupName = "Configuracoes", Order = 11)]
        public double Sl { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "TP - Nr x SL", GroupName = "Configuracoes", Order = 12)]
        public double Tp { get; set; }

        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "SaldoDaContaBacktest", Description = "Saldo ca conta para backtest", Order = 13, GroupName = "Configuracoes")]
        public double SaldoDaContaBacktest { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "IsBackTest", Description = "IsBackTest", Order = 14, GroupName = "Configuracoes")]
        public bool IsBackTest { get; set; }


        [Range(0.01, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "LoteMinimo", GroupName = "Configuracoes", Order = 16)]
        public double LoteMinimo
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Operação em Forex?", GroupName = "Configuracoes", Order = 0)]
        public bool OperarForex { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "CloseFriday", Description = "Close trading in Friday", Order = 17, GroupName = "Configuracoes")]
        public bool CloseFriday
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Use Session 1 times?", Description = "True to use Session 1 start/end times)", Order = 1, GroupName = "Times")]
        public bool Session1
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 235959)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Session 1 start time", Description = "Session 1 start time", Order = 2, GroupName = "Times")]
        public int Session1Start
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 235959)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Session 1 end time", Description = "Session 1 end time", Order = 3, GroupName = "Times")]
        public int Session1End
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Use Session 2 times?", Description = "True to use Session 3 start/end times)", Order = 4, GroupName = "Times")]
        public bool Session2
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 235959)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Session 2 start time", Description = "Session 2 start time", Order = 5, GroupName = "Times")]
        public int Session2Start
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 235959)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Session 2 end time", Description = "Session 1 end time", Order = 6, GroupName = "Times")]
        public int Session2End
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Use Session 3 times?", Description = "True to use Session 3 start/end times)", Order = 7, GroupName = "Times")]
        public bool Session3
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 235959)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Session 3 start time", Description = "Session 3 end time", Order = 8, GroupName = "Times")]
        public int Session3Start
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 235959)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Session 3 end time", Description = "Session 3 end time", Order = 9, GroupName = "Times")]
        public int Session3End
        { get; set; }



        
	}
}
