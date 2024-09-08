using Newtonsoft.Json;

namespace CoreLibrary
{
    public static class JsonNetExtension
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T JsonToObject<T>(this string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public static bool In<T>(this T target, params T[] whereToFind)
        {
            return whereToFind.Contains(target);
        }
    }
}