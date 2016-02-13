using System.Collections.Generic;

namespace elasticsearch_nest_webapi_angularjs.Models
{
    public class PageResult<T>
    {
        public int Total { get; set; }

        public int Page { get; set; }

        public IEnumerable<T> Data { get; set; }
    }
}