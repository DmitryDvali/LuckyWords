using LuckyWords.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuckyWords
{
    class Context : DbContext
    {
        public DbSet<InitialWord> InitialWordsList { get; set; }
        public DbSet<DictionaryWord> Dictionary { get; set; }
        public Context() : base("localsql")
        { }
    }
}
