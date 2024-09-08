using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using CoreLibrary;
using CoreLibrary.Interface;
using MatchTrade.Dtos.Messages;
using MatchTrade.Enums;
using MatchTrade.Services;
using MatchTrade.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace MatchTradeTest
{
    public class UnitTestWithDI
    {
        private IOrderStorageService _orderStorageService;
        private MmPartnerOrderMatchService _mmPartnerOrderMatchService;
        private MemberOrderMatchService _memberOrderMatchService;
        private static IHost _host = null;
        private ITimeService _matchTradeTimeService;

        static T GetService<T>(IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<T>();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            _host = MatchTrade.Program.CreateHostBuilder(null)
                    .ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        builder.RegisterType<FakeMatchTradeContextFactory>().AsImplementedInterfaces();
                        builder.RegisterType<FakeEtcdLocker>().AsImplementedInterfaces();
                        builder.RegisterType<FakeNotifyOrderService>().AsImplementedInterfaces();
                        builder.RegisterType<FakeRedisService>().AsImplementedInterfaces().SingleInstance();
                    })
                    .Build();

            var serviceScope = _host.Services.CreateScope();
            _orderStorageService = GetService<IOrderStorageService>(serviceScope);
            _mmPartnerOrderMatchService = GetService<MmPartnerOrderMatchService>(serviceScope);
            _memberOrderMatchService = GetService<MemberOrderMatchService>(serviceScope);
            _matchTradeTimeService = GetService<ITimeService>(serviceScope);
        }

        [TearDown]
        public void Clean()
        {
            ((MySqlOrderStorageService)_orderStorageService).Clean();
        }

        [Test]
        public void 建立OrderDto()
        {
            var matchOrderDto = CreateOrder(50, false, EnumOrderType.MMPay, 1);
            Console.WriteLine(matchOrderDto.ToJson());
        }

        [Test]
        public async Task 承兌商買會員賣_金額完全符合_無剩餘訂單()
        {
            var buyOrderDto = CreateOrder(100, true, EnumOrderType.MMPay, 1);
            await _mmPartnerOrderMatchService.Match(buyOrderDto);

            var sellOrderDto = CreateOrder(100, false, EnumOrderType.Member, 2);

            var matchResult = await _memberOrderMatchService.Match(sellOrderDto);

            var matchOrders = await _orderStorageService.GetMatchingPool();
            Assert.IsEmpty(matchOrders);
            CheckResult_ALL_DEAL(matchResult.taker);
        }

        [Test]
        public async Task 承兌商價錢低於會員價錢_不能成交_剩餘一筆_因為會員建立之後馬上取消()
        {
            var buyPrice = 100;
            var buyOrderDto = CreateOrder(buyPrice, true, EnumOrderType.MMPay, 1);

            await _mmPartnerOrderMatchService.Match(buyOrderDto);

            var sellPrice = 150;
            var sellOrderDto = CreateOrder(sellPrice, false, EnumOrderType.Member, 2);

            var matchResult = await _memberOrderMatchService.Match(sellOrderDto);

            CheckResult_ShouldBeCancelOrder(matchResult.taker, 150);
            var matchOrders = await _orderStorageService.GetMatchingPool();
            Assert.AreEqual(matchOrders.Count, 1);
        }

        [Test]
        public async Task 承兌商賣150會員買100_剩一筆承兌商_賣_額度50()
        {
            var sellPrice = 150;
            var sellOrderDto = CreateOrder(sellPrice, false, EnumOrderType.MMPay, 1);
            var sellResult = await _mmPartnerOrderMatchService.Match(sellOrderDto);
            CheckResult_StillNewOrder(sellResult.taker, 150);

            var buyPrice = 100;
            var buyOrderDto = CreateOrder(buyPrice, true, EnumOrderType.Member, 2);
            var buyResult = await _memberOrderMatchService.Match(buyOrderDto);


            var matchOrders = await _orderStorageService.GetMatchingPool();
            Assert.AreEqual(matchOrders.Count, 1);
            var sum = matchOrders.Sum(r => r.NoDealAmount);
            Assert.AreEqual(sum, sellPrice - buyPrice);
            CheckResult_ALL_DEAL(buyResult.taker);
        }

        [Test]
        public async Task 承兌商買200_會員賣100_會員賣50_最後剩餘一筆買()
        {
            var order1 = CreateOrder(200, true, EnumOrderType.MMPay, 1);
            var matchOrderDto1 = await _mmPartnerOrderMatchService.Match(order1);
            CheckResult_StillNewOrder(matchOrderDto1.taker, 200);

            var order2 = CreateOrder(100, false, EnumOrderType.Member, 2);
            var matchOrderDto2 = await _memberOrderMatchService.Match(order2);
            CheckResult_ALL_DEAL(matchOrderDto2.taker);

            var order3 = CreateOrder(50, false, EnumOrderType.Member, 3);
            var matchOrderDto3 = await _memberOrderMatchService.Match(order3);
            CheckResult_ALL_DEAL(matchOrderDto3.taker);

            var matchOrders = await _orderStorageService.GetMatchingPool();
            var count = matchOrders.Count(r => r.IsBuyOrder);
            Assert.AreEqual(count, 1);
            var noDealAmount = matchOrders.Where(r=>r.IsBuyOrder).Select(r=>r.NoDealAmount).First();
            Assert.AreEqual(noDealAmount, 50);
        }

        [Test]
        public async Task 承兌商與承兌商不能搓合()
        {
            var order1 = CreateOrder(100, true, EnumOrderType.MMPay, 1);
            var matchOrderDto1 = await _mmPartnerOrderMatchService.Match(order1);
            CheckResult_StillNewOrder(matchOrderDto1.taker, 100);

            var order2 = CreateOrder(100, false, EnumOrderType.MMPay, 2);
            var matchOrderDto2 = await _mmPartnerOrderMatchService.Match(order2);
            CheckResult_StillNewOrder(matchOrderDto2.taker, 100);

            var matchOrders = await _orderStorageService.GetMatchingPool();
            Assert.AreEqual(matchOrders.Count, 2);
        }

        [Test]
        public async Task 會員自己是承兌商時不能搓合到自己的出入款()
        {
            var order1 = CreateOrder(100, true, EnumOrderType.MMPay, 1);
            var matchOrderDto1 = await _mmPartnerOrderMatchService.Match(order1);
            CheckResult_StillNewOrder(matchOrderDto1.taker, 100);

            var order2 = CreateOrder(100, false, EnumOrderType.Member, 1);
            var matchOrderDto2 = await _memberOrderMatchService.Match(order2);
            CheckResult_ShouldBeCancelOrder(matchOrderDto2.taker, 100);

            var matchOrders = await _orderStorageService.GetMatchingPool();
            Assert.AreEqual(matchOrders.Count, 1);
        }

        [Test]
        public async Task 承兌商的單有最小交易金額_會員金額低於金額限制_無法成功配對()
        {
            var order1 = CreateOrder(100, true, EnumOrderType.MMPay, 1);
            SetMinPrice(order1, 60);
            var matchOrderDto1 = await _mmPartnerOrderMatchService.Match(order1);
            CheckResult_StillNewOrder(matchOrderDto1.taker, 100);

            var order2 = CreateOrder(40, false, EnumOrderType.Member, 1);
            var matchOrderDto2 = await _memberOrderMatchService.Match(order2);
            CheckResult_ShouldBeCancelOrder(matchOrderDto2.taker, 40);

            var matchOrders = await _orderStorageService.GetMatchingPool();
            Assert.AreEqual(matchOrders.Count, 1);
        }

        [Test]
        [Ignore("不適用於會員打API搓合失敗就直接取消的版本(MemberOrderMatchService)")]
        public async Task 整合測試_以承兌商取款_vs_會員公司入款為主()
        {
            // 1. 「承兌商(A)」MM付取款：申請$10,000，每筆最低限額$1,500
            var order1 = CreateOrder(10000, false, EnumOrderType.MMPay, 1);
            SetMinPrice(order1, 1500);
            var (taker, tradeResults) = await _mmPartnerOrderMatchService.Match(order1);

            // 2. 「會員」公司入款：搓和成功子單 aa、bb、cc（dd、gg已超出總上限，ff、ee不符最低限額，以下為依序產生的存款掛單）
            // aa：$2,000 (轉帳失敗)
            var order2 = CreateOrder(2000, true, EnumOrderType.Member, 2);
            var match2 = await _memberOrderMatchService.Match(order2);
            Assert.IsTrue(match2.tradeResults.Any(r => r.MakerUserId == 1 && r.Amount == 2000));
            // bb：$3,150 (轉帳OK)
            var order3 = CreateOrder(3150, true, EnumOrderType.Member, 3);
            var match3 = await _memberOrderMatchService.Match(order3);
            Assert.IsTrue(match3.tradeResults.Any(r => r.MakerUserId == 1 && r.Amount == 3150));
            // cc：$3,500 (轉帳OK)
            var order4 = CreateOrder(3500, true, EnumOrderType.Member, 4);
            var match4 = await _memberOrderMatchService.Match(order4);
            Assert.IsTrue(match4.tradeResults.Any(r => r.MakerUserId == 1 && r.Amount == 3500));
            // dd：$1,500
            var order5 = CreateOrder(1500, true, EnumOrderType.Member, 5);
            var match5 = await _memberOrderMatchService.Match(order5);
            Assert.IsFalse(match5.tradeResults.Any());
            // ee：$350
            var order6 = CreateOrder(350, true, EnumOrderType.Member, 6);
            var match6 = await _memberOrderMatchService.Match(order6);
            Assert.IsFalse(match6.tradeResults.Any());
            // ff：$1,100
            var order7 = CreateOrder(1100, true, EnumOrderType.Member, 7);
            var match7 = await _memberOrderMatchService.Match(order7);
            Assert.IsFalse(match7.tradeResults.Any());
            // gg：$1,800
            var order8 = CreateOrder(1800, true, EnumOrderType.Member, 8);
            var match8 = await _memberOrderMatchService.Match(order8);
            Assert.IsFalse(match8.tradeResults.Any());

            var matchingPool = await _orderStorageService.GetMatchingPool();
            var orderPools = matchingPool.Where(r => r.IsBuyOrder == false).ToList();
            Assert.AreEqual(1, orderPools.Count);
            Assert.AreEqual(8650m, orderPools.First().DealAmount);
            Assert.AreEqual(1350m, orderPools.First().NoDealAmount);

            // 3. 站長重新掛單：(總出款成功$6,650)
            // 撤單餘額：$1,350 ($10,000-$2,000-$3,150-$3,500) ，已不足每筆最低限額
            var cancelOrderDto = await _mmPartnerOrderMatchService.Cancel(taker);
            Assert.AreEqual(8650m, cancelOrderDto.DealAmount);
            Assert.AreEqual(1350m, cancelOrderDto.NoDealAmount);

            matchingPool = await _orderStorageService.GetMatchingPool();
            orderPools = matchingPool.Where(r => r.IsBuyOrder == false).ToList();
            Assert.AreEqual(0, orderPools.Count);

            // 重新掛單金額：$3,350 ($1,350+$2,000)
            var order9 = CreateOrder(3350, false, EnumOrderType.MMPay, 1);
            SetMinPrice(order9, 1500);
            var (taker2, tradeResults2) = await _mmPartnerOrderMatchService.Match(order9);
            // 重新匹配到兩筆掛單
            Assert.AreEqual(2, tradeResults2.Count);
            // dd：$1,500 掛單 > 最後成功
            Assert.AreEqual(1, tradeResults2.Count(r => r.MakerOrderId == match5.taker.Id));
            // gg：$1,800 掛單 > 最後成功
            Assert.AreEqual(1, tradeResults2.Count(r => r.MakerOrderId == match8.taker.Id));
            // 4. 歷程：（餘額掛單中：$50，ff 不符最低限額）
            Assert.AreEqual(50, taker2.NoDealAmount);

            matchingPool = await _orderStorageService.GetMatchingPool();
            var sellOrderPools = matchingPool.Where(r => r.IsBuyOrder == false).ToList();
            // 重新掛單
            Assert.AreEqual(1, sellOrderPools.Count);
            Assert.AreEqual(50, sellOrderPools.First().NoDealAmount);

            // ee、ff 尚在掛單
            var buyOrderPools = matchingPool.Where(r => r.IsBuyOrder == true).ToList();
            Assert.AreEqual(2, buyOrderPools.Count);
            Assert.IsTrue(buyOrderPools.Any(r => r.Id == match6.taker.Id));
            Assert.IsTrue(buyOrderPools.Any(r => r.Id == match7.taker.Id));

            // 5. 站長撤單：(餘額掛單中：$50)
            var cancelOrderDto2 = await _mmPartnerOrderMatchService.Cancel(taker2);
            Assert.AreEqual(3300m, cancelOrderDto2.DealAmount);
            Assert.AreEqual(50m, cancelOrderDto2.NoDealAmount);
        }

        [Test]
        public async Task 未處理額度()
        {
            var order1 = CreateOrder(50, false, EnumOrderType.MMPay, 1);
            var (taker, tradeResults) = await _mmPartnerOrderMatchService.Match(order1);

            var order2 = CreateOrder(1, true, EnumOrderType.Member, 2);
            var (taker2, tradeResults2) = await _memberOrderMatchService.Match(order2);

            var matchingPool = await _orderStorageService.GetMatchingPool();
            var orderPool1 = matchingPool.First(r => r.Id == taker.Id);
            Assert.AreEqual(orderPool1.NoDealAmount, taker.Amount - taker2.Amount);

            var order3 = CreateOrder(1, true, EnumOrderType.Member, 3);
            var (taker3, tradeResults3) = await _memberOrderMatchService.Match(order3);

            matchingPool = await _orderStorageService.GetMatchingPool();
            var orderPool2 = matchingPool.First(r => r.Id == taker.Id);
            Assert.AreEqual(orderPool2.NoDealAmount, taker.Amount - taker2.Amount - taker3.Amount);
        }

        private static void SetMinPrice(MatchOrderDto order1, int orderMinPriceEachMatch)
        {
            order1.MinPriceEachMatch = orderMinPriceEachMatch;
        }

        private static void CheckResult_StillNewOrder(MatchOrderDto matchOrderDto, int realNoDealAmount)
        {
            Assert.AreEqual(matchOrderDto.State, EnumOrderState.ORDER.GetCode());
            Assert.AreEqual(matchOrderDto.NoDealAmount, realNoDealAmount);
        }

        private static void CheckResult_ShouldBeCancelOrder(MatchOrderDto matchOrderDto, int realNoDealAmount)
        {
            Assert.AreEqual(matchOrderDto.State, EnumOrderState.CANCEL.GetCode());
            Assert.AreEqual(matchOrderDto.NoDealAmount, realNoDealAmount);
        }

        private MatchOrderDto CreateOrder(int orderPrice, bool isBuyOrder, EnumOrderType enumOrderType, int userId)
        {
            return new MatchOrderDto()
            {
                Price = 1,
                Number = orderPrice,
                Amount = orderPrice,
                Priority = 5,
                CreateTime = _matchTradeTimeService.GetNow(),
                UserId = userId,
                SourceOrderId = userId.ToString(), //(++identity).ToString(),
                OrderType = enumOrderType.GetCode(),
                SymbolId = 1,
                IsBuyOrder = isBuyOrder,
                NoDealAmount = orderPrice,
                NoDealNum = orderPrice,
                State = EnumOrderState.REQUEST.GetCode(),
            };
        }

        private static void CheckResult_ALL_DEAL(MatchOrderDto orderDto)
        {
            Assert.AreEqual(orderDto.State, EnumOrderState.ALL_DEAL.GetCode());
            Assert.AreEqual(orderDto.NoDealAmount, 0);
            Assert.AreEqual(orderDto.NoDealNum, 0);
        }
    }
}