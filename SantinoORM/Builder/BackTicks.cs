using System.Text;

namespace SantinoORM.Builder
{
    public static class BackTicks
    {
        /// <summary>
        /// Adds ` ` to a column or table to make sure the query is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AddBackTicks(string value)
        {
            return new StringBuilder("`").Append(value).Append("`").ToString();
        }
    }
}