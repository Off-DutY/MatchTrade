# 交易撮合系統

## 1. 目的

- 金融媒合系統，主要用於撮合買賣雙方的交易需求，以達到交易的目的。

## 2. 功能

- 交易撮合系統主要功能是撮合買賣雙方的交易需求，以達到交易的目的。撮合系統的主要功能如下：
    - 接收委託
    - 撮合交易
    - 交易成交

## 3. API

- **TradeController**: 提供 API 介面，用於處理交易相關的請求。
    - `GetMatchingPool`: 獲取當前的撮合池
    - `Order`: 提交新訂單
    - `MmPayOrder`: 提交 MM 商的訂單
    - `CancelOrder`: 取消訂單
    - `TakingOrderResult`: MM商接受訂單

## 4. 主要類別

- **IOrderMatchService**: 定義了撮合交易的主要流程和方法。
    - **OrderMatchService**: `IOrderMatchService` 的基礎實現類別，實現了撮合交易的主要流程和方法。
        - **MemberOrderMatchService**: `OrderMatchService` 的基礎實現類別，實現了**購買方**撮合交易的主要流程和方法。
        - **MmPartnerOrderMatchService**: `OrderMatchService` 的基礎實現類別，實現了**販售方**撮合交易的主要流程和方法。
    - **MatchStrategyFactory**: 用於創建 `OrderMatchService` 實例。

- **IOrderStorageService**: 定義了訂單存儲服務的接口。
    - **MySqlOrderStorageService**: `IOrderStorageService` 的基礎實現類別，採用MySql實現了訂單存儲服務的接口。

- **IMatchEngineService**: 定義了撮合引擎的接口。
    - **MatchEngineService**: `IMatchEngineService` 的基礎實現類別。

- **IOrderNotifyService**: 定義了媒合成功通知服務的接口。
    - **OrderNotifyService**: `IOrderNotifyService` 的基礎實現類別。

## 5. 使用技術

- 語言: C#
- 框架: .NET
- 日誌: Microsoft.Extensions.Logging
- 依賴注入: Microsoft.Extensions.DependencyInjection