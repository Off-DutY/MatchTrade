namespace CoreLibrary.Interface
{
    public interface IMultilingualService
    {
        /// <summary>
        /// 查詢語系文字
        /// </summary>
        /// <param name="textKey">key值</param>
        /// <param name="defaultText">資料庫對應不到key時，要顯示的預設字串。沒指定則直接把key吐回。</param>
        /// <returns></returns>
        string Get(string textKey, string defaultText = null);
    }
}