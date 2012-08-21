using System;
using jinx.types.Attributes;

namespace MvcApp.Demo.Models
{
    [JavaScript]
    public class ToDoItem
    {
        public Guid guid { get; set; }
        public string title { get; set; }
        public bool completed { get; set; }
    }
}