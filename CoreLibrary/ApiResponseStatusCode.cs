namespace CoreLibrary
{
    public enum ApiResponseStatusCode : int
    {
        /// <summary>
        /// 正確執行
        /// </summary>
        Success = 200,

        /// <summary>
        /// 發生已知錯誤，RD吐出的錯誤，前端"不"需彈窗顯示
        /// </summary>
        HideErrorMessage = 201,

        /// <summary>
        /// 發生已知錯誤，RD吐出的錯誤，前端需彈窗顯示
        /// </summary>
        PopupErrorMessage = 202,

        /// <summary>
        /// 未知錯誤噴Exception，由ExceptionHandleMiddleware處理
        /// </summary>
        UnknownErrorMessage = 400,

        /// <summary>
        /// Master专案使用, 子帐号没有权限执行此操作 
        /// </summary>
        NotAllowedExecute = 403,

        /// <summary>
        /// 呼叫三方轉帳失敗, 目前僅使用於FinanceCenter與FinancialSupervisorCenter
        /// </summary>
        WalletTransferFail = 601,

        /// <summary>
        /// 呼叫三方轉帳不明, 目前僅使用於FinanceCenter與FinancialSupervisorCenter
        /// </summary>
        WalletTransferUnknown = 602,
    }
}