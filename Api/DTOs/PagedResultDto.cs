namespace Api.DTOs
{
    public class PagedResultDto<T>
    {
        public int TotalCount { get; set; } // کل رکوردها
        public int PageNumber { get; set; } // صفحه فعلی
        public int PageSize { get; set; } // تعداد در هر صفحه
        public List<T> Items { get; set; } = new();
    }
}