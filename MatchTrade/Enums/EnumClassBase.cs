namespace MatchTrade.Enums
{
    public abstract class EnumClassBase
    {
        protected readonly int Code;

        protected readonly string Desc;

        
        protected EnumClassBase(int code, string desc)
        {
            Code = code;
            Desc = desc;
        }

        public int GetCode()
        {
            return Code;
        }

        public string GetDesc()
        {
            return Desc;
        }
    }
}