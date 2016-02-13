using System;
using System.Collections.Generic;

namespace elasticsearch_nest_webapi_angularjs.Models
{
    public class Post
    {        
        public string Id { get; set; }
        
        public DateTime? CreationDate { get; set; }
        
        public int? Score { get; set; }
        
        public int? ViewCount { get; set; }
        
        public string Body { get; set; }
        
        public string Title { get; set; }
        
        public IEnumerable<string> Tags { get; set; }
        
        public int? AnswerCount { get; set; }
        
        public int? CommentCount { get; set; }
        
        public int? FavoriteCount { get; set; }
        
        public DateTime? LastEditDate { get; set; }
        
        public DateTime? LastActivityDate { get; set; }
    }
}