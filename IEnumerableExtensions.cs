using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpExtensions.OpenSource
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TSRes> LeftJoin<T, S, TSKey, TSRes>(this IEnumerable<T> rightTbl,
            IEnumerable<S> leftTbl, Func<T, TSKey> rigthTblKey, Func<S, TSKey> leftTblKey, Func<T, S, TSRes> result)
            where T : class, new() where S : class, new()
        {
            return from item1 in rightTbl
                   join x in leftTbl on rigthTblKey(item1) equals leftTblKey(x) into leftWithoutNulls
                   from item2 in leftWithoutNulls.DefaultIfEmpty()
                   select result.Invoke(item1, item2);
        }

        public static T? GetFirstOrNull<T>(this IEnumerable<T> items, Func<T, bool>? predicate, T? defaultVal = null) where T : struct
        {
            var res = items.Where(predicate ?? (_ => true));
            return res.Any() ? res.First() : defaultVal;
        }


        public static async IAsyncEnumerable<T> AsyncEnumerableEmpty<T>()
        {
            await Task.CompletedTask;
            yield break;
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable.GetEmptyIfNull())
            {
                yield return item;
            }
            await Task.CompletedTask;
        }

        public static async IAsyncEnumerable<List<T>> Pagination<T>(this IAsyncEnumerable<T> asyncEnumerable, int bulkSize = 5, bool logPaginate = true, string extraInfoForLog = "", Action<string>? logger = null)
        {
            logger = logger ?? (str => { });
            var totalCount = 0;
            var lst = new List<T>();
            await foreach (var item in asyncEnumerable)
            {
                lst.Add(item);
                if (lst.Count == bulkSize)
                {
                    totalCount += lst.Count;
                    if (logPaginate) { logger($"{extraInfoForLog}Total Paginated {totalCount:n0}"); }
                    yield return lst.ToList();
                    lst.Clear();
                }
            }
            if (lst.Count > 0)
            {
                totalCount += lst.Count;
                if (logPaginate) { logger($"{extraInfoForLog}Total Paginated {totalCount:n0}"); }
                yield return lst.ToList();
            }
        }

        public static IEnumerable<List<T>> Pagination<T>(this IEnumerable<T> data, int bulkSize = 5, bool logPaginate = true)
            => data.PaginationBySizeOrCount(sizeInMb: null, bulkSize: bulkSize, logPaginate: logPaginate);

        public static IEnumerable<List<T>> PaginationBySize<T>(this IEnumerable<T> data, int sizeInMb = 50, bool logPaginate = true)
            => data.PaginationBySizeOrCount(sizeInMb: sizeInMb, bulkSize: null, logPaginate: logPaginate);

        public static IEnumerable<List<T>> PaginationBySizeOrCount<T>(this IEnumerable<T> data, int? sizeInMb = 50, int? bulkSize = 5, bool logPaginate = true, Func<T, string>? jsonFormat = null, Action<string>? logger = null)
        {
            logger = logger ?? (str => { });
            jsonFormat = jsonFormat ?? (item => JsonConvert.SerializeObject(item));
            var dataWithSize = data.Select(x => new { data = x, size = sizeInMb == null ? 0 : Encoding.Unicode.GetByteCount(jsonFormat(x)) / 1048576.0 });
            dataWithSize = sizeInMb == null ? dataWithSize : dataWithSize.OrderByDescending(x => x.size);
            var totalCount = 0;
            var lst = new List<T>();
            double currentMbSize = 0;
            foreach (var item in dataWithSize)
            {
                lst.Add(item.data);
                if ((currentMbSize + item.size >= sizeInMb && sizeInMb != null) || (lst.Count == bulkSize && bulkSize != null))
                {
                    totalCount += lst.Count;
                    if (logPaginate) { logger($"Total Paginated {totalCount:n0}"); }
                    yield return lst.ToList();
                    currentMbSize = 0;
                    lst.Clear();
                    continue;
                }
                currentMbSize += item.size;
            }
            if (lst.Count > 0)
            {
                totalCount += lst.Count;
                if (logPaginate) { logger($"Total Paginated {totalCount:n0}"); }
                yield return lst.ToList();
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> lst) => lst.OrderBy(x => new Random().Next());
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var lst = new List<T>();
            await foreach (var item in asyncEnumerable)
            {
                lst.Add(item);
            }
            return lst;
        }
        public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            await foreach (var item in asyncEnumerable)
            {
                return item;
            }
            return default;
        }

        public static IEnumerable<T> GetEmptyIfNull<T>(this IEnumerable<T>? coll) => coll ?? Enumerable.Empty<T>();

        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?>? coll) where T : struct => coll.GetEmptyIfNull().Where(x => x != null).Cast<T>();

        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?>? coll) where T : class => coll.GetEmptyIfNull().Where(x => x != null).Cast<T>();

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> coll) => coll.Select((item, index) => (item, index));
        public static string StrJoin<T>(this IEnumerable<T> lst, string separator = ",") => string.Join(separator, lst);
        public static Task ParallelAsync<T>(this IEnumerable<T> inputEnumerable, Func<T, Task> asyncProcessor, int maxDegreeOfParallelism = 15, bool autoRun = true)
           => inputEnumerable.ParallelAsync(async item => { await asyncProcessor(item); return true; }, maxDegreeOfParallelism, autoRun);

        public static async Task<List<S>> ParallelAsync<T, S>(this IEnumerable<T> inputEnumerable, Func<T, Task<S>> asyncProcessor, int maxDegreeOfParallelism = 15, bool autoRun = true)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
            var tasks = inputEnumerable.Select(async input =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    return await asyncProcessor(input).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();
            var foreachTask = autoRun ? Task.WhenAll(tasks) : new Task(() => Task.WhenAll(tasks));
            await foreachTask;
            return tasks.Select(x => x.Result).ToList();
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> inputEnumerable, Func<T, Task> invoke)
        {
            foreach (var item in inputEnumerable)
            {
                await invoke(item);
            }
        }

        public static async Task<IEnumerable<S>> SelectAsync<T, S>(this IEnumerable<T> inputEnumerable, Func<T, Task<S>> invoke)
        {
            var lst = new List<S>();
            foreach (var item in inputEnumerable)
            {
                lst.Add(await invoke(item));
            }
            return lst;
        }

        // DistinctBy for distinct list of object by dynamic select of the element
        // ex: [{ a: 1 }, { a: 1 }] DistinctBy(x => x.a) will return [{ a: 1 }]
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => source.GroupBy(keySelector).Select(x => x.First());

        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            foreach (var element in source)
                target.Add(element);
        }

        public static void ForEach<T>(this IEnumerable<T> value, Action<T> action)
        {
            foreach (T item in value)
            {
                action(item);
            }
        }
    }
}
