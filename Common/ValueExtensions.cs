using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pluralize.NET;

namespace Common
{
    public static class ValueExtensions
    {
        #region String

        public static string ToPlural(this bool condition, string word)
        {
            var pluralizer = new Pluralizer();
            return (condition && pluralizer.IsSingular(word)) ? new Pluralizer().Pluralize(word) : word;
        }

        #endregion

        #region Functions

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        public static void Throws<T>(this bool condition, T exception, ILogger logger = null, string loggingSource = "") where T : Exception
        {
            if (condition)
            {
                logger?.LogError(loggingSource, exception).Wait();
                throw exception;
            }
        }

        #endregion
    }
}
