using System;
using System.Collections.Generic;
using Nest;

namespace elasticsearch_nest_webapi_angularjs.Models
{
    public class Post
    {        
        public string Id { get; set; }
        
        public DateTime? CreationDate { get; set; }
        
        public int? Score { get; set; }
        
        public int? AnswerCount { get; set; }
        
        public string Body { get; set; }
        
        public string Title { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed)]
        public IEnumerable<string> Tags { get; set; }

        [Completion]
        public IEnumerable<string> Suggest { get; set; }
    }
}
